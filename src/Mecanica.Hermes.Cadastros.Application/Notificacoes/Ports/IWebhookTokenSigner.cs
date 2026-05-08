namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;

/// <summary>
///     Gera e valida tokens HMAC-SHA256 embutidos nos links Aprovar/Rejeitar enviados
///     por e-mail ao cliente. Usado tanto no <see cref="INotificacoesUrlBuilder" />
///     (para emitir as URLs assinadas) quanto no endpoint do WebHook (para validar
///     o clique recebido).
/// </summary>
public interface IWebhookTokenSigner
{
    /// <summary>
    ///     Retorna <c>(token, timestampUnixSeconds)</c> assinando o payload
    ///     <c>{ordemId}|{acao}|{ts}</c>. <paramref name="acao" /> é livre, mas no
    ///     fluxo de aprovação usamos <c>"aprovar"</c> e <c>"rejeitar"</c>.
    /// </summary>
    (string Token, long Timestamp) Sign(Guid ordemDeServicoId, string acao);

    /// <summary>
    ///     Valida o token recebido contra o payload reconstruído. Retorna <c>true</c>
    ///     quando a assinatura confere E o timestamp está dentro da janela de validade.
    /// </summary>
    bool Validate(Guid ordemDeServicoId, string acao, string token, long timestamp);
}
