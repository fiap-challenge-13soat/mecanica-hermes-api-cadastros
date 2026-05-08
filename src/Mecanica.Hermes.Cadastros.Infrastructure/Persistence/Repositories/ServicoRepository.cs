using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Mappings;
using Mecanica.Hermes.Shared.Core.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Repositories;

internal class ServicoRepository(AppDbContext db, IUnitOfWork uow) : IServicoRepository
{
    public async Task<Servico?> GetByIdAsync(Guid id)
    {
        var servicoEntity = await db.Servicos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return servicoEntity?.ToDomain();
    }

    public async Task<PagedResult<Servico>> ListByDescricaoAsync(DescricaoProdutoVo descricao, PaginationParams paginationParams)
    {
        var pattern = $"%{descricao.Valor}%";

        var countQuery = await db.Database
            .SqlQuery<int>($@"
                SELECT COUNT(*) As ""Value""
                FROM ""Servicos""
                WHERE unaccent(""Descricao"") ILIKE unaccent({pattern})")
            .FirstOrDefaultAsync();

        var entities = await db.Servicos
            .FromSqlInterpolated($@"
                SELECT * FROM ""Servicos""
                WHERE unaccent(""Descricao"") ILIKE unaccent({pattern})
                ORDER BY ""Descricao"" ASC
                LIMIT {paginationParams.PageSize} OFFSET {paginationParams.Skip}")
            .AsNoTracking()
            .ToListAsync();

        var domainServicos = entities.Select(e => e.ToDomain()).ToList();

        return PagedResult<Servico>.Create(
            domainServicos,
            paginationParams.Page,
            paginationParams.PageSize,
            countQuery);
    }

    public async Task<PagedResult<Servico>> ListAllAsync(PaginationParams paginationParams)
    {
        var totalCount = await db.Servicos.CountAsync();

        var entities = await db.Servicos
            .FromSqlInterpolated($@"
                SELECT * FROM ""Servicos""
                ORDER BY ""Descricao"" ASC
                LIMIT {paginationParams.PageSize} OFFSET {paginationParams.Skip}")
            .AsNoTracking()
            .ToListAsync();

        var domainServicos = entities.Select(e => e.ToDomain()).ToList();

        return PagedResult<Servico>.Create(
            domainServicos,
            paginationParams.Page,
            paginationParams.PageSize,
            totalCount);
    }

    public async Task AddAsync(Servico servico)
    {
        var entity = servico.ToEntity();
        await db.Servicos.AddAsync(entity);
        servico.Id = entity.Id;
        uow.CollectEventsFrom(servico);
    }

    public void Update(Servico servico)
    {
        db.Servicos.Update(servico.ToEntity());
        uow.CollectEventsFrom(servico);
    }
}
