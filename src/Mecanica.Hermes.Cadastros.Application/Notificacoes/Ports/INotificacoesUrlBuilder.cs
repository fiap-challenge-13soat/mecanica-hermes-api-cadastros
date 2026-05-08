namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;

/// <summary>
///     Constrói as URLs de aprovação/rejeição expostas nos e-mails enviados ao cliente.
///     Implementação fica em Infrastructure (lê <c>APP__BASE_URL</c>) e é trivialmente
///     trocável quando o WebHook da Fase 4 for introduzido.
/// </summary>
public interface INotificacoesUrlBuilder
{
    string BuildAprovacaoUrl(Guid ordemDeServicoId);
    string BuildRejeicaoUrl(Guid ordemDeServicoId);
}
