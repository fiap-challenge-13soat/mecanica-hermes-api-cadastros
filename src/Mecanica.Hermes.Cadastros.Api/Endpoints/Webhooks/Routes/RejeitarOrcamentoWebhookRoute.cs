using MassTransit;
using Mecanica.Hermes.Contracts.Integration;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Webhooks.Routes;

public static class RejeitarOrcamentoWebhookRoute
{
    private const string Acao = "rejeitar";

    public static void MapRejeitarOrcamentoWebhook(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/{ordemDeServicoId:guid}/rejeitar", RejeitarOrcamento)
            .WithName("RejeitarOrcamentoWebhook")
            .AllowAnonymous()
            .WithDescription("Endpoint público acionado pelo cliente ao clicar no botão Rejeitar do e-mail. " +
                             "Valida HMAC, garante idempotência e publica evento para a API de Ordem de Serviço.");
    }

    private static async Task<IResult> RejeitarOrcamento(
        [FromRoute] Guid ordemDeServicoId,
        [FromQuery] string token,
        [FromQuery] long ts,
        [FromQuery] string? motivo,
        [FromServices] IWebhookTokenSigner tokenSigner,
        [FromServices] IWebhookEventRepository webhookEventRepository,
        [FromServices] IPublishEndpoint publishEndpoint,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!tokenSigner.Validate(ordemDeServicoId, Acao, token, ts))
            return WebhookHtmlResponses.Error(StatusCodes.Status401Unauthorized,
                "Link inválido",
                "O link clicado é inválido ou expirou. Solicite um novo orçamento ao atendente.");

        var externalId = $"{ordemDeServicoId:D}|{Acao}|{ts}";
        var registered = await webhookEventRepository.TryRegisterAsync(
            scope: "orcamento.rejeicao",
            externalId: externalId,
            status: Acao,
            cancellationToken);

        if (!registered)
            return WebhookHtmlResponses.Success(
                "Orçamento já rejeitado",
                "Esta rejeição já havia sido registrada anteriormente. Você pode fechar esta página.");

        await publishEndpoint.Publish(
            new OrcamentoRejeitadoPeloClienteEvent(ordemDeServicoId, timeProvider.GetUtcNow().UtcDateTime, motivo),
            cancellationToken);

        return WebhookHtmlResponses.Success(
            "Orçamento rejeitado",
            "Registramos a sua rejeição. A oficina será notificada para encerrar a ordem de serviço.");
    }
}
