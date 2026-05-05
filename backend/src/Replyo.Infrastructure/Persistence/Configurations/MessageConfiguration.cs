using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Replyo.Domain.Entities;

namespace Replyo.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);

        // tenant_id is denormalized for query filtering and tenant isolation;
        // FK to tenants is enforced transitively through Conversation.
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.TenantId).HasColumnName("tenant_id");
        builder.Property(m => m.ConversationId).HasColumnName("conversation_id");
        builder.Property(m => m.Role).HasColumnName("role").HasConversion<int>();
        builder.Property(m => m.Content).HasColumnName("content").HasColumnType("text").IsRequired();
        builder.Property(m => m.AuthorUserId).HasColumnName("author_user_id");
        builder.Property(m => m.AiModel).HasColumnName("ai_model").HasMaxLength(100);
        builder.Property(m => m.AiPromptTokens).HasColumnName("ai_prompt_tokens");
        builder.Property(m => m.AiCompletionTokens).HasColumnName("ai_completion_tokens");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.Property(m => m.CitedChunkIds)
            .HasColumnName("cited_chunk_ids")
            .HasColumnType("uuid[]");

        builder.HasIndex(m => new { m.TenantId, m.ConversationId, m.CreatedAt });

        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.AuthorUser)
            .WithMany()
            .HasForeignKey(m => m.AuthorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}