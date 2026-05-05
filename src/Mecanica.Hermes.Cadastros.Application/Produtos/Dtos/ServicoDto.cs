namespace Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;

public sealed record ServicoDto(
    Guid Id,
    string Descricao,
    decimal Valor);
