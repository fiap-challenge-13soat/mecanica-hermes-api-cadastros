using Mecanica.Hermes.Cadastros.Api.Endpoints.Notificacoes.Routes;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Notificacoes;

public static class NotificacoesEndpoints
{
    public static void MapNotificacoesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notificacoes")
            .WithTags("Notificacoes");

        group.MapEnviarOrcamentoParaAprovacao();
        group.MapEnviarLinkPagamento();
    }
}
