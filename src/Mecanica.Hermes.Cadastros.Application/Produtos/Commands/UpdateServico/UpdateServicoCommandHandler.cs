using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.UpdateServico;

internal sealed class UpdateServicoCommandHandler(IServicoRepository repository, IUnitOfWork uow)
    : IRequestHandler<UpdateServicoCommand, Result<ServicoDto>>
{
    public async Task<Result<ServicoDto>> Handle(UpdateServicoCommand request, CancellationToken cancellationToken)
    {
        var servico = await repository.GetByIdAsync(request.Id);
        if (servico is null)
            return Result<ServicoDto>.NotFound();

        var result = servico.AtualizarDados(request.Descricao, request.Valor);
        if (result.IsFailure)
            return Result<ServicoDto>.BadRequest(result.Errors);

        repository.Update(servico);
        await uow.CommitAsync(cancellationToken);

        return Result<ServicoDto>.Ok(servico.ToDto());
    }
}