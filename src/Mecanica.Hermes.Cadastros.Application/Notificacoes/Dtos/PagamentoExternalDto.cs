namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;

/// <summary>
///     Snapshot do Pagamento retornado por <c>GET /api/pagamentos/{id}</c> na API de
///     Pagamentos. Apenas os campos que a Cadastros consome para montar a notificação
///     ao cliente (e-mail, nome, link e valor). Demais campos do contrato remoto são
///     ignorados na desserialização.
/// </summary>
public sealed record PagamentoExternalDto(
    Guid Id,
    Guid OrdemDeServicoId,
    Guid ClienteId,
    string EmailCliente,
    string NomeCliente,
    decimal Valor,
    string Moeda,
    string StatusAtual,
    PagamentoLinkExternalDto? LinkPagamento,
    DateTime? Vencimento);

public sealed record PagamentoLinkExternalDto(string PreferenceId, string Url);
