using Replyo.Domain.Common;

namespace Replyo.Domain.Entities;

public class KnowledgeChunk : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid KnowledgeDocumentId { get; private set; }
    public KnowledgeDocument KnowledgeDocument { get; private set; } = null!;

    public int ChunkIndex { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public float[] Embedding { get; private set; } = Array.Empty<float>();
    public int TokenCount { get; private set; }

    private KnowledgeChunk() { }

    public static KnowledgeChunk Create(
        Guid tenantId,
        Guid knowledgeDocumentId,
        int chunkIndex,
        string content,
        float[] embedding,
        int tokenCount)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));

        if (embedding == null || embedding.Length == 0)
            throw new ArgumentException("Embedding is required.", nameof(embedding));

        return new KnowledgeChunk
        {
            TenantId = tenantId,
            KnowledgeDocumentId = knowledgeDocumentId,
            ChunkIndex = chunkIndex,
            Content = content,
            Embedding = embedding,
            TokenCount = tokenCount
        };
    }
}