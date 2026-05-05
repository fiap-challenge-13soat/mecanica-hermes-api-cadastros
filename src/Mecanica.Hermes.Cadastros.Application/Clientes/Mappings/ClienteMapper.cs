using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Clientes;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;

internal static class ClienteMapper
{
    public static ClienteDto ToDto(this Cliente cliente)
    {
        var veiculos = cliente.Veiculos.Select(v => v.ToDto()).ToList();

        return new ClienteDto(cliente.Id,
            cliente.NomeCivil.Valor,
            cliente.NomeSocial?.Valor,
            cliente.IdentificacaoFiscal.Valor,
            cliente.Email.Valor,
            cliente.Telefone.Valor,
            veiculos);
    }

    public static VeiculoDto ToDto(this Veiculo veiculo)
    {
        return new VeiculoDto(
            veiculo.Id,
            veiculo.Modelo,
            veiculo.Marca,
            veiculo.Placa.Valor,
            veiculo.Ano);
    }
}