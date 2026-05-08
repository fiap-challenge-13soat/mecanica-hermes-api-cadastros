using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Shared.AspNetCore.Presenter;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarLinkPagamento;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Notificacoes.Routes;

public static class EnviarLinkPagamentoRoute
{
    public static void MapEnviarLinkPagamento(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/pagamento/{pagamentoId:guid}/enviar-link", EnviarLinkPagamento)
            .WithName("EnviarLinkPagamento")
            .Produces(statusCode: StatusCodes.Status202Accepted)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Consulta o Pagamento, monta o e-mail com o link e publica " +
                             "o EnviarEmailCommand no bus para envio assíncrono.");
    }

    private static async Task<IResult> EnviarLinkPagamento(
        [FromRoute] Guid pagamentoId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new EnviarLinkPagamentoCommand(pagamentoId), cancellationToken);
        return response.Present();
    }
}
