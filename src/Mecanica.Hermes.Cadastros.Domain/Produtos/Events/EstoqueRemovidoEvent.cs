using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Produtos.Events;

public class EstoqueRemovidoEvent(Guid produtoId, QuantidadeProdutoVo quantidade) : IDomainEvent
{
    public Guid ProdutoId { get; private set; } = produtoId;
    public QuantidadeProdutoVo Quantidade { get; private set; } = quantidade;
}
