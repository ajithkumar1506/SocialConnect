using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Messaging.Commands.CreateConversation;

public record CreateConversationCommand(Guid RecipientId) : IRequest<Result<Guid>>;
