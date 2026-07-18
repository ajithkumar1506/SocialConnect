using SocialConnect.Domain.Common;

namespace SocialConnect.Domain.Entities.Users;

public class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public string JwtId { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? DeviceInfo { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Navigation
    public User? User { get; private set; }

    private RefreshToken() { } // EF Core

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        string jwtId,
        DateTimeOffset expiresAt,
        string? ipAddress,
        string? deviceInfo
    )
    {
        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            JwtId = jwtId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = ipAddress,
            DeviceInfo = deviceInfo,
        };
    }

    public void Revoke(string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
