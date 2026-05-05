using Microsoft.AspNetCore.Http;

namespace Mecanica.Hermes.Cadastros.Api.Endpoints.Webhooks;

internal static class WebhookHtmlResponses
{
    public static IResult Success(string heading, string message)
    {
        var html = $$"""
            <!DOCTYPE html>
            <html lang="pt-BR"><head><meta charset="utf-8"><title>{{heading}}</title></head>
            <body style="font-family:Arial,sans-serif;background:#f5f5f5;margin:0;padding:48px;text-align:center;color:#222">
              <div style="max-width:480px;margin:0 auto;background:#fff;padding:32px;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.08)">
                <h1 style="color:#2e7d32;margin-top:0">{{heading}}</h1>
                <p style="color:#555;line-height:1.5">{{message}}</p>
                <p style="color:#888;font-size:12px;margin-top:24px">Mecânica Hermes</p>
              </div>
            </body></html>
            """;
        return Results.Content(html, "text/html; charset=utf-8");
    }

    public static IResult Error(int statusCode, string heading, string message)
    {
        var html = $$"""
            <!DOCTYPE html>
            <html lang="pt-BR"><head><meta charset="utf-8"><title>{{heading}}</title></head>
            <body style="font-family:Arial,sans-serif;background:#f5f5f5;margin:0;padding:48px;text-align:center;color:#222">
              <div style="max-width:480px;margin:0 auto;background:#fff;padding:32px;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.08)">
                <h1 style="color:#c62828;margin-top:0">{{heading}}</h1>
                <p style="color:#555;line-height:1.5">{{message}}</p>
                <p style="color:#888;font-size:12px;margin-top:24px">Mecânica Hermes</p>
              </div>
            </body></html>
            """;
        return Results.Content(html, "text/html; charset=utf-8", statusCode: statusCode);
    }
}
