using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;

internal static class DescricaoProdutoVoHelper
{
    public static DescricaoProdutoVo RecriarDescricao(string valor)
    {
        var resultado = DescricaoProdutoVo.Criar(valor);
        if (resultado.IsSuccess && resultado.Data is not null)
            return resultado.Data;

        throw new InvalidOperationException("Não foi possível reconstruir a descrição do produto armazenada.");
    }
}
