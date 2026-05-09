using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.UpdateServico;

public sealed record UpdateServicoCommand(
    Guid Id,
    string Descricao,
    decimal Valor) : IRequest<Result<ServicoDto>>;
