using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.AddVeiculo;

internal sealed class AddVeiculoCommandHandler(IClienteRepository repository, IUnitOfWork uow)
    : IRequestHandler<AddVeiculoCommand, Result<ClienteDto>>
{
    public async Task<Result<ClienteDto>> Handle(AddVeiculoCommand request, CancellationToken cancellationToken)
    {
        var cliente = await repository.GetByIdAsync(request.ClienteId);

        if (cliente is null)
            return Result<ClienteDto>.NotFound();

        var placa = PlacaVo.Criar(request.Placa);
        if (placa.IsFailure)
            return Result<ClienteDto>.BadRequest(placa.Errors);

        var veiculoJaCadastrado = cliente.Veiculos.Any(v => v.Placa == placa.Data);
        if (veiculoJaCadastrado)
            return Result<ClienteDto>.Conflict("Veículo já cadastrado.");

        var result = cliente.AdicionarVeiculo(request.Modelo, request.Marca, request.Placa, request.Ano);
        if (result.IsFailure)
            return Result<ClienteDto>.BadRequest(result.Errors);
        
        repository.Update(cliente);
        await uow.CommitAsync(cancellationToken);

        // Reload cliente from database to get the veiculo with its generated ID
        var clienteAtualizado = await repository.GetByIdAsync(request.ClienteId);
        return Result<ClienteDto>.Ok(clienteAtualizado!.ToDto());
    }
}