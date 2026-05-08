using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.UpdateProduto;

internal sealed class UpdateProdutoCommandHandler(IProdutoRepository repository, IUnitOfWork uow)
    : IRequestHandler<UpdateProdutoCommand, Result<ProdutoDto>>
{
    public async Task<Result<ProdutoDto>> Handle(UpdateProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await repository.GetByIdAsync(request.Id);
        if (produto is null)
            return Result<ProdutoDto>.NotFound();

        var result = produto.AtualizarDados(request.Descricao, request.Valor);
        if (result.IsFailure)
            return Result<ProdutoDto>.BadRequest(result.Errors);

        repository.Update(produto);
        await uow.CommitAsync(cancellationToken);

        return Result<ProdutoDto>.Ok(produto.ToDto());
    }
}
