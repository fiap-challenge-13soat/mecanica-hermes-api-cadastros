namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;

/// <summary>
///     Persistência de eventos de webhook para garantir idempotência. A chave única
///     <c>(scope, externalId)</c> evita que o mesmo clique do cliente seja processado
///     mais de uma vez (ex.: se o cliente clica duas vezes ou um proxy retransmite).
/// </summary>
public interface IWebhookEventRepository
{
    /// <summary>
    ///     Tenta registrar o evento. Retorna <c>true</c> na primeira ocorrência;
    ///     <c>false</c> quando já existe registro para a mesma combinação
    ///     <paramref name="scope" /> + <paramref name="externalId" /> (duplicata).
    /// </summary>
    Task<bool> TryRegisterAsync(
        string scope,
        string externalId,
        string status,
        CancellationToken cancellationToken);
}
