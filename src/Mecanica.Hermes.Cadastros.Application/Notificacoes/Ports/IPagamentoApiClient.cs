using Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;

public interface IPagamentoApiClient
{
    /// <summary>
    ///     Retorna o Pagamento ou <c>null</c> quando o serviço respondeu 404.
    ///     Outras falhas (5xx, timeout) lançam exceção.
    /// </summary>
    Task<PagamentoExternalDto?> GetByIdAsync(Guid pagamentoId, CancellationToken cancellationToken);
}
