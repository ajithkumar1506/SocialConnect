using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Social.Commands.UnblockUser;

public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UnblockUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var blockerId = _currentUserService.UserId;
        if (blockerId == null || blockerId == Guid.Empty)
        {
            return Result<bool>.Failure("Unauthorized access.");
        }

        var block = await _context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId.Value && b.BlockedId == request.BlockedId, cancellationToken);

        if (block == null)
        {
            return Result<bool>.Failure("Block relationship not found.");
        }

        _context.Blocks.Remove(block);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
