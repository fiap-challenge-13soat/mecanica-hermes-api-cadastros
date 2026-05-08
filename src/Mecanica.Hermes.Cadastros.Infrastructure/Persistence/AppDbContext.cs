using System.Reflection;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();
    public DbSet<VeiculoEntity> Veiculos => Set<VeiculoEntity>();
    public DbSet<ProdutoEntity> Produtos => Set<ProdutoEntity>();
    public DbSet<ServicoEntity> Servicos => Set<ServicoEntity>();
    public DbSet<WebhookEventEntity> WebhookEvents => Set<WebhookEventEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.IsNpgsql())
        {
            modelBuilder.HasPostgresExtension("pg_trgm");
            modelBuilder.HasPostgresExtension("unaccent");
        }

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
