using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Domain.Entities.Audit;

public class AuditLog
{
    public long Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    public User? User { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid? userId,
        string action,
        string entityType,
        string entityId,
        string? oldValues,
        string? newValues,
        string? ipAddress,
        string? userAgent
    )
    {
        return new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}
