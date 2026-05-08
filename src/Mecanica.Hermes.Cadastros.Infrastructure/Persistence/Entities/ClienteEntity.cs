using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities.Abstractions;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;

internal class ClienteEntity : BaseEntity
{
    private ClienteEntity()
    {
    }

    internal ClienteEntity(
        Guid id,
        NomeProprioVo nomeCivil,
        NomeProprioVo? nomeSocial,
        IdentificacaoFiscalVo identificacaoFiscal,
        EmailVo email,
        TelefoneVo telefone,
        ICollection<VeiculoEntity> veiculos,
        DateTime createdAt,
        DateTime? updatedAt,
        bool isDeleted
    )
    {
        Id = id;
        NomeCivil = nomeCivil;
        NomeSocial = nomeSocial;
        IdentificacaoFiscal = identificacaoFiscal;
        Email = email;
        Telefone = telefone;
        Veiculos = veiculos;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsDeleted = isDeleted;
    }

    public NomeProprioVo NomeCivil { get; private set; } = null!;
    public NomeProprioVo? NomeSocial { get; private set; }
    public IdentificacaoFiscalVo IdentificacaoFiscal { get; private set; } = null!;
    public EmailVo Email { get; private set; } = null!;
    public TelefoneVo Telefone { get; private set; } = null!;

    // Referencias EF
    public virtual ICollection<VeiculoEntity> Veiculos { get; private set; } = new List<VeiculoEntity>();
}
