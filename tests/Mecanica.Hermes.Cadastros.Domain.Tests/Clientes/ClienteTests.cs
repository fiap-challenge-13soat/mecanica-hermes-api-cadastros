using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Clientes.Events;
using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Clientes;

public class ClienteTests
{
    private const string Cpf = "52998224725";
    private const string Email = "joao@empresa.com";
    private const string Telefone = "11987654321";
    private const string NomeCivil = "João Silva";

    [Fact]
    public void Criar_DadosValidos_RetornaClienteComEventoCriado()
    {
        var result = Cliente.Criar(NomeCivil, null, Cpf, Email, Telefone);

        result.IsSuccess.Should().BeTrue();
        result.Data!.NomeCivil.Valor.Should().Be(NomeCivil);
        result.Data.NomeSocial.Should().BeNull();
        result.Data.Email.Valor.Should().Be(Email);
        result.Data.IdentificacaoFiscal.Valor.Should().Be(Cpf);
        result.Data.Telefone.Valor.Should().Be(Telefone);
        result.Data.Veiculos.Should().BeEmpty();
        result.Data.DomainEvents.Should().ContainSingle(e => e is ClienteCriadoEvent);
    }

    [Fact]
    public void Criar_ComNomeSocial_PreencheNomeSocial()
    {
        var result = Cliente.Criar(NomeCivil, "Joana Silva", Cpf, Email, Telefone);

        result.IsSuccess.Should().BeTrue();
        result.Data!.NomeSocial!.Valor.Should().Be("Joana Silva");
    }

    [Fact]
    public void Criar_DadosInvalidos_AcumulaErros()
    {
        var result = Cliente.Criar("a", "  ", "123", "email-ruim", "1");

        result.IsFailure.Should().BeTrue();
        result.Errors.Count.Should().BeGreaterThan(2);
    }

    [Fact]
    public void AtualizarDados_DadosValidos_AdicionaEventoEMarcaUpdated()
    {
        var cliente = NovoCliente();

        var result = cliente.AtualizarDados("João Pereira", "Joana Silva", "novo@x.com", "11888887777");

        result.IsSuccess.Should().BeTrue();
        cliente.NomeCivil.Valor.Should().Be("João Pereira");
        cliente.NomeSocial!.Valor.Should().Be("Joana Silva");
        cliente.Email.Valor.Should().Be("novo@x.com");
        cliente.UpdatedAt.Should().NotBeNull();
        cliente.DomainEvents.Should().Contain(e => e is ClienteAlteradoEvent);
    }

    [Fact]
    public void AtualizarDados_Invalidos_RetornaBadRequest()
    {
        var cliente = NovoCliente();

        var result = cliente.AtualizarDados("a", null, "x", "1");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AdicionarVeiculo_PlacaUnica_AdicionaEFiltraEvento()
    {
        var cliente = NovoCliente();

        var result = cliente.AdicionarVeiculo("Onix", "Chevrolet", "ABC1D23", 2022);

        result.IsSuccess.Should().BeTrue();
        cliente.Veiculos.Should().ContainSingle();
        cliente.DomainEvents.Should().Contain(e => e is VeiculoAdicionadoEvent);
    }

    [Fact]
    public void AdicionarVeiculo_PlacaInvalida_RetornaBadRequest()
    {
        var cliente = NovoCliente();

        var result = cliente.AdicionarVeiculo("Onix", "Chevrolet", "INVALIDA", 2022);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AdicionarVeiculo_DadosInvalidos_RetornaBadRequest()
    {
        var cliente = NovoCliente();

        var result = cliente.AdicionarVeiculo("", "", "ZZZ", 1);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AlterarVeiculo_VeiculoExistente_AtualizaCampos()
    {
        var cliente = NovoCliente();
        cliente.AdicionarVeiculo("Onix", "Chevrolet", "ABC1D23", 2022);
        var veiculoId = cliente.Veiculos.First().Id;

        var result = cliente.AlterarVeiculo(veiculoId, "Onix Plus", "Chevrolet", 2024);

        result.IsSuccess.Should().BeTrue();
        cliente.Veiculos.First().Modelo.Should().Be("Onix Plus");
        cliente.Veiculos.First().Ano.Should().Be(2024);
        cliente.DomainEvents.Should().Contain(e => e is VeiculoAlteradoEvent);
    }

    [Fact]
    public void AlterarVeiculo_NaoExistente_RetornaBadRequest()
    {
        var cliente = NovoCliente();

        var result = cliente.AlterarVeiculo(Guid.NewGuid(), "X", "Y", 2020);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("não encontrado"));
    }

    [Fact]
    public void AlterarVeiculo_DadosInvalidos_RetornaBadRequest()
    {
        var cliente = NovoCliente();
        cliente.AdicionarVeiculo("Onix", "Chevrolet", "ABC1D23", 2022);
        var id = cliente.Veiculos.First().Id;

        var result = cliente.AlterarVeiculo(id, "", "", 1899);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RemoverVeiculo_Existente_MarcaDeletado()
    {
        var cliente = NovoCliente();
        cliente.AdicionarVeiculo("Onix", "Chevrolet", "ABC1D23", 2022);
        var id = cliente.Veiculos.First().Id;

        var result = cliente.RemoverVeiculo(id);

        result.IsSuccess.Should().BeTrue();
        cliente.Veiculos.First().IsDeleted.Should().BeTrue();
        cliente.DomainEvents.Should().Contain(e => e is VeiculoRemovidoEvent);
    }

    [Fact]
    public void RemoverVeiculo_NaoExistente_RetornaNotFound()
    {
        var cliente = NovoCliente();

        var result = cliente.RemoverVeiculo(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Excluir_PrimeiraVez_MarcaIsDeleted()
    {
        var cliente = NovoCliente();

        var result = cliente.Excluir();

        result.IsSuccess.Should().BeTrue();
        cliente.IsDeleted.Should().BeTrue();
        cliente.DomainEvents.Should().Contain(e => e is ClienteExcluidoEvent);
    }

    [Fact]
    public void Excluir_JaExcluido_RetornaBadRequest()
    {
        var cliente = NovoCliente();
        cliente.Excluir();

        var result = cliente.Excluir();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("já foi excluído"));
    }

    [Fact]
    public void Restaurar_RestauraCamposBase()
    {
        var nome = NomeProprioVo.Criar(NomeCivil).Data!;
        var ident = IdentificacaoFiscalVo.Criar(Cpf).Data!;
        var email = EmailVo.Criar(Email).Data!;
        var tel = TelefoneVo.Criar(Telefone).Data!;
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);

        var cliente = Cliente.Restaurar(id, nome, null, ident, email, tel, [], createdAt, null, false);

        cliente.Id.Should().Be(id);
        cliente.CreatedAt.Should().Be(createdAt);
        cliente.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ClearDomainEvents_Esvazia()
    {
        var cliente = NovoCliente();
        cliente.DomainEvents.Should().NotBeEmpty();

        cliente.ClearDomainEvents();

        cliente.DomainEvents.Should().BeEmpty();
    }

    private static Cliente NovoCliente() =>
        Cliente.Criar(NomeCivil, null, Cpf, Email, Telefone).Data!;
}
