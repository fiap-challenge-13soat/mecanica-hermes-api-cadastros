using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.AddServico;

public sealed record AddServicoCommand(
    string Descricao,
    decimal Valor) : IRequest<Result<ServicoDto>>;