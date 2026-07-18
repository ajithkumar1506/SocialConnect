using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Social.Commands.FollowUser;

public record FollowUserCommand(Guid FollowingId) : IRequest<Result<Guid>>;
