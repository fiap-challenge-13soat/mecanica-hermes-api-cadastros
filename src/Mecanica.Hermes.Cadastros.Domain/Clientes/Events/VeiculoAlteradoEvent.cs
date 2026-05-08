using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.Events;

public class VeiculoAlteradoEvent(Guid id, PlacaVo placa) : IDomainEvent
{
    public Guid Id { get; private set; } = id;
    public PlacaVo Placa { get; private set; } = placa;
}
