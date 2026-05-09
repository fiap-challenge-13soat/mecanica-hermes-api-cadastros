using System.Globalization;
using System.Text;
using Mecanica.Hermes.Cadastros.Application.Common.Constants;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;
using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Produtos;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Mappings;

/// <summary>
///     Monta o conteúdo do e-mail enviado ao cliente quando o orçamento da Ordem
///     de Serviço entra no status "AguardandoAprovacao". Os links Aprovar/Rejeitar
///     apontam para o WebHook desta API (placeholder até Fase 4) — o serviço de
///     OS é responsável por aplicar a transição quando o webhook chamar de volta.
/// </summary>
public static class OrcamentoEmailBuilder
{
    private const string Subject = "Orçamento disponível para aprovação - Mecânica Hermes";

    public static string BuildSubject() => Subject;

    public static string BuildBody(
        OrdemDeServicoExternalDto ordem,
        Cliente cliente,
        IReadOnlyDictionary<Guid, Produto> produtosPorId,
        IReadOnlyDictionary<Guid, Servico> servicosPorId,
        string aprovarUrl,
        string rejeitarUrl)
    {
        ArgumentNullException.ThrowIfNull(ordem);
        ArgumentNullException.ThrowIfNull(cliente);
        ArgumentNullException.ThrowIfNull(produtosPorId);
        ArgumentNullException.ThrowIfNull(servicosPorId);

        var ptBr = CultureInfo.GetCultureInfo(ApplicationConstants.PtBrCulture);
        var saudacao = cliente.NomeSocial?.Valor ?? cliente.NomeCivil.Valor;

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html lang=\"pt-BR\"><body style=\"font-family:Arial,sans-serif;color:#222\">");
        sb.Append(CultureInfo.InvariantCulture, $"<p>Olá, <strong>{saudacao}</strong>,</p>");
        sb.Append("<p>O orçamento da sua ordem de serviço está disponível e aguarda sua análise.</p>");
        sb.Append("<p><strong>Ordem de Serviço:</strong> ").Append(ordem.Id.ToString("D")).Append("</p>");
        sb.Append("<p><strong>Valor Total Estimado:</strong> ")
            .Append(ordem.ValorTotal.ToString("C", ptBr)).Append("</p>");

        if (ordem.Produtos.Count > 0)
        {
            sb.Append("<h3>Produtos</h3><ul>");
            foreach (var item in ordem.Produtos)
            {
                var descricao = produtosPorId.TryGetValue(item.ProdutoId, out var produto)
                    ? produto.Descricao.Valor
                    : item.ProdutoId.ToString("D");
                var total = item.Valor * item.Quantidade;
                sb.Append("<li>").Append(descricao)
                    .Append(" — ").Append(item.Quantidade).Append(" × ")
                    .Append(item.Valor.ToString("C", ptBr))
                    .Append(" = <strong>").Append(total.ToString("C", ptBr)).Append("</strong></li>");
            }

            sb.Append("</ul>");
        }

        if (ordem.Servicos.Count > 0)
        {
            sb.Append("<h3>Serviços</h3><ul>");
            foreach (var item in ordem.Servicos)
            {
                var descricao = servicosPorId.TryGetValue(item.ServicoId, out var servico)
                    ? servico.Descricao.Valor
                    : item.ServicoId.ToString("D");
                sb.Append("<li>").Append(descricao)
                    .Append(" — <strong>").Append(item.Valor.ToString("C", ptBr)).Append("</strong></li>");
            }

            sb.Append("</ul>");
        }

        sb.Append("<p style=\"margin-top:24px\">");
        sb.Append("<a href=\"").Append(aprovarUrl).Append("\" ")
            .Append("style=\"background:#2e7d32;color:#fff;padding:12px 24px;text-decoration:none;")
            .Append("border-radius:4px;margin-right:8px\">Aprovar Orçamento</a>");
        sb.Append("<a href=\"").Append(rejeitarUrl).Append("\" ")
            .Append("style=\"background:#c62828;color:#fff;padding:12px 24px;text-decoration:none;")
            .Append("border-radius:4px\">Rejeitar Orçamento</a>");
        sb.Append("</p>");

        sb.Append("<hr style=\"margin-top:32px;border:0;border-top:1px solid #eee\"/>");
        sb.Append("<p style=\"color:#888;font-size:12px\">")
            .Append("Esta é uma mensagem automática enviada pela Mecânica Hermes. Não responda este e-mail.")
            .Append("</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
