using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos;

public static class ServicosEndpoints
{
    public static void MapServicosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/servicos")
            .WithTags("Servicos");

        group.MapGetServicoById();
        group.MapListServicosByDescricao();
        group.MapListAllServicos();
        group.MapAddServico();
        group.MapUpdateServico();
        group.MapDeleteServico();
    }
}
