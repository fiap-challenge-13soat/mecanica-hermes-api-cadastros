using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.DeleteProduto;

internal sealed class DeleteProdutoCommandHandler(IProdutoRepository repository, IUnitOfWork uow)
    : IRequestHandler<DeleteProdutoCommand, Result>
{
    public async Task<Result> Handle(DeleteProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await repository.GetByIdAsync(request.Id);
        if (produto is null)
            return Result.NotFound();

        var result = produto.Excluir();
        if (result.IsFailure)
            return Result.BadRequest(result.Errors);

        repository.Update(produto);
        await uow.CommitAsync(cancellationToken);

        return Result.Ok();
    }
}
