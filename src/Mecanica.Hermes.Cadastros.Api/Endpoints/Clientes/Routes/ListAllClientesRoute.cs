using AutoMapper;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Queries.ListAllClientes;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

public static class ListAllClientesRoute
{
    public static void MapListAllClientes(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/list", ListAllClientes)
            .WithName("ListAllClientes")
            .Produces<PaginatedResponse<ClienteResponse>>(statusCode: StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Retorna uma lista paginada de clientes pelo seu nome");
    }

    private static async Task<IResult> ListAllClientes(
        [FromServices] ISender sender,
        [FromServices] IMapper mapper,
        HttpRequest request,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new ListAllClientesQuery(page, pageSize);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.Present();

        var paginatedResponse = result.Data!.ToPaginatedResponse<ClienteDto, ClienteResponse>(mapper, request);
        return Results.Ok(paginatedResponse);
    }
}