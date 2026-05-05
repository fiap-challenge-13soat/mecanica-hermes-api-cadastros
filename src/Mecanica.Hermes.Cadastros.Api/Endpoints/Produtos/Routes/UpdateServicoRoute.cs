using AutoMapper;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.UpdateServico;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

public static class UpdateServicoRoute
{
    public static void MapUpdateServico(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/{id:guid}", UpdateServico)
            .WithName("UpdateServico")
            .Produces(statusCode: StatusCodes.Status202Accepted)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Atualiza os dados do serviço pelo seu id");
    }

    private static async Task<IResult> UpdateServico(
        [FromRoute] Guid id,
        [FromBody] UpdateServicoRequest request,
        [FromServices] ISender sender,
        [FromServices] IMapper mapper,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServicoCommand(id, request.Descricao, request.Valor);
        var response = await sender.Send(command, cancellationToken);

        return response.Present<ServicoDto, ServicoResponse>(mapper);
    }
}
