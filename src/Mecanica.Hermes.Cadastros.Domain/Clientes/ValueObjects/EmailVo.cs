using Mecanica.Hermes.Cadastros.Domain.Common.Helpers;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

public sealed class EmailVo
{
    public EmailVo()
    {
        // EF Construtor
    }

    public EmailVo(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; private set; } = default!;
    public override string ToString() => Valor;

    public static Result<EmailVo> Criar(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<EmailVo>.BadRequest("Email é obrigatório.");

        if (email.Length > 200)
            return Result<EmailVo>.BadRequest("Email não pode ter mais do que 200 caracteres.");

        var normalizado = email.Trim().ToLowerInvariant();

        if (!RegexHelper.FormatoEmail().IsMatch(normalizado))
            return Result<EmailVo>.BadRequest("Email em formato inválido.");

        return Result<EmailVo>.Ok(new EmailVo(normalizado));
    }
}