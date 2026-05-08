using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Mappings;

internal static class ProdutoMapper
{
    internal static Produto ToDomain(this ProdutoEntity entity)
    {
        return Produto.Restaurar(
            entity.Id,
            entity.Descricao,
            entity.Valor,
            entity.Quantidade,
            entity.Tipo,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.IsDeleted);
    }

    internal static ProdutoEntity ToEntity(this Produto produto)
    {
        return new ProdutoEntity(
            produto.Id,
            produto.Descricao,
            produto.Valor,
            produto.Quantidade,
            produto.Tipo,
            produto.CreatedAt,
            produto.UpdatedAt,
            produto.IsDeleted);
    }
}