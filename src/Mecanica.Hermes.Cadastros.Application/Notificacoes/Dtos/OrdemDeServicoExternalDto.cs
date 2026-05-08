namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;

/// <summary>
///     Snapshot da Ordem de Serviço retornado por <c>GET /api/ordens-de-servico/{id}</c>
///     na API de Ordem de Serviço. Mantém apenas os campos que a Cadastros precisa
///     para montar a notificação ao cliente.
/// </summary>
public sealed record OrdemDeServicoExternalDto(
    Guid Id,
    Guid ClienteId,
    Guid VeiculoId,
    string? ProblemaRelatado,
    string? Observacoes,
    string StatusAtual,
    decimal ValorTotal,
    IReadOnlyList<OrdemDeServicoProdutoExternalDto> Produtos,
    IReadOnlyList<OrdemDeServicoServicoExternalDto> Servicos);

public sealed record OrdemDeServicoProdutoExternalDto(
    Guid ProdutoId,
    decimal Valor,
    int Quantidade);

public sealed record OrdemDeServicoServicoExternalDto(
    Guid ServicoId,
    decimal Valor);
