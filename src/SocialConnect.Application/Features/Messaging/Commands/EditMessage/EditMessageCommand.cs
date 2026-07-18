using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Messaging.Commands.EditMessage;

public record EditMessageCommand(Guid MessageId, string Content) : IRequest<Result<bool>>;
