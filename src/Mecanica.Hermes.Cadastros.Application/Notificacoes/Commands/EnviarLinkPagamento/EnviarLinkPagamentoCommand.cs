using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarLinkPagamento;

public sealed record EnviarLinkPagamentoCommand(Guid PagamentoId) : IRequest<Result>;
