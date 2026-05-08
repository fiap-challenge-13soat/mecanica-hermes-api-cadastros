using Mecanica.Hermes.Cadastros.Domain.Clientes.Events;
using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using Mecanica.Hermes.Shared.Core.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes;

public sealed class Cliente : AggregateRoot
{
    private readonly List<Veiculo> _veiculos = [];

    private Cliente(
        NomeProprioVo nomeCivil,
        NomeProprioVo? nomeSocial,
        IdentificacaoFiscalVo identificacaoFiscal,
        EmailVo email,
        TelefoneVo telefone,
        List<Veiculo>? veiculos = null)
    {
        NomeCivil = nomeCivil;
        NomeSocial = nomeSocial;
        IdentificacaoFiscal = identificacaoFiscal;
        Email = email;
        Telefone = telefone;
        if (veiculos is not null)
            _veiculos = veiculos;
    }

    public NomeProprioVo NomeCivil { get; private set; }
    public NomeProprioVo? NomeSocial { get; private set; }
    public IdentificacaoFiscalVo IdentificacaoFiscal { get; private set; }
    public EmailVo Email { get; private set; }
    public TelefoneVo Telefone { get; private set; }
    public IReadOnlyCollection<Veiculo> Veiculos => _veiculos;

    public static Result<Cliente> Criar(
        string nomeCivil,
        string? nomeSocial,
        string identificacaoFiscal,
        string email,
        string telefone)
    {
        var errors = new List<string>();

        var nomeCivilVo = NomeProprioVo.Criar(nomeCivil);
        if (nomeCivilVo.IsFailure)
            errors.AddRange(nomeCivilVo.Errors);

        Result<NomeProprioVo>? nomeSocialVo = null;
        if (!string.IsNullOrWhiteSpace(nomeSocial))
        {
            nomeSocialVo = NomeProprioVo.Criar(nomeSocial);
            if (nomeSocialVo.IsFailure)
                errors.AddRange(nomeSocialVo.Errors);
        }

        var identificacaoFiscalVo = IdentificacaoFiscalVo.Criar(identificacaoFiscal);
        if (identificacaoFiscalVo.IsFailure)
            errors.AddRange(identificacaoFiscalVo.Errors);

        var emailVo = EmailVo.Criar(email);
        if (emailVo.IsFailure)
            errors.AddRange(emailVo.Errors);

        var telefoneVo = TelefoneVo.Criar(telefone);
        if (telefoneVo.IsFailure)
            errors.AddRange(telefoneVo.Errors);

        if (errors.Count > 0)
            return Result<Cliente>.BadRequest(errors);

        var cliente = new Cliente(
            nomeCivilVo.Data!,
            nomeSocialVo?.Data,
            identificacaoFiscalVo.Data!,
            emailVo.Data!,
            telefoneVo.Data!);

        cliente.AddDomainEvent(new ClienteCriadoEvent(cliente.IdentificacaoFiscal));

        return Result<Cliente>.Ok(cliente);
    }

    internal static Cliente Restaurar(
        Guid id,
        NomeProprioVo nomeCivil,
        NomeProprioVo? nomeSocial,
        IdentificacaoFiscalVo identificacaoFiscal,
        EmailVo email,
        TelefoneVo telefone,
        List<Veiculo> veiculos,
        DateTime createdAt,
        DateTime? updatedAt,
        bool isDeleted)
    {
        var cliente = new Cliente(
            nomeCivil,
            nomeSocial,
            identificacaoFiscal,
            email,
            telefone,
            veiculos);

        cliente.RestaurarBase(id, createdAt, updatedAt, isDeleted);
        return cliente;
    }


    public Result<Cliente> AtualizarDados(
        string nomeCivil,
        string? nomeSocial,
        string email,
        string telefone)
    {
        var errors = new List<string>();

        var nomeCivilVo = NomeProprioVo.Criar(nomeCivil);
        if (nomeCivilVo.IsFailure)
            errors.AddRange(nomeCivilVo.Errors);

        Result<NomeProprioVo>? nomeSocialVo = null;
        if (!string.IsNullOrWhiteSpace(nomeSocial))
        {
            nomeSocialVo = NomeProprioVo.Criar(nomeSocial);
            if (nomeSocialVo.IsFailure)
                errors.AddRange(nomeSocialVo.Errors);
        }

        var emailVo = EmailVo.Criar(email);
        if (emailVo.IsFailure)
            errors.AddRange(emailVo.Errors);

        var telefoneVo = TelefoneVo.Criar(telefone);
        if (telefoneVo.IsFailure)
            errors.AddRange(telefoneVo.Errors);

        if (errors.Count > 0)
            return Result<Cliente>.BadRequest(errors);

        NomeCivil = nomeCivilVo.Data!;
        NomeSocial = nomeSocialVo?.Data;
        Email = emailVo.Data!;
        Telefone = telefoneVo.Data!;

        MarkAsUpdated();
        AddDomainEvent(new ClienteAlteradoEvent(Id));

        return Result<Cliente>.Ok(this);
    }

    public Result<Veiculo> AdicionarVeiculo(
        string modelo,
        string marca,
        string placa,
        int ano)
    {
        var veiculo = Veiculo.Criar(modelo, marca, placa, ano);
        if (veiculo.IsFailure)
            return Result<Veiculo>.BadRequest(veiculo.Errors);

        if (_veiculos.Any(v => v.Placa == veiculo.Data!.Placa))
            return Result<Veiculo>.Conflict("Placa do veículo já cadastrada para este cliente.");

        _veiculos.Add(veiculo.Data!);

        MarkAsUpdated();
        AddDomainEvent(new VeiculoAdicionadoEvent(Id, veiculo.Data!.Placa));

        return Result<Veiculo>.Ok(veiculo.Data!);
    }

    public Result<Cliente> AlterarVeiculo(
        Guid veiculoId,
        string modelo,
        string marca,
        int ano)
    {
        var veiculoCadastrado = Veiculos.FirstOrDefault(v => v.Id == veiculoId);
        if (veiculoCadastrado is null)
            return Result<Cliente>.BadRequest("Veículo não encontrado.");

        var result = veiculoCadastrado.Alterar(modelo, marca, ano);
        if (result.IsFailure)
            return Result<Cliente>.BadRequest(result.Errors);

        result.Data!.MarkAsUpdated();
        MarkAsUpdated();
        AddDomainEvent(new VeiculoAlteradoEvent(Id, veiculoCadastrado.Placa));

        return Result<Cliente>.Ok();
    }

    public Result<Cliente> RemoverVeiculo(Guid veiculoId)
    {
        var veiculo = _veiculos.FirstOrDefault(v => v.Id == veiculoId);
        if (veiculo is null)
            return Result<Cliente>.NotFound();

        veiculo.MarkAsDeleted();

        MarkAsUpdated();
        AddDomainEvent(new VeiculoRemovidoEvent(Id, veiculo.Placa));

        return Result<Cliente>.Ok();
    }

    public Result<Cliente> Excluir()
    {
        if (IsDeleted)
            return Result<Cliente>.BadRequest("Cliente já foi excluído.");

        MarkAsDeleted();
        AddDomainEvent(new ClienteExcluidoEvent(Id));

        return Result<Cliente>.Ok();
    }
}