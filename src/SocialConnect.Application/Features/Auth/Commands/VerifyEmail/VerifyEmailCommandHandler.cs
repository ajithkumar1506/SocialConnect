using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public VerifyEmailCommandHandler(IApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<Result> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken
    )
    {
        var principal = _tokenService.ValidateTokenWithSecurityStamp(
            request.Token,
            "EmailVerification"
        );
        if (principal == null)
        {
            return Result.Failure("Invalid or expired verification token.");
        }

        var userIdClaim =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Result.Failure("Invalid token claims.");
        }

        var expectedSecurityStamp = principal.FindFirst("SecurityStamp")?.Value;
        if (string.IsNullOrEmpty(expectedSecurityStamp))
        {
            return Result.Failure("Invalid token format.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        if (user.SecurityStamp != expectedSecurityStamp)
        {
            return Result.Failure("Verification token has expired or is invalid.");
        }

        if (user.EmailVerified)
        {
            return Result.Success();
        }

        user.VerifyEmail();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
