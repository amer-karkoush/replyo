using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Replyo.Application.Common.Multitenancy;
using Replyo.Domain.Entities;

namespace Replyo.Infrastructure.Persistence;

public class ReplyoDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    public ReplyoDbContext(
        DbContextOptions<ReplyoDbContext> options,
        ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<KnowledgeDocument> KnowledgeDocuments => Set<KnowledgeDocument>();
    public DbSet<KnowledgeChunk> KnowledgeChunks => Set<KnowledgeChunk>();
    public DbSet<WidgetVisitor> WidgetVisitors => Set<WidgetVisitor>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public Guid? CurrentTenantId => _currentTenant.TenantId;
}