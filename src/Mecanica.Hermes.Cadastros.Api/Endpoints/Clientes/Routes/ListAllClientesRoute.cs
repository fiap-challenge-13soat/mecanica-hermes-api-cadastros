using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Shared.AspNetCore.Presenter;
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
            .WithDescription("Retorna uma lista paginada de clientes");
    }

    private static async Task<IResult> ListAllClientes(
        [FromServices] ISender sender,
        HttpRequest request,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await sender.Send(new ListAllClientesQuery(page, pageSize), cancellationToken);

        if (result.IsFailure)
            return result.Present();

        var paginatedResponse = result.Data!.ToPaginatedResponse(dto => dto.ToResponse(), request);
        return Results.Ok(paginatedResponse);
    }
}
