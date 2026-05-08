using System.Text.RegularExpressions;

namespace Mecanica.Hermes.Cadastros.Domain.Common.Helpers;

internal static partial class RegexHelper
{
    private const int RegexTimeoutMilliseconds = 1000;

    [GeneratedRegex(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", RegexOptions.None, RegexTimeoutMilliseconds)]
    public static partial Regex FormatoPlaca();

    [GeneratedRegex(@"^[0-9]{11}$", RegexOptions.None, RegexTimeoutMilliseconds)]
    public static partial Regex FormatoCpf();

    [GeneratedRegex(@"^[0-9]{8}000[0-9]{3}$", RegexOptions.None, RegexTimeoutMilliseconds)]
    public static partial Regex FormatoCnpj();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.None, RegexTimeoutMilliseconds)]
    public static partial Regex FormatoEmail();
    
    public static string NormalizarNumeros(string valor)
    {
        return Regex.Replace(valor, @"\D", "", RegexOptions.None, TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds));
    }
}