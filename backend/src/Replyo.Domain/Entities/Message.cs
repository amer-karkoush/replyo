using Replyo.Domain.Common;
using Replyo.Domain.Enums;

namespace Replyo.Domain.Entities;

public class Message : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = null!;

    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public Guid? AuthorUserId { get; private set; }
    public User? AuthorUser { get; private set; }

    public string? AiModel { get; private set; }
    public int? AiPromptTokens { get; private set; }
    public int? AiCompletionTokens { get; private set; }

    public ICollection<Guid> CitedChunkIds { get; private set; } = new List<Guid>();

    private Message() { }

    public static Message FromVisitor(Guid tenantId, Guid conversationId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content is required.", nameof(content));

        return new Message
        {
            TenantId = tenantId,
            ConversationId = conversationId,
            Role = MessageRole.Visitor,
            Content = content.Trim()
        };
    }

    public static Message FromAssistant(
        Guid tenantId,
        Guid conversationId,
        string content,
        string aiModel,
        int promptTokens,
        int completionTokens,
        IEnumerable<Guid>? citedChunkIds = null)
    {
        return new Message
        {
            TenantId = tenantId,
            ConversationId = conversationId,
            Role = MessageRole.Assistant,
            Content = content.Trim(),
            AiModel = aiModel,
            AiPromptTokens = promptTokens,
            AiCompletionTokens = completionTokens,
            CitedChunkIds = citedChunkIds?.ToList() ?? new List<Guid>()
        };
    }

    public static Message FromHumanAgent(Guid tenantId, Guid conversationId, Guid authorUserId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content is required.", nameof(content));

        return new Message
        {
            TenantId = tenantId,
            ConversationId = conversationId,
            Role = MessageRole.HumanAgent,
            AuthorUserId = authorUserId,
            Content = content.Trim()
        };
    }
}