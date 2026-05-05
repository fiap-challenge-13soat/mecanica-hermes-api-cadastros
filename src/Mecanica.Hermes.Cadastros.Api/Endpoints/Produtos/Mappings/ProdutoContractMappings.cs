using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.AddProduto;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.UpdateProduto;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Mappings;

internal static class ProdutoContractMappings
{
    public static AddProdutoCommand ToCommand(this AddProdutoRequest request) =>
        new(request.Descricao, request.Valor, request.Quantidade, request.Tipo.ToString());

    public static UpdateProdutoCommand ToCommand(this UpdateProdutoRequest request, Guid id) =>
        new(id, request.Descricao, request.Valor);

    public static ProdutoResponse ToResponse(this ProdutoDto dto) =>
        new(dto.Id, dto.Descricao, dto.Valor, dto.Quantidade, dto.Tipo);
}
