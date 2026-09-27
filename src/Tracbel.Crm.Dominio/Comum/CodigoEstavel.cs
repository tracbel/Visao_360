using System.Globalization;
using System.Text;

namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// O CÓDIGO ESTÁVEL DE UM TEXTO DA ORIGEM: sem acento, maiúsculo, só letra, número e sublinhado — "TRATOR MÉDIO" e
/// "Trator Medio" são o mesmo <c>TRATOR_MEDIO</c>.
///
/// <para><b>Mora no domínio desde a meta de venda (#138, 27/09/2026)</b> porque dois lados precisam da MESMA conta: a
/// carga do ART codifica a linha da venda, a da API Gestão de Negócios codifica a linha da meta, e a leitura do realizado
/// casa as duas pelo código. Duas cópias do algoritmo seriam dois lugares para discordar em silêncio — a regra de
/// <c>ClassificacaoDoArt.Codificar</c> passou a chamar esta.</para>
/// </summary>
public static class CodigoEstavel
{
    /// <summary>O código de um texto.</summary>
    /// <param name="texto">O texto da origem.</param>
    /// <param name="tamanhoMaximo">O tamanho da coluna.</param>
    public static string De(string texto, int tamanhoMaximo = 80)
    {
        var construtor = new StringBuilder(texto.Length);
        foreach (var caractere in texto.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark) continue;
            construtor.Append(char.IsAsciiLetterOrDigit(caractere) ? char.ToUpperInvariant(caractere) : '_');
        }

        var codigo = construtor.ToString();
        while (codigo.Contains("__", StringComparison.Ordinal))
            codigo = codigo.Replace("__", "_", StringComparison.Ordinal);

        codigo = codigo.Trim('_');
        if (codigo.Length > tamanhoMaximo) codigo = codigo[..tamanhoMaximo].TrimEnd('_');
        return codigo.Length == 0 ? "SEM_CODIGO" : codigo;
    }
}
