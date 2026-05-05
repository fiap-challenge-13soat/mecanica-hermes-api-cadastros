using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using Mecanica.Hermes.Cadastros.Domain.Produtos.Events;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Produtos;

public sealed class Servico : AggregateRoot
{
    private Servico(
        DescricaoProdutoVo descricao,
        ValorProdutoVo valor)
    {
        Descricao = descricao;
        Valor = valor;
    }

    public DescricaoProdutoVo Descricao { get; private set; }
    public ValorProdutoVo Valor { get; private set; }

    public static Result<Servico> Criar(string descricao, decimal valor)
    {
        var errors = new List<string>();

        var descricaoVo = DescricaoProdutoVo.Criar(descricao);
        if (descricaoVo.IsFailure)
            errors.AddRange(descricaoVo.Errors);

        var valorVo = ValorProdutoVo.Criar(valor);
        if (valorVo.IsFailure)
            errors.AddRange(valorVo.Errors);

        if (errors.Count > 0)
            return Result<Servico>.BadRequest(errors);

        var servico = new Servico(descricaoVo.Data!, valorVo.Data!);

        servico.MarkAsUpdated();
        servico.AddDomainEvent(new ServicoCriadoEvent(servico.Id, servico.Valor));

        return Result<Servico>.Ok(servico);
    }

    public Result<Servico> AtualizarDados(string descricao, decimal valor)
    {
        var errors = new List<string>();

        var descricaoVo = DescricaoProdutoVo.Criar(descricao);
        if (descricaoVo.IsFailure)
            errors.AddRange(descricaoVo.Errors);

        var valorVo = ValorProdutoVo.Criar(valor);
        if (valorVo.IsFailure)
            errors.AddRange(valorVo.Errors);

        if (errors.Count > 0)
            return Result<Servico>.BadRequest(errors);

        Descricao = descricaoVo.Data!;
        Valor = valorVo.Data!;

        MarkAsUpdated();
        AddDomainEvent(new ServicoAlteradoEvent(Id, Valor));

        return Result<Servico>.Ok(this);
    }

    public static Servico Restaurar(
        Guid id,
        DescricaoProdutoVo descricao,
        ValorProdutoVo valor,
        DateTime createdAt,
        DateTime? updatedAt,
        bool isDeleted)
    {
        var servico = new Servico(descricao, valor);
        servico.RestaurarBase(id, createdAt, updatedAt, isDeleted);
        return servico;
    }

    public Result<Servico> Excluir()
    {
        if (IsDeleted)
            return Result<Servico>.BadRequest("Serviço já foi excluído.");

        MarkAsDeleted();
        AddDomainEvent(new ServicoExcluidoEvent(Id));

        return Result<Servico>.Ok(this);
    }
}