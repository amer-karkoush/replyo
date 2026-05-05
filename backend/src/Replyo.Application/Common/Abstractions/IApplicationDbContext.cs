using Microsoft.EntityFrameworkCore;
using Replyo.Domain.Entities;

namespace Replyo.Application.Common.Abstractions;

/// <summary>
/// Persistence contract for the Application layer. Implemented by the EF Core DbContext
/// in the Infrastructure layer; exposes the DbSets handlers need plus a save method.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<KnowledgeDocument> KnowledgeDocuments { get; }
    DbSet<KnowledgeChunk> KnowledgeChunks { get; }
    DbSet<WidgetVisitor> WidgetVisitors { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}