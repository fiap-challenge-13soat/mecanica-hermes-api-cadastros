using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

public static class AddVeiculoRoute
{
    public static void MapAddVeiculo(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/{id:guid}/veiculos", AddVeiculo)
            .WithName("AddVeiculo")
            .Produces<ClienteResponse>(statusCode: StatusCodes.Status201Created)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Cria um novo veículo para o cliente");
    }

    private static async Task<IResult> AddVeiculo(
        [FromRoute] Guid id,
        [FromBody] AddVeiculoRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(request.ToCommand(id), cancellationToken);

        return response.Present(dto => dto.ToResponse(), r =>
            Results.CreatedAtRoute("GetClienteById", new { id = r.Id }, r));
    }
}
