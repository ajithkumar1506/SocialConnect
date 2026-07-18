using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Notifications.DTOs;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Notifications.Queries.GetUserNotifications;

public class GetUserNotificationsQueryHandler
    : IRequestHandler<GetUserNotificationsQuery, List<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService
    )
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<NotificationDto>> Handle(
        GetUserNotificationsQuery request,
        CancellationToken cancellationToken
    )
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedAccessException("User must be logged in to view notifications.");
        }

        var userId = _currentUserService.UserId.Value;

        var notifications = await _notificationRepository.GetUserNotificationsAsync(
            userId,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        return notifications
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                ActorId = n.ActorId,
                ActorName =
                    n.Actor != null
                        ? $"{n.Actor.Profile?.FirstName} {n.Actor.Profile?.LastName}".Trim()
                        : "Someone",
                ActorProfileImageUrl = n.Actor?.Profile?.ProfileImageUrl,
                Type = n.Type,
                TargetId = n.TargetId,
                TargetType = n.TargetType,
                Content = n.Content,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
            })
            .ToList();
    }
}
