using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarOrcamentoParaAprovacao;

public sealed record EnviarOrcamentoParaAprovacaoCommand(Guid OrdemDeServicoId) : IRequest<Result>;
