using Mecanica.Hermes.Cadastros.Application.Clientes.Dtos;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Clientes.Commands.AddCliente;

public sealed record AddClienteCommand(
    string NomeCivil,
    string? NomeSocial,
    string IdentificacaoFiscal,
    string Email,
    string Telefone) : IRequest<Result<ClienteDto>>;