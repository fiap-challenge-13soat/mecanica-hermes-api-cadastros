using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Clientes.ValueObjects;

public class PlacaVoTests
{
    [Theory]
    [InlineData("ABC1234", "ABC1234")]
    [InlineData("abc1d23", "ABC1D23")]
    [InlineData(" abc-1234 ", "ABC1234")]
    public void Criar_PlacaValida_NormalizaParaUpperSemHifen(string input, string expected)
    {
        var result = PlacaVo.Criar(input);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be(expected);
        result.Data.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_PlacaVazia_RetornaBadRequest(string input)
    {
        var result = PlacaVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("vazia"));
    }

    [Theory]
    [InlineData("AB1234")]
    [InlineData("ABCDEFG")]
    [InlineData("12345678")]
    public void Criar_PlacaFormatoInvalido_RetornaBadRequest(string input)
    {
        var result = PlacaVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("Placa inválida"));
    }
}
