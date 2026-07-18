using Microsoft.AspNetCore.SignalR;
using SocialConnect.API.Hubs;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Notifications.DTOs;

namespace SocialConnect.API.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(
        Guid userId,
        NotificationDto notificationDto,
        CancellationToken cancellationToken = default
    )
    {
        await _hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", notificationDto, cancellationToken);
    }
}
