using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Produtos.ValueObjects;

public class DescricaoProdutoVoTests
{
    [Theory]
    [InlineData("filtro de oleo", "Filtro de Oleo")]
    [InlineData("PASTILHA  DE freio", "Pastilha de Freio")]
    public void Criar_DescricaoValida_NormalizaTitleCase(string input, string expected)
    {
        var result = DescricaoProdutoVo.Criar(input);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be(expected);
        result.Data.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_DescricaoVazia_RetornaBadRequest(string input)
    {
        var result = DescricaoProdutoVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("vazia"));
    }

    [Fact]
    public void Criar_DescricaoMuitoLonga_RetornaBadRequest()
    {
        var longa = new string('a', 150);

        var result = DescricaoProdutoVo.Criar(longa);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("100"));
    }
}
