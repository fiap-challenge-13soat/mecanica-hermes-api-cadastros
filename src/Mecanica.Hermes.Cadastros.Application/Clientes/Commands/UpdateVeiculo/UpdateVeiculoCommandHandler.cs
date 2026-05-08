using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.UpdateVeiculo;

internal sealed class UpdateVeiculoCommandHandler(IClienteRepository repository, IUnitOfWork uow)
    : IRequestHandler<UpdateVeiculoCommand, Result<ClienteDto>>
{
    public async Task<Result<ClienteDto>> Handle(UpdateVeiculoCommand request, CancellationToken cancellationToken)
    {
        var cliente = await repository.GetByIdAsync(request.ClienteId);

        if (cliente is null)
            return Result<ClienteDto>.NotFound();

        var result = cliente.AlterarVeiculo(request.VeiculoId, request.Modelo, request.Marca, request.Ano);
        if (result.IsFailure)
            return Result<ClienteDto>.BadRequest(result.Errors);

        repository.Update(cliente);
        await uow.CommitAsync(cancellationToken);

        return Result<ClienteDto>.Ok(cliente.ToDto());
    }
}
