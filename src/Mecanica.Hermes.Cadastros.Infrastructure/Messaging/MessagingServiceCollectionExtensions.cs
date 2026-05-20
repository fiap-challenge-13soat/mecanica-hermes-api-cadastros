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
    // Amazon MQ for RabbitMQ recomenda heartbeat de 60s; valores menores derrubam a conexão
    // sob carga porque o broker gerenciado faz throttling agressivo em handshakes frequentes.
    private static readonly TimeSpan AmazonMqHeartbeat = TimeSpan.FromSeconds(60);

    /// <summary>
    ///     Configura o MassTransit para envio de e-mails (sem SAGA). Em produção usa Amazon MQ
    ///     (engine RabbitMQ, AMQPS sobre TLS) por default; aceita <c>RabbitMq</c> para broker
    ///     auto-hospedado (dev/local) e <c>None</c> para cair em transport in-memory
    ///     (testes unitários).
    /// </summary>
    public static IServiceCollection AddMessagingConfiguration(this IServiceCollection services)
    {
        var transport = ResolveTransport();
        var queuePrefix = Environment.GetEnvironmentVariable(VariableNames.MassTransitQueueNamePrefix)
                          ?? "mecanica-hermes-cadastros";

        // DLQ observability: o observer captura `ConsumeFault` (após retries esgotados) e
        // emite métrica `messaging.consume_faults` + log estruturado. Permite alarmar quando
        // mensagens caem em massa na _error queue (sinal de bug, schema drift, dep down).
        services.AddSingleton<DlqMetrics>();
        services.AddSingleton<MessagingFaultObserver>();

        services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(
                new KebabCaseEndpointNameFormatter(prefix: queuePrefix, includeNamespace: false));
            x.AddConsumer<EnviarEmailConsumer>();
            x.AddConsumer<OrdemDeServicoAguardandoAprovacaoConsumer>();
            x.AddConsumer<LinkPagamentoGeradoConsumer>();

            x.ConfigureHealthCheckOptions(opts =>
            {
                opts.MinimalFailureStatus = HealthStatus.Degraded;
                opts.Tags.Add(HealthCheckTags.Ready);
                opts.Tags.Add(HealthCheckTags.Messaging);
            });

            switch (transport)
            {
                case MessagingTransport.AmazonMq:
                    ConfigureAmazonMq(x);
                    break;
                case MessagingTransport.RabbitMq:
                    ConfigureRabbitMq(x);
                    break;
                default:
                    ConfigureInMemory(x);
                    break;
            }
        });

        return services;
    }

    private static void ConfigureRabbitMq(IBusRegistrationConfigurator x)
    {
        var connectionString = RequireConnectionString("RabbitMq");

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri(connectionString));
            cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
            cfg.ConnectConsumeObserver(context.GetRequiredService<MessagingFaultObserver>());
            cfg.ConfigureEndpoints(context);
        });
    }

    private static void ConfigureAmazonMq(IBusRegistrationConfigurator x)
    {
        var connectionString = RequireConnectionString("AmazonMq");

        x.UsingRabbitMq((context, cfg) =>
        {
            // SSL é ativado automaticamente quando o esquema é `amqps://`; protocolo TLS é
            // delegado ao .NET runtime / OS.
            cfg.Host(new Uri(connectionString), h => h.Heartbeat(AmazonMqHeartbeat));
            cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
            cfg.ConnectConsumeObserver(context.GetRequiredService<MessagingFaultObserver>());
            cfg.ConfigureEndpoints(context);
        });
    }

    private static void ConfigureInMemory(IBusRegistrationConfigurator x)
    {
        x.UsingInMemory((context, cfg) =>
        {
            cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
            cfg.ConnectConsumeObserver(context.GetRequiredService<MessagingFaultObserver>());
            cfg.ConfigureEndpoints(context);
        });
    }

    private static string RequireConnectionString(string transportLabel)
        => Environment.GetEnvironmentVariable(VariableNames.MassTransitConnectionString)
           ?? throw new InvalidOperationException(
               $"{VariableNames.MassTransitConnectionString} é obrigatória quando " +
               $"{VariableNames.MassTransitTransportService}={transportLabel}.");

    // Default = AmazonMq quando env var ausente. `None` (ou qualquer valor não reconhecido
    // explicitamente como RabbitMq/AmazonMq) cai em in-memory para testes unitários.
    private static MessagingTransport ResolveTransport()
    {
        var raw = Environment.GetEnvironmentVariable(VariableNames.MassTransitTransportService);
        if (string.IsNullOrWhiteSpace(raw))
            return MessagingTransport.AmazonMq;

        if (string.Equals(raw, "AmazonMq", StringComparison.OrdinalIgnoreCase))
            return MessagingTransport.AmazonMq;
        if (string.Equals(raw, "RabbitMq", StringComparison.OrdinalIgnoreCase))
            return MessagingTransport.RabbitMq;
        return MessagingTransport.InMemory;
    }

    private enum MessagingTransport
    {
        InMemory,
        RabbitMq,
        AmazonMq
    }
}
