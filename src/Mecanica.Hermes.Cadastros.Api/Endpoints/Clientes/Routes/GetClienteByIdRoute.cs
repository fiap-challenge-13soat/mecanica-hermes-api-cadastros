using AutoMapper;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteById;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

public static class GetClienteByIdRoute
{
    public static void MapGetClienteById(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/{id:guid}", GetClienteById)
            .WithName("GetClienteById")
            .Produces(statusCode: StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Retorna os dados do cliente pelo seu id");
    }

    internal static async Task<IResult> GetClienteById(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        [FromServices] IMapper mapper,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetClienteByIdQuery(id), cancellationToken);
        return response.Present<ClienteDto, ClienteResponse>(mapper);
    }
}
