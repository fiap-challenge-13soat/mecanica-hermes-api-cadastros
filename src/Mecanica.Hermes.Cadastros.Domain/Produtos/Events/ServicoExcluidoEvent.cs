using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;

namespace Mecanica.Hermes.Cadastros.Domain.Produtos.Events;

public sealed record ServicoExcluidoEvent(Guid ServicoId) : IDomainEvent;
