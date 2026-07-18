using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SocialConnect.Application.Common.Interfaces;

namespace SocialConnect.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default
    )
    {
        var message = new MimeMessage();
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@socialconnect.local";
        var fromName = _configuration["EmailSettings:FromName"] ?? "SocialConnect";

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var host = _configuration["EmailSettings:SmtpHost"] ?? "localhost";
            var port = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "1025"); // Mailhog default SMTP port

            // For local development with MailHog, we don't need SSL
            await client.ConnectAsync(host, port, SecureSocketOptions.None, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {ToEmail}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending email to {ToEmail}", to);
            throw;
        }
    }

    public async Task SendVerificationEmailAsync(
        string to,
        string token,
        CancellationToken cancellationToken = default
    )
    {
        var subject = "Verify your SocialConnect email";
        var body =
            $"<p>Please verify your email using this token:</p><p><strong>{token}</strong></p>";
        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(
        string to,
        string token,
        CancellationToken cancellationToken = default
    )
    {
        var subject = "Reset your SocialConnect password";
        var body =
            $"<p>Please reset your password using this token:</p><p><strong>{token}</strong></p>";
        await SendEmailAsync(to, subject, body, cancellationToken);
    }
}
