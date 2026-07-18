using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher
    )
    {
        _context = context;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
        {
            return Result.Failure("Unauthorized access.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.Id == userId.Value,
            cancellationToken
        );

        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        var isCurrentPasswordValid = _passwordHasher.VerifyPassword(
            request.CurrentPassword,
            user.PasswordHash.Hash
        );
        if (!isCurrentPasswordValid)
        {
            return Result.Failure("The current password provided is incorrect.");
        }

        var newHashedPassword = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatePassword(Password.Create(newHashedPassword));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
