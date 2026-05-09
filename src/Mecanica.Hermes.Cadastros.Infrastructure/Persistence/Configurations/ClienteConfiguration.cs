using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations;

internal class ClienteConfiguration : IEntityTypeConfiguration<ClienteEntity>
{
    public void Configure(EntityTypeBuilder<ClienteEntity> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NomeCivil)
            .HasConversion(
                nome => nome.Valor,
                valor => NomeProprioVoHelper.RecriarNome(valor))
            .HasColumnName("NomeCivil")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.NomeSocial)
            .HasConversion(
                nome => nome == null ? null : nome.Valor,
                valor => valor == null ? null : NomeProprioVoHelper.RecriarNome(valor))
            .HasColumnName("NomeSocial")
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Valor,
                valor => EmailVoHelper.RecriarEmail(valor))
            .HasColumnName("Email")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Telefone)
            .HasConversion(
                telefone => telefone.Valor,
                valor => TelefoneVoHelper.RecriarTelefone(valor))
            .HasColumnName("Telefone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.IdentificacaoFiscal)
            .HasConversion(
                identificacao => identificacao.Valor,
                valor => IdentificacaoFiscalVoHelper.RecriarIdentificacaoFiscal(valor))
            .HasColumnName("IdentificacaoFiscal")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.IdentificacaoFiscal)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(x => x.Veiculos)
            .WithOne(x => x.Cliente)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
