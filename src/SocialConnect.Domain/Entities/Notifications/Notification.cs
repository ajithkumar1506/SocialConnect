using SocialConnect.Domain.Common;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Events.Notifications;

namespace SocialConnect.Domain.Entities.Notifications;

public class Notification : Entity
{
    public Guid UserId { get; private set; }
    public Guid ActorId { get; private set; }
    public NotificationType Type { get; private set; }
    public Guid? TargetId { get; private set; }
    public string? TargetType { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    public User? User { get; private set; }
    public User? Actor { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        Guid actorId,
        NotificationType type,
        string content,
        Guid? targetId = null,
        string? targetType = null
    )
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActorId = actorId,
            Type = type,
            Content = content,
            TargetId = targetId,
            TargetType = targetType,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        notification.AddDomainEvent(new NotificationCreatedDomainEvent(notification.Id));

        return notification;
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTimeOffset.UtcNow;
        }
    }
}
