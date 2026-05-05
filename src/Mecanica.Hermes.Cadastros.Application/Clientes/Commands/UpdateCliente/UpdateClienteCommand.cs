using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.UpdateCliente;

public sealed record UpdateClienteCommand(
    Guid Id,
    string NomeCivil,
    string? NomeSocial,
    string Email,
    string Telefone) : IRequest<Result<ClienteDto>>;