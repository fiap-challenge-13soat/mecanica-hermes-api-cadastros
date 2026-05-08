using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Shared.Core.Pagination;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListAllServicos;

internal sealed class ListAllServicosQueryHandler(IServicoRepository repository)
    : IRequestHandler<ListAllServicosQuery, Result<PagedResult<ServicoDto>>>
{
    public async Task<Result<PagedResult<ServicoDto>>> Handle(ListAllServicosQuery request,
        CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams { Page = request.Page, PageSize = request.PageSize };

        var pagedServicos = await repository.ListAllAsync(paginationParams);

        var servicoDtos = pagedServicos.Items.Select(s => s.ToDto()).ToList();

        var pagedResult = PagedResult<ServicoDto>.Create(
            servicoDtos,
            pagedServicos.Page,
            pagedServicos.PageSize,
            pagedServicos.TotalCount);

        return Result<PagedResult<ServicoDto>>.Ok(pagedResult);
    }
}
