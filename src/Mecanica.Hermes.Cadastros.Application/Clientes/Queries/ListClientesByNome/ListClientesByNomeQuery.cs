using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Shared.Core.Pagination;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.ListClientesByNome;

public sealed record ListClientesByNomeQuery(
    string Nome,
    int Page = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<ClienteDto>>>;
