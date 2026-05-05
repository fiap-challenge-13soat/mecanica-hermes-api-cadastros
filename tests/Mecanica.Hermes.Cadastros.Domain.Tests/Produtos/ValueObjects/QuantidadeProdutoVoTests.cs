using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Produtos.ValueObjects;

public class QuantidadeProdutoVoTests
{
    [Fact]
    public void Criar_QuantidadePositiva_RetornaOk()
    {
        var result = QuantidadeProdutoVo.Criar(10);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be(10);
        result.Data.ToString().Should().Be("10");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Criar_QuantidadeZeroOuNegativa_RetornaBadRequest(int valor)
    {
        var result = QuantidadeProdutoVo.Criar(valor);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("positiva"));
    }
}
