using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;

namespace Mecanica.Hermes.Cadastros.Api.Tests.Endpoints.Produtos.Mappings;

public class ServicoContractMappingsTests
{
    [Fact]
    public void AddServicoRequest_ToCommand_MapeiaCampos()
    {
        var req = new AddServicoRequest("Troca", 100m);

        var cmd = req.ToCommand();

        cmd.Descricao.Should().Be(req.Descricao);
        cmd.Valor.Should().Be(req.Valor);
    }

    [Fact]
    public void UpdateServicoRequest_ToCommand_PreencheId()
    {
        var id = Guid.NewGuid();
        var req = new UpdateServicoRequest("Troca", 200m);

        var cmd = req.ToCommand(id);

        cmd.Id.Should().Be(id);
        cmd.Descricao.Should().Be(req.Descricao);
        cmd.Valor.Should().Be(req.Valor);
    }

    [Fact]
    public void ServicoDto_ToResponse_MapeiaCampos()
    {
        var dto = new ServicoDto(Guid.NewGuid(), "Troca", 100m);

        var resp = dto.ToResponse();

        resp.Id.Should().Be(dto.Id);
        resp.Descricao.Should().Be(dto.Descricao);
        resp.Valor.Should().Be(dto.Valor);
    }
}
