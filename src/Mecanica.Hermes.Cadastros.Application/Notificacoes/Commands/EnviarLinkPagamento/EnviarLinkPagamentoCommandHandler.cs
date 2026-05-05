using MassTransit;
using Mecanica.Hermes.Cadastros.Application.Common.Messaging;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarLinkPagamento;

internal sealed class EnviarLinkPagamentoCommandHandler(
    IPagamentoApiClient pagamentoApiClient,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<EnviarLinkPagamentoCommand, Result>
{
    public async Task<Result> Handle(EnviarLinkPagamentoCommand request, CancellationToken cancellationToken)
    {
        var pagamento = await pagamentoApiClient.GetByIdAsync(request.PagamentoId, cancellationToken);
        if (pagamento is null)
            return Result.NotFound("Pagamento não encontrado na API de Pagamentos.");

        if (pagamento.LinkPagamento is null || string.IsNullOrWhiteSpace(pagamento.LinkPagamento.Url))
            return Result.BadRequest("Pagamento ainda não possui link disponível para envio.");

        if (string.IsNullOrWhiteSpace(pagamento.EmailCliente))
            return Result.BadRequest("Pagamento sem e-mail do cliente — não é possível notificar.");

        var body = LinkPagamentoEmailBuilder.BuildBody(pagamento);
        await publishEndpoint.Publish(
            new EnviarEmailCommand(pagamento.EmailCliente, LinkPagamentoEmailBuilder.BuildSubject(), body),
            cancellationToken);

        return Result.Accepted();
    }
}
