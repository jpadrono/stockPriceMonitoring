using System.Text.Json;

namespace StockAlert.Config;

internal static class AppConfigLoader
{
    public static async Task<AppConfig> LoadConfigAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var config = await JsonSerializer.DeserializeAsync<AppConfig>(stream, options, cancellationToken);
        if (config is null)
        {
            throw new InvalidOperationException("Arquivo de configuração inválido.");
        }

        config.Normalize();
        return config;
    }
}
