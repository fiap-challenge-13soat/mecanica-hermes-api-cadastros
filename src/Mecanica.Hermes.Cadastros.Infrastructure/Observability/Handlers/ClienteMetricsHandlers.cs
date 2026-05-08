using System.Diagnostics.CodeAnalysis;
using Mecanica.Hermes.Cadastros.Domain.Clientes.Events;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Observability.Handlers;

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class ClienteCriadoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<ClienteCriadoEvent>
{
    public Task Handle(ClienteCriadoEvent notification, CancellationToken cancellationToken)
    {
        metrics.ClienteCriado.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class ClienteAlteradoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<ClienteAlteradoEvent>
{
    public Task Handle(ClienteAlteradoEvent notification, CancellationToken cancellationToken)
    {
        metrics.ClienteAlterado.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class ClienteExcluidoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<ClienteExcluidoEvent>
{
    public Task Handle(ClienteExcluidoEvent notification, CancellationToken cancellationToken)
    {
        metrics.ClienteExcluido.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class VeiculoAdicionadoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<VeiculoAdicionadoEvent>
{
    public Task Handle(VeiculoAdicionadoEvent notification, CancellationToken cancellationToken)
    {
        metrics.VeiculoAdicionado.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class VeiculoAlteradoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<VeiculoAlteradoEvent>
{
    public Task Handle(VeiculoAlteradoEvent notification, CancellationToken cancellationToken)
    {
        metrics.VeiculoAlterado.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class VeiculoRemovidoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<VeiculoRemovidoEvent>
{
    public Task Handle(VeiculoRemovidoEvent notification, CancellationToken cancellationToken)
    {
        metrics.VeiculoRemovido.Add(1);
        return Task.CompletedTask;
    }
}
