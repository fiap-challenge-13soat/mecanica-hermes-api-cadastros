using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Clientes.ValueObjects;

public class CnpjVoTests
{
    [Fact]
    public void Criar_CnpjValido_RetornaOk()
    {
        var result = CnpjVo.Criar("11444777000161");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be("11444777000161");
        result.Data.ToString().Should().Be("11444777000161");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_CnpjVazio_RetornaBadRequest(string input)
    {
        var result = CnpjVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("vazio"));
    }

    [Theory]
    [InlineData("11.444.777/0001-61")]
    [InlineData("123")]
    [InlineData("11444777999961")]
    public void Criar_CnpjFormatoInvalido_RetornaBadRequest(string input)
    {
        var result = CnpjVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("formato"));
    }

    [Fact]
    public void Criar_CnpjFormatoOkMasDvErrado_RetornaBadRequest()
    {
        var result = CnpjVo.Criar("11444777000100");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("inválido"));
    }

    [Fact]
    public void CnpjValido_RetornaTrueParaCnpjBomEFalseParaRuim()
    {
        CnpjVo.CnpjValido("11444777000161").Should().BeTrue();
        CnpjVo.CnpjValido("11444777000100").Should().BeFalse();
    }
}
