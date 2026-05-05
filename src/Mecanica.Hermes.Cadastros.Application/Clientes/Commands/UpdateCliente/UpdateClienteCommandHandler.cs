using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.UpdateCliente;

internal sealed class UpdateClienteCommandHandler(IClienteRepository repository, IUnitOfWork uow)
    : IRequestHandler<UpdateClienteCommand, Result<ClienteDto>>
{
    public async Task<Result<ClienteDto>> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await repository.GetByIdAsync(request.Id);

        if (cliente is null)
            return Result<ClienteDto>.NotFound();

        var result = cliente.AtualizarDados(
            request.NomeCivil,
            request.NomeSocial,
            request.Email,
            request.Telefone);

        if (result.IsFailure)
            return Result<ClienteDto>.BadRequest(result.Errors);

        repository.Update(cliente);
        await uow.CommitAsync(cancellationToken);

        return Result<ClienteDto>.Ok(cliente.ToDto());
    }
}