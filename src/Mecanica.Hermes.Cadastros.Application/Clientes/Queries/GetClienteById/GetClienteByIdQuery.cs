using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteById;

public sealed record GetClienteByIdQuery(Guid Id) : IRequest<Result<ClienteDto>>;
