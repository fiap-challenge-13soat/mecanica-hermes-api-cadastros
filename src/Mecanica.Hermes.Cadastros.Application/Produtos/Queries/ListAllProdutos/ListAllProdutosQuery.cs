using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Shared.Core.Pagination;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListAllProdutos;

public sealed record ListAllProdutosQuery(
    int Page = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<ProdutoDto>>>;
