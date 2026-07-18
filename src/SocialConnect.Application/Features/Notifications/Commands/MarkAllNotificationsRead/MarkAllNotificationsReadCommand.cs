using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

public record MarkAllNotificationsReadCommand : IRequest<Result<bool>>;
