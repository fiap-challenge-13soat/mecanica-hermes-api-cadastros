using Mecanica.Hermes.Cadastros.Application.Common.Persistence;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;
using Mecanica.Hermes.Cadastros.Application.Produtos.Mappings;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Domain.Common.Pagination;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using Mecanica.Hermes.Cadastros.Domain.Produtos;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Produtos.Commands.AddServico;

internal sealed class AddServicoCommandHandler(IServicoRepository repository, IUnitOfWork uow)
    : IRequestHandler<AddServicoCommand, Result<ServicoDto>>
{
    public async Task<Result<ServicoDto>> Handle(AddServicoCommand request, CancellationToken cancellationToken)
    {
        var servico = Servico.Criar(request.Descricao, request.Valor);

        if (servico.IsFailure)
            return Result<ServicoDto>.BadRequest(servico.Errors);

        var checkParams = new PaginationParams { Page = 1, PageSize = 1 };
        var servicosExistentes = await repository.ListByDescricaoAsync(servico.Data!.Descricao, checkParams);
        if (servicosExistentes.TotalCount > 0)
            return Result<ServicoDto>.Conflict("Já existe um serviço com esta descrição.");

        await repository.AddAsync(servico.Data!);
        await uow.CommitAsync(cancellationToken);

        return Result<ServicoDto>.Ok(servico.Data!.ToDto());
    }
}