using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Shared.AspNetCore.Presenter;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

public static class AddClienteRoute
{
    public static void MapAddCliente(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", AddCliente)
            .WithName("AddCliente")
            .Produces(statusCode: StatusCodes.Status201Created)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Cria um novo cliente");
    }

    private static async Task<IResult> AddCliente(
        [FromBody] AddClienteRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(request.ToCommand(), cancellationToken);

        return response.Present(dto => dto.ToResponse(), r =>
            Results.CreatedAtRoute("GetClienteById", new { id = r.Id }, r));
    }
}
