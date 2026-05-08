using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.DeleteCliente;

public sealed record DeleteClienteCommand(Guid Id) : IRequest<Result>;
