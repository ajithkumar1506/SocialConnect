using MediatR;
using SocialConnect.Application.Features.Social.DTOs;

namespace SocialConnect.Application.Features.Social.Queries.GetFollowers;

public record GetFollowersQuery(Guid UserId, int Page = 1, int PageSize = 10)
    : IRequest<List<FollowUserDto>>;
