using AiModoo.Core.Interfaces.Common;

namespace AiModoo.Infrastructure.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
}

public class EmailService : IEmailService
{
    // TODO: Implement with actual email provider (SMTP, SendGrid, etc.)
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        // Placeholder - log instead of sending
        await Task.CompletedTask;
    }
}
