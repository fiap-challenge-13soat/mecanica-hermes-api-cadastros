using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.AddCliente;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.AddVeiculo;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.DeleteCliente;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.DeleteVeiculo;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.UpdateCliente;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.UpdateVeiculo;
using Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteByEmail;
using Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteById;
using Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteByIdentificacaoFiscal;
using Mecanica.Hermes.Cadastros.Application.Clientes.Queries.ListAllClientes;
using Mecanica.Hermes.Cadastros.Application.Clientes.Queries.ListClientesByNome;
using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Tests.Common;
using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Domain.Common.Pagination;

namespace Mecanica.Hermes.Cadastros.Application.Tests.Clientes;

public class ClienteHandlersTests
{
    private readonly Mock<IClienteRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    // ---- AddCliente ----

    [Fact]
    public async Task AddCliente_DadosValidos_RetornaOk()
    {
        var handler = new AddClienteCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new AddClienteCommand(TestData.NomeCivil, null, TestData.Cpf, TestData.Email, TestData.Telefone),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Email.Should().Be(TestData.Email);
        _repository.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddCliente_DadosInvalidos_RetornaBadRequest()
    {
        var handler = new AddClienteCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new AddClienteCommand("a", null, "x", "y", "z"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task AddCliente_IdentificacaoFiscalDuplicada_RetornaConflict()
    {
        _repository.Setup(r => r.GetByIdentificacaoFiscalAsync(It.IsAny<IdentificacaoFiscalVo>()))
            .ReturnsAsync(TestData.NovoCliente());
        var handler = new AddClienteCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new AddClienteCommand(TestData.NomeCivil, null, TestData.Cpf, TestData.Email, TestData.Telefone),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("Identificação"));
    }

    [Fact]
    public async Task AddCliente_EmailDuplicado_RetornaConflict()
    {
        _repository.Setup(r => r.GetByEmailAsync(It.IsAny<EmailVo>()))
            .ReturnsAsync(TestData.NovoCliente());
        var handler = new AddClienteCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new AddClienteCommand(TestData.NomeCivil, null, TestData.Cpf, TestData.Email, TestData.Telefone),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("Email"));
    }

    // ---- UpdateCliente ----

    [Fact]
    public async Task UpdateCliente_ClienteExistente_AtualizaERetornaDto()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new UpdateClienteCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new UpdateClienteCommand(cliente.Id, "João Pereira", "Joana Silva", "novo@x.com", "11888887777"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.NomeCivil.Should().Be("João Pereira");
        _repository.Verify(r => r.Update(It.IsAny<Cliente>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCliente_NaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);
        var handler = new UpdateClienteCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new UpdateClienteCommand(Guid.NewGuid(), TestData.NomeCivil, null, TestData.Email, TestData.Telefone),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCliente_DadosInvalidos_RetornaBadRequest()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new UpdateClienteCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new UpdateClienteCommand(cliente.Id, "a", null, "x", "y"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ---- DeleteCliente ----

    [Fact]
    public async Task DeleteCliente_Existente_RetornaOk()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new DeleteClienteCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new DeleteClienteCommand(cliente.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        cliente.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCliente_NaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);
        var handler = new DeleteClienteCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new DeleteClienteCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCliente_JaExcluido_RetornaBadRequest()
    {
        var cliente = TestData.NovoCliente();
        cliente.Excluir();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new DeleteClienteCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new DeleteClienteCommand(cliente.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ---- AddVeiculo ----

    [Fact]
    public async Task AddVeiculo_ClienteExistentePlacaUnica_RetornaOk()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new AddVeiculoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new AddVeiculoCommand(cliente.Id, "Onix", "Chevrolet", TestData.Placa, 2022),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddVeiculo_ClienteNaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);
        var handler = new AddVeiculoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new AddVeiculoCommand(Guid.NewGuid(), "Onix", "Chevrolet", TestData.Placa, 2022),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task AddVeiculo_PlacaInvalida_RetornaBadRequest()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new AddVeiculoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new AddVeiculoCommand(cliente.Id, "Onix", "Chevrolet", "INVALIDA", 2022),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task AddVeiculo_DadosVeiculoInvalidos_RetornaBadRequest()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new AddVeiculoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new AddVeiculoCommand(cliente.Id, "", "", TestData.Placa, 1000),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ---- UpdateVeiculo ----

    [Fact]
    public async Task UpdateVeiculo_VeiculoExistente_AtualizaERetornaDto()
    {
        var cliente = TestData.NovoClienteComVeiculo();
        var veiculoId = cliente.Veiculos.First().Id;
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new UpdateVeiculoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new UpdateVeiculoCommand(cliente.Id, veiculoId, "Onix Plus", "Chevrolet", 2024),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateVeiculo_ClienteNaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);
        var handler = new UpdateVeiculoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new UpdateVeiculoCommand(Guid.NewGuid(), Guid.NewGuid(), "X", "Y", 2020),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateVeiculo_VeiculoNaoEncontrado_RetornaBadRequest()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new UpdateVeiculoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new UpdateVeiculoCommand(cliente.Id, Guid.NewGuid(), "X", "Y", 2020),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ---- DeleteVeiculo ----

    [Fact]
    public async Task DeleteVeiculo_Existente_RetornaOk()
    {
        var cliente = TestData.NovoClienteComVeiculo();
        var veiculoId = cliente.Veiculos.First().Id;
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new DeleteVeiculoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new DeleteVeiculoCommand(cliente.Id, veiculoId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteVeiculo_ClienteNaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);
        var handler = new DeleteVeiculoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new DeleteVeiculoCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteVeiculo_VeiculoNaoEncontrado_RetornaBadRequest()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new DeleteVeiculoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(
            new DeleteVeiculoCommand(cliente.Id, Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ---- Queries ----

    [Fact]
    public async Task GetClienteById_Existente_RetornaOk()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cliente);
        var handler = new GetClienteByIdQueryHandler(_repository.Object);

        var result = await handler.Handle(new GetClienteByIdQuery(cliente.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetClienteById_NaoExistente_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);
        var handler = new GetClienteByIdQueryHandler(_repository.Object);

        var result = await handler.Handle(new GetClienteByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetClienteByEmail_EmailValidoEExistente_RetornaOk()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByEmailAsync(It.IsAny<EmailVo>())).ReturnsAsync(cliente);
        var handler = new GetClienteByEmailQueryHandler(_repository.Object);

        var result = await handler.Handle(new GetClienteByEmailQuery(TestData.Email), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetClienteByEmail_EmailInvalido_RetornaBadRequest()
    {
        var handler = new GetClienteByEmailQueryHandler(_repository.Object);

        var result = await handler.Handle(new GetClienteByEmailQuery("invalido"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetClienteByEmail_NaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByEmailAsync(It.IsAny<EmailVo>())).ReturnsAsync((Cliente?)null);
        var handler = new GetClienteByEmailQueryHandler(_repository.Object);

        var result = await handler.Handle(new GetClienteByEmailQuery(TestData.Email), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetClienteByIdentificacaoFiscal_Valido_RetornaOk()
    {
        var cliente = TestData.NovoCliente();
        _repository.Setup(r => r.GetByIdentificacaoFiscalAsync(It.IsAny<IdentificacaoFiscalVo>()))
            .ReturnsAsync(cliente);
        var handler = new GetClienteByIdentificacaoFiscalQueryHandler(_repository.Object);

        var result = await handler.Handle(
            new GetClienteByIdentificacaoFiscalQuery(TestData.Cpf), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetClienteByIdentificacaoFiscal_Invalido_RetornaBadRequest()
    {
        var handler = new GetClienteByIdentificacaoFiscalQueryHandler(_repository.Object);

        var result = await handler.Handle(
            new GetClienteByIdentificacaoFiscalQuery("123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetClienteByIdentificacaoFiscal_NaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdentificacaoFiscalAsync(It.IsAny<IdentificacaoFiscalVo>()))
            .ReturnsAsync((Cliente?)null);
        var handler = new GetClienteByIdentificacaoFiscalQueryHandler(_repository.Object);

        var result = await handler.Handle(
            new GetClienteByIdentificacaoFiscalQuery(TestData.Cpf), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ListAllClientes_RetornaPagedResultMapeado()
    {
        var clientes = new[] { TestData.NovoCliente() };
        _repository.Setup(r => r.ListAllAsync(It.IsAny<PaginationParams>()))
            .ReturnsAsync(PagedResult<Cliente>.Create(clientes, 1, 10, 1));
        var handler = new ListAllClientesQueryHandler(_repository.Object);

        var result = await handler.Handle(new ListAllClientesQuery(1, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListClientesByNome_RetornaPagedResultMapeado()
    {
        _repository.Setup(r => r.ListByNomeAsync(It.IsAny<string>(), It.IsAny<PaginationParams>()))
            .ReturnsAsync(PagedResult<Cliente>.Create([TestData.NovoCliente()], 1, 10, 1));
        var handler = new ListClientesByNomeQueryHandler(_repository.Object);

        var result = await handler.Handle(new ListClientesByNomeQuery("Joao", 1, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
    }

    // ---- Mapper ----

    [Fact]
    public void ClienteMapper_ToDto_MapeiaCliente()
    {
        var cliente = TestData.NovoClienteComVeiculo();

        var dto = cliente.ToDto();

        dto.Id.Should().Be(cliente.Id);
        dto.NomeCivil.Should().Be(cliente.NomeCivil.Valor);
        dto.Email.Should().Be(cliente.Email.Valor);
        dto.Veiculos.Should().HaveCount(1);
    }
}
