using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RevokeTokenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        RevokeTokenCommand request,
        CancellationToken cancellationToken
    )
    {
        var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync(
            t => t.TokenHash == request.Token,
            cancellationToken
        );

        if (savedToken == null)
        {
            return Result.Failure("Token not found.");
        }

        if (!savedToken.IsActive)
        {
            return Result.Failure("Token is already inactive.");
        }

        savedToken.Revoke();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
