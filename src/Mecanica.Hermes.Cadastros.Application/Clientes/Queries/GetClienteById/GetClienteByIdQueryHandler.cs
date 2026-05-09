using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteById;

internal sealed class GetClienteByIdQueryHandler(IClienteRepository repository)
    : IRequestHandler<GetClienteByIdQuery, Result<ClienteDto>>
{
    public async Task<Result<ClienteDto>> Handle(GetClienteByIdQuery request, CancellationToken cancellationToken)
    {
        var cliente = await repository.GetByIdAsync(request.Id);

        return cliente == null
            ? Result<ClienteDto>.NotFound()
            : Result<ClienteDto>.Ok(cliente.ToDto());
    }
}
