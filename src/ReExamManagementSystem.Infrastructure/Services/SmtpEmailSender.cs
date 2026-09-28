using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReExamManagementSystem.Application.Interfaces;

namespace ReExamManagementSystem.Infrastructure.Services;

/// <summary>
/// Sends mail via SMTP using the "Smtp" configuration section. When no host
/// is configured (e.g. a fresh clone with no mail server available yet), it
/// logs the message instead of throwing, so password-reset/notification
/// flows stay fully usable during development and thesis demos.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning(
                "Smtp:Host is not configured - logging email instead of sending it. To: {ToEmail}, Subject: {Subject}\n{Body}",
                toEmail, subject, htmlMessage);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = _settings.EnableSsl
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}
