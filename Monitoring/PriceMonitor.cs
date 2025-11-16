using System.Globalization;
using StockAlert.Config;
using StockAlert.Services;

namespace StockAlert.Monitoring;

internal sealed class PriceMonitor
{
    private readonly AppConfig _config;
    private readonly IPriceProvider _priceProvider;
    private readonly IAlertSender _alertSender;
    private readonly AlertState _alertState = new();

    public PriceMonitor(AppConfig config, IPriceProvider priceProvider, IAlertSender alertSender)
    {
        _config = config;
        _priceProvider = priceProvider;
        _alertSender = alertSender;
    }

    public async Task RunAsync(
        string symbol,
        decimal sellTarget,
        decimal buyTarget,
        CancellationToken cancellationToken)
    {
        // Garante intervalo mínimo entre consultas para evitar sobrecarga da API remota
        var pollInterval = TimeSpan.FromSeconds(Math.Max(5, _config.PollIntervalSeconds));

        Console.WriteLine(
            $"Monitorando {symbol} - venda ≥ {FormatPrice(sellTarget)}, compra ≤ {FormatPrice(buyTarget)}. " +
            "Pressione Ctrl+C para encerrar."
        );

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var price = await _priceProvider.GetPriceAsync(symbol, cancellationToken);
                Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] {symbol} = {FormatPrice(price)}");

                if (_alertState.ShouldSendSell(price, sellTarget))
                {
                    var subject = $"Alerta de VENDA - {symbol} a {FormatPrice(price)}";
                    var body = ComposeBody(symbol, price, sellTarget, "venda");
                    await _alertSender.SendAsync(subject, body, cancellationToken);
                    Console.WriteLine("E-mail de alerta de venda enviado.");
                }

                if (_alertState.ShouldSendBuy(price, buyTarget))
                {
                    var subject = $"Alerta de COMPRA - {symbol} a {FormatPrice(price)}";
                    var body = ComposeBody(symbol, price, buyTarget, "compra");
                    await _alertSender.SendAsync(subject, body, cancellationToken);
                    Console.WriteLine("E-mail de alerta de compra enviado.");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Cancelamento solicitado (via Ctrl+C)
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] Falha ao monitorar: {ex.Message}");
            }

            try
            {
                await Task.Delay(pollInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        Console.WriteLine("Monitoramento encerrado.");
    }

    private static string FormatPrice(decimal value) =>
        value.ToString("F2", CultureInfo.InvariantCulture);

    private string ComposeBody(string symbol, decimal price, decimal referencePrice, string referenceType)
    {
        return $"O ativo {symbol} atingiu {FormatPrice(price)} às {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss} " +
               $"(referência de {referenceType}: {FormatPrice(referencePrice)}).";
    }
}
