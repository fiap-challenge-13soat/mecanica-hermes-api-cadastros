using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarOrcamentoParaAprovacao;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Notificacoes.Routes;

public static class EnviarOrcamentoParaAprovacaoRoute
{
    public static void MapEnviarOrcamentoParaAprovacao(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/orcamento/{ordemServicoId:guid}/enviar-aprovacao", EnviarOrcamentoParaAprovacao)
            .WithName("EnviarOrcamentoParaAprovacao")
            .Produces(statusCode: StatusCodes.Status202Accepted)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Consulta a Ordem de Serviço, monta o e-mail com os botões de aprovação " +
                             "e publica o EnviarEmailCommand no bus para envio assíncrono.");
    }

    private static async Task<IResult> EnviarOrcamentoParaAprovacao(
        [FromRoute] Guid ordemServicoId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new EnviarOrcamentoParaAprovacaoCommand(ordemServicoId),
            cancellationToken);
        return response.Present();
    }
}
