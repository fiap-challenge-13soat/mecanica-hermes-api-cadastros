using MassTransit;
using Mecanica.Hermes.Cadastros.Application.Common.Dtos;
using Mecanica.Hermes.Cadastros.Application.Common.Messaging;
using Mecanica.Hermes.Cadastros.Application.Common.Ports;
using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Messaging.Consumers;

/// <summary>
///     Consumer de <see cref="EnviarEmailCommand" /> que delega ao
///     <see cref="IEmailSenderService" /> (MailKit). Usado para desacoplar publicadores
///     (ex.: handlers de notificação) do envio síncrono ao SMTP — ganhamos retry,
///     redelivery e isolamento de falhas via MassTransit.
/// </summary>
public sealed partial class EnviarEmailConsumer(
    IEmailSenderService emailSender,
    ILogger<EnviarEmailConsumer> logger) : IConsumer<EnviarEmailCommand>
{
    public async Task Consume(ConsumeContext<EnviarEmailCommand> context)
    {
        var msg = context.Message;

        var emailResult = EmailVo.Criar(msg.To);
        if (emailResult.IsFailure || emailResult.Data is null)
        {
            LogEnderecoInvalido(logger, string.Join("; ", emailResult.Errors));
            return;
        }

        await emailSender.SendAsync(new EmailMessage(emailResult.Data, msg.Subject, msg.Body));
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "EnviarEmailCommand descartado — endereco invalido. Erros: {Errors}")]
    private static partial void LogEnderecoInvalido(ILogger logger, string errors);
}
