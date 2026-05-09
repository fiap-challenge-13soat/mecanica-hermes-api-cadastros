using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Mappings;
using Mecanica.Hermes.Shared.Core.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Repositories;

internal class ClienteRepository(AppDbContext db, IUnitOfWork uow) : IClienteRepository
{
    public async Task<Cliente?> GetByIdAsync(Guid id)
    {
        var clienteEntity = await db.Clientes
            .AsNoTracking()
            .Include(c => c.Veiculos)
            .FirstOrDefaultAsync(c => c.Id == id);

        return clienteEntity?.ToDomain();
    }

    public async Task<PagedResult<Cliente>> ListByNomeAsync(string nome, PaginationParams paginationParams)
    {
        var pattern = $"%{nome}%";

        var countQuery = await db.Database
            .SqlQuery<int>($@"
                SELECT COUNT(*) AS ""Value""
                FROM ""Clientes""
                WHERE unaccent(""NomeCivil"") ILIKE unaccent({pattern})
                   OR (""NomeSocial"" IS NOT NULL AND unaccent(""NomeSocial"") ILIKE unaccent({pattern}))")
            .FirstOrDefaultAsync();

        var entities = await db.Clientes
            .FromSqlInterpolated($@"
                SELECT * FROM ""Clientes""
                WHERE unaccent(""NomeCivil"") ILIKE unaccent({pattern})
                   OR (""NomeSocial"" IS NOT NULL AND unaccent(""NomeSocial"") ILIKE unaccent({pattern}))
                ORDER BY ""NomeCivil"" ASC
                LIMIT {paginationParams.PageSize} OFFSET {paginationParams.Skip}")
            .AsNoTracking()
            .ToListAsync();

        var domainClientes = entities.Select(e => e.ToDomain()).ToList();

        return PagedResult<Cliente>.Create(
            domainClientes,
            paginationParams.Page,
            paginationParams.PageSize,
            countQuery);
    }

    public async Task<PagedResult<Cliente>> ListAllAsync(PaginationParams paginationParams)
    {
        var totalCount = await db.Clientes.CountAsync();

        var entities = await db.Clientes
            .FromSqlInterpolated($@"
                SELECT * FROM ""Clientes""
                ORDER BY ""NomeCivil"" ASC
                LIMIT {paginationParams.PageSize} OFFSET {paginationParams.Skip}")
            .AsNoTracking()
            .ToListAsync();

        var domainClientes = entities.Select(e => e.ToDomain()).ToList();

        return PagedResult<Cliente>.Create(
            domainClientes,
            paginationParams.Page,
            paginationParams.PageSize,
            totalCount);
    }

    public async Task<Cliente?> GetByEmailAsync(EmailVo email)
    {
        var clienteEntity = await db.Clientes
            .Include(c => c.Veiculos)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email);

        return clienteEntity?.ToDomain();
    }

    public async Task<Cliente?> GetByIdentificacaoFiscalAsync(IdentificacaoFiscalVo identificacaoFiscal)
    {
        var clienteEntity = await db.Clientes
            .Include(c => c.Veiculos)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdentificacaoFiscal == identificacaoFiscal);

        return clienteEntity?.ToDomain();
    }

    public async Task AddAsync(Cliente cliente)
    {
        var entity = cliente.ToEntity();
        await db.Clientes.AddAsync(entity);
        cliente.Id = entity.Id;
        uow.CollectEventsFrom(cliente);
    }

    public void Update(Cliente cliente)
    {
        db.Clientes.Update(cliente.ToEntity());
        uow.CollectEventsFrom(cliente);
    }
}
