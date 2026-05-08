using Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;

/// <summary>
///     Cliente HTTP para a API de Ordem de Serviço. Único método por ora —
///     buscar uma OS pelo ID. Resiliência (retry, timeout, circuit-breaker) fica
///     a cargo da implementação em Infrastructure (Polly via
///     <c>AddStandardResilienceHandler</c>).
/// </summary>
public interface IOrdemServicoApiClient
{
    /// <summary>
    ///     Retorna a OS ou <c>null</c> quando o serviço respondeu 404.
    ///     Outras falhas (5xx, timeout) lançam exceção para que o caller decida
    ///     política de retry/fallback.
    /// </summary>
    Task<OrdemDeServicoExternalDto?> GetByIdAsync(Guid ordemServicoId, CancellationToken cancellationToken);
}
