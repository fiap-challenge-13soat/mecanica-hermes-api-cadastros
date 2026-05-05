using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Common.Pagination;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListServicosByDescricao;

public sealed record ListServicoByDescricaoQuery(
    string Descricao,
    int Page = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<ServicoDto>>>;