using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Common.Helpers;

public class TitleCaseHelperTests
{
    // TitleCaseHelper é internal — testamos via NomeProprioVo (mesmo assembly)
    [Theory]
    [InlineData("joão  silva", "João Silva")]
    [InlineData("MARIA dos santos", "Maria dos Santos")]
    [InlineData("ANTÔNIO E paulo", "Antônio e Paulo")]
    [InlineData("a b", "A B")]
    public void Normalizar_AjustaCapitalizacaoEPreposicoes(string input, string expected)
    {
        var result = NomeProprioVo.Criar(input);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be(expected);
    }
}
