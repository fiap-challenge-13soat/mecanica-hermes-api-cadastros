using System.Globalization;
using Mecanica.Hermes.Cadastros.Domain.Common.Helpers;
using Mecanica.Hermes.Shared.Core.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

public sealed class CnpjVo
{
    private CnpjVo()
    {
        // EF Construtor
    }

    private CnpjVo(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; private set; } = default!;
    public override string ToString() => Valor;

    public static Result<CnpjVo> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Result<CnpjVo>.BadRequest("CNPJ não pode ser vazio.");

        if (!RegexHelper.FormatoCnpj().IsMatch(valor))
            return Result<CnpjVo>.BadRequest("CNPJ não está no formato correto.");

        return !CnpjValido(valor)
            ? Result<CnpjVo>.BadRequest("CNPJ tem valor inválido.")
            : Result<CnpjVo>.Ok(new CnpjVo(valor));
    }

    public static bool CnpjValido(string valor)
    {
        var validador1 = 0;
        var validador2 = 0;

        int[] validadorPesos = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        for (var i = 0; i < 12; i++) validador1 += validadorPesos[i + 1] * int.Parse(valor[i].ToString(), CultureInfo.InvariantCulture);

        validador1 = validador1 % 11;

        if (validador1 < 2)
            validador1 = 0;
        else
            validador1 = 11 - validador1;

        if (valor[12].ToString() != validador1.ToString(CultureInfo.InvariantCulture))
            return false;

        for (var i = 0; i < 13; i++) validador2 += validadorPesos[i] * int.Parse(valor[i].ToString(), CultureInfo.InvariantCulture);

        validador2 = validador2 % 11;

        if (validador2 < 2)
            validador2 = 0;
        else
            validador2 = 11 - validador2;

        if (valor[13].ToString() != validador2.ToString(CultureInfo.InvariantCulture))
            return false;

        return true;
    }
}
