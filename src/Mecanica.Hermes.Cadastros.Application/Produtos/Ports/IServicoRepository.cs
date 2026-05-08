using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;
using Mecanica.Hermes.Shared.Core.Pagination;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Ports;

public interface IServicoRepository
{
    Task<Servico?> GetByIdAsync(Guid id);
    Task<PagedResult<Servico>> ListByDescricaoAsync(DescricaoProdutoVo descricao, PaginationParams paginationParams);
    Task<PagedResult<Servico>> ListAllAsync(PaginationParams paginationParams);
    Task AddAsync(Servico servico);
    void Update(Servico servico);
}
