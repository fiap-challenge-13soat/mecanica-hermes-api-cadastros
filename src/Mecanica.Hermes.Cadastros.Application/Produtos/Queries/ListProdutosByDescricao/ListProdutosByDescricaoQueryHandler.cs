using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;
using Mecanica.Hermes.Shared.Core.Pagination;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListProdutosByDescricao;

internal sealed class ListProdutosByDescricaoQueryHandler(IProdutoRepository repository)
    : IRequestHandler<ListProdutosByDescricaoQuery, Result<PagedResult<ProdutoDto>>>
{
    public async Task<Result<PagedResult<ProdutoDto>>> Handle(ListProdutosByDescricaoQuery request, CancellationToken cancellationToken)
    {
        var descricao = DescricaoProdutoVo.Criar(request.Descricao);
        if (descricao.IsFailure)
            return Result<PagedResult<ProdutoDto>>.BadRequest(descricao.Errors);

        var paginationParams = new PaginationParams { Page = request.Page, PageSize = request.PageSize };

        var pagedProdutos = await repository.ListByDescricaoAsync(descricao.Data!, paginationParams);

        var produtoDtos = pagedProdutos.Items.Select(p => p.ToDto()).ToList();

        var pagedResult = PagedResult<ProdutoDto>.Create(
            produtoDtos,
            pagedProdutos.Page,
            pagedProdutos.PageSize,
            pagedProdutos.TotalCount);

        return Result<PagedResult<ProdutoDto>>.Ok(pagedResult);
    }
}
