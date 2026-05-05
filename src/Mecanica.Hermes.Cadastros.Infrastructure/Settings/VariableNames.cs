namespace Mecanica.Hermes.Cadastros.Infrastructure.Settings;

internal static class VariableNames
{
    public const string MediatRLicenseKey = "MediatR__LicenseKey";
    public const string DefaultConnectionString = "ConnectionStrings__DefaultConnection";
    public const string AuthAuthority = "AUTH__AUTHORITY";
    public const string AuthIssuer = "AUTH__ISSUER";
    public const string AuthClienteScope = "AUTH__CLIENTE_SCOPE";
    public const string AuthAdminScope = "AUTH__ADMIN_SCOPE";
    public const string ObservabilityEnabled = "OBSERVABILITY__ENABLED";

    public const string OtelServiceName = "OTEL_SERVICE_NAME";
    public const string OtelExporterOtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";
    public const string OtelExporterOtlpHeaders = "OTEL_EXPORTER_OTLP_HEADERS";
    public const string OtelExporterOtlpProtocol = "OTEL_EXPORTER_OTLP_PROTOCOL";
    public const string OtelExporterOtlpLogsEndpoint = "OTEL_EXPORTER_OTLP_LOGS_ENDPOINT";
    public const string OtelExporterOtlpLogsHeaders = "OTEL_EXPORTER_OTLP_LOGS_HEADERS";
    public const string OtelExporterOtlpLogsProtocol = "OTEL_EXPORTER_OTLP_LOGS_PROTOCOL";

    public const string EmailSenderEnabled = "EmailSender__Enabled";
    public const string EmailSenderSmtpServer = "EmailSender__SmtpServer";
    public const string EmailSenderSmtpPort = "EmailSender__SmtpPort";
    public const string EmailSenderSslRequired = "EmailSender__SslRequired";
    public const string EmailSenderUserName = "EmailSender__UserName";
    public const string EmailSenderPassword = "EmailSender__Password";
    public const string EmailSenderInfoName = "EmailSender__SenderInformation__Name";
    public const string EmailSenderInfoAddress = "EmailSender__SenderInformation__Address";

    public const string MassTransitTransportService = "MASSTRANSIT__TRANSPORT_SERVICE";
    public const string MassTransitConnectionString = "MASSTRANSIT__CONNECTION_STRING";
    public const string MassTransitQueueNamePrefix = "MASSTRANSIT__QUEUE_NAME_PREFIX";

    public const string OrdemServicoApiBaseUrl = "ORDEM_SERVICO_API__BASE_URL";
    public const string AppBaseUrl = "APP__BASE_URL";

    public const string RunMigrationsOnStartup = "RUN_MIGRATIONS_ON_STARTUP";
}
