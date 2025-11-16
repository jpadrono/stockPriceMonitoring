namespace StockAlert.Config;

internal sealed class SmtpConfig
{
    public string Provider { get; set; } = "custom";
    public bool UseProviderDefaults { get; set; } = true;

    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? SenderEmail { get; set; }
    public string? SenderName { get; set; }

    public void Normalize()
    {
        // Aplica valores padrão com base no perfil de SMTP selecionado
        if (UseProviderDefaults)
        {
            var profile = SmtpProfileRegistry.Resolve(Provider);
            profile.ApplyDefaults(this);
        }

        // Valida campos essenciais após aplicação dos defaults
        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new InvalidOperationException("SMTP.Host não foi informado.");
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            throw new InvalidOperationException("SMTP.Username não foi informado.");
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            throw new InvalidOperationException("SMTP.Password não foi informado.");
        }

        if (Port <= 0)
        {
            Port = 587;
        }

        // Se o remetente não for especificado, assume o username como e-mail de envio
        if (string.IsNullOrWhiteSpace(SenderEmail))
        {
            SenderEmail = Username;
        }

        SenderName = string.IsNullOrWhiteSpace(SenderName) ? "Stock Alert" : SenderName;
    }
}
