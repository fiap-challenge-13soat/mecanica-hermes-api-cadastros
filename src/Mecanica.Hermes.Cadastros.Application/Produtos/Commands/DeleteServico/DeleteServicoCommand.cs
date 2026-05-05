using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.DeleteServico;

public sealed record DeleteServicoCommand(Guid Id) : IRequest<Result>;
