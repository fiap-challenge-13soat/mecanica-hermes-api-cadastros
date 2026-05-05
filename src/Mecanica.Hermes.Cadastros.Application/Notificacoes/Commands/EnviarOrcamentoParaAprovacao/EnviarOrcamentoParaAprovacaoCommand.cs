using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarOrcamentoParaAprovacao;

public sealed record EnviarOrcamentoParaAprovacaoCommand(Guid OrdemDeServicoId) : IRequest<Result>;
