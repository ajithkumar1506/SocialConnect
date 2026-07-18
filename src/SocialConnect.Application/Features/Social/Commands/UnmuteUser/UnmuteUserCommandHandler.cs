using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Social.Commands.UnmuteUser;

public class UnmuteUserCommandHandler : IRequestHandler<UnmuteUserCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UnmuteUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(UnmuteUserCommand request, CancellationToken cancellationToken)
    {
        var muterId = _currentUserService.UserId;
        if (muterId == null || muterId == Guid.Empty)
        {
            return Result<bool>.Failure("Unauthorized access.");
        }

        var mute = await _context.Mutes
            .FirstOrDefaultAsync(m => m.MuterId == muterId.Value && m.MutedId == request.MutedId, cancellationToken);

        if (mute == null)
        {
            return Result<bool>.Failure("Mute relationship not found.");
        }

        _context.Mutes.Remove(mute);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
