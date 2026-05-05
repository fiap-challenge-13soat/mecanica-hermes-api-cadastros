using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.DeleteVeiculo;

public sealed record DeleteVeiculoCommand(Guid ClienteId, Guid VeiculoId) : IRequest<Result>;
