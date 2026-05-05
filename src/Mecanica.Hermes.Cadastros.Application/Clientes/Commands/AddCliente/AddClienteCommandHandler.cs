using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.AddCliente;

internal sealed class AddClienteCommandHandler(IClienteRepository repository, IUnitOfWork uow)
    : IRequestHandler<AddClienteCommand, Result<ClienteDto>>
{
    public async Task<Result<ClienteDto>> Handle(AddClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = Cliente.Criar(
            request.NomeCivil,
            request.NomeSocial,
            request.IdentificacaoFiscal,
            request.Email,
            request.Telefone);

        if (cliente.IsFailure)
            return Result<ClienteDto>.BadRequest(cliente.Errors);

        var identificacaoFiscalJaCadastrado =
            await repository.GetByIdentificacaoFiscalAsync(cliente.Data!.IdentificacaoFiscal);
        if (identificacaoFiscalJaCadastrado is not null)
            return Result<ClienteDto>.Conflict("Identificação fiscal já cadastrada.");

        var emailJaCadastrado = await repository.GetByEmailAsync(cliente.Data!.Email);
        if (emailJaCadastrado is not null)
            return Result<ClienteDto>.Conflict("Email já cadastrado.");

        await repository.AddAsync(cliente.Data);
        await uow.CommitAsync(cancellationToken);

        return Result<ClienteDto>.Ok(cliente.Data.ToDto());
    }
}