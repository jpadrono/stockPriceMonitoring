namespace StockAlert.Config;

internal sealed class OutlookSmtpProfile : ISmtpProfile
{
    public string Name => "outlook";

    public void ApplyDefaults(SmtpConfig config)
    {
        config.Host ??= "smtp.office365.com";

        if (config.Port <= 0)
        {
            config.Port = 587;
        }

        config.UseSsl = true;
    }
}
