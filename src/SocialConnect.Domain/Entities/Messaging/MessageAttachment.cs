using SocialConnect.Domain.Common;

namespace SocialConnect.Domain.Entities.Messaging;

public class MessageAttachment : Entity
{
    public Guid MessageId { get; private set; }
    public string FileUrl { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public string? ThumbnailUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    public Message? Message { get; private set; }

    private MessageAttachment() { }

    internal static MessageAttachment Create(
        Guid messageId,
        string fileUrl,
        string fileName,
        long fileSize,
        string contentType,
        string? thumbnailUrl
    )
    {
        return new MessageAttachment
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            FileUrl = fileUrl,
            FileName = fileName,
            FileSize = fileSize,
            ContentType = contentType,
            ThumbnailUrl = thumbnailUrl,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void UpdateThumbnailUrl(string thumbnailUrl)
    {
        ThumbnailUrl = thumbnailUrl;
    }
}
