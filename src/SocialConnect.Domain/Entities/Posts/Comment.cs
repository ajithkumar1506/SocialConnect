using SocialConnect.Domain.Common;
using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Domain.Entities.Posts;

public class Comment : Entity, IHasTimestamps, ISoftDeletable
{
    public Guid PostId { get; private set; }
    public Guid AuthorId { get; private set; }
    public Guid? ParentCommentId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public int Depth { get; private set; }
    public int ReactionCount { get; private set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation
    public Post? Post { get; private set; }
    public User? Author { get; private set; }
    public Comment? ParentComment { get; private set; }

    private readonly List<Comment> _replies = new();
    public IReadOnlyCollection<Comment> Replies => _replies.AsReadOnly();

    private readonly List<Reaction> _reactions = new();
    public IReadOnlyCollection<Reaction> Reactions => _reactions.AsReadOnly();

    private Comment() { }

    public static Comment Create(
        Guid postId,
        Guid authorId,
        string content,
        Guid? parentId,
        string parentPath,
        int parentDepth
    )
    {
        var id = Guid.NewGuid();
        return new Comment
        {
            Id = id,
            PostId = postId,
            AuthorId = authorId,
            Content = content,
            ParentCommentId = parentId,
            Path = string.IsNullOrEmpty(parentPath) ? $"/{id}/" : $"{parentPath}{id}/",
            Depth = parentDepth + 1,
            ReactionCount = 0,
        };
    }

    public void UpdateContent(string content)
    {
        Content = content;
    }

    public void IncrementReactionCount() => ReactionCount++;

    public void DecrementReactionCount() => ReactionCount--;
}
