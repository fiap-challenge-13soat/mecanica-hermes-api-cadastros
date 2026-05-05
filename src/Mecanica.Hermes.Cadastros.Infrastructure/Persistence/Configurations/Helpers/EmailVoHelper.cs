using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;

internal static class EmailVoHelper
{
    public static EmailVo RecriarEmail(string valor)
    {
        var resultado = EmailVo.Criar(valor);
        if (resultado.IsSuccess && resultado.Data is not null)
            return resultado.Data;

        throw new InvalidOperationException("Não foi possível reconstruir o email armazenado.");
    }
}