namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;

public sealed record ProdutoResponse(
    Guid Id,
    string Descricao,
    decimal Valor,
    int Quantidade,
    string Tipo);
