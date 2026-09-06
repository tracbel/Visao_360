namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Telefone brasileiro normalizado.
///
/// [V] LIÇÃO DO VÓRTICE — <c>GE_Pessoa.FoneNro1</c> é <c>decimal(12)</c>. Telefone com DDI
/// ou zero à esquerda estoura o tipo, e a API devolve <c>{"Message":"An error has occurred."}</c>
/// sem dizer o motivo. Foram horas de investigação por causa de uma escolha de tipo.
///
/// Aqui o telefone é um TIPO. Quem tem um <see cref="Telefone"/> em mãos tem um telefone
/// válido: não existe caminho para construir um inválido.
/// </summary>
public readonly record struct Telefone
{
    private Telefone(string numero) => Numero = numero;

    /// <summary>Só dígitos, com DDD, sem DDI. Ex.: <c>"17999990000"</c>.</summary>
    public string Numero { get; }

    /// <summary>DDD, dois dígitos.</summary>
    public string Ddd => Numero[..2];

    /// <summary>Verdadeiro para celular (11 dígitos), falso para fixo (10).</summary>
    public bool EhCelular => Numero.Length == 11;

    /// <summary>
    /// Cria a partir de uma entrada de usuário. Lança se for inválida.
    /// </summary>
    /// <exception cref="RegraDeNegocioViolada">Quando a entrada não é um telefone brasileiro.</exception>
    public static Telefone Criar(string entrada) =>
        TentarCriar(entrada, out var telefone)
            ? telefone
            : throw new RegraDeNegocioViolada(
                $"Telefone inválido: '{entrada}'. Informe DDD e número, com 10 ou 11 dígitos.");

    /// <summary>
    /// Versão que não lança.
    ///
    /// Use na importação em massa, onde a linha ruim deve ser registrada e o lote deve
    /// CONTINUAR. [V] o sincronismo do Vórtico Mobile aborta a carga inteira numa única
    /// linha órfã — 5.302 vínculos quebrados travam o app de todo mundo cujo escopo os inclua.
    /// </summary>
    public static bool TentarCriar(string? entrada, out Telefone telefone)
    {
        telefone = default;
        if (string.IsNullOrWhiteSpace(entrada)) return false;

        // Sinal de número internacional: DDI explícito que não é o do Brasil (+55).
        // Sem esta guarda, um número estrangeiro pode coincidir em quantidade de dígitos
        // com um celular brasileiro válido e ser aceito silenciosamente como se fosse
        // nacional (ex.: "+1 397 123 4567" -> "13971234567", 11 dígitos, DDD 13 "válido"
        // e nono dígito "9" por coincidência). Aqui, DDI estrangeiro é rejeitado, nunca
        // adivinhado — o mesmo princípio de "rejeitar é melhor que corrigir em silêncio".
        var semEspacosNaPonta = entrada.TrimStart();
        if (semEspacosNaPonta.StartsWith('+'))
        {
            var digitosComSinal = new string(semEspacosNaPonta.Where(char.IsDigit).ToArray());
            if (!digitosComSinal.StartsWith("55", StringComparison.Ordinal)) return false;
        }

        // "+55 (17) 99999-0000" -> "5517999990000"
        var digitos = new string(entrada.Where(char.IsDigit).ToArray());

        // Remove o DDI 55 quando o que sobra tem tamanho de número nacional.
        if (digitos.Length is 12 or 13 && digitos.StartsWith("55", StringComparison.Ordinal))
            digitos = digitos[2..];

        // Celular antigo, sem o nono dígito: a migração nacional (2016) prefixou todo
        // celular com "9", mas números anteriores a ela ainda circulam em cadastros. O
        // número local de 8 dígitos que começa em 6-9 é celular, nunca fixo — fixo
        // brasileiro sempre começa em 2-5. Sem isto, "(17) 8888-0000" (celular pré-2016)
        // era aceito como se fosse fixo, e o mesmo assinante virava dois cadastros de
        // telefone diferentes conforme a máscara com que chegasse.
        if (digitos.Length == 10 && digitos[2] is >= '6' and <= '9')
            digitos = digitos[..2] + "9" + digitos[2..];

        // 10 = fixo com DDD; 11 = celular com DDD.
        if (digitos.Length is not (10 or 11)) return false;

        // DDD brasileiro válido vai de 11 a 99.
        if (!int.TryParse(digitos[..2], out var ddd) || ddd is < 11 or > 99) return false;

        // Celular brasileiro começa com 9 depois do DDD.
        if (digitos.Length == 11 && digitos[2] != '9') return false;

        telefone = new Telefone(digitos);
        return true;
    }

    /// <summary>Formata para exibição. Guardamos cru; formatamos só na saída.</summary>
    public string Formatado() => EhCelular
        ? $"({Numero[..2]}) {Numero[2..7]}-{Numero[7..]}"
        : $"({Numero[..2]}) {Numero[2..6]}-{Numero[6..]}";

    /// <inheritdoc />
    public override string ToString() => Formatado();
}
