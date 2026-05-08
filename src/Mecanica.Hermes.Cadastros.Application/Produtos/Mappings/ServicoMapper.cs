using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;

internal static class ServicoMapper
{
    public static ServicoDto ToDto(this Servico servico)
    {
        return new ServicoDto(
            servico.Id,
            servico.Descricao.Valor,
            servico.Valor.Valor);
    }
}
