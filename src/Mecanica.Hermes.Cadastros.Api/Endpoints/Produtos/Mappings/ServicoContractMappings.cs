using Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.AddServico;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.UpdateServico;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Mappings;

internal static class ServicoContractMappings
{
    public static AddServicoCommand ToCommand(this AddServicoRequest request) =>
        new(request.Descricao, request.Valor);

    public static UpdateServicoCommand ToCommand(this UpdateServicoRequest request, Guid id) =>
        new(id, request.Descricao, request.Valor);

    public static ServicoResponse ToResponse(this ServicoDto dto) =>
        new(dto.Id, dto.Descricao, dto.Valor);
}
