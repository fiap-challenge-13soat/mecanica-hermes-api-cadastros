using Mecanica.Hermes.Cadastros.Application.Common.Dtos;

namespace Mecanica.Hermes.Cadastros.Application.Common.Ports;

public interface IEmailSenderService
{
    Task SendAsync(EmailMessage message);
}