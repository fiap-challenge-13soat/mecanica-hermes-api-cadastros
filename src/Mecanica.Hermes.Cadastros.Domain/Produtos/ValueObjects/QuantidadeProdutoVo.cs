using System.Globalization;
using Mecanica.Hermes.Shared.Core.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

public sealed class QuantidadeProdutoVo
{
    public QuantidadeProdutoVo()
    {
        // EF Construtor
    }

    public QuantidadeProdutoVo(int valor)
    {
        Valor = valor;
    }

    public int Valor { get; private set; }
    public override string ToString() => Valor.ToString(CultureInfo.InvariantCulture);

    public static Result<QuantidadeProdutoVo> Criar(int valor)
    {
        if (valor <= 0)
            return Result<QuantidadeProdutoVo>.BadRequest("A quantidade do produto deve ser positiva.");

        return Result<QuantidadeProdutoVo>.Ok(new QuantidadeProdutoVo(valor));
    }

    internal Result<QuantidadeProdutoVo> Somar(QuantidadeProdutoVo quantidade)
    {
        Valor += quantidade.Valor;

        return Result<QuantidadeProdutoVo>.Ok();
    }

    internal Result<QuantidadeProdutoVo> Subtrair(QuantidadeProdutoVo quantidade)
    {
        if (Valor <= quantidade.Valor)
            return Result<QuantidadeProdutoVo>.BadRequest("A quantidade disponível do produto não permite atender esta demanda.");

        Valor -= quantidade.Valor;
        return Result<QuantidadeProdutoVo>.Ok();
    }
}
