using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Events;

/// <summary>
///     Implementação default do <see cref="IDomainEventDispatcher" /> baseada em MediatR.
///     <para>
///         Despacha cada evento via <c>Publish((object)evt)</c> — o cast para <c>object</c>
///         é essencial para que o MediatR resolva os handlers pelo tipo runtime do evento
///         (ex.: <c>ClienteCriadoEvent</c>) e não pela interface <c>IDomainEvent</c>.
///     </para>
/// </summary>
internal sealed class MediatorDomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken)
    {
        foreach (var evt in events)
            await publisher.Publish(evt, cancellationToken);
    }
}
