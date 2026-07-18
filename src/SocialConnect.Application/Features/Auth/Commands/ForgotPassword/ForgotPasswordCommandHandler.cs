using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.BackgroundJobs;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ClientSettings _clientSettings;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IBackgroundJobService backgroundJobService,
        ClientSettings clientSettings
    )
    {
        _context = context;
        _tokenService = tokenService;
        _backgroundJobService = backgroundJobService;
        _clientSettings = clientSettings;
    }

    public async Task<Result> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken
    )
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.NormalizedEmail == normalizedEmail,
            cancellationToken
        );

        if (user != null)
        {
            var token = _tokenService.GeneratePasswordResetToken(
                user.Id,
                user.Email.Value,
                user.SecurityStamp
            );
            var resetLink =
                $"{_clientSettings.ResetPasswordUrl}?token={Uri.EscapeDataString(token)}";

            // Send password reset email asynchronously in background
            _backgroundJobService.Enqueue<EmailSendingJob>(job =>
                job.SendPasswordResetEmailAsync(user.Email.Value, resetLink)
            );
        }

        // Always return success to prevent user enumeration attacks
        return Result.Success();
    }
}
