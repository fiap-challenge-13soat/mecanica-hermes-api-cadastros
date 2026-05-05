using AutoMapper;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListProdutosByDescricao;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

public static class ListProdutosByDescricaoRoute
{
    public static void MapListProdutosByDescricao(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/descricao/{descricao}", ListProdutosByDescricao)
            .WithName("ListProdutosByDescricao")
            .Produces<PaginatedResponse<ProdutoResponse>>(statusCode: StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Retorna uma lista paginada de produtos pela descrição");
    }

    private static async Task<IResult> ListProdutosByDescricao(
        [FromRoute] string descricao,
        [FromServices] ISender sender,
        [FromServices] IMapper mapper,
        HttpRequest request,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new ListProdutosByDescricaoQuery(descricao, page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        
        if (result.IsFailure)
            return result.Present();

        var paginatedResponse = result.Data!.ToPaginatedResponse<ProdutoDto, ProdutoResponse>(mapper, request);
        return Results.Ok(paginatedResponse);
    }
}
