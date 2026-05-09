namespace Mecanica.Hermes.Cadastros.Domain.Common.Helpers;

internal static class TitleCaseHelper
{
    internal static string Normalizar(string texto)
    {
        var palavras = texto
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizarPalavra);

        return string.Join(' ', palavras);
    }

    private static string NormalizarPalavra(string palavra)
    {
        var minuscula = palavra.ToLowerInvariant();

        if (Preposicoes.Contains(minuscula))
            return minuscula;

        return char.ToUpperInvariant(minuscula[0]) + minuscula[1..];
    }

    private static readonly HashSet<string> Preposicoes = new(StringComparer.OrdinalIgnoreCase)
    {
        "da", "de", "do", "das", "dos", "e"
    };
}
