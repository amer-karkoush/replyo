using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Replyo.Domain.Entities;

namespace Replyo.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.TenantId).HasColumnName("tenant_id");
        builder.Property(c => c.WidgetVisitorId).HasColumnName("widget_visitor_id");
        builder.Property(c => c.Subject).HasColumnName("subject").HasMaxLength(500);
        builder.Property(c => c.IsResolved).HasColumnName("is_resolved");
        builder.Property(c => c.IsEscalated).HasColumnName("is_escalated");
        builder.Property(c => c.AssignedHumanAgentId).HasColumnName("assigned_human_agent_id");
        builder.Property(c => c.LastMessageAt).HasColumnName("last_message_at");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(c => new { c.TenantId, c.LastMessageAt });
        builder.HasIndex(c => new { c.TenantId, c.IsResolved });

        builder.HasOne(c => c.Tenant)
            .WithMany(t => t.Conversations)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.WidgetVisitor)
            .WithMany()
            .HasForeignKey(c => c.WidgetVisitorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AssignedHumanAgent)
            .WithMany()
            .HasForeignKey(c => c.AssignedHumanAgentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}