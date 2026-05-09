using Microsoft.EntityFrameworkCore;
using Replyo.Application.Common.Multitenancy;
using Replyo.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Replyo.Application.Tests.TestInfrastructure;

/// <summary>
/// xUnit class fixture that runs a real Postgres + pgvector container for the duration
/// of a test class. The container starts once when the first test in the class runs,
/// and disposes after the last. Per-test isolation is achieved by data cleanup, not
/// by recreating the container.
/// </summary>
/// <remarks>
/// Why a real container instead of an in-memory provider:
///   - SQLite cannot map the vector(1536) column on KnowledgeChunk; the model fails
///     validation at first use even when the test never queries that entity.
///   - The EF Core in-memory provider doesn't enforce unique constraints, foreign keys,
///     or transaction semantics, so it hides bugs that production would surface.
///
/// Cost: ~5-10s container startup per test class. With xUnit's IClassFixture the cost
/// amortizes across every test method in the class — a single test pays the full cost,
/// 10 tests pay it once.
/// </remarks>
public sealed class PostgresFixture : IAsyncLifetime
{
    // pgvector/pgvector:pg16 is the official image — vanilla postgres:16 lacks the
    // vector extension and the InitialCreate migration's CREATE EXTENSION fails.
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Apply the real migration history once against the freshly-started container.
        // This exercises the same schema-creation path production uses — different from
        // EnsureCreated, which builds from the model and skips migration-specific SQL.
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Creates a fresh DbContext bound to the running container. Each call returns a new
    /// context with its own change tracker; tests should dispose theirs at end of method
    /// (await using).
    /// </summary>
    /// <param name="currentTenant">Optional override for ICurrentTenant. Defaults to a
    /// FakeCurrentTenant with TenantId=null, which matches the production auth flow.</param>
    public ReplyoDbContext CreateContext(ICurrentTenant? currentTenant = null)
    {
        var options = new DbContextOptionsBuilder<ReplyoDbContext>()
            .UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.UseVector())
            .Options;

        return new ReplyoDbContext(options, currentTenant ?? new FakeCurrentTenant());
    }

    /// <summary>
    /// Truncates all tenant-scoped tables in dependency order. Called by tests that need
    /// a clean slate; not called automatically because some test patterns (data-driven
    /// tests, scenarios that depend on prior state) want to manage cleanup themselves.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();

        // Order matters: child tables first, parent tables last, even though ON DELETE
        // CASCADE would handle it. Explicit ordering documents the dependency graph and
        // makes failures in cleanup easier to diagnose.
        context.RefreshTokens.RemoveRange(context.RefreshTokens);
        context.Messages.RemoveRange(context.Messages);
        context.Conversations.RemoveRange(context.Conversations);
        context.WidgetVisitors.RemoveRange(context.WidgetVisitors);
        context.KnowledgeChunks.RemoveRange(context.KnowledgeChunks);
        context.KnowledgeDocuments.RemoveRange(context.KnowledgeDocuments);
        context.Users.RemoveRange(context.Users);
        context.Tenants.RemoveRange(context.Tenants);

        await context.SaveChangesAsync();
    }
}