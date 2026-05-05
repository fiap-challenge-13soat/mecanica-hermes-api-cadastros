using Mecanica.Hermes.Cadastros.Domain.Common.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

public sealed class ValorProdutoVo
{
    public ValorProdutoVo()
    {
        // EF Construtor
    }

    private ValorProdutoVo(decimal valor)
    {
        Valor = valor;
    }

    public decimal Valor { get; private set; }
    public override string ToString() => Valor.ToString();

    public static Result<ValorProdutoVo> Criar(decimal valor)
    {
        if (valor < 0)
            return Result<ValorProdutoVo>.BadRequest("Valor do produto não pode ser negativo.");

        var valorNormalizado = LimitarDuasCasasDecimais(valor);

        return Result<ValorProdutoVo>.Ok(new ValorProdutoVo(valorNormalizado));
    }

    private static decimal LimitarDuasCasasDecimais(decimal valor)
    {
        return Math.Floor(valor * 100) / 100;
    }
}