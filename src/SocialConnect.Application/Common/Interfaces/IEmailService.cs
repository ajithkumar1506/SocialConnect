namespace SocialConnect.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default
    );
    Task SendVerificationEmailAsync(
        string to,
        string verificationLink,
        CancellationToken cancellationToken = default
    );
    Task SendPasswordResetEmailAsync(
        string to,
        string resetLink,
        CancellationToken cancellationToken = default
    );
}
