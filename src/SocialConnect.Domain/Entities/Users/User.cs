using SocialConnect.Domain.Common;
using SocialConnect.Domain.Events.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Domain.Entities.Users;

public class User : AggregateRoot, IHasTimestamps
{
    public Email Email { get; private set; }
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public Password PasswordHash { get; private set; }

    public bool EmailVerified { get; private set; }
    public bool IsActive { get; private set; }

    public int AccessFailedCount { get; private set; }
    public DateTimeOffset? LockoutEnd { get; private set; }
    public string SecurityStamp { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation properties
    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public void AddRefreshToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
    }

    public void RemoveRefreshToken(RefreshToken token)
    {
        _refreshTokens.Remove(token);
    }

    public UserProfile? Profile { get; private set; }

    public void SetProfile(UserProfile profile)
    {
        Profile = profile;
    }

    private User()
    {
        Email = Email.Create("placeholder@example.com");
        PasswordHash = Password.Create("placeholder_hash");
    }

    public static User Create(Email email, string userName, Password passwordHash)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.Value.ToUpperInvariant(),
            UserName = userName,
            PasswordHash = passwordHash,
            EmailVerified = false,
            IsActive = true,
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        user.AddDomainEvent(new UserRegisteredDomainEvent(user.Id));

        return user;
    }

    public void VerifyEmail()
    {
        EmailVerified = true;
        AddDomainEvent(new UserVerifiedDomainEvent(Id));
    }

    public void UpdatePassword(Password newPassword)
    {
        PasswordHash = newPassword;
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public void RecordAccessFailed()
    {
        AccessFailedCount++;
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    public void Lockout(DateTimeOffset until)
    {
        LockoutEnd = until;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
