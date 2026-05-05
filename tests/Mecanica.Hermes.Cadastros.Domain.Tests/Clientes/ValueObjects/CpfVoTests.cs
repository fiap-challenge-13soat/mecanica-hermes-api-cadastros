using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Clientes.ValueObjects;

public class CpfVoTests
{
    [Theory]
    [InlineData("52998224725", "52998224725")]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData(" 529 982 247 25 ", "52998224725")]
    public void Criar_CpfValido_NormalizaERetornaOk(string input, string expected)
    {
        var result = CpfVo.Criar(input);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be(expected);
        result.Data.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_CpfVazio_RetornaBadRequest(string input)
    {
        var result = CpfVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("não pode ser vazio"));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567890123")]
    public void Criar_CpfFormatoInvalido_RetornaBadRequest(string input)
    {
        var result = CpfVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("formato"));
    }

    [Theory]
    [InlineData("12345678900")]
    [InlineData("00000000001")]
    public void Criar_CpfDvInvalido_RetornaBadRequest(string input)
    {
        var result = CpfVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("inválido"));
    }

    [Fact]
    public void CpfValido_RetornaTrueParaCpfBomEFalseParaRuim()
    {
        CpfVo.CpfValido("52998224725").Should().BeTrue();
        CpfVo.CpfValido("12345678900").Should().BeFalse();
    }
}
