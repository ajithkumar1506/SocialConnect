using SocialConnect.Application.Common.Interfaces;

namespace SocialConnect.Application.BackgroundJobs;

public class EmailSendingJob
{
    private readonly IEmailService _emailService;

    public EmailSendingJob(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendPasswordResetEmailAsync(string email, string token)
    {
        await _emailService.SendPasswordResetEmailAsync(email, token);
    }

    public async Task SendVerificationEmailAsync(string email, string token)
    {
        await _emailService.SendVerificationEmailAsync(email, token);
    }

    public async Task SendGenericEmailAsync(string email, string subject, string body)
    {
        await _emailService.SendEmailAsync(email, subject, body);
    }
}
