using Replyo.Domain.Common;
using Replyo.Domain.Enums;

namespace Replyo.Domain.Entities;

public class Tenant : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; } = TenantStatus.Active;

    public string SystemPrompt { get; private set; } = string.Empty;
    public string BrandColor { get; private set; } = "#0F6E56";
    public string WelcomeMessage { get; private set; } = "Hi! How can I help you today?";

    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<KnowledgeDocument> KnowledgeDocuments { get; private set; } = new List<KnowledgeDocument>();
    public ICollection<Conversation> Conversations { get; private set; } = new List<Conversation>();

    private Tenant() { }

    public static Tenant Create(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Tenant slug is required.", nameof(slug));

        return new Tenant
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant()
        };
    }

    public void UpdateBranding(string brandColor, string welcomeMessage)
    {
        if (string.IsNullOrWhiteSpace(brandColor))
            throw new ArgumentException("Brand color is required.", nameof(brandColor));

        if (string.IsNullOrWhiteSpace(welcomeMessage))
            throw new ArgumentException("Welcome message is required.", nameof(welcomeMessage));

        BrandColor = brandColor;
        WelcomeMessage = welcomeMessage;
        Touch();
    }

    public void UpdateSystemPrompt(string systemPrompt)
    {
        SystemPrompt = systemPrompt ?? string.Empty;
        Touch();
    }

    public void Suspend()
    {
        if (Status == TenantStatus.Suspended) return;
        Status = TenantStatus.Suspended;
        Touch();
    }

    public void Reactivate()
    {
        if (Status == TenantStatus.Active) return;
        Status = TenantStatus.Active;
        Touch();
    }

}