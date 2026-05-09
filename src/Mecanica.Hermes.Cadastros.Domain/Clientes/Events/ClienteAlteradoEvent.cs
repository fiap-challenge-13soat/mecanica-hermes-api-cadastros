using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.Events;

public class ClienteAlteradoEvent(Guid id) : IDomainEvent
{
    public Guid Id { get; private set; } = id;
}
