using System.ComponentModel.DataAnnotations;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;

public sealed record UpdateClienteRequest(
    [Required(AllowEmptyStrings = false)] string NomeCivil,
    [Required(AllowEmptyStrings = true)] string NomeSocial,
    [Required(AllowEmptyStrings = false)] string Email,
    [Required(AllowEmptyStrings = false)] string Telefone);
