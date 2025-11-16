using System.Net;
using System.Net.Mail;
using StockAlert.Config;

namespace StockAlert.Services;

internal sealed class SmtpAlertSender : IAlertSender
{
    private readonly AppConfig _config;

    public SmtpAlertSender(AppConfig config)
    {
        _config = config;
    }

    public async Task SendAsync(string subject, string body, CancellationToken cancellationToken)
    {
        // Recupera as configurações normalizadas de SMTP
        var smtp = _config.Smtp!;
        var fromAddress = new MailAddress(smtp.SenderEmail!, smtp.SenderName);
        var toAddress = new MailAddress(_config.RecipientEmail!);

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        };

        // Cliente SMTP baseado nos parâmetros definidos em SmtpConfig
        using var client = new SmtpClient(smtp.Host!, smtp.Port)
        {
            EnableSsl = smtp.UseSsl,
            Credentials = new NetworkCredential(smtp.Username, smtp.Password)
        };

        await client.SendMailAsync(message);
    }
}
