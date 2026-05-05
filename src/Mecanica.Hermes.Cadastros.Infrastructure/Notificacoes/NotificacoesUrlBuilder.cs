using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Notificacoes;

/// <summary>
///     Default builder das URLs de aprovação/rejeição. Por enquanto aponta para a rota
///     pública <c>/api/webhooks/orcamentos/{id}/(aprovar|rejeitar)</c> desta API — o
///     endpoint real será implementado na Fase 4 (WebHook com HMAC + idempotência).
/// </summary>
internal sealed class NotificacoesUrlBuilder : INotificacoesUrlBuilder
{
    private readonly string _baseUrl;

    public NotificacoesUrlBuilder()
    {
        _baseUrl = (Environment.GetEnvironmentVariable(VariableNames.AppBaseUrl) ?? "http://localhost:5000")
            .TrimEnd('/');
    }

    public string BuildAprovacaoUrl(Guid ordemDeServicoId) =>
        $"{_baseUrl}/api/webhooks/orcamentos/{ordemDeServicoId:D}/aprovar";

    public string BuildRejeicaoUrl(Guid ordemDeServicoId) =>
        $"{_baseUrl}/api/webhooks/orcamentos/{ordemDeServicoId:D}/rejeitar";
}
