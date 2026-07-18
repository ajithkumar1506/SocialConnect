using SocialConnect.Application.Common.Mappings;
using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Application.Features.Users.DTOs;

public class UserProfileDto : IMapFrom<UserProfile>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
