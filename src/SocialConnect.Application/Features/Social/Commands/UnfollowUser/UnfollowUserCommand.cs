using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Social.Commands.UnfollowUser;

public record UnfollowUserCommand(Guid FollowingId) : IRequest<Result<Guid>>;
