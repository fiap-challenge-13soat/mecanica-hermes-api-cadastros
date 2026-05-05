using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteByEmail;

internal sealed class GetClienteByEmailQueryHandler(IClienteRepository repository)
    : IRequestHandler<GetClienteByEmailQuery, Result<ClienteDto>>
{
    public async Task<Result<ClienteDto>> Handle(GetClienteByEmailQuery request, CancellationToken cancellationToken)
    {
        var email = EmailVo.Criar(request.Email);
        if (email.IsFailure)
            return Result<ClienteDto>.BadRequest(email.Errors);

        var cliente = await repository.GetByEmailAsync(email.Data!);

        return cliente == null
            ? Result<ClienteDto>.NotFound()
            : Result<ClienteDto>.Ok(cliente.ToDto());
    }
}