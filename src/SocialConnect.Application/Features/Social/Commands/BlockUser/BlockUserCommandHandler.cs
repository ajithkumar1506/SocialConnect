using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Entities.Social;

namespace SocialConnect.Application.Features.Social.Commands.BlockUser;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public BlockUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var blockerId = _currentUserService.UserId;
        if (blockerId == null || blockerId == Guid.Empty)
        {
            return Result<bool>.Failure("Unauthorized access.");
        }

        var blockedId = request.BlockedId;
        if (blockerId.Value == blockedId)
        {
            return Result<bool>.Failure("You cannot block yourself.");
        }

        // Check if user exists
        var targetUserExists = await _context.Users.AnyAsync(u => u.Id == blockedId, cancellationToken);
        if (!targetUserExists)
        {
            return Result<bool>.Failure("User to block not found.");
        }

        // Check if block already exists
        var blockExists = await _context.Blocks
            .AnyAsync(b => b.BlockerId == blockerId.Value && b.BlockedId == blockedId, cancellationToken);

        if (blockExists)
        {
            return Result<bool>.Success(true); // Idempotent
        }

        // Remove follow relationships in both directions
        var follows = await _context.Follows
            .Where(f => (f.FollowerId == blockerId.Value && f.FollowingId == blockedId)
                     || (f.FollowerId == blockedId && f.FollowingId == blockerId.Value))
            .ToListAsync(cancellationToken);

        if (follows.Any())
        {
            _context.Follows.RemoveRange(follows);
        }

        var block = Block.Create(blockerId.Value, blockedId);
        await _context.Blocks.AddAsync(block, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
