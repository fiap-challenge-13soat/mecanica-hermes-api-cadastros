using Mecanica.Hermes.Cadastros.Api.Endpoints.Webhooks.Routes;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Webhooks;

public static class WebhooksEndpoints
{
    public static void MapWebhooksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/webhooks/orcamentos")
            .WithTags("Webhooks");

        group.MapAprovarOrcamentoWebhook();
        group.MapRejeitarOrcamentoWebhook();
    }
}
