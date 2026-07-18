using MediatR;
using SocialConnect.Application.Features.Social.DTOs;

namespace SocialConnect.Application.Features.Social.Queries.GetFollowing;

public record GetFollowingQuery(Guid UserId, int Page = 1, int PageSize = 10)
    : IRequest<List<FollowUserDto>>;
