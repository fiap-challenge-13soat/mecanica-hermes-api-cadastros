namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;

public sealed record ClienteResponse(
    Guid Id,
    string NomeCivil,
    string NomeSocial,
    string IdentificacaoFiscal,
    string Email,
    string Telefone,
    List<VeiculoResponse> Veiculos);

public sealed record VeiculoResponse(
    Guid Id,
    string Modelo,
    string Marca,
    string Placa,
    int Ano);
