using Replyo.Domain.Common;
using Replyo.Domain.Enums;

namespace Replyo.Domain.Entities;

public class KnowledgeDocument : EntityBase
{
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;
    public KnowledgeDocumentSource Source { get; private set; }
    public string SourceLocation { get; private set; } = string.Empty;
    public string? RawContent { get; private set; }
    public KnowledgeDocumentStatus Status { get; private set; } = KnowledgeDocumentStatus.Pending;
    public string? ErrorMessage { get; private set; }
    public int ChunkCount { get; private set; }

    public ICollection<KnowledgeChunk> Chunks { get; private set; } = new List<KnowledgeChunk>();

    private KnowledgeDocument() { }

    public static KnowledgeDocument CreateFromUpload(Guid tenantId, string title, string fileName)
    {
        return new KnowledgeDocument
        {
            TenantId = tenantId,
            Title = title.Trim(),
            Source = KnowledgeDocumentSource.UploadedFile,
            SourceLocation = fileName
        };
    }

    public static KnowledgeDocument CreateFromUrl(Guid tenantId, string title, string url)
    {
        return new KnowledgeDocument
        {
            TenantId = tenantId,
            Title = title.Trim(),
            Source = KnowledgeDocumentSource.Url,
            SourceLocation = url
        };
    }

    public static KnowledgeDocument CreateFromText(Guid tenantId, string title, string text)
    {
        return new KnowledgeDocument
        {
            TenantId = tenantId,
            Title = title.Trim(),
            Source = KnowledgeDocumentSource.RawText,
            SourceLocation = string.Empty,
            RawContent = text
        };
    }

    public void MarkProcessing()
    {
        Status = KnowledgeDocumentStatus.Processing;
        ErrorMessage = null;
        Touch();
    }

    public void MarkReady(int chunkCount)
    {
        Status = KnowledgeDocumentStatus.Ready;
        ChunkCount = chunkCount;
        ErrorMessage = null;
        Touch();
    }

    public void MarkFailed(string errorMessage)
    {
        Status = KnowledgeDocumentStatus.Failed;
        ErrorMessage = errorMessage;
        Touch();
    }
}