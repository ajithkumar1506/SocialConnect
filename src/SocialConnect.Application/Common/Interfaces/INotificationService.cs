using SocialConnect.Application.Features.Notifications.DTOs;

namespace SocialConnect.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, NotificationDto notificationDto, CancellationToken cancellationToken = default);
}
