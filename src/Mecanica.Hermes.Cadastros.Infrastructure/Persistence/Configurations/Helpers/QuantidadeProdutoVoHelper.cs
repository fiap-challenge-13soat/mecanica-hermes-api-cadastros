using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;

internal static class QuantidadeProdutoVoHelper
{
    public static QuantidadeProdutoVo RecriarQuantidade(int valor)
    {
        var resultado = QuantidadeProdutoVo.Criar(valor);
        if (resultado.IsSuccess && resultado.Data is not null)
            return resultado.Data;

        throw new InvalidOperationException("Não foi possível reconstruir a quantidade do produto armazenada.");
    }
}
