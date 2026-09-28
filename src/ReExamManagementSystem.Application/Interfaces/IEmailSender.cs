namespace ReExamManagementSystem.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default);
}
