using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.DeleteCliente;

public sealed record DeleteClienteCommand(Guid Id) : IRequest<Result>;
