using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarLinkPagamento;

public sealed record EnviarLinkPagamentoCommand(Guid PagamentoId) : IRequest<Result>;
