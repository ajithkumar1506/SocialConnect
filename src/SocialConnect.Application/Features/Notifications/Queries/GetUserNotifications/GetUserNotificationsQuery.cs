using MediatR;
using SocialConnect.Application.Features.Notifications.DTOs;

namespace SocialConnect.Application.Features.Notifications.Queries.GetUserNotifications;

public record GetUserNotificationsQuery(int Page = 1, int PageSize = 10)
    : IRequest<List<NotificationDto>>;
