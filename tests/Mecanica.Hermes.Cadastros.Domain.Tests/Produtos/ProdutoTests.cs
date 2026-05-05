using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos.Enums;
using Mecanica.Hermes.Cadastros.Domain.Produtos.Events;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Produtos;

public class ProdutoTests
{
    [Fact]
    public void Criar_DadosValidos_RetornaProdutoComEvento()
    {
        var result = Produto.Criar("Filtro de Oleo", 50.5m, 10, "Peca");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Descricao.Valor.Should().Be("Filtro de Oleo");
        result.Data.Valor.Valor.Should().Be(50.5m);
        result.Data.Quantidade.Valor.Should().Be(10);
        result.Data.Tipo.Should().Be(TipoProduto.Peca);
        result.Data.DomainEvents.Should().Contain(e => e is ProdutoCriadoEvent);
    }

    [Fact]
    public void Criar_TipoInvalido_RetornaBadRequest()
    {
        var result = Produto.Criar("X", 1m, 1, "Inexistente");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("Tipo"));
    }

    [Fact]
    public void Criar_DescricaoEValorInvalidos_AcumulaErros()
    {
        var result = Produto.Criar("", -1m, 0, "INVALIDO");

        result.IsFailure.Should().BeTrue();
        result.Errors.Count.Should().BeGreaterThan(2);
    }

    [Fact]
    public void AtualizarDados_DadosValidos_AdicionaEvento()
    {
        var produto = NovoProduto();

        var result = produto.AtualizarDados("Filtro Premium", 99.9m);

        result.IsSuccess.Should().BeTrue();
        produto.Descricao.Valor.Should().Be("Filtro Premium");
        produto.Valor.Valor.Should().Be(99.9m);
        produto.UpdatedAt.Should().NotBeNull();
        produto.DomainEvents.Should().Contain(e => e is ProdutoAlteradoEvent);
    }

    [Fact]
    public void AtualizarDados_DadosInvalidos_RetornaBadRequest()
    {
        var produto = NovoProduto();

        var result = produto.AtualizarDados("", -1m);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AdicionarEstoque_AumentaQuantidade()
    {
        var produto = NovoProduto();
        var qtd = QuantidadeProdutoVo.Criar(5).Data!;

        var result = produto.AdicionarEstoque(qtd);

        result.IsSuccess.Should().BeTrue();
        produto.Quantidade.Valor.Should().Be(15);
        produto.DomainEvents.Should().Contain(e => e is EstoqueAdicionadoEvent);
    }

    [Fact]
    public void RemoverEstoque_QuantidadeDisponivel_DiminuiQuantidade()
    {
        var produto = NovoProduto();
        var qtd = QuantidadeProdutoVo.Criar(3).Data!;

        var result = produto.RemoverEstoque(qtd);

        result.IsSuccess.Should().BeTrue();
        produto.Quantidade.Valor.Should().Be(7);
        produto.DomainEvents.Should().Contain(e => e is EstoqueRemovidoEvent);
    }

    [Fact]
    public void RemoverEstoque_QuantidadeMaiorQueDisponivel_PublicaIndisponivel()
    {
        var produto = NovoProduto();
        var qtd = QuantidadeProdutoVo.Criar(50).Data!;

        var result = produto.RemoverEstoque(qtd);

        result.IsFailure.Should().BeTrue();
        produto.DomainEvents.Should().Contain(e => e is EstoqueIndisponivelEvent);
    }

    [Fact]
    public void Excluir_PrimeiraVez_MarcaIsDeleted()
    {
        var produto = NovoProduto();

        var result = produto.Excluir();

        result.IsSuccess.Should().BeTrue();
        produto.IsDeleted.Should().BeTrue();
        produto.DomainEvents.Should().Contain(e => e is ProdutoExcluidoEvent);
    }

    [Fact]
    public void Excluir_JaExcluido_RetornaBadRequest()
    {
        var produto = NovoProduto();
        produto.Excluir();

        var result = produto.Excluir();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Restaurar_RestauraCamposBase()
    {
        var d = DescricaoProdutoVo.Criar("Filtro").Data!;
        var v = ValorProdutoVo.Criar(10m).Data!;
        var q = QuantidadeProdutoVo.Criar(5).Data!;
        var id = Guid.NewGuid();

        var produto = Produto.Restaurar(id, d, v, q, TipoProduto.Insumo, DateTime.UtcNow, null, false);

        produto.Id.Should().Be(id);
        produto.Tipo.Should().Be(TipoProduto.Insumo);
    }

    private static Produto NovoProduto() =>
        Produto.Criar("Filtro de Oleo", 50.5m, 10, "Peca").Data!;
}
