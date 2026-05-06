using MassTransit;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Messages.Events;

/// <summary>
///     Evento publicado quando o cliente clica em "Aprovar" no e-mail do orçamento.
///     Consumido pela API de Ordem de Serviço para promover o status da OS de
///     <c>AguardandoAprovacao</c> para <c>EmExecucao</c>.
/// </summary>
[EntityName("orcamento.aprovado-pelo-cliente.v1")]
public sealed record OrcamentoAprovadoPeloClienteEvent(Guid OrdemDeServicoId, DateTime AprovadoEm);
