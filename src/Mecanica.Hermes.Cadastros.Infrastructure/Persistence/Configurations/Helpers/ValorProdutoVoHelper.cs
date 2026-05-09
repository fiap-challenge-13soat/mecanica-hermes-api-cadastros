using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;

internal static class ValorProdutoVoHelper
{
    public static ValorProdutoVo RecriarValor(decimal valor)
    {
        var resultado = ValorProdutoVo.Criar(valor);
        if (resultado.IsSuccess && resultado.Data is not null)
            return resultado.Data;

        throw new InvalidOperationException("Não foi possível reconstruir o valor do produto armazenado.");
    }
}
