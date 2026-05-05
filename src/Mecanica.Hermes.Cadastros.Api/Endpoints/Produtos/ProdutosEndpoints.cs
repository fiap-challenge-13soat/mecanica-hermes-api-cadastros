using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Routes;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos;

public static class ProdutosEndpoints
{
    public static void MapProdutosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/produtos")
            .WithTags("Produtos");

        group.MapGetProdutoById();
        group.MapListProdutosByDescricao();
        group.MapListAllProdutos();
        group.MapAddProduto();
        group.MapUpdateProduto();
        group.MapDeleteProduto();
    }
}