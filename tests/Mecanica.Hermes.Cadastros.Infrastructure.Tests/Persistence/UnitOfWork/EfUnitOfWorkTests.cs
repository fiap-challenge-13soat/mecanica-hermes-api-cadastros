using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Update;
using ConcurrencyConflictException = Mecanica.Hermes.Shared.Core.Persistence.ConcurrencyConflictException;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Tests.Persistence.UnitOfWork;

[Trait("Category", "Unit")]
public sealed class EfUnitOfWorkTests
{
    private static AppDbContext NewDb()
    {
        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
    }

    private sealed class ConcurrencyThrowingInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default) =>
            throw new DbUpdateConcurrencyException("conflict", new List<IUpdateEntry>());
    }

    private static AppDbContext NewDbThatThrowsOnSave()
    {
        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(new ConcurrencyThrowingInterceptor())
                .Options);
    }

    [Fact]
    public async Task CommitAsync_SemEventosColetados_NaoChamaDispatcher()
    {
        var db = NewDb();
        var dispatcher = new Mock<IDomainEventDispatcher>();
        var sut = new EfUnitOfWork(db, dispatcher.Object);

        var rows = await sut.CommitAsync(CancellationToken.None);

        rows.Should().Be(0);
        dispatcher.Verify(
            d => d.DispatchAsync(It.IsAny<IDomainEvent[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CommitAsync_ComEventosColetados_DespachaELimpaPending()
    {
        var db = NewDb();
        var dispatcher = new Mock<IDomainEventDispatcher>();
        var sut = new EfUnitOfWork(db, dispatcher.Object);

        var clienteResult = Cliente.Criar("Maria Silva", null, "52998224725", "m@x.com", "11999998888");
        clienteResult.IsSuccess.Should().BeTrue("setup do teste exige Cliente válido");
        sut.CollectEventsFrom(clienteResult.Data!);

        await sut.CommitAsync(CancellationToken.None);

        dispatcher.Verify(
            d => d.DispatchAsync(
                It.Is<IDomainEvent[]>(events => events.Length >= 1),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Segunda chamada não deve redespachar
        await sut.CommitAsync(CancellationToken.None);
        dispatcher.Verify(
            d => d.DispatchAsync(It.IsAny<IDomainEvent[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CommitAsync_QuandoSaveChangesLancaConcurrencyException_TraduzParaConcurrencyConflictException()
    {
        var db = NewDbThatThrowsOnSave();
        var sut = new EfUnitOfWork(db, Mock.Of<IDomainEventDispatcher>());

        Func<Task> act = async () => await sut.CommitAsync(CancellationToken.None);

        await act.Should()
            .ThrowAsync<ConcurrencyConflictException>()
            .Where(ex => ex.InnerException is DbUpdateConcurrencyException);
    }
}
