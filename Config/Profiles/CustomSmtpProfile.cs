namespace StockAlert.Config;

internal sealed class CustomSmtpProfile : ISmtpProfile
{
    public string Name => "custom";

    public void ApplyDefaults(SmtpConfig config)
    {
        // Não aplica host nem port, cliente é obrigado a configurar
        // Poderia só garantir que UseSsl tem um default
        if (config.Port <= 0)
        {
            config.Port = 587;
        }
    }
}
