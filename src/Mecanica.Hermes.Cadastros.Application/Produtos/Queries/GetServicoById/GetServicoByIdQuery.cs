using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Queries.GetServicoById;

public sealed record GetServicoByIdQuery(Guid Id) : IRequest<Result<ServicoDto>>;