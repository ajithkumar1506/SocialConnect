using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public LogoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Success();
        }

        var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync(
            t => t.TokenHash == request.RefreshToken,
            cancellationToken
        );

        if (savedToken != null && savedToken.IsActive)
        {
            savedToken.Revoke();
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
