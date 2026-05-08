using System.Net;
using System.Net.Http.Json;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;

namespace Mecanica.Hermes.Cadastros.Infrastructure.HttpClients;

internal sealed class OrdemServicoApiClient(HttpClient httpClient) : IOrdemServicoApiClient
{
    public async Task<OrdemDeServicoExternalDto?> GetByIdAsync(Guid ordemServicoId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"/api/ordens-de-servico/{ordemServicoId:D}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrdemDeServicoExternalDto>(cancellationToken);
    }
}
