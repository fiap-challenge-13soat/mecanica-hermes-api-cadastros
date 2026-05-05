using System.Diagnostics.CodeAnalysis;
using MassTransit;
using Mecanica.Hermes.Cadastros.Infrastructure.HealthChecks;
using Mecanica.Hermes.Cadastros.Infrastructure.Messaging.Consumers;
using Mecanica.Hermes.Cadastros.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Messaging;

[ExcludeFromCodeCoverage(Justification = "Composition root do MassTransit — exercitada em integração.")]
internal static class MessagingServiceCollectionExtensions
{
    /// <summary>
    ///     Configura o MassTransit para envio de e-mails (sem SAGA). Em produção usa RabbitMQ
    ///     quando <c>MASSTRANSIT__TRANSPORT_SERVICE=RabbitMq</c>; em qualquer outro cenário
    ///     (Dev / Testing / unit tests) cai para o transport in-memory.
    /// </summary>
    public static IServiceCollection AddMessagingConfiguration(this IServiceCollection services)
    {
        var transport = Environment.GetEnvironmentVariable(VariableNames.MassTransitTransportService);
        var queuePrefix = Environment.GetEnvironmentVariable(VariableNames.MassTransitQueueNamePrefix)
                          ?? "mecanica-hermes-cadastros";

        services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(prefix: queuePrefix, includeNamespace: false));
            x.AddConsumer<EnviarEmailConsumer>();
            x.AddConsumer<OrdemDeServicoAguardandoAprovacaoConsumer>();

            x.ConfigureHealthCheckOptions(opts =>
            {
                opts.MinimalFailureStatus = HealthStatus.Degraded;
                opts.Tags.Add(HealthCheckTags.Ready);
                opts.Tags.Add(HealthCheckTags.Messaging);
            });

            if (string.Equals(transport, "RabbitMq", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = Environment.GetEnvironmentVariable(VariableNames.MassTransitConnectionString)
                                       ?? throw new InvalidOperationException(
                                           $"{VariableNames.MassTransitConnectionString} é obrigatória quando " +
                                           $"{VariableNames.MassTransitTransportService}=RabbitMq.");

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(connectionString));
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }
}
