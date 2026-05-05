using AutoMapper;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Queries.GetProdutoById;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

public static class GetProdutoByIdRoute
{
    public static void MapGetProdutoById(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/{id:guid}", GetProdutoById)
            .WithName("GetProdutoById")
            .Produces(statusCode: StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Retorna os dados do produto pelo seu id");
    }

    internal static async Task<IResult> GetProdutoById(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        [FromServices] IMapper mapper,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetProdutoByIdQuery(id), cancellationToken);
        return response.Present<ProdutoDto, ProdutoResponse>(mapper);
    }
}
