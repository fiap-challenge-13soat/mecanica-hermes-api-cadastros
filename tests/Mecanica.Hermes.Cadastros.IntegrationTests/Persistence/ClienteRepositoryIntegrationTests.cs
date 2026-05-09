using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence;
using Mecanica.Hermes.Cadastros.IntegrationTests.Infrastructure;
using Mecanica.Hermes.Shared.Core.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Mecanica.Hermes.Cadastros.IntegrationTests.Persistence;

[Collection(IntegrationTestCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ClienteRepositoryIntegrationTests
{
    private readonly IntegrationTestWebApplicationFactory _factory;

    public ClienteRepositoryIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task LimparAsync(AppDbContext db)
    {
        await db.Clientes.ExecuteDeleteAsync();
    }

    private static Cliente NovoCliente(string cpf = "52998224725", string email = "joao.silva@empresa.com",
        string nome = "João Silva")
    {
        return Cliente.Criar(nome, null, cpf, email, "11987654321").Data!;
    }

    [Fact]
    public async Task AddAsync_PersisteClienteEPermiteRecuperarPorId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<
            Cadastros.Application.Common.Persistence.IUnitOfWork>();
        await LimparAsync(db);

        var cliente = NovoCliente();
        await sut.AddAsync(cliente);
        await uow.CommitAsync(CancellationToken.None);

        var recuperado = await sut.GetByIdAsync(cliente.Id);
        recuperado.Should().NotBeNull();
        recuperado!.Email.Valor.Should().Be("joao.silva@empresa.com");
    }

    [Fact]
    public async Task GetByIdAsync_QuandoNaoExiste_RetornaNull()
    {
        using var scope = _factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IClienteRepository>();

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmail_RetornaCliente()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<
            Cadastros.Application.Common.Persistence.IUnitOfWork>();
        await LimparAsync(db);

        var cliente = NovoCliente(email: "by.email@test.com", cpf: "11144477735");
        await sut.AddAsync(cliente);
        await uow.CommitAsync(CancellationToken.None);

        var result = await sut.GetByEmailAsync(EmailVo.Criar("by.email@test.com").Data!);
        result.Should().NotBeNull();
        result!.Id.Should().Be(cliente.Id);
    }

    [Fact]
    public async Task GetByIdentificacaoFiscal_RetornaCliente()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<
            Cadastros.Application.Common.Persistence.IUnitOfWork>();
        await LimparAsync(db);

        var cliente = NovoCliente(cpf: "11144477735", email: "by.cpf@test.com");
        await sut.AddAsync(cliente);
        await uow.CommitAsync(CancellationToken.None);

        var result = await sut.GetByIdentificacaoFiscalAsync(
            IdentificacaoFiscalVo.Criar("11144477735").Data!);
        result.Should().NotBeNull();
        result!.Id.Should().Be(cliente.Id);
    }

    [Fact]
    public async Task ListByNomeAsync_FiltraPorNomeIgnorandoAcentoECase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<
            Cadastros.Application.Common.Persistence.IUnitOfWork>();
        await LimparAsync(db);

        await sut.AddAsync(NovoCliente(nome: "João Pereira", email: "joao.p@test.com", cpf: "52998224725"));
        await sut.AddAsync(NovoCliente(nome: "Maria Souza", email: "maria.s@test.com", cpf: "11144477735"));
        await sut.AddAsync(NovoCliente(nome: "Joana Lima", email: "joana.l@test.com", cpf: "01234567890"));
        await uow.CommitAsync(CancellationToken.None);

        // "joa" deve casar com João, Joana — case e acento insensível
        var result = await sut.ListByNomeAsync("joa", new PaginationParams { Page = 1, PageSize = 50 });

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().Contain(c => c.NomeCivil.Valor == "João Pereira");
        result.Items.Should().Contain(c => c.NomeCivil.Valor == "Joana Lima");
    }

    [Fact]
    public async Task ListAllAsync_PaginaCorretamente()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sut = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<
            Cadastros.Application.Common.Persistence.IUnitOfWork>();
        await LimparAsync(db);

        var cpfs = new[] { "52998224725", "11144477735", "01234567890" };
        var nomes = new[] { "Alice Alves", "Bruno Borges", "Carla Costa" };
        for (var i = 0; i < cpfs.Length; i++)
        {
            await sut.AddAsync(NovoCliente(nome: nomes[i], email: $"u{i}@x.com", cpf: cpfs[i]));
        }

        await uow.CommitAsync(CancellationToken.None);

        var page1 = await sut.ListAllAsync(new PaginationParams { Page = 1, PageSize = 2 });
        page1.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(3);
        page1.Items.First().NomeCivil.Valor.Should().Be("Alice Alves");

        var page2 = await sut.ListAllAsync(new PaginationParams { Page = 2, PageSize = 2 });
        page2.Items.Should().HaveCount(1);
        page2.Items.First().NomeCivil.Valor.Should().Be("Carla Costa");
    }

    [Fact]
    public async Task Update_PersisteAlteracaoDeNomeEEmail()
    {
        Cliente cliente;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sut = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<
                Cadastros.Application.Common.Persistence.IUnitOfWork>();
            await LimparAsync(db);

            cliente = NovoCliente();
            await sut.AddAsync(cliente);
            await uow.CommitAsync(CancellationToken.None);
        }

        // Scope fresco — DbContext novo não rastreia a entity do Add anterior
        using (var scope = _factory.Services.CreateScope())
        {
            var sut = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<
                Cadastros.Application.Common.Persistence.IUnitOfWork>();

            cliente.AtualizarDados("José Silva", null, "jose.silva@empresa.com", "11999998888");
            sut.Update(cliente);
            await uow.CommitAsync(CancellationToken.None);

            var recuperado = await sut.GetByIdAsync(cliente.Id);
            recuperado!.NomeCivil.Valor.Should().Be("José Silva");
            recuperado.Email.Valor.Should().Be("jose.silva@empresa.com");
        }
    }
}
