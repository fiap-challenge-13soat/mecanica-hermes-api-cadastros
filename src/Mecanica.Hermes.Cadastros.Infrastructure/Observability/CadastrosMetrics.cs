using System.Diagnostics.Metrics;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Observability;

/// <summary>
///     Counters de domínio expostos via OpenTelemetry. O <see cref="Meter" /> é
///     registrado em <c>ObservabilityConfiguration.AddObservability</c> via
///     <c>metrics.AddMeter(MeterName)</c>.
/// </summary>
internal sealed class CadastrosMetrics : IDisposable
{
    public const string MeterName = "Mecanica.Hermes.Cadastros";

    private readonly Meter _meter;

    public CadastrosMetrics()
    {
        _meter = new Meter(MeterName, "1.0.0");

        ClienteCriado = _meter.CreateCounter<long>("cadastros.cliente.criado");
        ClienteAlterado = _meter.CreateCounter<long>("cadastros.cliente.alterado");
        ClienteExcluido = _meter.CreateCounter<long>("cadastros.cliente.excluido");
        VeiculoAdicionado = _meter.CreateCounter<long>("cadastros.veiculo.adicionado");
        VeiculoAlterado = _meter.CreateCounter<long>("cadastros.veiculo.alterado");
        VeiculoRemovido = _meter.CreateCounter<long>("cadastros.veiculo.removido");

        ProdutoCriado = _meter.CreateCounter<long>("cadastros.produto.criado");
        ProdutoAlterado = _meter.CreateCounter<long>("cadastros.produto.alterado");
        ProdutoExcluido = _meter.CreateCounter<long>("cadastros.produto.excluido");
        EstoqueAdicionado = _meter.CreateCounter<long>("cadastros.produto.estoque.adicionado");
        EstoqueRemovido = _meter.CreateCounter<long>("cadastros.produto.estoque.removido");
        EstoqueIndisponivel = _meter.CreateCounter<long>("cadastros.produto.estoque.indisponivel");

        ServicoCriado = _meter.CreateCounter<long>("cadastros.servico.criado");
        ServicoAlterado = _meter.CreateCounter<long>("cadastros.servico.alterado");
        ServicoExcluido = _meter.CreateCounter<long>("cadastros.servico.excluido");
    }

    public Counter<long> ClienteCriado { get; }
    public Counter<long> ClienteAlterado { get; }
    public Counter<long> ClienteExcluido { get; }
    public Counter<long> VeiculoAdicionado { get; }
    public Counter<long> VeiculoAlterado { get; }
    public Counter<long> VeiculoRemovido { get; }

    public Counter<long> ProdutoCriado { get; }
    public Counter<long> ProdutoAlterado { get; }
    public Counter<long> ProdutoExcluido { get; }
    public Counter<long> EstoqueAdicionado { get; }
    public Counter<long> EstoqueRemovido { get; }
    public Counter<long> EstoqueIndisponivel { get; }

    public Counter<long> ServicoCriado { get; }
    public Counter<long> ServicoAlterado { get; }
    public Counter<long> ServicoExcluido { get; }

    public void Dispose() => _meter.Dispose();
}
