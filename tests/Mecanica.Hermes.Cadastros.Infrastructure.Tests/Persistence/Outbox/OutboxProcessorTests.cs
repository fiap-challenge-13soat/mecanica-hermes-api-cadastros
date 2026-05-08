using System.Text.Json;
using MassTransit;
using MassTransit.Testing;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Outbox;
using Mecanica.Hermes.Contracts.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Tests.Persistence.Outbox;

/// <summary>
///     Testes unitários do <see cref="OutboxProcessor" /> usando InMemory provider.
///     Exercita o caminho de fallback (sem FOR UPDATE SKIP LOCKED) para cobrir o
///     processamento de lotes, DLQ, caminhos de erro e dispatch via
///     <see cref="IPublishEndpoint" />.
/// </summary>
[Trait("Category", "Unit")]
public class OutboxProcessorTests
{
    private static AppDbContext NewDb()
    {
        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
    }

    private static (OutboxProcessor sut, Mock<IPublishEndpoint> publishMock, AppDbContext db) BuildSut(
        int maxRetries = 3)
    {
        var db = NewDb();

        var publishMock = new Mock<IPublishEndpoint>();

        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton(publishMock.Object);
        var sp = services.BuildServiceProvider();

        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        scope.Setup(s => s.ServiceProvider).Returns(sp);
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        var options = Options.Create(new OutboxOptions
        {
            BatchSize = 10,
            MaxRetries = maxRetries,
            PollInterval = TimeSpan.FromMilliseconds(50),
            AllowNonLockingFallback = true // InMemory provider — locking SQL não suportado
        });

        var metrics = new OutboxHealthMetrics();

        var sut = new OutboxProcessor(
            scopeFactory.Object,
            metrics,
            options,
            NullLogger<OutboxProcessor>.Instance,
            TimeProvider.System);

        return (sut, publishMock, db);
    }

