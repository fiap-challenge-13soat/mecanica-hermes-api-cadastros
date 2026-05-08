using System.Globalization;
using Mecanica.Hermes.Cadastros.Domain.Common.Helpers;
using Mecanica.Hermes.Shared.Core.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

public sealed class CpfVo
{
    private CpfVo()
    {
        // EF Construtor
    }

    private CpfVo(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; private set; } = default!;
    public override string ToString() => Valor;

    public static Result<CpfVo> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Result<CpfVo>.BadRequest("CPF não pode ser vazio.");

        var normalizado = RegexHelper.NormalizarNumeros(valor);

        if (!RegexHelper.FormatoCpf().IsMatch(normalizado))
            return Result<CpfVo>.BadRequest("CPF não está no formato correto.");

        return !CpfValido(normalizado)
            ? Result<CpfVo>.BadRequest("CPF tem valor inválido.")
            : Result<CpfVo>.Ok(new CpfVo(normalizado));
    }

    public static bool CpfValido(string valor)
    {
        var validador1 = 0;
        var validador2 = 0;

        for (var i = 0; i < 9; i++) validador1 += (10 - i) * int.Parse(valor[i].ToString(), CultureInfo.InvariantCulture);

        validador1 = validador1 % 11;

        if (validador1 < 2)
            validador1 = 0;
        else
            validador1 = 11 - validador1;

        if (valor[9].ToString() != validador1.ToString(CultureInfo.InvariantCulture))
            return false;

        for (var i = 0; i < 10; i++) validador2 += (11 - i) * int.Parse(valor[i].ToString(), CultureInfo.InvariantCulture);

        validador2 = validador2 % 11;

        if (validador2 < 2)
            validador2 = 0;
        else
            validador2 = 11 - validador2;

        if (valor[10].ToString() != validador2.ToString(CultureInfo.InvariantCulture))
            return false;

        return true;
    }
}
