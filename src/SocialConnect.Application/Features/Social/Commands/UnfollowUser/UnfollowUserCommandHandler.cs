using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Social.Commands.UnfollowUser;

public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, Result<Guid>>
{
    private readonly IFollowRepository _followRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UnfollowUserCommandHandler(
        IFollowRepository followRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _followRepository = followRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(
        UnfollowUserCommand request,
        CancellationToken cancellationToken
    )
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Guid>.Failure("User must be logged in to unfollow.");
        }

        var followerId = _currentUserService.UserId.Value;
        var followingId = request.FollowingId;

        var existingFollow = await _followRepository.GetFollowAsync(
            followerId,
            followingId,
            cancellationToken
        );
        if (existingFollow == null)
        {
            return Result<Guid>.Failure("You are not following this user.");
        }

        await _followRepository.DeleteAsync(existingFollow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(followingId);
    }
}
