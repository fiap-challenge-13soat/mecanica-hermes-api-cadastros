using System.Diagnostics;
using System.Text.Json;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Repositories;

internal sealed class WebhookEventRepository(AppDbContext db, TimeProvider timeProvider) : IWebhookEventRepository
{
    private const string PostgresUniqueViolationSqlState = "23505";

    public async Task<bool> TryRegisterAsync(
        string scope,
        string externalId,
        string status,
        object integrationEvent,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var webhookEntity = new WebhookEventEntity(scope, externalId, status, now);
        db.Set<WebhookEventEntity>().Add(webhookEntity);

        // Outbox transacional: o integration event entra na MESMA transação do
        // SaveChanges abaixo. Se a constraint UNIQUE de WebhookEvents disparar
        // (duplicata), o rollback descarta também o OutboxMessage — nenhum evento é
        // publicado em duplicata. Se commit funcionar, o OutboxProcessor garante a
        // publicação no broker mesmo após restart/failover.
        var eventType = integrationEvent.GetType();
        var outboxMessage = new OutboxMessageEntity(
            Guid.NewGuid(),
            eventType.AssemblyQualifiedName ?? eventType.FullName!,
            JsonSerializer.Serialize(integrationEvent, eventType),
            now,
            Activity.Current?.Id);
        db.Set<OutboxMessageEntity>().Add(outboxMessage);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg
                                           && pg.SqlState == PostgresUniqueViolationSqlState)
        {
            // Duplicata — outro processamento já registrou esse evento e enfileirou o integration
            // event. Detacha ambas as entidades para não vazarem no ChangeTracker.
            db.Entry(webhookEntity).State = EntityState.Detached;
            db.Entry(outboxMessage).State = EntityState.Detached;
            return false;
        }
    }
}
