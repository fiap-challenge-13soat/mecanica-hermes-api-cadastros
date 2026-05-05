using System.Reflection;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();
    public DbSet<VeiculoEntity> Veiculos => Set<VeiculoEntity>();
    public DbSet<ProdutoEntity> Produtos => Set<ProdutoEntity>();
    public DbSet<ServicoEntity> Servicos => Set<ServicoEntity>();

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
