using System.Globalization;
using System.Text;
using Mecanica.Hermes.Cadastros.Application.Common.Constants;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;

namespace Mecanica.Hermes.Cadastros.Application.Notificacoes.Mappings;

public static class LinkPagamentoEmailBuilder
{
    private const string Subject = "Link de pagamento disponível - Mecânica Hermes";

    public static string BuildSubject() => Subject;

    public static string BuildBody(PagamentoExternalDto pagamento)
    {
        ArgumentNullException.ThrowIfNull(pagamento);
        if (pagamento.LinkPagamento is null)
            throw new InvalidOperationException("Pagamento sem link disponível.");

        var ptBr = CultureInfo.GetCultureInfo(ApplicationConstants.PtBrCulture);

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html lang=\"pt-BR\"><body style=\"font-family:Arial,sans-serif;color:#222\">");
        sb.Append($"<p>Olá, <strong>{pagamento.NomeCliente}</strong>,</p>");
        sb.Append("<p>O link para pagamento do seu serviço está disponível.</p>");
        sb.Append("<p><strong>Ordem de Serviço:</strong> ").Append(pagamento.OrdemDeServicoId.ToString("D")).Append("</p>");
        sb.Append("<p><strong>Valor:</strong> ").Append(pagamento.Valor.ToString("C", ptBr)).Append("</p>");
        if (pagamento.Vencimento is { } vencimento)
            sb.Append("<p><strong>Validade do link:</strong> ")
                .Append(vencimento.ToString("dd/MM/yyyy HH:mm", ptBr))
                .Append("</p>");

        sb.Append("<p style=\"margin-top:24px\">");
        sb.Append("<a href=\"").Append(pagamento.LinkPagamento.Url).Append("\" ")
            .Append("style=\"background:#1565c0;color:#fff;padding:14px 28px;text-decoration:none;")
            .Append("border-radius:4px;font-weight:bold\">Pagar agora</a>");
        sb.Append("</p>");

        sb.Append("<p style=\"font-size:12px;color:#666\">Caso o botão não funcione, copie e cole este endereço: ")
            .Append("<a href=\"").Append(pagamento.LinkPagamento.Url).Append("\">")
            .Append(pagamento.LinkPagamento.Url).Append("</a></p>");

        sb.Append("<hr style=\"margin-top:32px;border:0;border-top:1px solid #eee\"/>");
        sb.Append("<p style=\"color:#888;font-size:12px\">")
            .Append("Esta é uma mensagem automática enviada pela Mecânica Hermes. Não responda este e-mail.")
            .Append("</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
