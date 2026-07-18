using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Social.Commands.UnblockUser;

public record UnblockUserCommand(Guid BlockedId) : IRequest<Result<bool>>;
