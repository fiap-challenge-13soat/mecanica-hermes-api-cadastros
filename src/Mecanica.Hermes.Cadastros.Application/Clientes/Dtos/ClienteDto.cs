namespace Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;

public sealed record ClienteDto(
    Guid Id,
    string NomeCivil,
    string? NomeSocial,
    string IdentificacaoFiscal,
    string Email,
    string Telefone,
    List<VeiculoDto> Veiculos);