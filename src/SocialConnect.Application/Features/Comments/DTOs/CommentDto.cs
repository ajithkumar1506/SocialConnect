namespace SocialConnect.Application.Features.Comments.DTOs;

public record CommentDto
{
    public Guid Id { get; init; }
    public Guid PostId { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? AuthorProfileImageUrl { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid? ParentCommentId { get; init; }
    public int Depth { get; init; }
    public int ReactionCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
