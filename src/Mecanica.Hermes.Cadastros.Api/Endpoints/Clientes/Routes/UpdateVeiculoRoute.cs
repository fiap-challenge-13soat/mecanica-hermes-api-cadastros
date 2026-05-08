using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Shared.AspNetCore.Presenter;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

public static class UpdateVeiculoRoute
{
    public static void MapUpdateVeiculo(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/veiculos/{clienteId:guid}/{veiculoId:guid}", UpdateVeiculo)
            .WithName("UpdateVeiculo")
            .Produces<ClienteResponse>(statusCode: StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Atualiza um veículo do cliente");
    }

    private static async Task<IResult> UpdateVeiculo(
        [FromRoute] Guid clienteId,
        [FromRoute] Guid veiculoId,
        [FromBody] UpdateVeiculoRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(request.ToCommand(clienteId, veiculoId), cancellationToken);
        return response.Present(dto => dto.ToResponse());
    }
}
