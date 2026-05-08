using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Shared.AspNetCore.Presenter;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.DeleteProduto;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

public static class DeleteProdutoRoute
{
    public static void MapDeleteProduto(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/{id:guid}", DeleteProduto)
            .WithName("DeleteProduto")
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Exclui (soft delete) um produto pelo seu id");
    }

    private static async Task<IResult> DeleteProduto(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new DeleteProdutoCommand(id), cancellationToken);
        return response.Present();
    }
}
