using MassTransit;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Messages.Events;

/// <summary>
///     Evento publicado quando o cliente clica em "Rejeitar" no e-mail do orçamento.
///     Consumido pela API de Ordem de Serviço para promover o status da OS para
///     <c>Rejeitada</c>.
/// </summary>
[EntityName("orcamento.rejeitado-pelo-cliente")]
public sealed record OrcamentoRejeitadoPeloClienteEvent(Guid OrdemDeServicoId, DateTime RejeitadoEm, string? Motivo);
