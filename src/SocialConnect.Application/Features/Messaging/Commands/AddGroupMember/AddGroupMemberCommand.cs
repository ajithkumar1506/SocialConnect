using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Messaging.Commands.AddGroupMember;

public record AddGroupMemberCommand(Guid ConversationId, Guid UserId) : IRequest<Result<bool>>;
