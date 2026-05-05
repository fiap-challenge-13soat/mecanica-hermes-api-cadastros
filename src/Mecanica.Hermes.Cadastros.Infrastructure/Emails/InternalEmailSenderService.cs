using MailKit.Net.Smtp;
using Mecanica.Hermes.Cadastros.Application.Common.Dtos;
using Mecanica.Hermes.Cadastros.Application.Common.Ports;
using MimeKit;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Emails;

internal class InternalEmailSenderService(EmailSenderOptions options) : IEmailSenderService
{
    public async Task SendAsync(EmailMessage message)
    {
        if (!options.Enabled)
            return;

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(options.SmtpServer, options.SmtpPort, options.SslRequired);
            if (!string.IsNullOrWhiteSpace(options.UserName) && !string.IsNullOrWhiteSpace(options.Password))
                await client.AuthenticateAsync(options.UserName, options.Password);

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(options.SenderInformation.Name, options.SenderInformation.Address));
            mimeMessage.To.Add(new MailboxAddress("", message.To.Valor));
            mimeMessage.Subject = message.Subject;
            mimeMessage.Body = new BodyBuilder { HtmlBody = message.Body }.ToMessageBody();

            await client.SendAsync(mimeMessage);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true);
        }
    }
}