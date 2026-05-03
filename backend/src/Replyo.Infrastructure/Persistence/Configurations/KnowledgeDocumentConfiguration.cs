using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Replyo.Domain.Entities;

namespace Replyo.Infrastructure.Persistence.Configurations;

public class KnowledgeDocumentConfiguration : IEntityTypeConfiguration<KnowledgeDocument>
{
    public void Configure(EntityTypeBuilder<KnowledgeDocument> builder)
    {
        builder.ToTable("knowledge_documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.TenantId).HasColumnName("tenant_id");
        builder.Property(d => d.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(d => d.Source).HasColumnName("source").HasConversion<int>();
        builder.Property(d => d.SourceLocation).HasColumnName("source_location").HasMaxLength(2000);
        builder.Property(d => d.RawContent).HasColumnName("raw_content").HasColumnType("text");
        builder.Property(d => d.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(d => d.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
        builder.Property(d => d.ChunkCount).HasColumnName("chunk_count");
        builder.Property(d => d.CreatedAt).HasColumnName("created_at");
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(d => new { d.TenantId, d.Status });
        builder.HasIndex(d => new { d.TenantId, d.CreatedAt });

        builder.HasOne(d => d.Tenant)
            .WithMany(t => t.KnowledgeDocuments)
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}