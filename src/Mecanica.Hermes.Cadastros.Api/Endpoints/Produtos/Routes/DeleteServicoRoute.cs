using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.DeleteServico;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

public static class DeleteServicoRoute
{
    public static void MapDeleteServico(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/{id:guid}", DeleteServico)
            .WithName("DeleteServico")
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Exclui (soft delete) um serviço pelo seu id");
    }

    private static async Task<IResult> DeleteServico(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new DeleteServicoCommand(id), cancellationToken);
        return response.Present();
    }
}
