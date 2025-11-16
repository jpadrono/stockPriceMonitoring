using System.Collections.Generic;
using System.Linq;

namespace StockAlert.Config;

internal static class SmtpProfileRegistry
{
    // Dicionário imutável associado aos provedores SMTP disponíveis.
    // Permite resolver perfis com base em uma string configurada pelo cliente.
    private static readonly IReadOnlyDictionary<string, ISmtpProfile> Profiles =
        new ISmtpProfile[]
        {
            new GmailSmtpProfile(),
            new OutlookSmtpProfile(),
            new MailboxSmtpProfile(),
            new CustomSmtpProfile()
        }.ToDictionary(
            p => p.Name,
            StringComparer.OrdinalIgnoreCase
        );

    // Resolve o perfil de SMTP com base no nome do provedor definido no config.json.
    // Retorna "custom" como fallback para qualquer valor desconhecido.
    public static ISmtpProfile Resolve(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Profiles["custom"];
        }

        return Profiles.TryGetValue(provider, out var profile)
            ? profile
            : Profiles["custom"]; // fallback se o cliente colocar algo desconhecido
    }
}
