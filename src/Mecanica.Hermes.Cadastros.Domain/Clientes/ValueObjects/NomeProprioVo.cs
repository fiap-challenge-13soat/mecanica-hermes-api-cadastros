using Mecanica.Hermes.Cadastros.Domain.Common.Helpers;
using Mecanica.Hermes.Shared.Core.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

public sealed class NomeProprioVo
{
    private NomeProprioVo()
    {
        // EF Construtor
    }

    private NomeProprioVo(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; private set; } = default!;
    public override string ToString() => Valor;

    public static Result<NomeProprioVo> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Result<NomeProprioVo>.BadRequest("Nome não pode ser vazio.");

        var normalizado = TitleCaseHelper.Normalizar(valor);

        if (normalizado.Length > 200)
            return Result<NomeProprioVo>.BadRequest("Nome não pode ter mais de 200 caracteres.");

        if (ContarPalavras(normalizado) < 2)
            return Result<NomeProprioVo>.BadRequest("Nome deve conter pelo menos nome e sobrenome.");

        return Result<NomeProprioVo>.Ok(new NomeProprioVo(normalizado));
    }

    private static int ContarPalavras(string texto) =>
        texto.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
}
