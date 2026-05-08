using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Queries.GetServicoById;

internal sealed class GetServicoByIdQueryHandler : IRequestHandler<GetServicoByIdQuery, Result<ServicoDto>>
{
    private readonly IServicoRepository _repository;

    public GetServicoByIdQueryHandler(IServicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ServicoDto>> Handle(GetServicoByIdQuery request, CancellationToken cancellationToken)
    {
        var servico = await _repository.GetByIdAsync(request.Id);

        return servico == null
            ? Result<ServicoDto>.NotFound()
            : Result<ServicoDto>.Ok(servico.ToDto());
    }
}