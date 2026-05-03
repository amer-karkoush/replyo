using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Replyo.Domain.Entities;

namespace Replyo.Infrastructure.Persistence.Configurations;

public class KnowledgeChunkConfiguration : IEntityTypeConfiguration<KnowledgeChunk>
{
    public const int EmbeddingDimensions = 1536;

    public void Configure(EntityTypeBuilder<KnowledgeChunk> builder)
    {
        builder.ToTable("knowledge_chunks");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.TenantId).HasColumnName("tenant_id");
        builder.Property(c => c.KnowledgeDocumentId).HasColumnName("knowledge_document_id");
        builder.Property(c => c.ChunkIndex).HasColumnName("chunk_index");
        builder.Property(c => c.Content).HasColumnName("content").HasColumnType("text").IsRequired();
        builder.Property(c => c.TokenCount).HasColumnName("token_count");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.Property(c => c.Embedding)
            .HasColumnName("embedding")
            .HasColumnType($"vector({EmbeddingDimensions})")
            .HasConversion(
                v => new Vector(v),
                v => v.ToArray());

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.KnowledgeDocumentId);

        builder.HasOne(c => c.KnowledgeDocument)
            .WithMany(d => d.Chunks)
            .HasForeignKey(c => c.KnowledgeDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}