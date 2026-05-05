using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Queries.GetProdutoById;

internal sealed class GetProdutoByIdQueryHandler(IProdutoRepository repository)
    : IRequestHandler<GetProdutoByIdQuery, Result<ProdutoDto>>
{
    public async Task<Result<ProdutoDto>> Handle(GetProdutoByIdQuery request, CancellationToken cancellationToken)
    {
        var produto = await repository.GetByIdAsync(request.Id);

        return produto == null
            ? Result<ProdutoDto>.NotFound()
            : Result<ProdutoDto>.Ok(produto.ToDto());
    }
}
