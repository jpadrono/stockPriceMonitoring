namespace StockAlert.Config;

internal sealed class MailboxSmtpProfile : ISmtpProfile
{
    public string Name => "mailbox";

    public void ApplyDefaults(SmtpConfig config)
    {
        config.Host ??= "smtp.mailbox.com";

        if (config.Port <= 0)
        {
            config.Port = 587;
        }

        config.UseSsl = true;
    }
}
