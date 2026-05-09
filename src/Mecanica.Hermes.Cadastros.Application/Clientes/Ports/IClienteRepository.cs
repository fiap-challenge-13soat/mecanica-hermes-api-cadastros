using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Shared.Core.Pagination;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Ports;

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(Guid id);
    Task<PagedResult<Cliente>> ListByNomeAsync(string nome, PaginationParams paginationParams);
    Task<PagedResult<Cliente>> ListAllAsync(PaginationParams paginationParams);
    Task<Cliente?> GetByEmailAsync(EmailVo email);
    Task<Cliente?> GetByIdentificacaoFiscalAsync(IdentificacaoFiscalVo identificacaoFiscal);
    Task AddAsync(Cliente cliente);
    void Update(Cliente cliente);
}
