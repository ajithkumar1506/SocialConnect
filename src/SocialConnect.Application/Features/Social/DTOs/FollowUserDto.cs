namespace SocialConnect.Application.Features.Social.DTOs;

public record FollowUserDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
    public string? Headline { get; init; }
}
