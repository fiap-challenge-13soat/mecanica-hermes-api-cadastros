using Mecanica.Hermes.Cadastros.Application.Common.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Emails.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static void AddEmailConfiguration(this IServiceCollection services)
    {
        services.AddSingleton(BuildEmailSenderOptions());
        services.AddScoped<IEmailSenderService, InternalEmailSenderService>();
    }

    private static EmailSenderOptions BuildEmailSenderOptions() => new()
    {
        Enabled = bool.TryParse(Environment.GetEnvironmentVariable("EmailSender__Enabled"), out var enabled) && enabled,
        SmtpServer = Environment.GetEnvironmentVariable("EmailSender__SmtpServer") ?? string.Empty,
        SmtpPort = int.TryParse(Environment.GetEnvironmentVariable("EmailSender__SmtpPort"), out var port) ? port : 25,
        SslRequired = bool.TryParse(Environment.GetEnvironmentVariable("EmailSender__SslRequired"), out var ssl) && ssl,
        UserName = Environment.GetEnvironmentVariable("EmailSender__UserName"),
        Password = Environment.GetEnvironmentVariable("EmailSender__Password"),
        SenderInformation = new EmailSenderOptions.EmailAddress
        {
            Name = Environment.GetEnvironmentVariable("EmailSender__SenderInformation__Name") ?? string.Empty,
            Address = Environment.GetEnvironmentVariable("EmailSender__SenderInformation__Address") ?? string.Empty
        }
    };
}