    private static OutboxMessageEntity PendingMessage(string type = "Inexistent.Type, NoAssembly", string payload = "{}")
    {
        return new OutboxMessageEntity(Guid.NewGuid(), type, payload, DateTime.UtcNow);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoNaoHaMensagens_NaoChamaPublishEndpoint()
    {
        var (sut, publishMock, _) = BuildSut();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await sut.StartAsync(cts.Token);
        await Task.Delay(150);
        await sut.StopAsync(CancellationToken.None);

        publishMock.Verify(
            p => p.Publish(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessBatch_QuandoTipoNaoResolve_DeveMarcarFalha()
    {
        var (sut, _, db) = BuildSut(2);

        var msg = PendingMessage();
        db.OutboxMessages.Add(msg);
        await db.SaveChangesAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        await sut.StartAsync(cts.Token);
        await Task.Delay(200);
        await sut.StopAsync(CancellationToken.None);

        var saved = await db.OutboxMessages.FirstAsync();
        saved.RetryCount.Should().BeGreaterThan(0);
        saved.LastError.Should().NotBeNullOrEmpty();
        saved.Status.Should().NotBe(OutboxMessageStatus.Processed);
    }

    [Fact]
    public async Task ProcessBatch_QuandoTipoNaoResolveEExcedeLimite_DeveDeadLetter()
    {
        var (sut, _, db) = BuildSut(1);

        var msg = PendingMessage();
        db.OutboxMessages.Add(msg);
        await db.SaveChangesAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(400));
        await sut.StartAsync(cts.Token);
        await Task.Delay(300);
        await sut.StopAsync(CancellationToken.None);

        var saved = await db.OutboxMessages.FirstAsync();
        saved.Status.Should().Be(OutboxMessageStatus.DeadLettered);
    }

    [Fact]
    public void OutboxMessageEntity_MarkAsFailed_DeveIncrementarRetryCount()
    {
        var msg = PendingMessage();

        msg.MarkAsFailed("erro simulado", 5);

        msg.RetryCount.Should().Be(1);
        msg.LastError.Should().Be("erro simulado");
        msg.Status.Should().Be(OutboxMessageStatus.Pending);
    }

    [Fact]
    public void OutboxMessageEntity_MarkAsFailed_QuandoExcedeLimite_DeveDeadLetter()
    {
        var msg = PendingMessage();

        msg.MarkAsFailed("err", 1);

        msg.Status.Should().Be(OutboxMessageStatus.DeadLettered);
        msg.RetryCount.Should().Be(1);
    }

    [Fact]
    public void OutboxMessageEntity_MarkAsProcessed_DeveMarcarStatusProcessed()
    {
        var msg = PendingMessage();
        var when = DateTime.UtcNow;

        msg.MarkAsProcessed(when);

        msg.Status.Should().Be(OutboxMessageStatus.Processed);
        msg.ProcessedOn.Should().BeCloseTo(when, TimeSpan.FromSeconds(1));
        msg.LastError.Should().BeNull();
    }

    [Fact]
    public async Task ProcessBatch_CaminhoFeliz_DeveMarcarStatusProcessedAposDespacho()
    {
        // Usa MassTransit.Testing.ITestHarness em vez de mock — `harness.Published.Any<T>()`
        // espera deterministicamente até o evento ser publicado (com timeout claro), eliminando
        // o `Task.Delay(300)` mágico da versão anterior que era flaky sob carga.
        var evt = new OrcamentoAprovadoPeloClienteEvent(Guid.NewGuid(), DateTime.UtcNow);
        var evtType = typeof(OrcamentoAprovadoPeloClienteEvent);

        var db = NewDb();
        var msg = new OutboxMessageEntity(
            Guid.NewGuid(),
            evtType.AssemblyQualifiedName!,
            JsonSerializer.Serialize(evt, evtType),
            DateTime.UtcNow);
        db.OutboxMessages.Add(msg);
        await db.SaveChangesAsync();

        // Service provider com bus real (in-memory via TestHarness) — IPublishEndpoint resolvido
        // pelo OutboxProcessor sai daqui e cai no harness.
        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddMassTransitTestHarness();
        var provider = services.BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var options = Options.Create(new OutboxOptions
            {
                BatchSize = 10,
                MaxRetries = 3,
                PollInterval = TimeSpan.FromMilliseconds(50),
                AllowNonLockingFallback = true
            });
            using var metrics = new OutboxHealthMetrics();
            var sut = new OutboxProcessor(
                provider.GetRequiredService<IServiceScopeFactory>(),
                metrics,
                options,
                NullLogger<OutboxProcessor>.Instance,
                TimeProvider.System);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await sut.StartAsync(cts.Token);

            // Espera **deterministicamente** até o evento ser publicado (ou timeout).
            (await harness.Published.Any<OrcamentoAprovadoPeloClienteEvent>(cts.Token))
                .Should().BeTrue("OutboxProcessor deve publicar o integration event no bus");

            await sut.StopAsync(CancellationToken.None);

            // O OutboxProcessor faz SaveChanges depois do publish bem-sucedido — o status
            // já deve estar Processed quando harness viu o publish.
            var saved = await db.OutboxMessages.FirstAsync();
            saved.Status.Should().Be(OutboxMessageStatus.Processed,
                "mensagem com tipo válido e publish bem-sucedido deve ser marcada como Processed");
            saved.ProcessedOn.Should().NotBeNull();
            saved.RetryCount.Should().Be(0);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task ProcessBatch_QuandoPublishLanca_DeveMarcarFalhaEPreservarMensagem()
    {
        var evt = new OrcamentoAprovadoPeloClienteEvent(Guid.NewGuid(), DateTime.UtcNow);
        var evtType = typeof(OrcamentoAprovadoPeloClienteEvent);

        var (sut, publishMock, db) = BuildSut(maxRetries: 5);

        publishMock
            .Setup(p => p.Publish(
                It.IsAny<object>(),
                It.IsAny<Type>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("RabbitMQ down"));

        var msg = new OutboxMessageEntity(
            Guid.NewGuid(),
            evtType.AssemblyQualifiedName!,
            JsonSerializer.Serialize(evt, evtType),
            DateTime.UtcNow);
        db.OutboxMessages.Add(msg);
        await db.SaveChangesAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        await sut.StartAsync(cts.Token);
        await Task.Delay(200);
        await sut.StopAsync(CancellationToken.None);

        var saved = await db.OutboxMessages.FirstAsync();
        saved.Status.Should().NotBe(OutboxMessageStatus.Processed,
            "Publish lançou — mensagem deve permanecer em retry, não Processed");
        saved.RetryCount.Should().BeGreaterThan(0);
        saved.LastError.Should().Contain("RabbitMQ down");
    }
}
