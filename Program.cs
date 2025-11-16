using System.Globalization;
using StockAlert.Config;
using StockAlert.Monitoring;
using StockAlert.Services;

const string ConfigFileName = "config.json";

if (args.Length != 3)
{
    ShowUsage();
    return;
}

var ticker = args[0].Trim().ToUpperInvariant();
if (string.IsNullOrWhiteSpace(ticker))
{
    Console.Error.WriteLine("O ativo a ser monitorado não pode ser vazio.");
    return;
}

if (!decimal.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var sellTarget))
{
    Console.Error.WriteLine("Preço de referência para venda inválido. Utilize ponto como separador decimal.");
    return;
}

if (!decimal.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var buyTarget))
{
    Console.Error.WriteLine("Preço de referência para compra inválido. Utilize ponto como separador decimal.");
    return;
}

if (sellTarget <= buyTarget)
{
    Console.Error.WriteLine("O preço de venda deve ser MAIOR do que o preço de compra.");
    return;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
};

try
{
    var configPath = LocateConfigFile(ConfigFileName);
    var config = await AppConfigLoader.LoadConfigAsync(configPath, cts.Token);

    IPriceProvider priceProvider = new BrapiPriceProvider(config);
    IAlertSender alertSender = new SmtpAlertSender(config);

    var monitor = new PriceMonitor(config, priceProvider, alertSender);
    await monitor.RunAsync(ticker, sellTarget, buyTarget, cts.Token);
}
catch (OperationCanceledException)
{
    // execução cancelada pelo usuário
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Erro fatal: {ex.Message}");
    Environment.ExitCode = 1;
}

static void ShowUsage()
{
    Console.WriteLine("Uso: dotnet run --project StockAlert -- <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>");
    Console.WriteLine("Exemplo: dotnet run --project StockAlert -- PETR4 22.67 22.59");
}

static string LocateConfigFile(string fileName)
{
    string? directory = AppContext.BaseDirectory;

    for (var i = 0; i < 6 && directory is not null; i++)
    {
        var candidate = Path.Combine(directory, fileName);
        if (File.Exists(candidate))
        {
            return candidate;
        }

        directory = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(directory));
    }

    var fallback = Path.Combine(Directory.GetCurrentDirectory(), fileName);
    if (File.Exists(fallback))
    {
        return fallback;
    }

    throw new FileNotFoundException($"Arquivo de configuração '{fileName}' não foi localizado.", fallback);
}
