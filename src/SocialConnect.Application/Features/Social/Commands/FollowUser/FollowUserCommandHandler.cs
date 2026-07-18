using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Entities.Social;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Social.Commands.FollowUser;

public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, Result<Guid>>
{
    private readonly IFollowRepository _followRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public FollowUserCommandHandler(
        IFollowRepository followRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _followRepository = followRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(
        FollowUserCommand request,
        CancellationToken cancellationToken
    )
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Guid>.Failure("User must be logged in to follow.");
        }

        var followerId = _currentUserService.UserId.Value;
        var followingId = request.FollowingId;

        if (followerId == followingId)
        {
            return Result<Guid>.Failure("You cannot follow yourself.");
        }

        // Verify following user exists
        var followingUser = await _userRepository.GetByIdAsync(followingId, cancellationToken);
        if (followingUser == null)
        {
            return Result<Guid>.Failure("Target user not found.");
        }

        // Check if already following
        var existingFollow = await _followRepository.GetFollowAsync(
            followerId,
            followingId,
            cancellationToken
        );
        if (existingFollow != null)
        {
            return Result<Guid>.Failure("You are already following this user.");
        }

        var follow = Follow.Create(followerId, followingId);
        await _followRepository.AddAsync(follow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(followingId);
    }
}
