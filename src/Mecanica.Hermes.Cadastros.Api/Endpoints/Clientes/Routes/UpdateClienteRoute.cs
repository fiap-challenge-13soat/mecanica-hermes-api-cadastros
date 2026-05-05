using AutoMapper;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.UpdateCliente;
using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

public static class UpdateClienteRoute
{
    public static void MapUpdateCliente(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/{id:guid}", UpdateCliente)
            .WithName("UpdateCliente")
            .Produces(statusCode: StatusCodes.Status202Accepted)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Atualiza os dados do cliente pelo seu id");
    }
    private static async Task<IResult> UpdateCliente(
        [FromRoute] Guid id,
        [FromBody] UpdateClienteRequest request,
        [FromServices] ISender sender,
        [FromServices] IMapper mapper,
        CancellationToken cancellationToken)
    {
        var command = new UpdateClienteCommand(
            id,
            request.NomeCivil,
            request.NomeSocial,
            request.Email,
            request.Telefone);
        var response = await sender.Send(command, cancellationToken);

        return response.Present<ClienteDto, ClienteResponse>(mapper);
    }
}