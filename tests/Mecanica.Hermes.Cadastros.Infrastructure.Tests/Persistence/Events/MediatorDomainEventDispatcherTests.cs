using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Events;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Tests.Persistence.Events;

public class MediatorDomainEventDispatcherTests
{
    private sealed record TestDomainEvent(int Id) : IDomainEvent;

    [Fact]
    public async Task DispatchAsync_PublicaCadaEvento()
    {
        var publisher = new Mock<IPublisher>();
        var dispatcher = new MediatorDomainEventDispatcher(publisher.Object);
        var events = new IDomainEvent[] { new TestDomainEvent(1), new TestDomainEvent(2) };

        await dispatcher.DispatchAsync(events, CancellationToken.None);

        publisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DispatchAsync_ListaVazia_NaoChamaPublisher()
    {
        var publisher = new Mock<IPublisher>();
        var dispatcher = new MediatorDomainEventDispatcher(publisher.Object);

        await dispatcher.DispatchAsync(Array.Empty<IDomainEvent>(), CancellationToken.None);

        publisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
