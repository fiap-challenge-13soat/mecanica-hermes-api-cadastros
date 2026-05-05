using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations;

internal class ProdutoConfiguration : IEntityTypeConfiguration<ProdutoEntity>
{
    public void Configure(EntityTypeBuilder<ProdutoEntity> builder)
    {
        builder.ToTable("Produtos");

        builder.HasKey(p => p.Id);

        builder.Property(x => x.Descricao)
            .HasConversion(
                descricao => descricao.Valor,
                valor => DescricaoProdutoVoHelper.RecriarDescricao(valor))
            .HasColumnName("Descricao")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Descricao)
            .IsUnique();

        builder.Property(x => x.Valor)
            .HasConversion(
                valor => valor.Valor,
                valorDecimal => ValorProdutoVoHelper.RecriarValor(valorDecimal))
            .HasColumnName("Valor")
            .IsRequired();

        builder.Property(x => x.Quantidade)
            .HasConversion(
                quantidade => quantidade.Valor,
                valor => QuantidadeProdutoVoHelper.RecriarQuantidade(valor))
            .HasColumnName("Quantidade")
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => new { x.Descricao, x.Tipo })
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