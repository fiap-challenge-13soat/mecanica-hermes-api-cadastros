using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Mappings;

internal static class ServicoMapper
{
    internal static Servico ToDomain(this ServicoEntity entity)
    {
        return Servico.Restaurar(
            entity.Id,
            entity.Descricao,
            entity.Valor,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.IsDeleted);
    }

    internal static ServicoEntity ToEntity(this Servico servico)
    {
        return new ServicoEntity(
            servico.Id,
            servico.Descricao,
            servico.Valor,
            servico.CreatedAt,
            servico.UpdatedAt,
            servico.IsDeleted);
    }
}
