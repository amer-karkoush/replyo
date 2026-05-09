using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Replyo.Application.Auth.Commands.RefreshTokens;
using Replyo.Application.Common.Abstractions;
using Replyo.Application.Common.Exceptions;
using Replyo.Application.Tests.TestInfrastructure;
using Replyo.Domain.Entities;

namespace Replyo.Application.Tests.Auth.Commands.RefreshTokens;

/// <summary>
/// Tests for the rotation chain replay-detection property. Covers the security-critical
/// path (replay detection revokes all active tokens) plus a control test that proves the
/// fixture and handler wiring work end-to-end.
/// </summary>
public class RefreshTokenHandlerTests : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public RefreshTokenHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Each test gets a clean slate. The container persists across tests; only data resets.
        await _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task HandleAsync_WhenPresentedWithValidToken_RotatesAndIssuesNewPair()
    {
        // Control test. Validates that the fixture and handler wiring work: a valid
        // refresh token gets rotated, a new pair is issued, the old token is marked
        // revoked with ReplacedByTokenHash pointing at the new one.
        await using var ctx = _fixture.CreateContext();
        var (handler, _) = BuildHandler(ctx);
        var (user, originalToken, originalPlaintext) = await SeedUserWithRefreshTokenAsync(ctx);

        var command = new RefreshTokenCommand(originalPlaintext, CreatedByIp: "127.0.0.1");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(originalPlaintext, "rotation must produce a new token");

        // Reload from DB rather than asserting on the in-memory tracked entity — the
        // change tracker can lie about persistence state until SaveChanges flushes.
        await using var verifyCtx = _fixture.CreateContext();
        var reloadedOld = await verifyCtx.RefreshTokens
            .SingleAsync(rt => rt.Id == originalToken.Id);

        reloadedOld.IsRevoked.Should().BeTrue("the presented token is revoked on rotation");
        reloadedOld.ReplacedByTokenHash.Should().NotBeNullOrEmpty(
            "the rotation chain points the old token at the new one");

        var newActive = await verifyCtx.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .SingleAsync();
        newActive.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenPresentedWithRevokedToken_RevokesAllActiveTokensForUser()
    {
        // Security test. Replay scenario: an attacker presents a token whose chain has
        // already rotated past it. The handler should revoke ALL active tokens for the
        // user (including the legitimate current one) and refuse the refresh.
        await using var ctx = _fixture.CreateContext();
        var (handler, _) = BuildHandler(ctx);
        var (user, oldToken, oldPlaintext) = await SeedUserWithRefreshTokenAsync(ctx);

        // Simulate a prior legitimate rotation: the old token is already revoked,
        // and a new active token exists for the same user.
        oldToken.Revoke(revokedByIp: "127.0.0.1", replacedByTokenHash: "some-newer-hash");

        var currentActiveToken = RefreshToken.Issue(
            userId: user.Id,
            tokenHash: "current-active-hash",
            expiresAt: DateTimeOffset.UtcNow.AddDays(30),
            createdByIp: "127.0.0.1");
        ctx.RefreshTokens.Add(currentActiveToken);
        await ctx.SaveChangesAsync();

        var attackerCommand = new RefreshTokenCommand(oldPlaintext, CreatedByIp: "10.0.0.99");

        var act = async () => await handler.HandleAsync(attackerCommand, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*replay*", "the handler must signal replay detection");

        // The currently-active token should now be revoked too — log out everywhere is
        // the correct response when we can't distinguish legitimate from malicious replay.
        await using var verifyCtx = _fixture.CreateContext();
        var reloadedActive = await verifyCtx.RefreshTokens
            .SingleAsync(rt => rt.Id == currentActiveToken.Id);
        reloadedActive.IsRevoked.Should().BeTrue(
            "replay detection must revoke all active tokens for the user, not just the chain");
    }

private static (RefreshTokenHandler handler, FakeJwtTokenService fakeJwt) BuildHandler(
    IApplicationDbContext ctx)
{
    var validator = new RefreshTokenCommandValidator();
    var fakeJwt = new FakeJwtTokenService();
    var handler = new RefreshTokenHandler(validator, ctx, fakeJwt);
    return (handler, fakeJwt);
}

 private static async Task<(User user, RefreshToken token, string plaintext)>
    SeedUserWithRefreshTokenAsync(IApplicationDbContext ctx)
{
        var tenant = Tenant.Create("Test Tenant", "test-tenant");
        ctx.Tenants.Add(tenant);

        var user = User.CreateOwner(
            tenant.Id,
            email: "test@example.com",
            passwordHash: "fake-hash:irrelevant",
            fullName: "Test User");
        ctx.Users.Add(user);

        // The plaintext we seed must match what FakeJwtTokenService would have produced
        // for this user. We don't go through the service at seed time because we want
        // a stable, predictable plaintext we control.
        var plaintext = $"fake-refresh-{user.Id}-1";
        var hash = HashForTest(plaintext);

        var token = RefreshToken.Issue(
            userId: user.Id,
            tokenHash: hash,
            expiresAt: DateTimeOffset.UtcNow.AddDays(30),
            createdByIp: "127.0.0.1");
        ctx.RefreshTokens.Add(token);

        await ctx.SaveChangesAsync();
        return (user, token, plaintext);
    }

    private static string HashForTest(string plaintext)
    {
        // Mirrors RefreshTokenHandler.HashRefreshToken and FakeJwtTokenService.HashRefreshToken.
        // Three call sites for the same algorithm — the duplication is tracked in PROGRESS;
        // a fourth caller triggers extraction to a shared helper in Common/.
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(plaintext));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}