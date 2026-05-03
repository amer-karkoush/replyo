using Replyo.Domain.Common;

namespace Replyo.Domain.Entities;

public class WidgetVisitor : EntityBase
{
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;

    public string SessionId { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Name { get; private set; }
    public string? UserAgent { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; } = DateTimeOffset.UtcNow;

    private WidgetVisitor() { }

    public static WidgetVisitor Create(Guid tenantId, string sessionId, string? userAgent, string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID is required.", nameof(sessionId));

        return new WidgetVisitor
        {
            TenantId = tenantId,
            SessionId = sessionId,
            UserAgent = userAgent,
            IpAddress = ipAddress
        };
    }

    public void IdentifyVisitor(string? email, string? name)
    {
        Email = email?.Trim().ToLowerInvariant();
        Name = name?.Trim();
        Touch();
    }

    public void RecordSeen()
    {
        LastSeenAt = DateTimeOffset.UtcNow;
        Touch();
    }
}