using MassTransit;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Messages.Events;

/// <summary>
///     Evento publicado pela API de Ordem de Serviço quando uma OS transita para o status
///     "AguardandoAprovacao". A API de Cadastros consome via
///     <c>OrdemDeServicoAguardandoAprovacaoConsumer</c> e enfileira o envio de e-mail.
///     <para>
///         O <see cref="EntityNameAttribute" /> fixa o nome da exchange/topic para
///         <c>ordem-de-servico.aguardando-aprovacao</c>, desacoplando o roteamento do
///         namespace .NET — a API de OS pode publicar usando seu próprio namespace e
///         desde que decore com o mesmo <c>EntityName</c>, o MassTransit roteia
///         corretamente para esta fila.
///     </para>
/// </summary>
[EntityName("ordem-de-servico.aguardando-aprovacao.v1")]
public sealed record OrdemDeServicoAguardandoAprovacaoEvent(Guid OrdemDeServicoId);
