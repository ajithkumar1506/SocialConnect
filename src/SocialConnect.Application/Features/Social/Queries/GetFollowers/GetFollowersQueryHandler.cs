using MediatR;
using SocialConnect.Application.Features.Social.DTOs;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Social.Queries.GetFollowers;

public class GetFollowersQueryHandler : IRequestHandler<GetFollowersQuery, List<FollowUserDto>>
{
    private readonly IFollowRepository _followRepository;

    public GetFollowersQueryHandler(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    public async Task<List<FollowUserDto>> Handle(
        GetFollowersQuery request,
        CancellationToken cancellationToken
    )
    {
        var follows = await _followRepository.GetFollowersAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        return follows
            .Select(f => new FollowUserDto
            {
                UserId = f.FollowerId,
                UserName = f.Follower?.UserName ?? string.Empty,
                FirstName = f.Follower?.Profile?.FirstName ?? string.Empty,
                LastName = f.Follower?.Profile?.LastName ?? string.Empty,
                ProfileImageUrl = f.Follower?.Profile?.ProfileImageUrl,
                Headline = f.Follower?.Profile?.Headline,
            })
            .ToList();
    }
}
