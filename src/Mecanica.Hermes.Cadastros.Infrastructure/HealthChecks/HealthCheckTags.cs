namespace Mecanica.Hermes.Cadastros.Infrastructure.HealthChecks;

/// <summary>
///     Tags de health checks usadas pela camada de Infrastructure (ex.: registro de
///     RabbitMQ via MassTransit). Os valores espelham os de
///     <c>Mecanica.Hermes.Shared.AspNetCore.HealthCheckTags</c> — duplicação intencional
///     de constantes para evitar que Infrastructure herde o <c>FrameworkReference
///     Microsoft.AspNetCore.App</c> via SDK só por causa de strings.
/// </summary>
internal static class HealthCheckTags
{
    public const string Ready = "ready";
    public const string Database = "database";
    public const string Messaging = "messaging";
}
