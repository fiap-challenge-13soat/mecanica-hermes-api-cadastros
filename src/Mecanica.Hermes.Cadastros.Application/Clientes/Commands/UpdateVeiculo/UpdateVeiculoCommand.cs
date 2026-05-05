using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.UpdateVeiculo;

public sealed record UpdateVeiculoCommand(
    Guid ClienteId,
    Guid VeiculoId,
    string Modelo,
    string Marca,
    int Ano) : IRequest<Result<ClienteDto>>;