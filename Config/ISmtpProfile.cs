namespace StockAlert.Config;

internal interface ISmtpProfile
{
    string Name { get; }
    
    void ApplyDefaults(SmtpConfig config);
}
