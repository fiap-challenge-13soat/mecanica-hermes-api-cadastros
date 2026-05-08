using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Clientes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Queries.GetClienteByIdentificacaoFiscal;

internal sealed class GetClienteByIdentificacaoFiscalQueryHandler(IClienteRepository repository)
    : IRequestHandler<GetClienteByIdentificacaoFiscalQuery, Result<ClienteDto>>
{
    public async Task<Result<ClienteDto>> Handle(GetClienteByIdentificacaoFiscalQuery request,
        CancellationToken cancellationToken)
    {
        var identificacaoFiscal = IdentificacaoFiscalVo.Criar(request.IdentificacaoFiscal);
        if (identificacaoFiscal.IsFailure)
            return Result<ClienteDto>.BadRequest(identificacaoFiscal.Errors);

        var cliente = await repository.GetByIdentificacaoFiscalAsync(identificacaoFiscal.Data!);
        return cliente == null
            ? Result<ClienteDto>.NotFound()
            : Result<ClienteDto>.Ok(cliente.ToDto());
    }
}