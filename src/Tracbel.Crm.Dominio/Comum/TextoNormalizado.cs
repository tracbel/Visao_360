using System.Text.RegularExpressions;

namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Texto livre normalizado: sem espaço nas pontas, sem espaço duplo e sem caractere
/// invisível.
///
/// [V] LIÇÃO DO VÓRTICE — não existe normalização de texto em nenhum ponto do Vórtice.
/// Campos como <c>IV_Historico.ResultadoCmpl</c> chegam com espaço duplo e, em textos
/// colados de e-mail, caracteres invisíveis (espaço de largura zero, NBSP) que fazem duas
/// strings "iguais" aos olhos do usuário comparar como diferentes — e reaparecem como
/// entradas duplicadas em qualquer relatório agrupado por texto.
/// </summary>
public readonly partial record struct TextoNormalizado
{
    private TextoNormalizado(string valor) => Valor = valor;

    /// <summary>O texto já limpo: sem espaço nas pontas, sem espaço duplo, sem caractere invisível.</summary>
    public string Valor { get; }

    /// <summary>Quantidade de caracteres do texto normalizado.</summary>
    public int Tamanho => Valor.Length;

    /// <summary>Cria a partir de uma entrada de usuário. Lança se não sobrar texto depois de normalizado.</summary>
    /// <exception cref="RegraDeNegocioViolada">Entrada nula, vazia, só espaço/invisível, ou maior que o tamanho máximo.</exception>
    public static TextoNormalizado Criar(string entrada, int tamanhoMaximo = 4000) =>
        TentarCriar(entrada, out var texto, tamanhoMaximo)
            ? texto
            : throw new RegraDeNegocioViolada(
                $"Texto inválido: vazio depois de normalizado, ou maior que {tamanhoMaximo} caracteres.");

    /// <summary>
    /// Versão que não lança. Use em importação em massa.
    ///
    /// Texto que excede <paramref name="tamanhoMaximo"/> é REJEITADO, não cortado. [V] é a
    /// correção direta do defeito de <c>IV_Historico.ResultadoCmpl</c> (<c>varchar(150)</c>
    /// gravado truncado em 20 pela aplicação, em silêncio — documento 01, achado 9.2).
    /// </summary>
    public static bool TentarCriar(string? entrada, out TextoNormalizado texto, int tamanhoMaximo = 4000)
    {
        texto = default;
        if (entrada is null) return false;

        var limpo = RemoverInvisiveis(entrada);
        limpo = EspacosMultiplos().Replace(limpo, " ").Trim();

        if (limpo.Length == 0) return false;
        if (limpo.Length > tamanhoMaximo) return false;

        texto = new TextoNormalizado(limpo);
        return true;
    }

    // Caracteres sem glifo visível mas que ocupam posição na string. Usamos escapes \uXXXX
    // explícitos (em vez do caractere literal) de propósito: um caractere invisível colado
    // direto no código-fonte é ilegível e fácil de corromper num editor ou num diff.
    // O NBSP vira espaço comum ANTES do colapso de espaços múltiplos, para não sobreviver
    // como "espaço disfarçado"; os demais (largura zero, marcas de direção, BOM) somem.
    private static string RemoverInvisiveis(string entrada)
    {
        var semMarcas = entrada
            .Replace('\u00A0', ' ')  // NBSP -> espaço comum
            .Replace("\u200B", "")   // zero-width space
            .Replace("\u200C", "")   // zero-width non-joiner
            .Replace("\u200D", "")   // zero-width joiner
            .Replace("\uFEFF", "")   // BOM
            .Replace("\u200E", "")   // left-to-right mark
            .Replace("\u200F", "");  // right-to-left mark

        // Demais caracteres de controle somem, exceto os que o colapso de espaço abaixo
        // já trata como espaço (tab, quebra de linha).
        return new string(semMarcas.Where(c => !char.IsControl(c) || c is '\t' or '\n' or '\r').ToArray());
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacosMultiplos();

    /// <inheritdoc />
    public override string ToString() => Valor;
}
