using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Replyo.Domain.Entities;

namespace Replyo.Infrastructure.Persistence.Configurations;

public class WidgetVisitorConfiguration : IEntityTypeConfiguration<WidgetVisitor>
{
    public void Configure(EntityTypeBuilder<WidgetVisitor> builder)
    {
        builder.ToTable("widget_visitors");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.TenantId).HasColumnName("tenant_id");
        builder.Property(v => v.SessionId).HasColumnName("session_id").HasMaxLength(100).IsRequired();
        builder.Property(v => v.Email).HasColumnName("email").HasMaxLength(255);
        builder.Property(v => v.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(v => v.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        // 45 covers the longest IPv6 representation (including IPv4-mapped form like ::ffff:192.0.2.0).
        builder.Property(v => v.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(v => v.LastSeenAt).HasColumnName("last_seen_at");
        builder.Property(v => v.CreatedAt).HasColumnName("created_at");
        builder.Property(v => v.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(v => new { v.TenantId, v.SessionId })
            .IsUnique();

        builder.HasOne(v => v.Tenant)
            .WithMany()
            .HasForeignKey(v => v.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}