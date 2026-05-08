using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;

namespace Mecanica.Hermes.Cadastros.Application.Common.Persistence;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken);
    void CollectEventsFrom(AggregateRoot aggregate);
}
