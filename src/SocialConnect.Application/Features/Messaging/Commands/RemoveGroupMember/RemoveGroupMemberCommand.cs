using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Messaging.Commands.RemoveGroupMember;

public record RemoveGroupMemberCommand(Guid ConversationId, Guid UserId) : IRequest<Result<bool>>;
