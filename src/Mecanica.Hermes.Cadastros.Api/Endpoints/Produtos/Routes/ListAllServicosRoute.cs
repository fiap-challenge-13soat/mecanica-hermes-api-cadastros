using AutoMapper;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListAllServicos;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

public static class ListAllServicosRoute
{
    public static void MapListAllServicos(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/list", ListAllServicos)
            .WithName("ListAllServicos")
            .Produces<PaginatedResponse<ServicoResponse>>(statusCode: StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Retorna uma lista paginada de todos os serviços");
    }

    private static async Task<IResult> ListAllServicos(
        [FromServices] ISender sender,
        [FromServices] IMapper mapper,
        HttpRequest request,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new ListAllServicosQuery(page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        
        if (result.IsFailure)
            return result.Present();

        var paginatedResponse = result.Data!.ToPaginatedResponse<ServicoDto, ServicoResponse>(mapper, request);
        return Results.Ok(paginatedResponse);
    }
}
