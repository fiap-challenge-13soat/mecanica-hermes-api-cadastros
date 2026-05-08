using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Clientes.ValueObjects;

public class EmailVoTests
{
    [Theory]
    [InlineData("user@dominio.com", "user@dominio.com")]
    [InlineData("USER@DOMINIO.COM", "user@dominio.com")]
    [InlineData("  Joao.Silva@Empresa.com.br  ", "joao.silva@empresa.com.br")]
    public void Criar_EmailValido_NormalizaParaLowercase(string input, string expected)
    {
        var result = EmailVo.Criar(input);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be(expected);
        result.Data.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Criar_EmailVazio_RetornaBadRequest(string? input)
    {
        var result = EmailVo.Criar(input!);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("obrigatório"));
    }

    [Fact]
    public void Criar_EmailMuitoLongo_RetornaBadRequest()
    {
        var longo = new string('a', 200) + "@a.co";

        var result = EmailVo.Criar(longo);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("200 caracteres"));
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("@dominio.com")]
    [InlineData("user@")]
    [InlineData("user@dominio")]
    [InlineData("user @dominio.com")]
    public void Criar_EmailInvalido_RetornaBadRequest(string input)
    {
        var result = EmailVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("formato inválido"));
    }
}
