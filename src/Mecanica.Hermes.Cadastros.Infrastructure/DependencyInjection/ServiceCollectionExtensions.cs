using System.Reflection;
using Mecanica.Hermes.Cadastros.Application;
using Mecanica.Hermes.Cadastros.Application.Common.Behaviors;
using Mecanica.Hermes.Shared.Application.Behaviors;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Domain;
using Mecanica.Hermes.Cadastros.Infrastructure.Emails.DependencyInjection;
using Mecanica.Hermes.Cadastros.Infrastructure.HttpClients;
using Mecanica.Hermes.Cadastros.Infrastructure.Messaging;
using Mecanica.Hermes.Cadastros.Infrastructure.Notificacoes;
using Mecanica.Hermes.Cadastros.Infrastructure.Observability;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.DependencyInjection;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Repositories;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Mecanica.Hermes.Cadastros.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void ConfigureInfrastructureServices(this IServiceCollection services, Assembly apiAssemblyMarker)
    {
        services.AddSingleton<CadastrosMetrics>();
        services.AddSingleton<IWebhookTokenSigner, HmacWebhookTokenSigner>();
        services.AddSingleton<INotificacoesUrlBuilder, NotificacoesUrlBuilder>();

        services.AddEntityFrameworkConfiguration();
        services.AddObservability();
        services.AddEmailConfiguration();
        services.AddMessagingConfiguration();
        services.AddHttpClientsConfiguration();
        services.AddRepositories();

        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = Environment.GetEnvironmentVariable(VariableNames.MediatRLicenseKey);
            cfg.RegisterServicesFromAssemblies(apiAssemblyMarker);
            cfg.RegisterServicesFromAssemblyContaining<IApplicationAssemblyMarker>();
            cfg.RegisterServicesFromAssemblyContaining<IDomainAssemblyMarker>();
            cfg.RegisterServicesFromAssemblyContaining<IInfrastructureAssemblyMarker>();
            cfg.RegisterGenericHandlers = true;
            cfg.AddOpenBehavior(typeof(TracingBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = Environment.GetEnvironmentVariable(VariableNames.AuthAuthority);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Environment.GetEnvironmentVariable(VariableNames.AuthIssuer),
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.AllowClienteScope, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var scopeClaims = context.User.FindAll("scope");
                    return scopeClaims.Any(scopeClaim =>
                    {
                        var scopes = scopeClaim.Value
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        return scopes.Contains(AuthPolicies.AllowClienteScope) ||
                               scopes.Contains(AuthPolicies.OnlyAdminScope);
                    });
                });
            })
            .AddPolicy(AuthPolicies.OnlyAdminScope, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var scopeClaims = context.User.FindAll("scope");
                    return scopeClaims.Any(scopeClaim =>
                    {
                        var scopes = scopeClaim.Value
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        return scopes.Contains(AuthPolicies.OnlyAdminScope);
                    });
                });
            })
            .AddPolicy(AuthPolicies.AllowServicoOrAdminScopePolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var scopeClaims = context.User.FindAll("scope");
                    return scopeClaims.Any(scopeClaim =>
                    {
                        var scopes = scopeClaim.Value
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        return scopes.Contains(AuthPolicies.OnlyAdminScope) ||
                               scopes.Contains(AuthPolicies.AllowServicoScope);
                    });
                });
            });
    }
}
