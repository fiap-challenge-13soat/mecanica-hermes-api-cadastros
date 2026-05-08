using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.AddCliente;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.AddVeiculo;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.UpdateCliente;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.UpdateVeiculo;
using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Mappings;

internal static class ClienteContractMappings
{
    public static AddClienteCommand ToCommand(this AddClienteRequest request) =>
        new(request.NomeCivil, request.NomeSocial, request.IdentificacaoFiscal, request.Email, request.Telefone);

    public static UpdateClienteCommand ToCommand(this UpdateClienteRequest request, Guid id) =>
        new(id, request.NomeCivil, request.NomeSocial, request.Email, request.Telefone);

    public static AddVeiculoCommand ToCommand(this AddVeiculoRequest request, Guid clienteId) =>
        new(clienteId, request.Modelo, request.Marca, request.Placa, request.Ano);

    public static UpdateVeiculoCommand ToCommand(this UpdateVeiculoRequest request, Guid clienteId, Guid veiculoId) =>
        new(clienteId, veiculoId, request.Modelo, request.Marca, request.Ano);

    public static ClienteResponse ToResponse(this ClienteDto dto) =>
        new(
            dto.Id,
            dto.NomeCivil,
            dto.NomeSocial ?? string.Empty,
            dto.IdentificacaoFiscal,
            dto.Email,
            dto.Telefone,
            dto.Veiculos.Select(ToResponse).ToList());

    public static VeiculoResponse ToResponse(this VeiculoDto dto) =>
        new(dto.Id, dto.Modelo, dto.Marca, dto.Placa, dto.Ano);
}
