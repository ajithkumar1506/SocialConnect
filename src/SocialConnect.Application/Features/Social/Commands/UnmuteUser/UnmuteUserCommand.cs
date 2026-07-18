using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Social.Commands.UnmuteUser;

public record UnmuteUserCommand(Guid MutedId) : IRequest<Result<bool>>;
