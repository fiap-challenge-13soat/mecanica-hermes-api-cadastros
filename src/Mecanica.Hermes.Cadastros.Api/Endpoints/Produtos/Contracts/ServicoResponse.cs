namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;

public sealed record ServicoResponse(
    Guid Id,
    string Descricao,
    decimal Valor);