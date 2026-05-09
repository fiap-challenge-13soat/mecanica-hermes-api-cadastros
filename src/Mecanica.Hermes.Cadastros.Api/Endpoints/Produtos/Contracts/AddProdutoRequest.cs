using System.ComponentModel.DataAnnotations;
using Mecanica.Hermes.Cadastros.Domain.Produtos.Enums;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;

public sealed record AddProdutoRequest(
    [Required(AllowEmptyStrings = false)] string Descricao,
    [Required] decimal Valor,
    [Required] int Quantidade,
    [Required(AllowEmptyStrings = false)]
    TipoProduto Tipo);
