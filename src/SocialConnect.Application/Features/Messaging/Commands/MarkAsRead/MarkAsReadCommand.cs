using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Messaging.Commands.MarkAsRead;

public record MarkAsReadCommand(Guid ConversationId) : IRequest<Result<bool>>;
