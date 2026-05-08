using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Mappings;

internal static class ClienteMapper
{
    internal static Cliente ToDomain(this ClienteEntity entity)
    {
        var veiculos = entity.Veiculos.Select(veiculoEntity =>
            Veiculo.Restaurar(
                veiculoEntity.Id,
                veiculoEntity.Modelo,
                veiculoEntity.Marca,
                veiculoEntity.Placa,
                veiculoEntity.Ano,
                veiculoEntity.CreatedAt,
                veiculoEntity.UpdatedAt,
                veiculoEntity.IsDeleted)
        ).ToList();

        var cliente = Cliente.Restaurar(
            entity.Id,
            entity.NomeCivil,
            entity.NomeSocial,
            entity.IdentificacaoFiscal,
            entity.Email,
            entity.Telefone,
            veiculos,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.IsDeleted);

        return cliente;
    }

    internal static ClienteEntity ToEntity(this Cliente cliente)
    {
        var veiculosEntity = cliente.Veiculos.Select(veiculo => new VeiculoEntity(
            veiculo.Id,
            veiculo.Modelo,
            veiculo.Marca,
            veiculo.Placa,
            veiculo.Ano,
            veiculo.CreatedAt,
            veiculo.UpdatedAt,
            veiculo.IsDeleted
        )).ToList();

        return new ClienteEntity(
            cliente.Id,
            cliente.NomeCivil,
            cliente.NomeSocial,
            cliente.IdentificacaoFiscal,
            cliente.Email,
            cliente.Telefone,
            veiculosEntity,
            cliente.CreatedAt,
            cliente.UpdatedAt,
            cliente.IsDeleted
        );
    }

    internal static VeiculoEntity ToEntity(this Veiculo veiculo, Cliente cliente)
    {
        return new VeiculoEntity(
            veiculo.Id,
            veiculo.Modelo,
            veiculo.Marca,
            veiculo.Placa,
            veiculo.Ano,
            veiculo.CreatedAt,
            veiculo.UpdatedAt,
            veiculo.IsDeleted)
        {
            ClienteId = cliente.Id
        };
    }
}
