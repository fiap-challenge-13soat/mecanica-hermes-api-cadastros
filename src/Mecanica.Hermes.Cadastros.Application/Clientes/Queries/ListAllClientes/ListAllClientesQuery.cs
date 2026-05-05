using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Common.Pagination;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.ListAllClientes;

public sealed record ListAllClientesQuery(
    int Page = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<ClienteDto>>>;
