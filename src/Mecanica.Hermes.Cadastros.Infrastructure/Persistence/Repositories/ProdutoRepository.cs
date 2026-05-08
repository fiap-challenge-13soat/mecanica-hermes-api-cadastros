using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Shared.Core.Pagination;
using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Repositories;

internal class ProdutoRepository(AppDbContext db, IUnitOfWork uow) : IProdutoRepository
{
    public async Task<Produto?> GetByIdAsync(Guid id)
    {
        var produtoEntity = await db.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return produtoEntity?.ToDomain();
    }

    public async Task<PagedResult<Produto>> ListByDescricaoAsync(DescricaoProdutoVo descricao, PaginationParams paginationParams)
    {
        var pattern = $"%{descricao.Valor}%";

        var countQuery = await db.Database
            .SqlQuery<int>($@"
                SELECT COUNT(*) AS ""Value""
                FROM ""Produtos""
                WHERE unaccent(""Descricao"") ILIKE unaccent({pattern})")
            .FirstOrDefaultAsync();

        var entities = await db.Produtos
            .FromSqlInterpolated($@"
                SELECT * FROM ""Produtos""
                WHERE unaccent(""Descricao"") ILIKE unaccent({pattern})
                ORDER BY ""Descricao"" ASC
                LIMIT {paginationParams.PageSize} OFFSET {paginationParams.Skip}")
            .AsNoTracking()
            .ToListAsync();

        var domainProdutos = entities.Select(e => e.ToDomain()).ToList();

        return PagedResult<Produto>.Create(
            domainProdutos,
            paginationParams.Page,
            paginationParams.PageSize,
            countQuery);
    }

    public async Task<PagedResult<Produto>> ListAllAsync(PaginationParams paginationParams)
    {
        var totalCount = await db.Produtos.CountAsync();

        var entities = await db.Produtos
            .FromSqlInterpolated($@"
                SELECT * FROM ""Produtos""
                ORDER BY ""Descricao"" ASC
                LIMIT {paginationParams.PageSize} OFFSET {paginationParams.Skip}")
            .AsNoTracking()
            .ToListAsync();

        var domainProdutos = entities.Select(e => e.ToDomain()).ToList();

        return PagedResult<Produto>.Create(
            domainProdutos,
            paginationParams.Page,
            paginationParams.PageSize,
            totalCount);
    }

    public async Task AddAsync(Produto servico)
    {
        var entity = servico.ToEntity();
        await db.Produtos.AddAsync(entity);
        servico.Id = entity.Id;
        uow.CollectEventsFrom(servico);
    }

    public void Update(Produto servico)
    {
        db.Produtos.Update(servico.ToEntity());
        uow.CollectEventsFrom(servico);
    }
}