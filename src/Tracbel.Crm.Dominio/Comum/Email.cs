using System.Text.RegularExpressions;

namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Endereço de e-mail válido e normalizado (sempre minúsculo, sem espaço nas pontas).
///
/// [V] LIÇÃO DO VÓRTICE — existem TRÊS modelos concorrentes de e-mail no mesmo banco
/// (<c>GE_Pessoa.Email</c>, <c>GE_Email</c>, <c>GE_PessoaEmail</c>) e ninguém sabe qual é a
/// verdade. Aqui existe um tipo só, e a normalização acontece na construção — o que torna
/// a deduplicação confiável.
/// </summary>
public readonly partial record struct Email
{
    private Email(string endereco) => Endereco = endereco;

    /// <summary>O endereço, já normalizado em minúsculas.</summary>
    public string Endereco { get; }

    /// <summary>A parte depois do arroba. Útil para segmentar por empresa.</summary>
    public string Dominio => Endereco[(Endereco.IndexOf('@') + 1)..];

    /// <summary>Cria a partir de uma entrada de usuário. Lança se for inválida.</summary>
    /// <exception cref="RegraDeNegocioViolada">Quando a entrada não é um e-mail válido.</exception>
    public static Email Criar(string entrada) =>
        TentarCriar(entrada, out var email)
            ? email
            : throw new RegraDeNegocioViolada($"E-mail inválido: '{entrada}'.");

    /// <summary>Versão que não lança. Use em importação em massa.</summary>
    public static bool TentarCriar(string? entrada, out Email email)
    {
        email = default;
        if (string.IsNullOrWhiteSpace(entrada)) return false;

        var normalizado = entrada.Trim().ToLowerInvariant();
        if (normalizado.Length > 200) return false;
        if (!PadraoEmail().IsMatch(normalizado)) return false;

        email = new Email(normalizado);
        return true;
    }

    // Validação deliberadamente simples. Não existe regex que valide e-mail conforme a RFC,
    // e tentar é armadilha: o que prova que um e-mail existe é mandar mensagem para ele.
    [GeneratedRegex(@"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$", RegexOptions.CultureInvariant)]
    private static partial Regex PadraoEmail();

    /// <inheritdoc />
    public override string ToString() => Endereco;
}
