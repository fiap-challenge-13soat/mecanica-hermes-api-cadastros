using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Notificacoes;

/// <summary>
///     Signer HMAC-SHA256 para tokens de webhook. Lê o segredo de
///     <c>WEBHOOK__SIGNING_SECRET</c> (obrigatório em produção). Janela de validade
///     padrão: 30 dias — janela folgada para tolerar clientes que demoram a abrir o
///     e-mail; pode ser estreitada via env futura sem mudança de assinatura.
/// </summary>
internal sealed class HmacWebhookTokenSigner : IWebhookTokenSigner
{
    private static readonly TimeSpan Validity = TimeSpan.FromDays(30);
    private readonly byte[] _secret;

    public HmacWebhookTokenSigner()
    {
        var secret = Environment.GetEnvironmentVariable(VariableNames.WebhookSigningSecret);
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException(
                $"{VariableNames.WebhookSigningSecret} é obrigatória para emitir e validar tokens de webhook.");
        _secret = Encoding.UTF8.GetBytes(secret);
    }

    public (string Token, long Timestamp) Sign(Guid ordemDeServicoId, string acao)
    {
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var token = ComputeHmac(ordemDeServicoId, acao, ts);
        return (token, ts);
    }

    public bool Validate(Guid ordemDeServicoId, string acao, string token, long timestamp)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var minimum = now - (long)Validity.TotalSeconds;
        if (timestamp < minimum || timestamp > now + 60)
            return false;

        var expected = ComputeHmac(ordemDeServicoId, acao, timestamp);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expected),
            Encoding.ASCII.GetBytes(token));
    }

    private string ComputeHmac(Guid ordemDeServicoId, string acao, long ts)
    {
        var payload = string.Create(CultureInfo.InvariantCulture,
            $"{ordemDeServicoId:D}|{acao}|{ts}");
        var hash = HMACSHA256.HashData(_secret, Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }
}
