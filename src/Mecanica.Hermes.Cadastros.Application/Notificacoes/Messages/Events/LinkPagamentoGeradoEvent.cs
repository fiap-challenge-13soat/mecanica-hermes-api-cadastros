using MassTransit;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Messages.Events;

/// <summary>
///     Evento publicado pela API de Pagamentos quando o link do Mercado Pago é gerado e
///     está pronto para envio ao cliente. A API de Cadastros consome via
///     <c>LinkPagamentoGeradoConsumer</c> e enfileira o envio do e-mail.
///     <para>
///         A API de Pagamentos hoje publica <c>EnviarEmailPagamentoCommand</c> direcionado
///         a uma lambda externa. Quando essa publicação for substituída por um integration
///         event próprio, basta decorar com o mesmo <see cref="EntityNameAttribute" />
///         abaixo para que o roteamento bata neste consumer sem outras alterações.
///     </para>
/// </summary>
[EntityName("pagamento.link-gerado")]
public sealed record LinkPagamentoGeradoEvent(Guid PagamentoId);
