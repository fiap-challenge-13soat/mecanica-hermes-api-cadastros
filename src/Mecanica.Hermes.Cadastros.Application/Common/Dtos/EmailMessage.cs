using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Application.Common.Dtos;

public record EmailMessage(EmailVo To, string Subject, string Body);
