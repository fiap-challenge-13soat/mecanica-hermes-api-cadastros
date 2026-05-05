using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Domain.Common.Pagination;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.ListClientesByNome;

internal sealed class ListClientesByNomeQueryHandler(IClienteRepository repository)
    : IRequestHandler<ListClientesByNomeQuery, Result<PagedResult<ClienteDto>>>
{
    public async Task<Result<PagedResult<ClienteDto>>> Handle(ListClientesByNomeQuery request,
        CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams { Page = request.Page, PageSize = request.PageSize };
        
        var pagedClientes = await repository.ListByNomeAsync(request.Nome, paginationParams);
        
        var clienteDtos = pagedClientes.Items.Select(c => c.ToDto()).ToList();
        
        var pagedResult = PagedResult<ClienteDto>.Create(
            clienteDtos,
            pagedClientes.Page,
            pagedClientes.PageSize,
            pagedClientes.TotalCount);

        return Result<PagedResult<ClienteDto>>.Ok(pagedResult);
    }
}