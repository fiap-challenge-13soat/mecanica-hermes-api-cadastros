using Mecanica.Hermes.Cadastros.Infrastructure.Observability;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Tests.Observability;

public class CadastrosMetricsTests
{
    [Fact]
    public void Construtor_InicializaTodosCounters()
    {
        using var metrics = new CadastrosMetrics();

        metrics.ClienteCriado.Should().NotBeNull();
        metrics.ClienteAlterado.Should().NotBeNull();
        metrics.ClienteExcluido.Should().NotBeNull();
        metrics.VeiculoAdicionado.Should().NotBeNull();
        metrics.VeiculoAlterado.Should().NotBeNull();
        metrics.VeiculoRemovido.Should().NotBeNull();
        metrics.ProdutoCriado.Should().NotBeNull();
        metrics.ProdutoAlterado.Should().NotBeNull();
        metrics.ProdutoExcluido.Should().NotBeNull();
        metrics.EstoqueAdicionado.Should().NotBeNull();
        metrics.EstoqueRemovido.Should().NotBeNull();
        metrics.EstoqueIndisponivel.Should().NotBeNull();
        metrics.ServicoCriado.Should().NotBeNull();
        metrics.ServicoAlterado.Should().NotBeNull();
        metrics.ServicoExcluido.Should().NotBeNull();
    }

    [Fact]
    public void IncrementaCounter_SemErro()
    {
        using var metrics = new CadastrosMetrics();

        Action act = () => metrics.ClienteCriado.Add(1);

        act.Should().NotThrow();
    }
}
