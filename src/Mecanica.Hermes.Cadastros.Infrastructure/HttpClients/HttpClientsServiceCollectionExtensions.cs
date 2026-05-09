using System.Diagnostics.CodeAnalysis;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Mecanica.Hermes.Cadastros.Infrastructure.HttpClients;

[ExcludeFromCodeCoverage(Justification = "Composition root de HttpClient — exercitada em integração.")]
internal static class HttpClientsServiceCollectionExtensions
{
    public static IServiceCollection AddHttpClientsConfiguration(this IServiceCollection services)
    {
        services.AddHttpClient<IOrdemServicoApiClient, OrdemServicoApiClient>(client =>
            {
                var baseUrl = Environment.GetEnvironmentVariable(VariableNames.OrdemServicoApiBaseUrl)
                              ?? throw new InvalidOperationException(
                                  $"{VariableNames.OrdemServicoApiBaseUrl} é obrigatória "
                                  + "para acionar a API de Ordem de Serviço.");
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler(ConfigureResilience);

        services.AddHttpClient<IPagamentoApiClient, PagamentoApiClient>(client =>
            {
                var baseUrl = Environment.GetEnvironmentVariable(VariableNames.PagamentoApiBaseUrl)
                              ?? throw new InvalidOperationException(
                                  $"{VariableNames.PagamentoApiBaseUrl} é obrigatória "
                                  + "para acionar a API de Pagamentos.");
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler(ConfigureResilience);

        return services;
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options)
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
        options.Retry.MaxRetryAttempts = 3;
    }
}
