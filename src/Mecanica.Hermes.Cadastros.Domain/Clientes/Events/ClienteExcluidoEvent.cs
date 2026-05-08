using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.Events;

public sealed record ClienteExcluidoEvent(Guid ClienteId) : IDomainEvent;
