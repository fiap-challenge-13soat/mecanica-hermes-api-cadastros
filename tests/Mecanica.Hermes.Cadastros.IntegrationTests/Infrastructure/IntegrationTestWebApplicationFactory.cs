using Mecanica.Hermes.Cadastros.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Mecanica.Hermes.Cadastros.IntegrationTests.Infrastructure;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public IntegrationTestWebApplicationFactory()
    {
        _dbContainer = new PostgreSqlBuilder("postgres:18.1")
            .WithDatabase("mecanica_hermes_cadastros_test")
            .WithUsername("postgres")
            .WithPassword("teste123A!")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("EmailSender__Enabled", "false");

        _ = CreateClient();

        try
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to run migrations. Connection String: {_dbContainer.GetConnectionString()}. Error: {ex.Message}",
                ex);
        }
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("AUTH__AUTHORITY", "https://test-authority");
        Environment.SetEnvironmentVariable("AUTH__ISSUER", "https://test-issuer");

        builder.UseEnvironment("Testing");
    }

    public override async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
