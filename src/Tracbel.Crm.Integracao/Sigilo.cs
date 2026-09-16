using System.Text.RegularExpressions;

namespace Tracbel.Crm.Integracao;

/// <summary>
/// O que tira credencial de uma mensagem antes de ela ir para log, tela ou banco.
///
/// <para><b>Um lugar só, para toda integração.</b> Morava dentro do serviço do ART; subiu para cá na
/// issue [001] do backlog mestre, porque o Protheus tinha um filtro próprio (só por padrão, sem os
/// valores configurados) e o cliente da API Gestão de Negócios vai precisar do mesmo. Duas regras
/// convivem, e as duas são necessárias:</para>
/// <list type="bullet">
///   <item><b>por valor</b> — o usuário e a senha que a própria integração conhece são trocados onde
///   quer que apareçam, inclusive no meio de uma frase do driver ("Login failed for user 'x'");</item>
///   <item><b>por padrão</b> — qualquer <c>Password=</c> ou <c>Pwd=</c> que uma exceção tenha copiado de
///   uma cadeia de conexão ou de uma URL, mesmo sem saber o valor.</item>
/// </list>
/// </summary>
public static partial class Sigilo
{
    /// <summary>
    /// Troca por <c>***</c> cada segredo conhecido (usuário e senha das conexões) e qualquer trecho
    /// <c>Password=</c> ou <c>Pwd=</c> que uma exceção tenha copiado de uma cadeia de conexão.
    /// </summary>
    /// <param name="texto">A mensagem.</param>
    /// <param name="segredos">Os valores que não podem aparecer. Valores com menos de três caracteres
    /// são ignorados: trocar "sa" apagaria pedaços de qualquer palavra.</param>
    public static string Mascarar(string? texto, IEnumerable<string?> segredos)
    {
        if (string.IsNullOrEmpty(texto)) return string.Empty;

        foreach (var segredo in segredos
                     .Where(s => !string.IsNullOrWhiteSpace(s) && s.Length >= 3)
                     .Distinct(StringComparer.Ordinal)
                     .OrderByDescending(s => s!.Length))
            texto = texto.Replace(segredo!, "***", StringComparison.OrdinalIgnoreCase);

        return TrechoDeSenha().Replace(texto, "$1=***");
    }

    [GeneratedRegex(@"(?i)\b(password|pwd)\s*=\s*[^;'""\s]*")]
    private static partial Regex TrechoDeSenha();
}
