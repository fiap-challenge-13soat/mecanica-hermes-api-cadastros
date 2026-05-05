using AutoMapper;
using Mecanica.Hermes.Cadastros.Application.Produtos.Commands.AddProduto;
using Mecanica.Hermes.Cadastros.Application.Produtos.Dtos;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Produtos.Contracts;

public class ProdutoMappingProfile : Profile
{
    public ProdutoMappingProfile()
    {
        CreateMap<AddProdutoRequest, AddProdutoCommand>();
        CreateMap<ProdutoDto, ProdutoResponse>();
    }
}
