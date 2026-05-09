using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;

internal static class NomeProprioVoHelper
{
    public static NomeProprioVo RecriarNome(string valor)
    {
        var resultado = NomeProprioVo.Criar(valor);
        if (resultado.IsSuccess && resultado.Data is not null)
            return resultado.Data;

        throw new InvalidOperationException("Não foi possível reconstruir o nome armazenado.");
    }
}
