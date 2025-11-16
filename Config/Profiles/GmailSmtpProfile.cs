namespace StockAlert.Config;

internal sealed class GmailSmtpProfile : ISmtpProfile
{
    public string Name => "gmail";

    public void ApplyDefaults(SmtpConfig config)
    {
        // Só aplica se o cliente não tiver definido manualmente
        config.Host ??= "smtp.gmail.com";

        if (config.Port <= 0)
        {
            config.Port = 587;
        }

        config.UseSsl = true;
    }
}
