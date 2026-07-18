using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Entities.Social;

namespace SocialConnect.Application.Features.Social.Commands.MuteUser;

public class MuteUserCommandHandler : IRequestHandler<MuteUserCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MuteUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(MuteUserCommand request, CancellationToken cancellationToken)
    {
        var muterId = _currentUserService.UserId;
        if (muterId == null || muterId == Guid.Empty)
        {
            return Result<bool>.Failure("Unauthorized access.");
        }

        var mutedId = request.MutedId;
        if (muterId.Value == mutedId)
        {
            return Result<bool>.Failure("You cannot mute yourself.");
        }

        // Check if user exists
        var targetUserExists = await _context.Users.AnyAsync(u => u.Id == mutedId, cancellationToken);
        if (!targetUserExists)
        {
            return Result<bool>.Failure("User to mute not found.");
        }

        // Check if mute already exists
        var muteExists = await _context.Mutes
            .AnyAsync(m => m.MuterId == muterId.Value && m.MutedId == mutedId, cancellationToken);

        if (muteExists)
        {
            return Result<bool>.Success(true); // Idempotent
        }

        var mute = Mute.Create(muterId.Value, mutedId);
        await _context.Mutes.AddAsync(mute, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
