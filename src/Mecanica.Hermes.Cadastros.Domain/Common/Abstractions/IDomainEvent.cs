using MediatR;
using SdkDomainEvent = Mecanica.Hermes.Shared.Core.Domain.IDomainEvent;

namespace Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;

/// <summary>
///     Marker para domain events do Cadastros.
///     <para>
///         Estende <c>Mecanica.Hermes.Shared.Core.Domain.IDomainEvent</c> (do SDK) para que o
///         agregado e o UnitOfWork sejam compatíveis com a hierarquia de domínio compartilhada,
///         e <c>MediatR.INotification</c> para que o <c>MediatorDomainEventDispatcher</c>
///         continue despachando in-process via <c>IPublisher.Publish</c>.
///     </para>
/// </summary>
public interface IDomainEvent : SdkDomainEvent, INotification
{
}
