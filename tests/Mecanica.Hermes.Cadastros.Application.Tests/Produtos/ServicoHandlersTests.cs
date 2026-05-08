using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.AddServico;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.DeleteServico;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.UpdateServico;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Application.Produtos.Queries.GetServicoById;
using Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListAllServicos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListServicosByDescricao;
using Mecanica.Hermes.Cadastros.Application.Tests.Common;
using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;
using Mecanica.Hermes.Shared.Core.Pagination;

namespace Mecanica.Hermes.Cadastros.Application.Tests.Produtos;

public class ServicoHandlersTests
{
    private readonly Mock<IServicoRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private static PagedResult<Servico> EmptyPage =>
        PagedResult<Servico>.Create(Array.Empty<Servico>(), 1, 10, 0);

    [Fact]
    public async Task AddServico_DadosValidos_RetornaOk()
    {
        _repository.Setup(r => r.ListByDescricaoAsync(It.IsAny<DescricaoProdutoVo>(), It.IsAny<PaginationParams>()))
            .ReturnsAsync(EmptyPage);
        var handler = new AddServicoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new AddServicoCommand("Troca", 100m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddServico_DadosInvalidos_RetornaBadRequest()
    {
        var handler = new AddServicoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new AddServicoCommand("", -1m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task AddServico_DescricaoDuplicada_RetornaConflict()
    {
        _repository.Setup(r => r.ListByDescricaoAsync(It.IsAny<DescricaoProdutoVo>(), It.IsAny<PaginationParams>()))
            .ReturnsAsync(PagedResult<Servico>.Create([TestData.NovoServico()], 1, 1, 1));
        var handler = new AddServicoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new AddServicoCommand("Troca", 100m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateServico_Existente_RetornaOk()
    {
        var servico = TestData.NovoServico();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(servico);
        var handler = new UpdateServicoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new UpdateServicoCommand(servico.Id, "Novo", 200m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateServico_NaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Servico?)null);
        var handler = new UpdateServicoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new UpdateServicoCommand(Guid.NewGuid(), "X", 1m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateServico_DadosInvalidos_RetornaBadRequest()
    {
        var servico = TestData.NovoServico();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(servico);
        var handler = new UpdateServicoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new UpdateServicoCommand(servico.Id, "", -1m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteServico_Existente_RetornaOk()
    {
        var servico = TestData.NovoServico();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(servico);
        var handler = new DeleteServicoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new DeleteServicoCommand(servico.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteServico_NaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Servico?)null);
        var handler = new DeleteServicoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new DeleteServicoCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteServico_JaExcluido_RetornaBadRequest()
    {
        var servico = TestData.NovoServico();
        servico.Excluir();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(servico);
        var handler = new DeleteServicoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new DeleteServicoCommand(servico.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetServicoById_Existente_RetornaOk()
    {
        var servico = TestData.NovoServico();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(servico);
        var handler = new GetServicoByIdQueryHandler(_repository.Object);

        var result = await handler.Handle(new GetServicoByIdQuery(servico.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetServicoById_NaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Servico?)null);
        var handler = new GetServicoByIdQueryHandler(_repository.Object);

        var result = await handler.Handle(new GetServicoByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ListAllServicos_RetornaPagedResult()
    {
        _repository.Setup(r => r.ListAllAsync(It.IsAny<PaginationParams>()))
            .ReturnsAsync(PagedResult<Servico>.Create([TestData.NovoServico()], 1, 10, 1));
        var handler = new ListAllServicosQueryHandler(_repository.Object);

        var result = await handler.Handle(new ListAllServicosQuery(1, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListServicosByDescricao_DescricaoValida_RetornaOk()
    {
        _repository.Setup(r => r.ListByDescricaoAsync(It.IsAny<DescricaoProdutoVo>(), It.IsAny<PaginationParams>()))
            .ReturnsAsync(PagedResult<Servico>.Create([TestData.NovoServico()], 1, 10, 1));
        var handler = new ListServicoByDescricaoQueryHandler(_repository.Object);

        var result = await handler.Handle(new ListServicoByDescricaoQuery("Troca", 1, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ListServicosByDescricao_DescricaoInvalida_RetornaBadRequest()
    {
        var handler = new ListServicoByDescricaoQueryHandler(_repository.Object);

        var result = await handler.Handle(new ListServicoByDescricaoQuery("", 1, 10), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ServicoMapper_ToDto_MapeiaServico()
    {
        var servico = TestData.NovoServico();

        var dto = servico.ToDto();

        dto.Id.Should().Be(servico.Id);
        dto.Descricao.Should().Be(servico.Descricao.Valor);
    }
}
