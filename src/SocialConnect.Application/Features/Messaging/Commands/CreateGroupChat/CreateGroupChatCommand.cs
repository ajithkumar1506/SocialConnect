using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Messaging.Commands.CreateGroupChat;

public record CreateGroupChatCommand(
    string Name,
    List<Guid> MemberIds,
    string? ImageUrl = null
) : IRequest<Result<Guid>>;
