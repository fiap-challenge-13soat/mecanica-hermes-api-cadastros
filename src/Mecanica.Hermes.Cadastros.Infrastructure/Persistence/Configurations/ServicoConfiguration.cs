using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations;

internal class ServicoConfiguration : IEntityTypeConfiguration<ServicoEntity>
{
    public void Configure(EntityTypeBuilder<ServicoEntity> builder)
    {
        builder.ToTable("Servicos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Descricao)
            .HasConversion(
                descricao => descricao.Valor,
                valor => DescricaoProdutoVoHelper.RecriarDescricao(valor))
            .HasColumnName("Descricao")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Valor)
            .HasConversion(
                valor => valor.Valor,
                valorDecimal => ValorProdutoVoHelper.RecriarValor(valorDecimal))
            .HasColumnName("Valor")
            .IsRequired();

        builder.HasIndex(x => x.Descricao)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}