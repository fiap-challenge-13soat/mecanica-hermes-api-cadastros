using AutoMapper;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.AddProduto;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

public static class AddProdutoRoute
{
    public static void MapAddProduto(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", AddProduto)
            .WithName("AddProduto")
            .Produces(statusCode: StatusCodes.Status201Created)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Cria um novo produto");
    }

    private static async Task<IResult> AddProduto(
        [FromBody] AddProdutoRequest request,
        [FromServices] ISender sender,
        [FromServices] IMapper mapper,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<AddProdutoCommand>(request);
        var response = await sender.Send(command, cancellationToken);

        return response.Present<ProdutoDto, ProdutoResponse>(mapper, r =>
            Results.CreatedAtRoute(
                "GetProdutoById",
                new { id = r.Id },
                r));
    }
}
