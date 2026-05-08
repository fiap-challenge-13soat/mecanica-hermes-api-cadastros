using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.DeleteServico;

public sealed record DeleteServicoCommand(Guid Id) : IRequest<Result>;
