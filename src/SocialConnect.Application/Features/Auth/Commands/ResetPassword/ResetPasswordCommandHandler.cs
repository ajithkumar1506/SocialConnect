using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IPasswordHasher passwordHasher
    )
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken
    )
    {
        var principal = _tokenService.ValidateTokenWithSecurityStamp(
            request.Token,
            "PasswordReset"
        );
        if (principal == null)
        {
            return Result.Failure("Invalid or expired password reset token.");
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
            return Result.Failure(
                "This password reset token has already been used or is no longer valid."
            );
        }

        var newHashedPassword = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatePassword(Password.Create(newHashedPassword));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
