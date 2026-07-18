using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Messaging.Commands.DeleteMessage;

public record DeleteMessageCommand(Guid MessageId) : IRequest<Result<bool>>;
