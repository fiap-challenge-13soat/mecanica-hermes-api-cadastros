using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;
using Mecanica.Hermes.Shared.Core.Pagination;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListServicosByDescricao;

internal sealed class ListServicoByDescricaoQueryHandler(IServicoRepository repository)
    : IRequestHandler<ListServicoByDescricaoQuery, Result<PagedResult<ServicoDto>>>
{
    public async Task<Result<PagedResult<ServicoDto>>> Handle(ListServicoByDescricaoQuery request, CancellationToken cancellationToken)
    {
        var descricao = DescricaoProdutoVo.Criar(request.Descricao);
        if (descricao.IsFailure)
            return Result<PagedResult<ServicoDto>>.BadRequest(descricao.Errors);

        var paginationParams = new PaginationParams { Page = request.Page, PageSize = request.PageSize };

        var pagedServicos = await repository.ListByDescricaoAsync(descricao.Data!, paginationParams);

        var servicoDtos = pagedServicos.Items.Select(s => s.ToDto()).ToList();

        var pagedResult = PagedResult<ServicoDto>.Create(
            servicoDtos,
            pagedServicos.Page,
            pagedServicos.PageSize,
            pagedServicos.TotalCount);

        return Result<PagedResult<ServicoDto>>.Ok(pagedResult);
    }
}
