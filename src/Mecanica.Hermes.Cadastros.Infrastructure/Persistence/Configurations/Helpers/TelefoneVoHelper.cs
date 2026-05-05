using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;

internal static class TelefoneVoHelper
{
    public static TelefoneVo RecriarTelefone(string valor)
    {
        var resultado = TelefoneVo.Criar(valor);
        if (resultado.IsSuccess && resultado.Data is not null)
            return resultado.Data;

        throw new InvalidOperationException("Não foi possível reconstruir o telefone armazenado.");
    }
}