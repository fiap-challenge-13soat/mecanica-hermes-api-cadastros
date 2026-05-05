using AutoMapper;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.UpdateProduto;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

public static class UpdateProdutoRoute
{
    public static void MapUpdateProduto(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/{id:guid}", UpdateProduto)
            .WithName("UpdateProduto")
            .Produces(statusCode: StatusCodes.Status202Accepted)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Atualiza os dados do produto pelo seu id");
    }

    private static async Task<IResult> UpdateProduto(
        [FromRoute] Guid id,
        [FromBody] UpdateProdutoRequest request,
        [FromServices] ISender sender,
        [FromServices] IMapper mapper,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProdutoCommand(id, request.Descricao, request.Valor);
        var response = await sender.Send(command, cancellationToken);

        return response.Present<ProdutoDto, ProdutoResponse>(mapper);
    }
}
