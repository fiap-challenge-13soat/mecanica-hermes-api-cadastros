using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.DeleteVeiculo;

public sealed record DeleteVeiculoCommand(Guid ClienteId, Guid VeiculoId) : IRequest<Result>;
