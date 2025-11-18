using System.Net;
using System.Text.Json;
using StockAlert.Config;

namespace StockAlert.Services;

internal sealed class BrapiPriceProvider : IPriceProvider
{
    // HttpClient compartilhado para evitar exaustão de sockets e melhorar reutilização de conexão
    private static readonly HttpClient SharedHttpClient = CreateClient();
    private readonly AppConfig _config;

    public BrapiPriceProvider(AppConfig config)
    {
        _config = config;
    }

    public async Task<decimal> GetPriceAsync(string symbol, CancellationToken cancellationToken)
    {
        var remoteSymbol = BuildRemoteSymbol(symbol);

        var baseUrl = $"https://brapi.dev/api/quote/{Uri.EscapeDataString(remoteSymbol)}";

        string url;
        if (!string.IsNullOrWhiteSpace(_config.BrapiToken))
        {
            url = $"{baseUrl}?token={Uri.EscapeDataString(_config.BrapiToken)}";
        }
        else
        {
            url = baseUrl;
        }

        using var response = await SharedHttpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        // Espera o formato: { "results": [ { "regularMarketPrice": ... } ] }
        if (!document.RootElement.TryGetProperty("results", out var results) ||
            results.ValueKind != JsonValueKind.Array ||
            results.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("A API de cotação (brapi) não retornou resultados.");
        }

        var entry = results[0];

        if (!entry.TryGetProperty("regularMarketPrice", out var priceElement))
        {
            throw new InvalidOperationException("A API de cotação (brapi) não retornou o campo 'regularMarketPrice'.");
        }

        if (priceElement.TryGetDecimal(out var decimalPrice))
        {
            return decimalPrice;
        }

        if (priceElement.TryGetDouble(out var doublePrice))
        {
            return (decimal)doublePrice;
        }

        throw new InvalidOperationException("Formato de preço inesperado recebido da brapi.");
    }

    private string BuildRemoteSymbol(string symbol)
    {
        // Permite configurar um sufixo opcional de símbolo (por exemplo, '.SA')
        var suffix = _config.SymbolSuffix ?? string.Empty;
        if (suffix.Length == 0)
        {
            return symbol;
        }

        return symbol.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? symbol
            : symbol + suffix;
    }

    private static HttpClient CreateClient()
    {
        var handler = new SocketsHttpHandler
        {
            // Força HTTP/1.1 e evita alguns problemas de conexão mantida
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30),
            MaxConnectionsPerServer = 4
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(10),
            DefaultRequestVersion = HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd("StockAlertBot/1.0 (+https://brapi.dev)");
        return client;
    }

}
