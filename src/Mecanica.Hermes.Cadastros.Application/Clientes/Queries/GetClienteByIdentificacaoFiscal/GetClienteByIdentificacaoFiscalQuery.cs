using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteByIdentificacaoFiscal;

public sealed record GetClienteByIdentificacaoFiscalQuery(string IdentificacaoFiscal) : IRequest<Result<ClienteDto>>;