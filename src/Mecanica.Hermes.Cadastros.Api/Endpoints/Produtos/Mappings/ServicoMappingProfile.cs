using AutoMapper;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.AddServico;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;

public class ServicoMappingProfile : Profile
{
    public ServicoMappingProfile()
    {
        CreateMap<AddServicoRequest, AddServicoCommand>();
        CreateMap<ServicoDto, ServicoResponse>();
    }
}
