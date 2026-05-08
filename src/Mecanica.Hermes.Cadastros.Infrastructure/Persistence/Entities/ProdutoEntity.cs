using Mecanica.Hermes.Cadastros.Domain.Produtos.Enums;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities.Abstractions;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;

internal class ProdutoEntity : BaseEntity
{
    private ProdutoEntity()
    {
    }

    internal ProdutoEntity(
        Guid id,
        DescricaoProdutoVo descricao,
        ValorProdutoVo valor,
        QuantidadeProdutoVo quantidade,
        TipoProduto tipo,
        DateTime createdAt,
        DateTime? updatedAt,
        bool isDeleted)
    {
        Id = id;
        Descricao = descricao;
        Valor = valor;
        Quantidade = quantidade;
        Tipo = tipo;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsDeleted = isDeleted;
    }

    public DescricaoProdutoVo Descricao { get; private set; } = null!;
    public ValorProdutoVo Valor { get; private set; } = null!;
    public QuantidadeProdutoVo Quantidade { get; private set; } = null!;
    public TipoProduto Tipo { get; private set; }
}
