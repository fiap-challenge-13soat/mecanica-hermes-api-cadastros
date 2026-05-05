using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

public static class AddServicoRoute
{
    public static void MapAddServico(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", AddServico)
            .WithName("AddServico")
            .Produces<ServicoResponse>(statusCode: StatusCodes.Status201Created)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Cria um novo serviço");
    }

    private static async Task<IResult> AddServico(
        [FromBody] AddServicoRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(request.ToCommand(), cancellationToken);

        return response.Present(dto => dto.ToResponse(), r =>
            Results.CreatedAtRoute("GetServicoById", new { id = r.Id }, r));
    }
}
