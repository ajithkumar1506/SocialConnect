using MediatR;
using SocialConnect.Application.Features.Social.DTOs;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Social.Queries.GetFollowing;

public class GetFollowingQueryHandler : IRequestHandler<GetFollowingQuery, List<FollowUserDto>>
{
    private readonly IFollowRepository _followRepository;

    public GetFollowingQueryHandler(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    public async Task<List<FollowUserDto>> Handle(
        GetFollowingQuery request,
        CancellationToken cancellationToken
    )
    {
        var follows = await _followRepository.GetFollowingAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        return follows
            .Select(f => new FollowUserDto
            {
                UserId = f.FollowingId,
                UserName = f.Following?.UserName ?? string.Empty,
                FirstName = f.Following?.Profile?.FirstName ?? string.Empty,
                LastName = f.Following?.Profile?.LastName ?? string.Empty,
                ProfileImageUrl = f.Following?.Profile?.ProfileImageUrl,
                Headline = f.Following?.Profile?.Headline,
            })
            .ToList();
    }
}
