using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.AddProduto;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.DeleteProduto;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.UpdateProduto;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Application.Produtos.Queries.GetProdutoById;
using Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListAllProdutos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Queries.ListProdutosByDescricao;
using Mecanica.Hermes.Cadastros.Application.Tests.Common;
using Mecanica.Hermes.Cadastros.Domain.Common.Pagination;
using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Application.Tests.Produtos;

public class ProdutoHandlersTests
{
    private readonly Mock<IProdutoRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private static PagedResult<Produto> EmptyPage =>
        PagedResult<Produto>.Create(Array.Empty<Produto>(), 1, 10, 0);

    [Fact]
    public async Task AddProduto_DadosValidos_RetornaOk()
    {
        _repository.Setup(r => r.ListByDescricaoAsync(It.IsAny<DescricaoProdutoVo>(), It.IsAny<PaginationParams>()))
            .ReturnsAsync(EmptyPage);
        var handler = new AddProdutoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new AddProdutoCommand("Filtro", 50m, 10, "Peca"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.AddAsync(It.IsAny<Produto>()), Times.Once);
    }

    [Fact]
    public async Task AddProduto_DadosInvalidos_RetornaBadRequest()
    {
        var handler = new AddProdutoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new AddProdutoCommand("", -1m, 0, "x"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task AddProduto_DescricaoDuplicada_RetornaConflict()
    {
        var existente = TestData.NovoProduto();
        _repository.Setup(r => r.ListByDescricaoAsync(It.IsAny<DescricaoProdutoVo>(), It.IsAny<PaginationParams>()))
            .ReturnsAsync(PagedResult<Produto>.Create([existente], 1, 1, 1));
        var handler = new AddProdutoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new AddProdutoCommand("Filtro", 50m, 10, "Peca"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("descrição"));
    }

    [Fact]
    public async Task UpdateProduto_Existente_RetornaOk()
    {
        var produto = TestData.NovoProduto();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(produto);
        var handler = new UpdateProdutoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new UpdateProdutoCommand(produto.Id, "Novo Nome", 99m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProduto_NaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Produto?)null);
        var handler = new UpdateProdutoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new UpdateProdutoCommand(Guid.NewGuid(), "X", 1m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProduto_DadosInvalidos_RetornaBadRequest()
    {
        var produto = TestData.NovoProduto();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(produto);
        var handler = new UpdateProdutoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new UpdateProdutoCommand(produto.Id, "", -1m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProduto_Existente_RetornaOk()
    {
        var produto = TestData.NovoProduto();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(produto);
        var handler = new DeleteProdutoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new DeleteProdutoCommand(produto.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProduto_NaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Produto?)null);
        var handler = new DeleteProdutoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new DeleteProdutoCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProduto_JaExcluido_RetornaBadRequest()
    {
        var produto = TestData.NovoProduto();
        produto.Excluir();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(produto);
        var handler = new DeleteProdutoCommandHandler(_repository.Object, _uow.Object);

        var result = await handler.Handle(new DeleteProdutoCommand(produto.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetProdutoById_Existente_RetornaOk()
    {
        var produto = TestData.NovoProduto();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(produto);
        var handler = new GetProdutoByIdQueryHandler(_repository.Object);

        var result = await handler.Handle(new GetProdutoByIdQuery(produto.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetProdutoById_NaoEncontrado_RetornaNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Produto?)null);
        var handler = new GetProdutoByIdQueryHandler(_repository.Object);

        var result = await handler.Handle(new GetProdutoByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ListAllProdutos_RetornaPagedResult()
    {
        _repository.Setup(r => r.ListAllAsync(It.IsAny<PaginationParams>()))
            .ReturnsAsync(PagedResult<Produto>.Create([TestData.NovoProduto()], 1, 10, 1));
        var handler = new ListAllProdutosQueryHandler(_repository.Object);

        var result = await handler.Handle(new ListAllProdutosQuery(1, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListProdutosByDescricao_DescricaoValida_RetornaOk()
    {
        _repository.Setup(r => r.ListByDescricaoAsync(It.IsAny<DescricaoProdutoVo>(), It.IsAny<PaginationParams>()))
            .ReturnsAsync(PagedResult<Produto>.Create([TestData.NovoProduto()], 1, 10, 1));
        var handler = new ListProdutosByDescricaoQueryHandler(_repository.Object);

        var result = await handler.Handle(new ListProdutosByDescricaoQuery("Filtro", 1, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ListProdutosByDescricao_DescricaoInvalida_RetornaBadRequest()
    {
        var handler = new ListProdutosByDescricaoQueryHandler(_repository.Object);

        var result = await handler.Handle(new ListProdutosByDescricaoQuery("", 1, 10), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ProdutoMapper_ToDto_MapeiaProduto()
    {
        var produto = TestData.NovoProduto();

        var dto = produto.ToDto();

        dto.Id.Should().Be(produto.Id);
        dto.Descricao.Should().Be(produto.Descricao.Valor);
        dto.Tipo.Should().Be(produto.Tipo.ToString());
    }
}
