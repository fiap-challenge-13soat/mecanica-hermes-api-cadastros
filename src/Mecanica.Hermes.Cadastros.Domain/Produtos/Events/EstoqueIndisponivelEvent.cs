using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Produtos.Events;

public class EstoqueIndisponivelEvent(Guid produtoId, QuantidadeProdutoVo quantidade, List<string> errors)
    : IDomainEvent
{
    public Guid ProdutoId { get; private set; } = produtoId;
    public QuantidadeProdutoVo Quantidade { get; private set; } = quantidade;
    public List<string> Errors { get; private set; } = errors;
}
