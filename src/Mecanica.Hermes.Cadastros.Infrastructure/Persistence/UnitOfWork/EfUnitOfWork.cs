using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using ConcurrencyConflictException = Mecanica.Hermes.Shared.Core.Persistence.ConcurrencyConflictException;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.UnitOfWork;

internal sealed class EfUnitOfWork(
    AppDbContext db,
    IDomainEventDispatcher domainEventDispatcher) : IUnitOfWork
{
    private readonly List<IDomainEvent> _pendingEvents = [];

    public void CollectEventsFrom(AggregateRoot aggregate)
    {
        _pendingEvents.AddRange(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken)
    {
        int rows;
        try
        {
            rows = await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(
                "Conflito de concorrência ao salvar o agregado. O registro foi modificado por outra operação.",
                ex);
        }

        if (_pendingEvents.Count == 0)
            return rows;

        var snapshot = _pendingEvents.ToArray();
        _pendingEvents.Clear();
        await domainEventDispatcher.DispatchAsync(snapshot, cancellationToken);
        return rows;
    }
}
