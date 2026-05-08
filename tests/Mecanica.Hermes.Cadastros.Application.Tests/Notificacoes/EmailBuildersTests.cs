using Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Tests.Common;
using Mecanica.Hermes.Cadastros.Domain.Produtos;

namespace Mecanica.Hermes.Cadastros.Application.Tests.Notificacoes;

public class EmailBuildersTests
{
    [Fact]
    public void OrcamentoEmailBuilder_GeraHtmlComCamposChave()
    {
        var ordem = new OrdemDeServicoExternalDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null,
            "AguardandoAprovacao", 250m,
            [new OrdemDeServicoProdutoExternalDto(Guid.NewGuid(), 100m, 2)],
            [new OrdemDeServicoServicoExternalDto(Guid.NewGuid(), 50m)]);
        var cliente = TestData.NovoCliente();
        var produto = TestData.NovoProduto();
        var servico = TestData.NovoServico();

        var produtos = new Dictionary<Guid, Produto> { [ordem.Produtos[0].ProdutoId] = produto };
        var servicos = new Dictionary<Guid, Servico> { [ordem.Servicos[0].ServicoId] = servico };

        var html = OrcamentoEmailBuilder.BuildBody(
            ordem, cliente, produtos, servicos,
            "https://app/aprovar", "https://app/rejeitar");

        html.Should().Contain("Aprovar Orçamento");
        html.Should().Contain("Rejeitar Orçamento");
        html.Should().Contain(produto.Descricao.Valor);
        html.Should().Contain(servico.Descricao.Valor);
        html.Should().Contain("https://app/aprovar");
        html.Should().Contain("https://app/rejeitar");
    }

    [Fact]
    public void OrcamentoEmailBuilder_SemProdutosOuServicos_NaoFalha()
    {
        var ordem = new OrdemDeServicoExternalDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null,
            "AguardandoAprovacao", 0m, [], []);
        var cliente = TestData.NovoCliente();

        var html = OrcamentoEmailBuilder.BuildBody(ordem, cliente,
            new Dictionary<Guid, Produto>(), new Dictionary<Guid, Servico>(),
            "https://a", "https://b");

        html.Should().Contain("Aprovar");
    }

    [Fact]
    public void OrcamentoEmailBuilder_ProdutosNaoEncontradosLocal_UsaIdComoFallback()
    {
        var produtoId = Guid.NewGuid();
        var ordem = new OrdemDeServicoExternalDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null,
            "AguardandoAprovacao", 100m,
            [new OrdemDeServicoProdutoExternalDto(produtoId, 100m, 1)],
            []);

        var html = OrcamentoEmailBuilder.BuildBody(
            ordem, TestData.NovoCliente(),
            new Dictionary<Guid, Produto>(), new Dictionary<Guid, Servico>(),
            "https://a", "https://b");

        html.Should().Contain(produtoId.ToString("D"));
    }

    [Fact]
    public void OrcamentoEmailBuilder_BuildSubject_RetornaAssunto()
    {
        OrcamentoEmailBuilder.BuildSubject().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LinkPagamentoEmailBuilder_GeraHtmlComLink()
    {
        var pagamento = new PagamentoExternalDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "cliente@x.com", "Cliente Teste",
            150m, "BRL", "AguardandoPagamento",
            new PagamentoLinkExternalDto("pref", "https://mp/checkout"),
            DateTime.UtcNow.AddDays(1));

        var html = LinkPagamentoEmailBuilder.BuildBody(pagamento);

        html.Should().Contain("Pagar agora");
        html.Should().Contain("https://mp/checkout");
        html.Should().Contain("Cliente Teste");
    }

    [Fact]
    public void LinkPagamentoEmailBuilder_SemVencimento_NaoExibeValidade()
    {
        var pagamento = new PagamentoExternalDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "cliente@x.com", "Cliente Teste", 150m, "BRL", "AguardandoPagamento",
            new PagamentoLinkExternalDto("pref", "https://mp/x"), null);

        var html = LinkPagamentoEmailBuilder.BuildBody(pagamento);

        html.Should().NotContain("Validade");
    }

    [Fact]
    public void LinkPagamentoEmailBuilder_SemLink_LancaExcecao()
    {
        var pagamento = new PagamentoExternalDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "x", "y", 1m, "BRL", "X", null, null);

        Action act = () => LinkPagamentoEmailBuilder.BuildBody(pagamento);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LinkPagamentoEmailBuilder_BuildSubject_RetornaAssunto()
    {
        LinkPagamentoEmailBuilder.BuildSubject().Should().NotBeNullOrEmpty();
    }
}
