using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos.Events;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Produtos;

public class ServicoTests
{
    [Fact]
    public void Criar_DadosValidos_RetornaServicoComEvento()
    {
        var result = Servico.Criar("Troca de Oleo", 120m);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Descricao.Valor.Should().Be("Troca de Oleo");
        result.Data.Valor.Valor.Should().Be(120m);
        result.Data.DomainEvents.Should().Contain(e => e is ServicoCriadoEvent);
    }

    [Fact]
    public void Criar_DadosInvalidos_AcumulaErros()
    {
        var result = Servico.Criar("", -1m);

        result.IsFailure.Should().BeTrue();
        result.Errors.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void AtualizarDados_DadosValidos_AdicionaEvento()
    {
        var servico = NovoServico();

        var result = servico.AtualizarDados("Troca Premium", 200m);

        result.IsSuccess.Should().BeTrue();
        servico.Descricao.Valor.Should().Be("Troca Premium");
        servico.Valor.Valor.Should().Be(200m);
        servico.DomainEvents.Should().Contain(e => e is ServicoAlteradoEvent);
    }

    [Fact]
    public void AtualizarDados_DadosInvalidos_RetornaBadRequest()
    {
        var servico = NovoServico();

        var result = servico.AtualizarDados("", -1m);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Excluir_PrimeiraVez_MarcaIsDeleted()
    {
        var servico = NovoServico();

        var result = servico.Excluir();

        result.IsSuccess.Should().BeTrue();
        servico.IsDeleted.Should().BeTrue();
        servico.DomainEvents.Should().Contain(e => e is ServicoExcluidoEvent);
    }

    [Fact]
    public void Excluir_JaExcluido_RetornaBadRequest()
    {
        var servico = NovoServico();
        servico.Excluir();

        var result = servico.Excluir();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Restaurar_RestauraCamposBase()
    {
        var d = DescricaoProdutoVo.Criar("Servico").Data!;
        var v = ValorProdutoVo.Criar(50m).Data!;
        var id = Guid.NewGuid();

        var servico = Servico.Restaurar(id, d, v, DateTime.UtcNow, null, false);

        servico.Id.Should().Be(id);
        servico.Descricao.Valor.Should().Be("Servico");
    }

    private static Servico NovoServico() => Servico.Criar("Troca de Oleo", 120m).Data!;
}
