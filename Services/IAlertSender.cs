namespace StockAlert.Services;

internal interface IAlertSender
{
    Task SendAsync(string subject, string body, CancellationToken cancellationToken);
}
