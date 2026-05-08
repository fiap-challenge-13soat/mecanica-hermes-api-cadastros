using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using Mecanica.Hermes.Shared.Core.Results;
using Mecanica.Hermes.Cadastros.Domain.Produtos.Enums;
using Mecanica.Hermes.Cadastros.Domain.Produtos.Events;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Produtos;

public sealed class Produto : AggregateRoot
{
    private Produto(
        DescricaoProdutoVo descricao,
        ValorProdutoVo valor,
        QuantidadeProdutoVo quantidade,
        TipoProduto tipo)
    {
        Descricao = descricao;
        Valor = valor;
        Quantidade = quantidade;
        Tipo = tipo;
    }

    public DescricaoProdutoVo Descricao { get; private set; }
    public ValorProdutoVo Valor { get; private set; }
    public QuantidadeProdutoVo Quantidade { get; private set; }
    public TipoProduto Tipo { get; private set; }

    public static Result<Produto> Criar(
        string descricao,
        decimal valor,
        int quantidade,
        string tipo)
    {
        var errors = new List<string>();

        var descricaoVo = DescricaoProdutoVo.Criar(descricao);
        if (descricaoVo.IsFailure)
            errors.AddRange(descricaoVo.Errors);

        var valorVo = ValorProdutoVo.Criar(valor);
        if (valorVo.IsFailure)
            errors.AddRange(valorVo.Errors);

        var quantidadeVo = QuantidadeProdutoVo.Criar(quantidade);
        if (quantidadeVo.IsFailure)
            errors.AddRange(quantidadeVo.Errors);

        if (!Enum.TryParse<TipoProduto>(tipo, true, out var tipoEnum))
            errors.Add("Tipo de produto inválido.");

        if (errors.Count > 0)
            return Result<Produto>.BadRequest(errors);

        var produto = new Produto(descricaoVo.Data!, valorVo.Data!, quantidadeVo.Data!, tipoEnum);

        produto.AddDomainEvent(new ProdutoCriadoEvent(produto.Id, produto.Valor));

        return Result<Produto>.Ok(produto);
    }

    public Result<Produto> AtualizarDados(string descricao, decimal valor)
    {
        var errors = new List<string>();

        var descricaoVo = DescricaoProdutoVo.Criar(descricao);
        if (descricaoVo.IsFailure)
            errors.AddRange(descricaoVo.Errors);

        var valorVo = ValorProdutoVo.Criar(valor);
        if (valorVo.IsFailure)
            errors.AddRange(valorVo.Errors);

        if (errors.Count > 0)
            return Result<Produto>.BadRequest(errors);

        Descricao = descricaoVo.Data!;
        Valor = valorVo.Data!;

        MarkAsUpdated();
        AddDomainEvent(new ProdutoAlteradoEvent(Id, Valor));

        return Result<Produto>.Ok(this);
    }

    internal static Produto Restaurar(
        Guid id,
        DescricaoProdutoVo descricao,
        ValorProdutoVo valor,
        QuantidadeProdutoVo quantidade,
        TipoProduto tipo,
        DateTime createdAt,
        DateTime? updatedAt,
        bool isDeleted)
    {
        var produto = new Produto(descricao, valor, quantidade, tipo);
        produto.RestaurarBase(id, createdAt, updatedAt, isDeleted);
        return produto;
    }

    public Result<QuantidadeProdutoVo> AdicionarEstoque(QuantidadeProdutoVo quantidade)
    {
        var result = Quantidade.Somar(quantidade);

        MarkAsUpdated();
        AddDomainEvent(new EstoqueAdicionadoEvent(Id, Quantidade));

        return result;
    }

    public Result<QuantidadeProdutoVo> RemoverEstoque(QuantidadeProdutoVo quantidade)
    {
        var result = Quantidade.Subtrair(quantidade);

        if (result.IsSuccess)
        {
            MarkAsUpdated();
            AddDomainEvent(new EstoqueRemovidoEvent(Id, Quantidade));
        }
        else
            AddDomainEvent(new EstoqueIndisponivelEvent(Id, Quantidade, result.Errors));

        return result;
    }

    public Result<Produto> Excluir()
    {
        if (IsDeleted)
            return Result<Produto>.BadRequest("Produto já foi excluído.");

        MarkAsDeleted();
        AddDomainEvent(new ProdutoExcluidoEvent(Id));

        return Result<Produto>.Ok();
    }
}