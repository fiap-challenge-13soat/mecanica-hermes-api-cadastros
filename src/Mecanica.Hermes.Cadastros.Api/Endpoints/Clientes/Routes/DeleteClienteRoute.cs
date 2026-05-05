using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.DeleteCliente;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

public static class DeleteClienteRoute
{
    public static void MapDeleteCliente(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/{id:guid}", DeleteCliente)
            .WithName("DeleteCliente")
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Exclui (soft delete) um cliente pelo seu id");
    }

    private static async Task<IResult> DeleteCliente(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new DeleteClienteCommand(id), cancellationToken);
        return response.Present();
    }
}
