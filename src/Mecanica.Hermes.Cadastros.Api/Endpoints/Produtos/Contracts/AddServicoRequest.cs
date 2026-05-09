using System.ComponentModel.DataAnnotations;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;

public sealed record AddServicoRequest(
    [Required(AllowEmptyStrings = false)] string Descricao,
    [Required] decimal Valor);
