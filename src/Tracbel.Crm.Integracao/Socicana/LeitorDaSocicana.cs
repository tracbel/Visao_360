using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace Tracbel.Crm.Integracao.Socicana;

/// <summary>
/// UM MÊS DO PREÇO DO KG DE ATR, como a Socicana publica: o valor do mês e o acumulado da safra.
/// </summary>
/// <param name="Mes">O primeiro dia do mês civil a que o valor se refere.</param>
/// <param name="Safra">A safra, como a Socicana escreve (<c>26/27</c>).</param>
/// <param name="MensalBruto">O preço do mês, em R$ por kg de ATR, com vírgula decimal.</param>
/// <param name="AcumuladoBruto">O preço acumulado da safra até este mês.</param>
public sealed record LinhaDoAtr(DateOnly Mes, string Safra, string MensalBruto, string AcumuladoBruto);

/// <summary>
/// A LEITURA DO PREÇO DO KG DE ATR DA SOCICANA — o preço da cana em São Paulo.
///
/// <para><b>Por que a Socicana.</b> A cana não se vende por saca: o fornecedor recebe por quilo de
/// ATR (açúcar total recuperável) entregue, e o preço do kg de ATR do sistema Consecana é o que a
/// planilha do comercial usava. A Socicana (associação de fornecedores de Guariba) publica a série
/// mensal e acumulada desde a safra 2015/16, numa página pública — agosto de 2026 = R$ 0,8692, o mesmo
/// valor da planilha (conferido em 17/09/2026). O <c>robots.txt</c> do site não restringe nada
/// (conferido em 21/09/2026).</para>
///
/// <para><b>A safra vai de abril a março.</b> "Abril" da safra 26/27 é abril de 2026; "Março" da mesma
/// safra é março de <b>2027</b>. Ler o mês sem a safra poria janeiro a março no ano errado.</para>
///
/// <para><b>É uma página HTML</b>, com uma tabela por safra. O leitor procura o padrão
/// "SAFRA aa/aa" e os pares mês-valor-valor; se a página mudar de forma, ele não acha nenhuma safra e
/// para com erro, em vez de gravar zero.</para>
/// </summary>
/// <param name="http">O cliente HTTP.</param>
public sealed partial class LeitorDaSocicana(HttpClient http)
{
    /// <summary>A página do preço do kg de ATR.</summary>
    public const string EnderecoDoPrecoDoKg = "https://www.socicana.com.br/calculadora-de-atr/preco-do-kg/";

    private static readonly string[] MesesDaSafra =
        ["abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro",
         "janeiro", "fevereiro", "março"];

    /// <summary>Lê todas as safras publicadas.</summary>
    /// <param name="ct">Cancelamento.</param>
    /// <exception cref="InvalidDataException">Quando a página não traz nenhuma safra reconhecível.</exception>
    public async Task<IReadOnlyList<LinhaDoAtr>> LerAsync(CancellationToken ct) =>
        Interpretar(await http.GetStringAsync(EnderecoDoPrecoDoKg, ct));

    /// <summary>Interpreta a página. Público para os testes.</summary>
    /// <param name="html">O HTML da página.</param>
    /// <exception cref="InvalidDataException">Quando a página não traz nenhuma safra reconhecível.</exception>
    public static IReadOnlyList<LinhaDoAtr> Interpretar(string html)
    {
        var linhas = new List<LinhaDoAtr>();

        foreach (Match tabela in Tabela().Matches(html))
        {
            // O TEXTO DA TABELA, sem estilo, sem marcação e com as entidades resolvidas — "Mar&ccedil;o"
            // precisa virar "Março" antes da comparação.
            var texto = WebUtility.HtmlDecode(Estilo().Replace(tabela.Value, " "));
            texto = Espacos().Replace(Marcacao().Replace(texto, " "), " ");

            var safra = Safra().Match(texto);
            if (!safra.Success) continue;

            var anoInicial = 2000 + int.Parse(safra.Groups[1].Value, CultureInfo.InvariantCulture);
            var anoFinal = 2000 + int.Parse(safra.Groups[2].Value, CultureInfo.InvariantCulture);

            if (anoFinal != anoInicial + 1)
                throw new InvalidDataException($"Safra da Socicana fora do padrão: \"{safra.Value}\".");

            foreach (Match mes in MesComValores().Matches(texto))
            {
                var indice = Array.IndexOf(MesesDaSafra, mes.Groups[1].Value.ToLowerInvariant());
                if (indice < 0) continue;

                // De abril (0) a dezembro (8), o ano inicial da safra; de janeiro (9) a março (11), o final.
                var ano = indice <= 8 ? anoInicial : anoFinal;
                var numeroDoMes = indice <= 8 ? indice + 4 : indice - 8;

                linhas.Add(new LinhaDoAtr(
                    new DateOnly(ano, numeroDoMes, 1),
                    $"{safra.Groups[1].Value}/{safra.Groups[2].Value}",
                    mes.Groups[2].Value,
                    mes.Groups[3].Value));
            }
        }

        if (linhas.Count == 0)
            throw new InvalidDataException(
                "A página da Socicana não trouxe nenhuma safra reconhecível. A página mudou de forma; " +
                "nada foi gravado.");

        return linhas;
    }

    /// <summary>O valor em R$ por kg de ATR, com vírgula decimal (<c>0,8692</c>).</summary>
    /// <param name="bruto">O valor como veio.</param>
    /// <exception cref="FormatException">Quando não é um número positivo.</exception>
    public static decimal Valor(string bruto)
    {
        if (!decimal.TryParse(bruto, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("pt-BR"), out var valor)
            || valor <= 0)
            throw new FormatException($"Preço do kg de ATR fora do formato: \"{bruto}\".");

        return valor;
    }

    [GeneratedRegex(@"<table.*?</table>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex Tabela();

    [GeneratedRegex(@"<style.*?</style>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex Estilo();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex Marcacao();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Espacos();

    [GeneratedRegex(@"SAFRA\s+(\d{2})/(\d{2})", RegexOptions.IgnoreCase)]
    private static partial Regex Safra();

    // O MÊS SÓ ENTRA COM OS DOIS VALORES. "Setembro" sem número é mês da safra corrente que ainda não
    // aconteceu — não é zero, é ausência, e não vira linha.
    [GeneratedRegex(@"\b(Abril|Maio|Junho|Julho|Agosto|Setembro|Outubro|Novembro|Dezembro|Janeiro|Fevereiro|Março)\s+(\d+,\d+)\s+(\d+,\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex MesComValores();
}
