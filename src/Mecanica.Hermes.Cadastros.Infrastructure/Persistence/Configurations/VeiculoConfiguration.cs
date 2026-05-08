using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations;

internal sealed class VeiculoConfiguration : IEntityTypeConfiguration<VeiculoEntity>
{
    public void Configure(EntityTypeBuilder<VeiculoEntity> builder)
    {
        builder.ToTable("Veiculos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Modelo)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Marca)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Ano).IsRequired();
        builder.Property(x => x.ClienteId).IsRequired();

        builder.Property(x => x.Placa)
            .HasConversion(
                placa => placa.Valor,
                valor => PlacaVoHelper.RecriarPlaca(valor))
            .HasColumnName("Placa")
            .HasMaxLength(10)
            .IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => new { x.ClienteId, x.Placa })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
