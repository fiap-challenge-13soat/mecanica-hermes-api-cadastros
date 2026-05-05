using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Api.Presenter;
using Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteByIdentificacaoFiscal;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

public static class GetClienteByIdentificacaoFiscalRoute
{
    public static void MapGetClienteByIdentificacaoFiscal(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/fiscal/{identificacaoFiscal}", GetClienteByIdentificacaoFiscal)
            .WithName("GetClienteByIdentificacaoFiscal")
            .Produces<ClienteResponse>(statusCode: StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(statusCode: StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthPolicies.OnlyAdminScope)
            .WithDescription("Retorna os dados do cliente pela sua identificação fiscal");
    }

    private static async Task<IResult> GetClienteByIdentificacaoFiscal(
        [FromRoute] string identificacaoFiscal,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new GetClienteByIdentificacaoFiscalQuery(identificacaoFiscal),
            cancellationToken);
        return response.Present(dto => dto.ToResponse());
    }
}
