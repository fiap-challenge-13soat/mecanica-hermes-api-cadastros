namespace Mecanica.Hermes.Cadastros.Application.Common.Messaging;

/// <summary>
///     Comando publicado no bus para que o consumer envie um e-mail ao destinatário.
///     Mantemos os campos como strings primitivas (sem ValueObjects do Domain) para
///     que o contrato de mensagem permaneça serializável e estável independente de
///     mudanças no Domain.
/// </summary>
public sealed record EnviarEmailCommand(string To, string Subject, string Body);
