using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;
using Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;

namespace Mecanica.Hermes.Cadastros.Api.Tests.Endpoints.Clientes.Mappings;

public class ClienteContractMappingsTests
{
    [Fact]
    public void AddClienteRequest_ToCommand_MapeiaTodosCampos()
    {
        var req = new AddClienteRequest("Joao Silva", "Joana", "52998224725", "j@x.com", "11987654321");

        var cmd = req.ToCommand();

        cmd.NomeCivil.Should().Be(req.NomeCivil);
        cmd.NomeSocial.Should().Be(req.NomeSocial);
        cmd.IdentificacaoFiscal.Should().Be(req.IdentificacaoFiscal);
        cmd.Email.Should().Be(req.Email);
        cmd.Telefone.Should().Be(req.Telefone);
    }

    [Fact]
    public void UpdateClienteRequest_ToCommand_PreencheId()
    {
        var id = Guid.NewGuid();
        var req = new UpdateClienteRequest("Joao", "Joana", "j@x.com", "11987654321");

        var cmd = req.ToCommand(id);

        cmd.Id.Should().Be(id);
        cmd.NomeCivil.Should().Be(req.NomeCivil);
        cmd.NomeSocial.Should().Be(req.NomeSocial);
        cmd.Email.Should().Be(req.Email);
    }

    [Fact]
    public void AddVeiculoRequest_ToCommand_PreencheClienteId()
    {
        var clienteId = Guid.NewGuid();
        var req = new AddVeiculoRequest("Onix", "Chevrolet", "ABC1D23", 2022);

        var cmd = req.ToCommand(clienteId);

        cmd.ClienteId.Should().Be(clienteId);
        cmd.Modelo.Should().Be(req.Modelo);
        cmd.Marca.Should().Be(req.Marca);
        cmd.Placa.Should().Be(req.Placa);
        cmd.Ano.Should().Be(req.Ano);
    }

    [Fact]
    public void UpdateVeiculoRequest_ToCommand_PreencheClienteIdEVeiculoId()
    {
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var req = new UpdateVeiculoRequest("Onix", "Chevrolet", 2024);

        var cmd = req.ToCommand(clienteId, veiculoId);

        cmd.ClienteId.Should().Be(clienteId);
        cmd.VeiculoId.Should().Be(veiculoId);
        cmd.Modelo.Should().Be(req.Modelo);
    }

    [Fact]
    public void ClienteDto_ToResponse_MapeiaCamposEVeiculos()
    {
        var dto = new ClienteDto(
            Guid.NewGuid(), "Joao", "Joana", "52998224725", "j@x.com", "11987654321",
            [new VeiculoDto(Guid.NewGuid(), "Onix", "Chevrolet", "ABC1D23", 2022)]);

        var resp = dto.ToResponse();

        resp.Id.Should().Be(dto.Id);
        resp.NomeCivil.Should().Be(dto.NomeCivil);
        resp.NomeSocial.Should().Be(dto.NomeSocial);
        resp.Veiculos.Should().HaveCount(1);
        resp.Veiculos[0].Placa.Should().Be("ABC1D23");
    }

    [Fact]
    public void ClienteDto_ToResponse_NomeSocialNull_PreencheStringVazia()
    {
        var dto = new ClienteDto(Guid.NewGuid(), "Joao", null, "X", "Y", "Z", []);

        var resp = dto.ToResponse();

        resp.NomeSocial.Should().BeEmpty();
    }

    [Fact]
    public void VeiculoDto_ToResponse_MapeiaCampos()
    {
        var dto = new VeiculoDto(Guid.NewGuid(), "Onix", "Chevrolet", "ABC1D23", 2022);

        var resp = dto.ToResponse();

        resp.Id.Should().Be(dto.Id);
        resp.Modelo.Should().Be(dto.Modelo);
        resp.Placa.Should().Be(dto.Placa);
        resp.Ano.Should().Be(dto.Ano);
    }
}
