using Mecanica.Hermes.Shared.Core.Pagination;
using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Ports;

public interface IProdutoRepository
{
    Task<Produto?> GetByIdAsync(Guid id);
    Task<PagedResult<Produto>> ListByDescricaoAsync(DescricaoProdutoVo descricao, PaginationParams paginationParams);
    Task<PagedResult<Produto>> ListAllAsync(PaginationParams paginationParams);
    Task AddAsync(Produto servico);
    void Update(Produto servico);
}