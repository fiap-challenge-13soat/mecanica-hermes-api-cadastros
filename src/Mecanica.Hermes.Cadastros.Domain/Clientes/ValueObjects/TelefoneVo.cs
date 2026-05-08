using Mecanica.Hermes.Cadastros.Domain.Common.Helpers;
using Mecanica.Hermes.Shared.Core.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

public sealed class TelefoneVo
{
    private TelefoneVo()
    {
        // EF Construtor
    }

    private TelefoneVo(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; private set; } = default!;
    public override string ToString() => Valor;

    public static Result<TelefoneVo> Criar(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return Result<TelefoneVo>.BadRequest("Telefone é obrigatório.");

        var normalizado = RegexHelper.NormalizarNumeros(telefone);

        if (normalizado.Length < 10 || normalizado.Length > 11)
            return Result<TelefoneVo>.BadRequest("Telefone deve conter DDD e número válido.");

        return Result<TelefoneVo>.Ok(new TelefoneVo(normalizado));
    }
}