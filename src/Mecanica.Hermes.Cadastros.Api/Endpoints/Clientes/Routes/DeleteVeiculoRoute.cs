using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Shared.AspNetCore.Presenter;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.DeleteVeiculo;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

public static class DeleteVeiculoRoute
{
    public static void MapDeleteVeiculo(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/veiculos/{clienteId:guid}/{veiculoId:guid}", DeleteVeiculo)
            .WithName("DeleteVeiculo")
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Remove (soft delete) um veículo do cliente");
    }

    private static async Task<IResult> DeleteVeiculo(
        [FromRoute] Guid clienteId,
        [FromRoute] Guid veiculoId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new DeleteVeiculoCommand(clienteId, veiculoId), cancellationToken);
        return response.Present();
    }
}
