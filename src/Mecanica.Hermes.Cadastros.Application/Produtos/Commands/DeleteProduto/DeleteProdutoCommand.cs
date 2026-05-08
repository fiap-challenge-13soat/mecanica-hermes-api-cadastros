using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.DeleteProduto;

public sealed record DeleteProdutoCommand(Guid Id) : IRequest<Result>;
