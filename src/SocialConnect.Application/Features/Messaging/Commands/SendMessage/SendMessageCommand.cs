using MediatR;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Messaging.Commands.SendMessage;

public record SendMessageCommand(Guid ConversationId, string Content, MessageType Type)
    : IRequest<Result<Guid>>;
