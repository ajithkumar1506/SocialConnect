using SocialConnect.Domain.Common;

namespace SocialConnect.Domain.Events.Notifications;

public class NotificationCreatedDomainEvent : IDomainEvent
{
    public Guid NotificationId { get; }

    public NotificationCreatedDomainEvent(Guid notificationId)
    {
        NotificationId = notificationId;
    }
}
