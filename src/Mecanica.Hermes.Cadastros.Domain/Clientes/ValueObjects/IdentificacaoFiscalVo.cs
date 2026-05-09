using Mecanica.Hermes.Cadastros.Domain.Common.Helpers;
using Mecanica.Hermes.Shared.Core.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

public sealed class IdentificacaoFiscalVo
{
    private IdentificacaoFiscalVo()
    {
        // EF Construtor
    }

    private IdentificacaoFiscalVo(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; private set; } = default!;
    public override string ToString() => Valor;

    public static Result<IdentificacaoFiscalVo> Criar(string valor)
    {
        var normalizado = RegexHelper.NormalizarNumeros(valor);

        switch (normalizado.Length)
        {
            case 11:
                {
                    var result = CpfVo.Criar(valor);
                    return result.IsSuccess
                        ? Result<IdentificacaoFiscalVo>.Ok(new IdentificacaoFiscalVo(result.Data!.ToString()))
                        : Result<IdentificacaoFiscalVo>.BadRequest(result.Errors);
                }
            case 14:
                {
                    var result = CnpjVo.Criar(valor);
                    return result.IsSuccess
                        ? Result<IdentificacaoFiscalVo>.Ok(new IdentificacaoFiscalVo(result.Data!.ToString()))
                        : Result<IdentificacaoFiscalVo>.BadRequest(result.Errors);
                }
            default:
                return Result<IdentificacaoFiscalVo>.BadRequest(
                    "A identificação fiscal deve ser um CPF ou CNPJ válido.");
        }
    }
}
