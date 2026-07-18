using SocialConnect.Domain.Common;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Domain.Entities.Posts;

public class PostMedia : Entity
{
    public Guid PostId { get; private set; }
    public MediaUrl MediaUrl { get; private set; }
    public MediaType MediaType { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public int OrderIndex { get; private set; }
    public long FileSize { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    public Post? Post { get; private set; }

    private PostMedia()
    {
        MediaUrl = MediaUrl.Create("https://placeholder.com");
    }

    internal static PostMedia Create(
        Guid postId,
        MediaUrl mediaUrl,
        MediaType mediaType,
        int orderIndex,
        long fileSize,
        string? thumbnailUrl
    )
    {
        return new PostMedia
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            MediaUrl = mediaUrl,
            MediaType = mediaType,
            OrderIndex = orderIndex,
            FileSize = fileSize,
            ThumbnailUrl = thumbnailUrl,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void UpdateThumbnailUrl(string thumbnailUrl)
    {
        ThumbnailUrl = thumbnailUrl;
    }
}
