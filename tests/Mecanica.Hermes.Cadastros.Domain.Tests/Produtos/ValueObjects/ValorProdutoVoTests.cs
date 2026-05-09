using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Produtos.ValueObjects;

public class ValorProdutoVoTests
{
    [Theory]
    [InlineData(10.50, 10.50)]
    [InlineData(10.999, 10.99)]
    [InlineData(0, 0)]
    public void Criar_ValorValido_LimitaDuasCasasDecimais(decimal input, decimal expected)
    {
        var result = ValorProdutoVo.Criar(input);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be(expected);
    }

    [Fact]
    public void Criar_ValorNegativo_RetornaBadRequest()
    {
        var result = ValorProdutoVo.Criar(-1m);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("negativo"));
    }

    [Fact]
    public void ToString_RetornaRepresentacaoTextual()
    {
        var vo = ValorProdutoVo.Criar(99.5m).Data!;

        vo.ToString().Should().NotBeNullOrEmpty();
    }
}
