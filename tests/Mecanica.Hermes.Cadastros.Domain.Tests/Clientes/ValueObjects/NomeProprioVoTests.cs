using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Clientes.ValueObjects;

public class NomeProprioVoTests
{
    [Theory]
    [InlineData("joão silva", "João Silva")]
    [InlineData("MARIA DA SILVA", "Maria da Silva")]
    [InlineData("  pedro DOS  santos  ", "Pedro dos Santos")]
    public void Criar_NormalizaTitleCaseEPreposicoes(string input, string expected)
    {
        var result = NomeProprioVo.Criar(input);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be(expected);
        result.Data.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Criar_NomeVazio_RetornaBadRequest(string? input)
    {
        var result = NomeProprioVo.Criar(input!);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("vazio"));
    }

    [Fact]
    public void Criar_NomeMuitoLongo_RetornaBadRequest()
    {
        var longo = new string('a', 250);

        var result = NomeProprioVo.Criar(longo);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("200"));
    }

    [Fact]
    public void Criar_NomeUnico_RetornaBadRequestPorFaltaSobrenome()
    {
        var result = NomeProprioVo.Criar("Joao");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("nome e sobrenome"));
    }
}
