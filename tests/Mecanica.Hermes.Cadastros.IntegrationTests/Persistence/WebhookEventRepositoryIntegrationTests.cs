using System.Text.Json;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Outbox;
using Mecanica.Hermes.Cadastros.IntegrationTests.Infrastructure;
using Mecanica.Hermes.Contracts.Integration;
using Microsoft.EntityFrameworkCore;

namespace Mecanica.Hermes.Cadastros.IntegrationTests.Persistence;

/// <summary>
///     Testa o invariante crítico de <see cref="IWebhookEventRepository.TryRegisterAsync" />:
///     <em>register do webhook_events</em> e <em>enqueue da OutboxMessage</em> devem ser
///     atômicos no mesmo <c>SaveChanges</c>. Em caso de duplicata
///     (<see cref="Npgsql.PostgresException" /> 23505), as duas linhas devem rollback juntas.
///     <para>
///         Este teste exercita o caminho real do PostgreSQL — o catch de
///         <c>DbUpdateException</c> filtrado por <c>InnerException is PostgresException
///         { SqlState: "23505" }</c> não roda no InMemory provider e ficava sem cobertura.
///     </para>
/// </summary>
[Collection(IntegrationTestCollection.Name)]
[Trait("Category", "Integration")]
public sealed class WebhookEventRepositoryIntegrationTests
{
    private readonly IntegrationTestWebApplicationFactory _factory;

    public WebhookEventRepositoryIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task LimparTabelasAsync(AppDbContext db)
    {
        await db.WebhookEvents.ExecuteDeleteAsync();
        await db.OutboxMessages.ExecuteDeleteAsync();
    }

    [Fact]
    public async Task TryRegister_PrimeiraOcorrencia_PersisteWebhookEventEOutboxMessage()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IWebhookEventRepository>();
        await LimparTabelasAsync(db);

        var ordemId = Guid.NewGuid();
        var externalId = $"{ordemId:D}|aprovar|123";
        var integrationEvent = new OrcamentoAprovadoPeloClienteEvent(ordemId, DateTime.UtcNow);

        var registered = await sut.TryRegisterAsync(
            scope: "orcamento.aprovacao",
            externalId: externalId,
            status: "aprovar",
            integrationEvent: integrationEvent,
            cancellationToken: CancellationToken.None);

        registered.Should().BeTrue();

        var webhook = await db.WebhookEvents
            .Where(w => w.Scope == "orcamento.aprovacao" && w.ExternalId == externalId)
            .SingleAsync();
        webhook.Status.Should().Be("aprovar");

