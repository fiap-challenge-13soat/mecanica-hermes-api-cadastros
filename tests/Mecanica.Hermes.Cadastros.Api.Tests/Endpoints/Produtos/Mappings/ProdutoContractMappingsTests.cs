using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos.Enums;

namespace Mecanica.Hermes.Cadastros.Api.Tests.Endpoints.Produtos.Mappings;

public class ProdutoContractMappingsTests
{
    [Fact]
    public void AddProdutoRequest_ToCommand_ConverteEnumParaString()
    {
        var req = new AddProdutoRequest("Filtro", 50m, 10, TipoProduto.Peca);

        var cmd = req.ToCommand();

        cmd.Descricao.Should().Be(req.Descricao);
        cmd.Valor.Should().Be(req.Valor);
        cmd.Quantidade.Should().Be(req.Quantidade);
        cmd.Tipo.Should().Be("Peca");
    }

    [Fact]
    public void UpdateProdutoRequest_ToCommand_PreencheId()
    {
        var id = Guid.NewGuid();
        var req = new UpdateProdutoRequest("Filtro", 5, 50m);

        var cmd = req.ToCommand(id);

        cmd.Id.Should().Be(id);
        cmd.Descricao.Should().Be(req.Descricao);
        cmd.Valor.Should().Be(req.Valor);
    }

    [Fact]
    public void ProdutoDto_ToResponse_MapeiaCampos()
    {
        var dto = new ProdutoDto(Guid.NewGuid(), "Filtro", 50m, 10, "Peca");

        var resp = dto.ToResponse();

        resp.Id.Should().Be(dto.Id);
        resp.Descricao.Should().Be(dto.Descricao);
        resp.Valor.Should().Be(dto.Valor);
        resp.Quantidade.Should().Be(dto.Quantidade);
        resp.Tipo.Should().Be(dto.Tipo);
    }
}
