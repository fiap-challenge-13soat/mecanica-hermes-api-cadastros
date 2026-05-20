using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Mecanica.Hermes.Cadastros.IntegrationTests.Infrastructure;

public sealed class AuthorizationTestWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    internal const string TestIssuer = "https://test-issuer";

    internal const string AdminScope = "admin-scope";
    internal const string ClientScope = "cliente-scope";

    private static readonly SymmetricSecurityKey TestSigningKey =
        new(Encoding.UTF8.GetBytes("test-signing-key-must-be-long-enough-for-hs256-algorithm!"));

    public Task InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing_Auth");

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(VariableNames.DefaultConnectionString)))
            Environment.SetEnvironmentVariable(VariableNames.DefaultConnectionString,
                "Host=localhost;Port=5432;Database=placeholder;Username=test;Password=test");
        Environment.SetEnvironmentVariable("AUTH__AUTHORITY", TestIssuer);
        Environment.SetEnvironmentVariable("AUTH__ISSUER", TestIssuer);
        Environment.SetEnvironmentVariable("RUN_MIGRATIONS_ON_STARTUP", "false");
        Environment.SetEnvironmentVariable("EmailSender__Enabled", "false");
        // Default mudou para AmazonMq em prod; testes precisam de in-memory transport.
        Environment.SetEnvironmentVariable("MASSTRANSIT__TRANSPORT_SERVICE", "None");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<NpgsqlDataSource>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<AppDbContext>();

            var efInMemoryProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase("auth-tests-db-" + Guid.NewGuid())
                    .UseInternalServiceProvider(efInMemoryProvider));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = null;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = TestIssuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TestSigningKey,
                    ClockSkew = TimeSpan.Zero
                };
            });
        });
    }

    internal static string GenerateToken(IEnumerable<string> scopes)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "test-user"),
            new(JwtRegisteredClaimNames.Iss, TestIssuer),
            new("scope", string.Join(" ", scopes))
        };

        var credentials = new SigningCredentials(TestSigningKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            TestIssuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
