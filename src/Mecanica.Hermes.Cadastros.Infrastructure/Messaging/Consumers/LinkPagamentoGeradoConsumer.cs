using MassTransit;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarLinkPagamento;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Messages.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Messaging.Consumers;

public sealed class LinkPagamentoGeradoConsumer(
    ISender sender,
    ILogger<LinkPagamentoGeradoConsumer> logger)
    : IConsumer<LinkPagamentoGeradoEvent>
{
    public async Task Consume(ConsumeContext<LinkPagamentoGeradoEvent> context)
    {
        var pagamentoId = context.Message.PagamentoId;

        var result = await sender.Send(
            new EnviarLinkPagamentoCommand(pagamentoId),
            context.CancellationToken);

        if (result.IsFailure)
            logger.LogWarning(
                "Envio de link do pagamento {PagamentoId} ignorado pelo consumer. " +
                "Status: {StatusCode}. Erros: {Errors}",
                pagamentoId,
                result.StatusCode,
                string.Join("; ", result.Errors));
    }
}
