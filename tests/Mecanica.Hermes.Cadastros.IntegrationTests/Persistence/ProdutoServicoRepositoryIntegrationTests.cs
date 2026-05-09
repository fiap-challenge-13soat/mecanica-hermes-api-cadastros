using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Domain.Produtos.ValueObjects;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence;
using Mecanica.Hermes.Cadastros.IntegrationTests.Infrastructure;
using Mecanica.Hermes.Shared.Core.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Mecanica.Hermes.Cadastros.IntegrationTests.Persistence;

[Collection(IntegrationTestCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ProdutoServicoRepositoryIntegrationTests
{
    private readonly IntegrationTestWebApplicationFactory _factory;

    public ProdutoServicoRepositoryIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task LimparAsync(AppDbContext db)
    {
        await db.Produtos.ExecuteDeleteAsync();
        await db.Servicos.ExecuteDeleteAsync();
    }

    [Fact]
    public async Task ProdutoRepository_AddGetByIdRoundTrip()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IProdutoRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await LimparAsync(db);

        var produto = Produto.Criar("Filtro de Óleo", 50m, 10, "Peca").Data!;
        await sut.AddAsync(produto);
        await uow.CommitAsync(CancellationToken.None);

        var recuperado = await sut.GetByIdAsync(produto.Id);
        recuperado.Should().NotBeNull();
        recuperado!.Descricao.Valor.Should().Be("Filtro de Óleo");
    }

    [Fact]
    public async Task ProdutoRepository_GetByIdAsync_NaoExiste_RetornaNull()
    {
        using var scope = _factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IProdutoRepository>();

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ProdutoRepository_ListByDescricaoAsync_FiltroComUnaccent()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IProdutoRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await LimparAsync(db);

        await sut.AddAsync(Produto.Criar("Pastilha de Freio", 90m, 5, "Peca").Data!);
        await sut.AddAsync(Produto.Criar("Óleo de Motor", 35m, 100, "Peca").Data!);
        await sut.AddAsync(Produto.Criar("Filtro Lubrificante", 25m, 30, "Peca").Data!);
        await uow.CommitAsync(CancellationToken.None);

        var result = await sut.ListByDescricaoAsync(
            DescricaoProdutoVo.Criar("oleo").Data!,
            new PaginationParams { Page = 1, PageSize = 50 });

        result.Items.Should().ContainSingle()
            .Which.Descricao.Valor.Should().Be("Óleo de Motor");
    }

    [Fact]
    public async Task ProdutoRepository_ListAllAsync_PaginaOrdenadoPorDescricao()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IProdutoRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await LimparAsync(db);

        await sut.AddAsync(Produto.Criar("Zenith Item", 10m, 1, "Peca").Data!);
        await sut.AddAsync(Produto.Criar("Alfa Item", 10m, 1, "Peca").Data!);
        await sut.AddAsync(Produto.Criar("Mid Item", 10m, 1, "Peca").Data!);
        await uow.CommitAsync(CancellationToken.None);

        var page1 = await sut.ListAllAsync(new PaginationParams { Page = 1, PageSize = 2 });

        page1.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(3);
        page1.Items.First().Descricao.Valor.Should().Be("Alfa Item");
    }

    [Fact]
    public async Task ProdutoRepository_Update_PersisteAlteracao()
    {
        Produto produto;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sut = scope.ServiceProvider.GetRequiredService<IProdutoRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await LimparAsync(db);

            produto = Produto.Criar("Original", 10m, 1, "Peca").Data!;
            await sut.AddAsync(produto);
            await uow.CommitAsync(CancellationToken.None);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var sut = scope.ServiceProvider.GetRequiredService<IProdutoRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            produto.AtualizarDados("Atualizado", 20m);
            sut.Update(produto);
            await uow.CommitAsync(CancellationToken.None);

            var recuperado = await sut.GetByIdAsync(produto.Id);
            recuperado!.Descricao.Valor.Should().Be("Atualizado");
            recuperado.Valor.Valor.Should().Be(20m);
        }
    }

    [Fact]
    public async Task ServicoRepository_AddGetByIdRoundTrip()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IServicoRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await LimparAsync(db);

        var servico = Servico.Criar("Troca de Óleo", 120m).Data!;
        await sut.AddAsync(servico);
        await uow.CommitAsync(CancellationToken.None);

        var recuperado = await sut.GetByIdAsync(servico.Id);
        recuperado.Should().NotBeNull();
        recuperado!.Descricao.Valor.Should().Be("Troca de Óleo");
    }

    [Fact]
    public async Task ServicoRepository_ListByDescricaoAsync_FiltroComUnaccent()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IServicoRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await LimparAsync(db);

        await sut.AddAsync(Servico.Criar("Alinhamento", 80m).Data!);
        await sut.AddAsync(Servico.Criar("Revisão Geral", 250m).Data!);
        await uow.CommitAsync(CancellationToken.None);

        var result = await sut.ListByDescricaoAsync(
            DescricaoProdutoVo.Criar("revisao").Data!,
            new PaginationParams { Page = 1, PageSize = 50 });

        result.Items.Should().ContainSingle()
            .Which.Descricao.Valor.Should().Be("Revisão Geral");
    }

    [Fact]
    public async Task ServicoRepository_ListAllAsync_RetornaPaginado()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IServicoRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await LimparAsync(db);

        await sut.AddAsync(Servico.Criar("Beta", 10m).Data!);
        await sut.AddAsync(Servico.Criar("Alpha", 10m).Data!);
        await uow.CommitAsync(CancellationToken.None);

        var result = await sut.ListAllAsync(new PaginationParams { Page = 1, PageSize = 10 });
        result.Items.Should().HaveCount(2);
        result.Items.First().Descricao.Valor.Should().Be("Alpha");
    }

    [Fact]
    public async Task ServicoRepository_Update_PersisteAlteracao()
    {
        Servico servico;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sut = scope.ServiceProvider.GetRequiredService<IServicoRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await LimparAsync(db);

            servico = Servico.Criar("Antigo", 10m).Data!;
            await sut.AddAsync(servico);
            await uow.CommitAsync(CancellationToken.None);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var sut = scope.ServiceProvider.GetRequiredService<IServicoRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            servico.AtualizarDados("Novo", 30m);
            sut.Update(servico);
            await uow.CommitAsync(CancellationToken.None);

            var recuperado = await sut.GetByIdAsync(servico.Id);
            recuperado!.Descricao.Valor.Should().Be("Novo");
            recuperado.Valor.Valor.Should().Be(30m);
        }
    }
}
