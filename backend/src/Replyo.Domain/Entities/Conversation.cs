using Replyo.Domain.Common;

namespace Replyo.Domain.Entities;

public class Conversation : EntityBase
{
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;

    public Guid WidgetVisitorId { get; private set; }
    public WidgetVisitor WidgetVisitor { get; private set; } = null!;

    public string? Subject { get; private set; }
    public bool IsResolved { get; private set; }
    public bool IsEscalated { get; private set; }
    public Guid? AssignedHumanAgentId { get; private set; }
    public User? AssignedHumanAgent { get; private set; }
    public DateTimeOffset LastMessageAt { get; private set; } = DateTimeOffset.UtcNow;

    public ICollection<Message> Messages { get; private set; } = new List<Message>();

    private Conversation() { }

    public static Conversation Create(Guid tenantId, Guid widgetVisitorId)
    {
        return new Conversation
        {
            TenantId = tenantId,
            WidgetVisitorId = widgetVisitorId
        };
    }

    public void RecordNewMessage()
    {
        LastMessageAt = DateTimeOffset.UtcNow;
        Touch();
    }

    public void Escalate(Guid humanAgentId)
    {
        if (IsEscalated) return;
        IsEscalated = true;
        AssignedHumanAgentId = humanAgentId;
        Touch();
    }

    public void Resolve()
    {
        if (IsResolved) return;
        IsResolved = true;
        Touch();
    }

    public void Reopen()
    {
        if (!IsResolved) return;
        IsResolved = false;
        Touch();
    }

    public void UpdateSubject(string? subject)
    {
        Subject = string.IsNullOrWhiteSpace(subject) ? null : subject.Trim();
        Touch();
    }
}