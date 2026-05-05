using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Clientes.ValueObjects;

public class TelefoneVoTests
{
    [Theory]
    [InlineData("11987654321", "11987654321")]
    [InlineData("(11) 98765-4321", "11987654321")]
    [InlineData("(11) 3210-4567", "1132104567")]
    public void Criar_TelefoneValido_NormalizaApenasDigitos(string input, string expected)
    {
        var result = TelefoneVo.Criar(input);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be(expected);
        result.Data.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_TelefoneVazio_RetornaBadRequest(string input)
    {
        var result = TelefoneVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("obrigatório"));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")]
    public void Criar_TelefoneTamanhoInvalido_RetornaBadRequest(string input)
    {
        var result = TelefoneVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("DDD"));
    }
}
