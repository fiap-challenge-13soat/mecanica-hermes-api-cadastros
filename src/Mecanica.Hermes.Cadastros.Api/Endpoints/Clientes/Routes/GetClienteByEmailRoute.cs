using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteByEmail;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using Mecanica.Hermes.Shared.AspNetCore.Presenter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

public static class GetClienteByEmailRoute
{
    public static void MapGetClienteByEmail(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/email/{email}", GetClienteByEmail)
            .WithName("GetClienteByEmail")
            .Produces<ClienteResponse>(statusCode: StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Retorna os dados do cliente pelo seu email");
    }

    private static async Task<IResult> GetClienteByEmail(
        [FromRoute] string email,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetClienteByEmailQuery(email), cancellationToken);
        return response.Present(dto => dto.ToResponse());
    }
}
