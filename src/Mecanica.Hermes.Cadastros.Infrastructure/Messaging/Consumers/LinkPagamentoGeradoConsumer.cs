using MassTransit;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarLinkPagamento;
using Mecanica.Hermes.Contracts.Integration;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Messaging.Consumers;

public sealed partial class LinkPagamentoGeradoConsumer(
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
            LogEnvioIgnorado(
                logger,
                pagamentoId,
                result.StatusCode,
                string.Join("; ", result.Errors));
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Envio de link do pagamento {PagamentoId} ignorado pelo consumer. "
                  + "Status: {StatusCode}. Erros: {Errors}")]
    private static partial void LogEnvioIgnorado(
        ILogger logger,
        Guid pagamentoId,
        System.Net.HttpStatusCode statusCode,
        string errors);
}
