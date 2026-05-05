using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;
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
        CancellationToken cancellationToken)
    {
        var entity = new WebhookEventEntity(scope, externalId, status, timeProvider.GetUtcNow().UtcDateTime);
        db.Set<WebhookEventEntity>().Add(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg
                                           && pg.SqlState == PostgresUniqueViolationSqlState)
        {
            // Duplicata — outro processamento já registrou esse evento.
            db.Entry(entity).State = EntityState.Detached;
            return false;
        }
    }
}
