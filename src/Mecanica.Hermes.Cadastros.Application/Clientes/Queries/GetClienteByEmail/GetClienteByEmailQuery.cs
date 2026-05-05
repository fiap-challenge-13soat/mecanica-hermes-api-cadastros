using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteByEmail;

public sealed record GetClienteByEmailQuery(string Email) : IRequest<Result<ClienteDto>>;