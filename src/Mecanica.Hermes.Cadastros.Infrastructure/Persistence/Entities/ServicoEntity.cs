using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities.Abstractions;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;

internal class ServicoEntity : BaseEntity
{
    private ServicoEntity()
    {
    }

    internal ServicoEntity(
        Guid id,
        DescricaoProdutoVo descricao,
        ValorProdutoVo valor,
        DateTime createdAt,
        DateTime? updatedAt,
        bool isDeleted)
    {
        Id = id;
        Descricao = descricao;
        Valor = valor;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsDeleted = isDeleted;
    }

    public DescricaoProdutoVo Descricao { get; private set; } = null!;
    public ValorProdutoVo Valor { get; private set; } = null!;
}
