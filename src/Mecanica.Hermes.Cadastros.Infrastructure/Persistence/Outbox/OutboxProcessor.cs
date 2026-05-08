#pragma warning disable CA1873 // Log arguments are simple property accesses — not expensive
using System.Diagnostics;
using System.Text.Json;
using MassTransit;
using Mecanica.Hermes.Shared.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Outbox;

/// <summary>
///     Background service que processa mensagens pendentes da tabela <c>OutboxMessages</c>
///     e despacha via <c>IPublishEndpoint</c> (MassTransit).
///     <list type="bullet">
///         <item>
///             <b>Concorrência:</b> usa <c>FOR UPDATE SKIP LOCKED</c> (PostgreSQL) para que múltiplas
///             réplicas do serviço não processem o mesmo lote simultaneamente.
///         </item>
///         <item>
///             <b>DLQ:</b> após <see cref="OutboxOptions.MaxRetries" /> falhas consecutivas, a mensagem
///             é marcada como <see cref="OutboxMessageStatus.DeadLettered" /> e não reprocessada.
///         </item>
///         <item>
///             <b>Tracing:</b> cada ciclo abre um <see cref="Activity" /> filho propagando o
///             <c>TraceContext</c> original do webhook que gerou o evento.
///         </item>
///         <item>
///             <b>Métricas:</b> emite <c>outbox.messages.dispatched</c>, <c>outbox.messages.failed</c>
///             e <c>outbox.messages.dead_lettered</c> via <see cref="OutboxHealthMetrics" />.
///         </item>
///     </list>
/// </summary>
internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    OutboxHealthMetrics metrics,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger,
    TimeProvider timeProvider) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxProcessor iniciado. Polling a cada {Interval}.", _options.PollInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Falha ao processar lote do Outbox. Próxima tentativa em {Interval}.",
                    _options.PollInterval);
            }

            try
            {
                await Task.Delay(_options.PollInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                /* shutdown */
            }
        }

        logger.LogInformation("OutboxProcessor parado.");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // SELECT … FOR UPDATE SKIP LOCKED garante que instâncias concorrentes não peguem o
        // mesmo lote. O SKIP LOCKED é suportado nativamente no PostgreSQL 9.5+.
        // Em InMemory (testes) o FromSqlRaw não é suportado — retorna 0 e o path de fallback
        // é exercitado nos testes unitários.
        List<OutboxMessageEntity> pending;
        try
        {
            pending = await db.OutboxMessages
                .FromSqlRaw(
                    """
                    SELECT * FROM "OutboxMessages"
                    WHERE "Status" = 0
                    ORDER BY "OccurredOn"
                    LIMIT {0}
                    FOR UPDATE SKIP LOCKED
                    """,
                    _options.BatchSize)
                .ToListAsync(ct);
        }
        catch (InvalidOperationException)
        {
            // Fallback para providers que não suportam FromSqlRaw (ex: InMemory em testes).
            // ATENÇÃO: sem FOR UPDATE SKIP LOCKED, múltiplas instâncias podem processar o mesmo lote.
            // Use OUTBOX__ALLOW_NON_LOCKING_FALLBACK=true apenas em ambientes de teste.
            if (!_options.AllowNonLockingFallback)
                logger.LogCritical(
                    "OutboxProcessor: o provider de banco não suporta FOR UPDATE SKIP LOCKED. " +
                    "O fallback LINQ está sendo usado sem AllowNonLockingFallback=true. " +
                    "Em múltiplas instâncias, isso pode causar double-delivery de eventos. " +
                    "Defina OUTBOX__ALLOW_NON_LOCKING_FALLBACK=true para suprimir este aviso em testes.");

            pending = await db.OutboxMessages
                .Where(m => m.Status == OutboxMessageStatus.Pending)
                .OrderBy(m => m.OccurredOn)
                .Take(_options.BatchSize)
                .ToListAsync(ct);
        }

        if (pending.Count == 0) return;

        foreach (var message in pending)
        {
            using var activity = ActivitySources.Api.StartActivity(
                $"outbox.dispatch/{message.Type}",
                ActivityKind.Internal,
                message.TraceContext ?? string.Empty);

            activity?.SetTag("outbox.message_id", message.Id.ToString());
            activity?.SetTag("outbox.event_type", message.Type);
            activity?.SetTag("outbox.retry_count", message.RetryCount);

            try
            {
                var evtType = ResolveEventType(message.Type);
                if (evtType is null)
                {
                    var err = $"Tipo '{message.Type}' não pôde ser resolvido.";
                    logger.LogWarning("OutboxMessage {Id}: {Error}", message.Id, err);
                    message.MarkAsFailed(err, _options.MaxRetries);
                    RecordFailure(message, activity);
                    continue;
                }

                var integrationEvent = JsonSerializer.Deserialize(message.Payload, evtType);
                if (integrationEvent is null)
                {
                    const string err = "Payload deserializado é null.";
                    logger.LogWarning("OutboxMessage {Id}: {Error}", message.Id, err);
                    message.MarkAsFailed(err, _options.MaxRetries);
                    RecordFailure(message, activity);
                    continue;
                }

                await publishEndpoint.Publish(integrationEvent, evtType, ct);
                message.MarkAsProcessed(_timeProvider.GetUtcNow().UtcDateTime);
                metrics.MessageDispatched();
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao despachar OutboxMessage {Id} ({Type}). Retry={Retry}",
                    message.Id, message.Type, message.RetryCount + 1);
                message.MarkAsFailed(ex.Message, _options.MaxRetries);
                RecordFailure(message, activity);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private void RecordFailure(OutboxMessageEntity message, Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Error, message.LastError);
        if (message.Status == OutboxMessageStatus.DeadLettered)
        {
            metrics.MessageDeadLettered();
            logger.LogCritical(
                "OutboxMessage {Id} ({Type}) atingiu o limite de {MaxRetries} tentativas e foi movida " +
                "para Dead Letter.",
                message.Id, message.Type, _options.MaxRetries);
        }
        else
        {
            metrics.MessageFailed();
        }
    }

    /// <summary>
    ///     Resolve o <see cref="System.Type" /> concreto a partir do discriminador persistido.
    ///     O Type é o <c>AssemblyQualifiedName</c> do record do integration event (gravado
    ///     pelo <c>WebhookEventRepository</c> no momento do enqueue).
    /// </summary>
    private static Type? ResolveEventType(string typeName)
    {
        return Type.GetType(typeName, false);
    }
}
