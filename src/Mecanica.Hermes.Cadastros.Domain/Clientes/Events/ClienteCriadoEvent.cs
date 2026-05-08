using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes.Events;

public class ClienteCriadoEvent(IdentificacaoFiscalVo identificacaoFiscal) : IDomainEvent
{
    public IdentificacaoFiscalVo IdentificacaoFiscal { get; private set; } = identificacaoFiscal;
}