using Mecanica.Hermes.Cadastros.Domain.Common.Helpers;
using Mecanica.Hermes.Shared.Core.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

public sealed class PlacaVo
{
    private PlacaVo()
    {
        // EF Construtor
    }

    private PlacaVo(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; private set; } = default!;
    public override string ToString() => Valor;

    public static Result<PlacaVo> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Result<PlacaVo>.BadRequest("A placa não pode ser vazia.");

        var normalizado = valor.ToUpper().Trim().Replace("-", "");

        if (!RegexHelper.FormatoPlaca().IsMatch(normalizado))
            return Result<PlacaVo>.BadRequest("Placa inválida. Formatos aceitos: ABC1234 ou ABC1D23.");

        return Result<PlacaVo>.Ok(new PlacaVo(normalizado));
    }
}