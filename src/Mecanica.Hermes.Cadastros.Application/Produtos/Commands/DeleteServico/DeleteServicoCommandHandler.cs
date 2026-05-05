using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.DeleteServico;

internal sealed class DeleteServicoCommandHandler(IServicoRepository repository, IUnitOfWork uow)
    : IRequestHandler<DeleteServicoCommand, Result>
{
    public async Task<Result> Handle(DeleteServicoCommand request, CancellationToken cancellationToken)
    {
        var servico = await repository.GetByIdAsync(request.Id);
        if (servico is null)
            return Result.NotFound();

        var result = servico.Excluir();
        if (result.IsFailure)
            return Result.BadRequest(result.Errors);

        repository.Update(servico);
        await uow.CommitAsync(cancellationToken);

        return Result.Ok();
    }
}
