using AutoMapper;
using Mecanica.Hermes.Cadastros.Application.Clientes.Commands.AddCliente;
using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Clientes.Contracts;

public class ClienteMappingProfile : Profile
{
    public ClienteMappingProfile()
    {
        CreateMap<ClienteDto, ClienteResponse>();
        CreateMap<VeiculoDto, VeiculoResponse>();
        CreateMap<AddClienteRequest, AddClienteCommand>();
    }
}