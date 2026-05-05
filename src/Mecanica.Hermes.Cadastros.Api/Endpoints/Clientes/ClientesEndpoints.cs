using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Routes;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes;

public static class ClientesEndpoints
{
    public static void MapClientesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clientes")
            .WithTags("Clientes");

        group.MapGetClienteById();
        group.MapListClientesByNome();
        group.MapListAllClientes();
        group.MapGetClienteByEmail();
        group.MapGetClienteByIdentificacaoFiscal();
        group.MapAddCliente();
        group.MapUpdateCliente();
        group.MapDeleteCliente();

        group.MapAddVeiculo();
        group.MapUpdateVeiculo();
        group.MapDeleteVeiculo();
    }
}