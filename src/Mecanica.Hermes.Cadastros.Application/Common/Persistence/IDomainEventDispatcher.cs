using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;

namespace Mecanica.Hermes.Cadastros.Application.Common.Persistence;

/// <summary>
///     Despacha Domain Events coletados pelo <see cref="IUnitOfWork" /> para os handlers
///     in-process registrados (ex.: incremento de métricas OTel). Implementação fica em
///     Infrastructure para que Application permaneça livre da dependência ao MediatR.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
}
