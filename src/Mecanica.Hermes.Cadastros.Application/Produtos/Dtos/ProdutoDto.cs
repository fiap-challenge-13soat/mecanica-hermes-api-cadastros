namespace Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;

public sealed record ProdutoDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    int Quantidade,
    string Tipo);
