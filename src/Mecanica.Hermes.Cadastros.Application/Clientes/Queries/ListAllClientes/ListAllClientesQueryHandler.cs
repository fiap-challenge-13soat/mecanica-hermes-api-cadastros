using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Shared.Core.Pagination;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.ListAllClientes;

internal sealed class ListAllClientesQueryHandler(IClienteRepository repository)
    : IRequestHandler<ListAllClientesQuery, Result<PagedResult<ClienteDto>>>
{
    public async Task<Result<PagedResult<ClienteDto>>> Handle(ListAllClientesQuery request,
        CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams { Page = request.Page, PageSize = request.PageSize };

        var pagedClientes = await repository.ListAllAsync(paginationParams);

        var clienteDtos = pagedClientes.Items.Select(c => c.ToDto()).ToList();

        var pagedResult = PagedResult<ClienteDto>.Create(
            clienteDtos,
            pagedClientes.Page,
            pagedClientes.PageSize,
            pagedClientes.TotalCount);

        return Result<PagedResult<ClienteDto>>.Ok(pagedResult);
    }
}
