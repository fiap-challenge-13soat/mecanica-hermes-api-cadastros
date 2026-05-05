using System.Diagnostics.CodeAnalysis;
using Mecanica.Hermes.Cadastros.Domain.Produtos.Events;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Observability.Handlers;

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class ProdutoCriadoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<ProdutoCriadoEvent>
{
    public Task Handle(ProdutoCriadoEvent notification, CancellationToken cancellationToken)
    {
        metrics.ProdutoCriado.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class ProdutoAlteradoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<ProdutoAlteradoEvent>
{
    public Task Handle(ProdutoAlteradoEvent notification, CancellationToken cancellationToken)
    {
        metrics.ProdutoAlterado.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class ProdutoExcluidoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<ProdutoExcluidoEvent>
{
    public Task Handle(ProdutoExcluidoEvent notification, CancellationToken cancellationToken)
    {
        metrics.ProdutoExcluido.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class EstoqueAdicionadoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<EstoqueAdicionadoEvent>
{
    public Task Handle(EstoqueAdicionadoEvent notification, CancellationToken cancellationToken)
    {
        metrics.EstoqueAdicionado.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class EstoqueRemovidoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<EstoqueRemovidoEvent>
{
    public Task Handle(EstoqueRemovidoEvent notification, CancellationToken cancellationToken)
    {
        metrics.EstoqueRemovido.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class EstoqueIndisponivelMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<EstoqueIndisponivelEvent>
{
    public Task Handle(EstoqueIndisponivelEvent notification, CancellationToken cancellationToken)
    {
        metrics.EstoqueIndisponivel.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class ServicoCriadoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<ServicoCriadoEvent>
{
    public Task Handle(ServicoCriadoEvent notification, CancellationToken cancellationToken)
    {
        metrics.ServicoCriado.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class ServicoAlteradoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<ServicoAlteradoEvent>
{
    public Task Handle(ServicoAlteradoEvent notification, CancellationToken cancellationToken)
    {
        metrics.ServicoAlterado.Add(1);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage(Justification = "Handler de métricas — efeito observado via OTel.")]
internal sealed class ServicoExcluidoMetricsHandler(CadastrosMetrics metrics)
    : INotificationHandler<ServicoExcluidoEvent>
{
    public Task Handle(ServicoExcluidoEvent notification, CancellationToken cancellationToken)
    {
        metrics.ServicoExcluido.Add(1);
        return Task.CompletedTask;
    }
}
