namespace StockAlert.Config;

internal sealed class AppConfig
{
    public string? RecipientEmail { get; set; }
    public int PollIntervalSeconds { get; set; } = 60;
    public string? SymbolSuffix { get; set; } = ".SA";
    public string? BrapiToken { get; set; }   // opcional
    public SmtpConfig? Smtp { get; set; }

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(RecipientEmail))
        {
            throw new InvalidOperationException("RecipientEmail não foi configurado.");
        }

        if (Smtp is null)
        {
            throw new InvalidOperationException("As configurações de SMTP são obrigatórias.");
        }

        Smtp.Normalize();

        if (PollIntervalSeconds <= 0)
        {
            PollIntervalSeconds = 60;
        }

        SymbolSuffix = SymbolSuffix?.Trim() ?? string.Empty;
        // BrapiToken é opcional, então não precisa validar
    }
}
