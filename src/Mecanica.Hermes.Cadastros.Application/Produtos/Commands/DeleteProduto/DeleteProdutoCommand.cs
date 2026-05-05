using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.DeleteProduto;

public sealed record DeleteProdutoCommand(Guid Id) : IRequest<Result>;
