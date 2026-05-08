using Mecanica.Hermes.Contracts.Integration;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Webhooks.Routes;

public static class AprovarOrcamentoWebhookRoute
{
    private const string Acao = "aprovar";

    public static void MapAprovarOrcamentoWebhook(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/{ordemDeServicoId:guid}/aprovar", AprovarOrcamento)
            .WithName("AprovarOrcamentoWebhook")
            .AllowAnonymous()
            .WithDescription("Endpoint público acionado pelo cliente ao clicar no botão Aprovar do e-mail. " +
                             "Valida HMAC, garante idempotência e enfileira evento (via Outbox transacional) " +
                             "para a API de Ordem de Serviço.");
    }

    private static async Task<IResult> AprovarOrcamento(
        [FromRoute] Guid ordemDeServicoId,
        [FromQuery] string token,
        [FromQuery] long ts,
        [FromServices] IWebhookTokenSigner tokenSigner,
        [FromServices] IWebhookEventRepository webhookEventRepository,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!tokenSigner.Validate(ordemDeServicoId, Acao, token, ts))
            return WebhookHtmlResponses.Error(StatusCodes.Status401Unauthorized,
                "Link inválido",
                "O link clicado é inválido ou expirou. Solicite um novo orçamento ao atendente.");

        var externalId = $"{ordemDeServicoId:D}|{Acao}|{ts}";
        var integrationEvent = new OrcamentoAprovadoPeloClienteEvent(
            ordemDeServicoId,
            timeProvider.GetUtcNow().UtcDateTime);

        // Atômico: registro de idempotência + enqueue no Outbox no mesmo SaveChanges.
        // Publicação no RabbitMQ é responsabilidade do OutboxProcessor (background).
        var registered = await webhookEventRepository.TryRegisterAsync(
            scope: "orcamento.aprovacao",
            externalId: externalId,
            status: Acao,
            integrationEvent: integrationEvent,
            cancellationToken);

        if (!registered)
            return WebhookHtmlResponses.Success(
                "Orçamento já aprovado",
                "Esta aprovação já havia sido registrada anteriormente. Você pode fechar esta página.");

        return WebhookHtmlResponses.Success(
            "Orçamento aprovado",
            "Recebemos a sua aprovação. A oficina já foi notificada e dará continuidade ao serviço.");
    }
}
