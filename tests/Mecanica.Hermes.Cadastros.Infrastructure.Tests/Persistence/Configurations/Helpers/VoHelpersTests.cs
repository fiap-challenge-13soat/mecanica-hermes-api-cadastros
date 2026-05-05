using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Tests.Persistence.Configurations.Helpers;

public class VoHelpersTests
{
    [Fact]
    public void EmailVoHelper_RecriarComValorValido_RetornaVo()
    {
        EmailVoHelper.RecriarEmail("user@x.com").Valor.Should().Be("user@x.com");
    }

    [Fact]
    public void EmailVoHelper_RecriarComValorInvalido_LancaInvalidOperation()
    {
        Action act = () => EmailVoHelper.RecriarEmail("invalido");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IdentificacaoFiscalVoHelper_RecriarComValorValido_RetornaVo()
    {
        IdentificacaoFiscalVoHelper.RecriarIdentificacaoFiscal("52998224725").Valor.Should().Be("52998224725");
    }

    [Fact]
    public void IdentificacaoFiscalVoHelper_RecriarComValorInvalido_LancaInvalidOperation()
    {
        Action act = () => IdentificacaoFiscalVoHelper.RecriarIdentificacaoFiscal("123");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NomeProprioVoHelper_RecriarComValorValido_RetornaVo()
    {
        NomeProprioVoHelper.RecriarNome("João Silva").Valor.Should().Be("João Silva");
    }

    [Fact]
    public void NomeProprioVoHelper_RecriarComValorInvalido_LancaInvalidOperation()
    {
        Action act = () => NomeProprioVoHelper.RecriarNome("");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TelefoneVoHelper_RecriarComValorValido_RetornaVo()
    {
        TelefoneVoHelper.RecriarTelefone("11987654321").Valor.Should().Be("11987654321");
    }

    [Fact]
    public void TelefoneVoHelper_RecriarComValorInvalido_LancaInvalidOperation()
    {
        Action act = () => TelefoneVoHelper.RecriarTelefone("");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PlacaVoHelper_RecriarComValorValido_RetornaVo()
    {
        PlacaVoHelper.RecriarPlaca("ABC1D23").Valor.Should().Be("ABC1D23");
    }

    [Fact]
    public void PlacaVoHelper_RecriarComValorInvalido_LancaInvalidOperation()
    {
        Action act = () => PlacaVoHelper.RecriarPlaca("");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DescricaoProdutoVoHelper_RecriarComValorValido_RetornaVo()
    {
        DescricaoProdutoVoHelper.RecriarDescricao("Filtro").Valor.Should().Be("Filtro");
    }

    [Fact]
    public void DescricaoProdutoVoHelper_RecriarComValorInvalido_LancaInvalidOperation()
    {
        Action act = () => DescricaoProdutoVoHelper.RecriarDescricao("");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void QuantidadeProdutoVoHelper_RecriarComValorValido_RetornaVo()
    {
        QuantidadeProdutoVoHelper.RecriarQuantidade(5).Valor.Should().Be(5);
    }

    [Fact]
    public void QuantidadeProdutoVoHelper_RecriarComValorInvalido_LancaInvalidOperation()
    {
        Action act = () => QuantidadeProdutoVoHelper.RecriarQuantidade(0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ValorProdutoVoHelper_RecriarComValorValido_RetornaVo()
    {
        ValorProdutoVoHelper.RecriarValor(99.5m).Valor.Should().Be(99.5m);
    }

    [Fact]
    public void ValorProdutoVoHelper_RecriarComValorInvalido_LancaInvalidOperation()
    {
        Action act = () => ValorProdutoVoHelper.RecriarValor(-1m);
        act.Should().Throw<InvalidOperationException>();
    }
}
