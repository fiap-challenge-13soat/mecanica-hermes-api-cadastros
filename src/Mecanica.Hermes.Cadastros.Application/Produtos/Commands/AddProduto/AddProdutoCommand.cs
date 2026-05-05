using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.AddProduto;

public sealed record AddProdutoCommand(
    string Descricao,
    decimal Valor,
    int Quantidade,
    string Tipo) : IRequest<Result<ProdutoDto>>;
