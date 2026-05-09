using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using Mecanica.Hermes.Shared.AspNetCore.Presenter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

public static class AddProdutoRoute
{
    public static void MapAddProduto(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", AddProduto)
            .WithName("AddProduto")
            .Produces<ProdutoResponse>(statusCode: StatusCodes.Status201Created)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Cria um novo produto");
    }

    private static async Task<IResult> AddProduto(
        [FromBody] AddProdutoRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(request.ToCommand(), cancellationToken);

        return response.Present(dto => dto.ToResponse(), r =>
            Results.CreatedAtRoute("GetProdutoById", new { id = r.Id }, r));
    }
}
