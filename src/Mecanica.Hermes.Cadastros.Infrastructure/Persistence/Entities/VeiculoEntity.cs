using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities.Abstractions;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;

internal class VeiculoEntity : BaseEntity
{
    private VeiculoEntity()
    {
    }

    internal VeiculoEntity(
        Guid id,
        string modelo,
        string marca,
        PlacaVo placa,
        int ano,
        DateTime createdAt,
        DateTime? updatedAt,
        bool isDeleted)
    {
        Id = id;
        Modelo = modelo;
        Marca = marca;
        Placa = placa;
        Ano = ano;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsDeleted = isDeleted;
    }

    public string Modelo { get; private set; } = null!;
    public string Marca { get; private set; } = null!;
    public PlacaVo Placa { get; private set; } = null!;
    public int Ano { get; private set; }

    // Referencias EF
    public Guid ClienteId { get; internal set; }
    public ClienteEntity Cliente { get; private set; } = null!;
}