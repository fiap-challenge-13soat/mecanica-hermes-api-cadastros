using MassTransit;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarOrcamentoParaAprovacao;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Messages.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Messaging.Consumers;

/// <summary>
///     Consumer da fila publicada pela API de Ordem de Serviço quando uma OS entra
///     em "AguardandoAprovacao". Reaproveita o mesmo
///     <see cref="EnviarOrcamentoParaAprovacaoCommand" /> usado pelo endpoint REST 4.1 —
///     zero duplicação de regra entre os dois entry-points.
/// </summary>
/// <remarks>
///     Falhas ao consultar a API de OS, ao buscar Cliente/Produto/Servico locais ou ao
///     publicar o <c>EnviarEmailCommand</c> são logadas e propagadas — o pipeline do
///     MassTransit aplica retry conforme configurado em
///     <c>MessagingServiceCollectionExtensions</c>. Resultados de regra de negócio
///     (status incorreto, cliente inexistente) são logados como warning e a mensagem é
///     descartada — não há retry em erros determinísticos.
/// </remarks>
public sealed class OrdemDeServicoAguardandoAprovacaoConsumer(
    ISender sender,
    ILogger<OrdemDeServicoAguardandoAprovacaoConsumer> logger)
    : IConsumer<OrdemDeServicoAguardandoAprovacaoEvent>
{
    public async Task Consume(ConsumeContext<OrdemDeServicoAguardandoAprovacaoEvent> context)
    {
        var ordemServicoId = context.Message.OrdemDeServicoId;

        var result = await sender.Send(
            new EnviarOrcamentoParaAprovacaoCommand(ordemServicoId),
            context.CancellationToken);

        if (result.IsFailure)
            logger.LogWarning(
                "Envio de orçamento da OS {OrdemDeServicoId} ignorado pelo consumer. " +
                "Status: {StatusCode}. Erros: {Errors}",
                ordemServicoId,
                result.StatusCode,
                string.Join("; ", result.Errors));
    }
}
