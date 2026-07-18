using SocialConnect.Domain.Common;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Events.Posts;
using SocialConnect.Domain.Exceptions;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Domain.Entities.Posts;

public class Post : AggregateRoot, IHasTimestamps, ISoftDeletable
{
    public Guid AuthorId { get; private set; }
    public PostContent? Content { get; private set; }
    public PostType PostType { get; private set; }
    public PostStatus Status { get; private set; }

    public DateTimeOffset? ScheduledAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }

    public int ReactionCount { get; private set; }
    public int CommentCount { get; private set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation
    public User? Author { get; private set; }

    private readonly List<PostMedia> _media = new();
    public IReadOnlyCollection<PostMedia> Media => _media.AsReadOnly();

    private readonly List<Comment> _comments = new();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    private readonly List<Reaction> _reactions = new();
    public IReadOnlyCollection<Reaction> Reactions => _reactions.AsReadOnly();

    private Post() { }

    public static Post Create(
        Guid authorId,
        PostContent? content,
        PostType type,
        PostStatus status,
        DateTimeOffset? scheduledAt = null
    )
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            Content = content,
            PostType = type,
            Status = status,
            ScheduledAt = scheduledAt,
            ReactionCount = 0,
            CommentCount = 0,
        };

        if (status == PostStatus.Published)
        {
            post.PublishedAt = DateTimeOffset.UtcNow;
            post.AddDomainEvent(new PostPublishedDomainEvent(post.Id));
        }
        else
        {
            post.AddDomainEvent(new PostCreatedDomainEvent(post.Id));
        }

        return post;
    }

    public void UpdateContent(PostContent? content)
    {
        Content = content;
    }

    public void Publish()
    {
        if (Status == PostStatus.Published)
            return;

        Status = PostStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new PostPublishedDomainEvent(Id));
    }

    public void Schedule(DateTimeOffset scheduledAt)
    {
        if (Status == PostStatus.Published)
            throw new BusinessRuleViolationException("Cannot schedule an already published post.");

        Status = PostStatus.Scheduled;
        ScheduledAt = scheduledAt;
    }

    public void AddMedia(
        MediaUrl mediaUrl,
        MediaType type,
        int orderIndex,
        long fileSize,
        string? thumbnailUrl = null
    )
    {
        _media.Add(PostMedia.Create(Id, mediaUrl, type, orderIndex, fileSize, thumbnailUrl));
    }

    public void IncrementCommentCount() => CommentCount++;

    public void DecrementCommentCount() => CommentCount--;

    public void IncrementReactionCount() => ReactionCount++;

    public void DecrementReactionCount() => ReactionCount--;
}