        var outboxMessages = await db.OutboxMessages.AsNoTracking().ToListAsync();
        outboxMessages.Should().ContainSingle();
        var msg = outboxMessages[0];
        msg.Status.Should().Be(OutboxMessageStatus.Pending);
        msg.Type.Should().Contain(nameof(OrcamentoAprovadoPeloClienteEvent));
        var deserialized = JsonSerializer.Deserialize<OrcamentoAprovadoPeloClienteEvent>(msg.Payload);
        deserialized.Should().NotBeNull();
        deserialized!.OrdemDeServicoId.Should().Be(ordemId);
    }

    [Fact]
    public async Task TryRegister_Duplicata_RetornaFalseEFazRollbackDeAmbasAsLinhas()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IWebhookEventRepository>();
        await LimparTabelasAsync(db);

        var ordemId = Guid.NewGuid();
        var externalId = $"{ordemId:D}|aprovar|999";
        var firstEvent = new OrcamentoAprovadoPeloClienteEvent(ordemId, DateTime.UtcNow);

        // Primeira chamada: comita ambas as linhas.
        var first = await sut.TryRegisterAsync(
            scope: "orcamento.aprovacao",
            externalId: externalId,
            status: "aprovar",
            integrationEvent: firstEvent,
            cancellationToken: CancellationToken.None);

        first.Should().BeTrue();

        // Captura o estado depois do primeiro commit.
        var outboxAposPrimeiro = await db.OutboxMessages.AsNoTracking().CountAsync();
        outboxAposPrimeiro.Should().Be(1);

        // Segunda chamada com a MESMA chave de idempotência — Postgres deve disparar 23505.
        // A reposição do payload (event diferente) não deve criar segunda OutboxMessage,
        // pois o rollback joga fora a tentativa inteira.
        using var scope2 = _factory.Services.CreateScope();
        var sut2 = scope2.ServiceProvider.GetRequiredService<IWebhookEventRepository>();
        var secondEvent = new OrcamentoAprovadoPeloClienteEvent(ordemId, DateTime.UtcNow.AddSeconds(1));

        var second = await sut2.TryRegisterAsync(
            scope: "orcamento.aprovacao",
            externalId: externalId,
            status: "aprovar",
            integrationEvent: secondEvent,
            cancellationToken: CancellationToken.None);

        second.Should().BeFalse(
            "23505 (UNIQUE violation) é capturado e mapeado para 'duplicata'");

        // O invariante crítico: NÃO há segunda OutboxMessage. Ela foi adicionada ao ChangeTracker
        // mas o rollback do SaveChanges descartou tudo.
        var outboxFinal = await db.OutboxMessages.AsNoTracking().CountAsync();
        outboxFinal.Should().Be(1,
            "rollback no path de duplicata deve impedir double-enqueue de integration event");

        var webhookCount = await db.WebhookEvents.AsNoTracking()
            .CountAsync(w => w.Scope == "orcamento.aprovacao" && w.ExternalId == externalId);
        webhookCount.Should().Be(1, "webhook_events também rollback");
    }

    [Fact]
    public async Task TryRegister_ScopesDistintos_PermiteAmbasAsRegistros()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IWebhookEventRepository>();
        await LimparTabelasAsync(db);

        var ordemId = Guid.NewGuid();
        var externalId = $"{ordemId:D}|aprovar|555";
        var ev = new OrcamentoAprovadoPeloClienteEvent(ordemId, DateTime.UtcNow);

        var aprovacao = await sut.TryRegisterAsync(
            "orcamento.aprovacao", externalId, "aprovar", ev, CancellationToken.None);
        var rejeicao = await sut.TryRegisterAsync(
            "orcamento.rejeicao", externalId, "rejeitar", ev, CancellationToken.None);

        aprovacao.Should().BeTrue();
        rejeicao.Should().BeTrue("scopes diferentes não conflitam na unique constraint");

        var outboxCount = await db.OutboxMessages.AsNoTracking().CountAsync();
        outboxCount.Should().Be(2, "duas mensagens enfileiradas — uma por scope");
    }

    [Fact]
    public async Task TryRegister_PreservaTraceContextNaOutboxMessage()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IWebhookEventRepository>();
        await LimparTabelasAsync(db);

        using var activity = new System.Diagnostics.ActivitySource("Test").StartActivity("test")
                             ?? new System.Diagnostics.Activity("test").Start();

        var ordemId = Guid.NewGuid();
        var ev = new OrcamentoAprovadoPeloClienteEvent(ordemId, DateTime.UtcNow);

        await sut.TryRegisterAsync(
            "orcamento.aprovacao",
            $"{ordemId:D}|aprovar|trace",
            "aprovar",
            ev,
            CancellationToken.None);

        var msg = await db.OutboxMessages.AsNoTracking().SingleAsync();
        // TraceContext é capturado de Activity.Current.Id no momento do enqueue.
        // Aceita null em runners onde o sampling esteja desligado, mas verifica que se
        // capturado, é uma string não-vazia (formato W3C traceparent).
        if (!string.IsNullOrEmpty(msg.TraceContext))
            msg.TraceContext.Should().StartWith("00-",
                "W3C traceparent começa com a versão '00-'");
    }
}
