namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;

/// <summary>
///     Persistência de eventos de webhook para garantir idempotência. A chave única
///     <c>(scope, externalId)</c> evita que o mesmo clique do cliente seja processado
///     mais de uma vez (ex.: se o cliente clica duas vezes ou um proxy retransmite).
/// </summary>
public interface IWebhookEventRepository
{
    /// <summary>
    ///     Registra atomicamente o evento de webhook na tabela de idempotência <em>e</em> enfileira
    ///     <paramref name="integrationEvent" /> no Outbox transacional, ambos no mesmo
    ///     <c>SaveChanges</c>. Em caso de duplicata (mesma chave <paramref name="scope" /> +
    ///     <paramref name="externalId" />), <em>nenhuma</em> linha é gravada (rollback).
    ///     <para>
    ///         Garante que: se o registro de idempotência foi commitado, o integration event
    ///         <b>vai ser publicado</b> (em background pelo <c>OutboxProcessor</c>), mesmo que o
    ///         broker (RabbitMQ) esteja indisponível no momento do webhook. Mitiga o risco aceito
    ///         em ADR-002 do publish direto via <c>IPublishEndpoint</c>.
    ///     </para>
    /// </summary>
    /// <returns>
    ///     <c>true</c> na primeira ocorrência (registrado + enfileirado);
    ///     <c>false</c> quando já existe registro (duplicata — outro webhook idêntico já foi
    ///     processado e o evento já foi enfileirado anteriormente).
    /// </returns>
    Task<bool> TryRegisterAsync(
        string scope,
        string externalId,
        string status,
        object integrationEvent,
        CancellationToken cancellationToken);
}
