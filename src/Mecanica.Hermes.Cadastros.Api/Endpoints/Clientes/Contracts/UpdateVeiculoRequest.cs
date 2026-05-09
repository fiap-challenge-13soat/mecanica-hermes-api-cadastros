using System.ComponentModel.DataAnnotations;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;

public sealed record UpdateVeiculoRequest(
    [Required(AllowEmptyStrings = false)] string Modelo,
    [Required(AllowEmptyStrings = false)] string Marca,
    [Required(AllowEmptyStrings = false)] int Ano);
