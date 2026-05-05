using Mecanica.Hermes.Cadastros.Application.Common.Exceptions;
using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.UnitOfWork;

internal sealed class EfUnitOfWork(
    AppDbContext db,
    IPublisher publisher) : IUnitOfWork
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

        if (_pendingEvents.Count > 0)
        {
            var snapshot = _pendingEvents.ToArray();
            _pendingEvents.Clear();
            foreach (var evt in snapshot)
                await publisher.Publish(evt, cancellationToken);
        }

        return rows;
    }
}
