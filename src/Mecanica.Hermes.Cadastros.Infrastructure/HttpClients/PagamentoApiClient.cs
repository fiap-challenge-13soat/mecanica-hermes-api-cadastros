using System.Net;
using System.Net.Http.Json;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;

namespace Mecanica.Hermes.Cadastros.Infrastructure.HttpClients;

internal sealed class PagamentoApiClient(HttpClient httpClient) : IPagamentoApiClient
{
    public async Task<PagamentoExternalDto?> GetByIdAsync(Guid pagamentoId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"/api/pagamentos/{pagamentoId:D}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PagamentoExternalDto>(cancellationToken);
    }
}
