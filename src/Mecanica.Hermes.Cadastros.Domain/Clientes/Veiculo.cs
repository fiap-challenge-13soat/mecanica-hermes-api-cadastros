using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;
using Mecanica.Hermes.Cadastros.Domain.Common.Results;

namespace Mecanica.Hermes.Cadastros.Domain.Clientes;

public sealed class Veiculo : BaseDomain
{
    private Veiculo(string modelo, string marca, PlacaVo placa, int ano)
    {
        Modelo = modelo;
        Marca = marca;
        Placa = placa;
        Ano = ano;
    }

    public string Modelo { get; private set; }
    public string Marca { get; private set; }
    public PlacaVo Placa { get; private set; }
    public int Ano { get; private set; }

    internal static Result<Veiculo> Criar(
        string modelo,
        string marca,
        string placa,
        int ano)
    {
        var errors = ValidarDados(modelo, marca, ano);

        var placaVo = PlacaVo.Criar(placa);
        if (placaVo.IsFailure)
            errors.AddRange(placaVo.Errors);

        if (errors.Count != 0)
            return Result<Veiculo>.BadRequest(errors);

        var veiculo = new Veiculo(
            modelo.Trim(),
            marca.Trim(),
            placaVo.Data!,
            ano);

        return Result<Veiculo>.Ok(veiculo);
    }

    internal Result<Veiculo> Alterar(string modelo, string marca, int ano)
    {
        var errors = ValidarDados(modelo, marca, ano);

        if (errors.Count != 0)
            return Result<Veiculo>.BadRequest(errors);

        Modelo = modelo;
        Marca = marca;
        Ano = ano;

        return Result<Veiculo>.Ok(this);
    }

    private static List<string> ValidarDados(
        string modelo,
        string marca,
        int ano)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(modelo))
            errors.Add("Modelo é obrigatório.");
        else if (modelo.Length > 100)
            errors.Add("Modelo não pode ter mais de 100 caracteres.");

        if (string.IsNullOrWhiteSpace(marca))
            errors.Add("Marca é obrigatória.");
        else if (marca.Length > 100)
            errors.Add("Marca não pode ter mais de 100 caracteres.");

        if (ano is < 1900 or > 2100)
            errors.Add("Ano deve estar entre 1900 e 2100.");

        return errors;
    }

    internal static Veiculo Restaurar(
        Guid id,
        string modelo,
        string marca,
        PlacaVo placa,
        int ano,
        DateTime createdAt,
        DateTime? updatedAt,
        bool isDeleted)
    {
        var veiculo = new Veiculo(
            modelo,
            marca,
            placa,
            ano);

        veiculo.RestaurarBase(id, createdAt, updatedAt, isDeleted);

        return veiculo;
    }
}