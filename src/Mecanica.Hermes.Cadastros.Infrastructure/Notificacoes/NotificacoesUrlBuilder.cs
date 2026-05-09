using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Notificacoes;

/// <summary>
///     Constrói as URLs <c>/api/webhooks/orcamentos/{id}/(aprovar|rejeitar)</c>
///     anexando token HMAC + timestamp. As URLs são consumidas pelos botões dos
///     e-mails enviados ao cliente.
/// </summary>
internal sealed class NotificacoesUrlBuilder(IWebhookTokenSigner tokenSigner) : INotificacoesUrlBuilder
{
    private const string AcaoAprovar = "aprovar";
    private const string AcaoRejeitar = "rejeitar";

    private readonly string _baseUrl =
        (Environment.GetEnvironmentVariable(VariableNames.AppBaseUrl)
         ?? throw new InvalidOperationException(
             $"A variável de ambiente '{VariableNames.AppBaseUrl}' é obrigatória "
             + "para construir as URLs dos webhooks de orçamento."))
        .TrimEnd('/');

    public string BuildAprovacaoUrl(Guid ordemDeServicoId) => Build(ordemDeServicoId, AcaoAprovar);

    public string BuildRejeicaoUrl(Guid ordemDeServicoId) => Build(ordemDeServicoId, AcaoRejeitar);

    private string Build(Guid ordemDeServicoId, string acao)
    {
        var (token, ts) = tokenSigner.Sign(ordemDeServicoId, acao);
        return $"{_baseUrl}/api/webhooks/orcamentos/{ordemDeServicoId:D}/{acao}?token={token}&ts={ts}";
    }
}
