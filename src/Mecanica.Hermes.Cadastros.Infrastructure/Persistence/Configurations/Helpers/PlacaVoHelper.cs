using Mecanica.Hermes.Cadastros.Domain.Clientes.ValueObjects;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Configurations.Helpers;

internal static class PlacaVoHelper
{
    public static PlacaVo RecriarPlaca(string valor)
    {
        var resultado = PlacaVo.Criar(valor);
        if (resultado.IsSuccess && resultado.Data is not null)
            return resultado.Data;

        throw new InvalidOperationException("Não foi possível reconstruir a placa armazenada.");
    }
}