using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Clientes.ValueObjects;

public class IdentificacaoFiscalVoTests
{
    [Fact]
    public void Criar_Cpf_RetornaOk()
    {
        var result = IdentificacaoFiscalVo.Criar("529.982.247-25");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be("52998224725");
        result.Data.ToString().Should().Be("52998224725");
    }

    [Fact]
    public void Criar_Cnpj_RetornaOk()
    {
        var result = IdentificacaoFiscalVo.Criar("11444777000161");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Valor.Should().Be("11444777000161");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901234567890")]
    public void Criar_TamanhoInvalido_RetornaBadRequest(string input)
    {
        var result = IdentificacaoFiscalVo.Criar(input);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("CPF ou CNPJ"));
    }

    [Fact]
    public void Criar_CpfDvErrado_PropagaErrosDoCpfVo()
    {
        var result = IdentificacaoFiscalVo.Criar("12345678900");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Criar_CnpjDvErrado_PropagaErrosDoCnpjVo()
    {
        var result = IdentificacaoFiscalVo.Criar("11444777000100");

        result.IsFailure.Should().BeTrue();
    }
}
