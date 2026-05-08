using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Queries.GetProdutoById;

public sealed record GetProdutoByIdQuery(Guid Id) : IRequest<Result<ProdutoDto>>;
