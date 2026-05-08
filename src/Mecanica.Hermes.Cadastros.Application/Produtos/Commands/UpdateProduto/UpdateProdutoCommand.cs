using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.UpdateProduto;

public sealed record UpdateProdutoCommand(
    Guid Id,
    string Descricao,
    decimal Valor) : IRequest<Result<ProdutoDto>>;
