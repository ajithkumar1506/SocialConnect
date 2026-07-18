using SocialConnect.Domain.Common;

namespace SocialConnect.Domain.Entities.Users;

public class UserProfile : Entity, IHasTimestamps
{
    public Guid UserId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Headline { get; private set; }
    public string? Bio { get; private set; }
    public string? Location { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation properties
    public User? User { get; private set; }

    private UserProfile() { } // EF Core

    public static UserProfile Create(Guid userId, string firstName, string lastName)
    {
        return new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
        };
    }

    public void UpdateDetails(
        string firstName,
        string lastName,
        string? headline,
        string? bio,
        string? location,
        DateOnly? dateOfBirth
    )
    {
        FirstName = firstName;
        LastName = lastName;
        Headline = headline;
        Bio = bio;
        Location = location;
        DateOfBirth = dateOfBirth;
    }

    public void UpdateProfileImage(string imageUrl)
    {
        ProfileImageUrl = imageUrl;
    }

    public void UpdateCoverImage(string imageUrl)
    {
        CoverImageUrl = imageUrl;
    }
}
