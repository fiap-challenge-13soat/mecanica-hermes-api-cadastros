using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteByIdentificacaoFiscal;

public sealed record GetClienteByIdentificacaoFiscalQuery(string IdentificacaoFiscal) : IRequest<Result<ClienteDto>>;