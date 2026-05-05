using Mecanica.Hermes.Cadastros.Domain.Common.Helpers;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

public sealed class DescricaoProdutoVo
{
    private DescricaoProdutoVo()
    {
        // EF Construtor
    }

    private DescricaoProdutoVo(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; private set; } = default!;
    public override string ToString() => Valor;

    public static Result<DescricaoProdutoVo> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Result<DescricaoProdutoVo>.BadRequest("A descrição não pode ser vazia.");

        var normalizado = TitleCaseHelper.Normalizar(valor);

        if (normalizado.Length > 100)
            return Result<DescricaoProdutoVo>.BadRequest("A descrição não pode ter mais de 100 caracteres.");

        return Result<DescricaoProdutoVo>.Ok(new DescricaoProdutoVo(normalizado));
    }
}