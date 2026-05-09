using MassTransit;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Application.Common.Messaging;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Mappings;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarOrcamentoParaAprovacao;

internal sealed class EnviarOrcamentoParaAprovacaoCommandHandler(
    IOrdemServicoApiClient ordemServicoApiClient,
    IClienteRepository clienteRepository,
    IProdutoRepository produtoRepository,
    IServicoRepository servicoRepository,
    INotificacoesUrlBuilder urlBuilder,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<EnviarOrcamentoParaAprovacaoCommand, Result>
{
    private const string StatusAguardandoAprovacao = "AguardandoAprovacao";

    public async Task<Result> Handle(EnviarOrcamentoParaAprovacaoCommand request, CancellationToken cancellationToken)
    {
        var ordem = await ordemServicoApiClient.GetByIdAsync(request.OrdemDeServicoId, cancellationToken);
        if (ordem is null)
            return Result.NotFound("Ordem de Serviço não encontrada na API de Ordem de Serviço.");

        if (!string.Equals(ordem.StatusAtual, StatusAguardandoAprovacao, StringComparison.OrdinalIgnoreCase))
            return Result.BadRequest(
                $"A Ordem de Serviço está no status '{ordem.StatusAtual}'. " +
                $"Apenas ordens em '{StatusAguardandoAprovacao}' podem ter orçamento enviado para aprovação.");

        var cliente = await clienteRepository.GetByIdAsync(ordem.ClienteId);
        if (cliente is null)
            return Result.BadRequest(
                $"Cliente {ordem.ClienteId} não foi encontrado na base local de Cadastros.");

        var produtosPorId = new Dictionary<Guid, Produto>();
        foreach (var produtoId in ordem.Produtos.Select(p => p.ProdutoId).Distinct())
        {
            var produto = await produtoRepository.GetByIdAsync(produtoId);
            if (produto is not null)
                produtosPorId[produtoId] = produto;
        }

        var servicosPorId = new Dictionary<Guid, Servico>();
        foreach (var servicoId in ordem.Servicos.Select(s => s.ServicoId).Distinct())
        {
            var servico = await servicoRepository.GetByIdAsync(servicoId);
            if (servico is not null)
                servicosPorId[servicoId] = servico;
        }

        var aprovarUrl = urlBuilder.BuildAprovacaoUrl(ordem.Id);
        var rejeitarUrl = urlBuilder.BuildRejeicaoUrl(ordem.Id);

        var body = OrcamentoEmailBuilder.BuildBody(
            ordem, cliente, produtosPorId, servicosPorId, aprovarUrl, rejeitarUrl);

        await publishEndpoint.Publish(
            new EnviarEmailCommand(cliente.Email.Valor, OrcamentoEmailBuilder.BuildSubject(), body),
            cancellationToken);

        return Result.Accepted();
    }
}
