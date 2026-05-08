using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations;

internal sealed class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEventEntity>
{
    public void Configure(EntityTypeBuilder<WebhookEventEntity> builder)
    {
        builder.ToTable("WebhookEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Scope).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ExternalId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ReceivedAt).IsRequired();

        builder.HasIndex(x => new { x.Scope, x.ExternalId }).IsUnique();
    }
}
