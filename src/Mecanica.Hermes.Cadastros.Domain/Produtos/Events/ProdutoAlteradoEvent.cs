using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Produtos.Events;

public class ProdutoAlteradoEvent(Guid id, ValorProdutoVo valor) : IDomainEvent
{
    public Guid Id { get; private set; } = id;
    public ValorProdutoVo Valor { get; private set; } = valor;
}