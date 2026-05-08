using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Shared.Core.Pagination;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListAllProdutos;

internal sealed class ListAllProdutosQueryHandler(IProdutoRepository repository)
    : IRequestHandler<ListAllProdutosQuery, Result<PagedResult<ProdutoDto>>>
{
    public async Task<Result<PagedResult<ProdutoDto>>> Handle(ListAllProdutosQuery request,
        CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams { Page = request.Page, PageSize = request.PageSize };
        
        var pagedProdutos = await repository.ListAllAsync(paginationParams);
        
        var produtoDtos = pagedProdutos.Items.Select(p => p.ToDto()).ToList();
        
        var pagedResult = PagedResult<ProdutoDto>.Create(
            produtoDtos,
            pagedProdutos.Page,
            pagedProdutos.PageSize,
            pagedProdutos.TotalCount);

        return Result<PagedResult<ProdutoDto>>.Ok(pagedResult);
    }
}
