using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.AddVeiculo;

public sealed record AddVeiculoCommand(
    Guid ClienteId,
    string Modelo,
    string Marca,
    string Placa,
    int Ano) : IRequest<Result<ClienteDto>>;
