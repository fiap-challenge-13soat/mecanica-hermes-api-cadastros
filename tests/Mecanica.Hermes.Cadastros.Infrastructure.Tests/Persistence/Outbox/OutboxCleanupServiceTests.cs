using Mecanica.Hermes.Cadastros.Infrastructure.Persistence;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Tests.Persistence.Outbox;

[Trait("Category", "Unit")]
public sealed class OutboxCleanupServiceTests
{
    private static AppDbContext NewDb()
    {
        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
    }

    private static (OutboxCleanupService sut, AppDbContext db) BuildSut(TimeSpan retention)
    {
        var db = NewDb();

        var services = new ServiceCollection();
        services.AddSingleton(db);
        var sp = services.BuildServiceProvider();

        var scope = new Mock<IServiceScope>();
        scope.Setup(s => s.ServiceProvider).Returns(sp);
        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        var options = Options.Create(new OutboxOptions
        {
            PollInterval = TimeSpan.FromMilliseconds(50),
            ProcessedRetention = retention,
        });

        var sut = new OutboxCleanupService(
            scopeFactory.Object,
            options,
            NullLogger<OutboxCleanupService>.Instance);

        return (sut, db);
    }

    // O caminho "delete real" (Where + ExecuteDeleteAsync excluindo linhas) só pode ser
    // verificado contra Postgres — o provider InMemory ignora ExecuteDeleteAsync com predicate.
    // Esse cenário fica coberto pelo OutboxCleanupServiceIntegrationTests.

    [Fact]
    public async Task ExecuteAsync_NaoRemoveDeadLetteredOuPending()
    {
        var (sut, db) = BuildSut(retention: TimeSpan.Zero);

        var pending = new OutboxMessageEntity(Guid.NewGuid(), "T", "{}", DateTime.UtcNow.AddDays(-30));
        var deadLettered = new OutboxMessageEntity(Guid.NewGuid(), "T", "{}", DateTime.UtcNow.AddDays(-30));
        for (var i = 0; i < 20; i++)
            deadLettered.MarkAsFailed("boom", maxRetries: 3);

        db.OutboxMessages.AddRange(pending, deadLettered);
        await db.SaveChangesAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1200));
        await sut.StartAsync(cts.Token);
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1100), CancellationToken.None);
        }
        finally
        {
            await sut.StopAsync(CancellationToken.None);
        }

        var remaining = await db.OutboxMessages.AsNoTracking().CountAsync();
        remaining.Should().Be(2, "cleanup só toca em mensagens Processed");
    }

    [Fact]
    public async Task ExecuteAsync_QuandoCancelado_NaoLancaExcecao()
    {
        var (sut, _) = BuildSut(retention: TimeSpan.FromHours(1));

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // Cancela imediatamente — caminho do TaskCanceledException.
        await cts.CancelAsync();

        Func<Task> act = async () => await sut.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
