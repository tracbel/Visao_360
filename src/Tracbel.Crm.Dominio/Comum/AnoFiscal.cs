namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// UMA JANELA DE COMPETÊNCIAS — o primeiro e o último mês, os dois inclusive, sempre no dia 1.
///
/// <para><b>Por que um tipo, e não dois <c>DateOnly</c> soltos.</b> A comparação com o ano anterior
/// (decidida em 27/09/2026) pede a MESMA janela doze meses para trás, e cada leitura que fizer essa conta
/// à mão é uma chance de uma delas errar o mês de novembro. A conta mora aqui, uma vez, com teste.</para>
/// </summary>
/// <param name="Inicial">O primeiro mês, no dia 1.</param>
/// <param name="Final">O último mês, no dia 1, inclusive.</param>
public sealed record JanelaDeCompetencia(DateOnly Inicial, DateOnly Final)
{
    private static readonly string[] Meses3 = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];

    /// <summary>Quantos meses a janela cobre, contando os dois das pontas.</summary>
    public int Meses => ((Final.Year - Inicial.Year) * 12) + Final.Month - Inicial.Month + 1;

    /// <summary>
    /// O MESMO TRECHO DO ANO ANTERIOR — a janela inteira doze meses para trás.
    ///
    /// <para><b>É a comparação que a diretoria faz</b> (decisão do Ricardo de 27/09/2026): o ano fiscal até
    /// agosto contra o ano fiscal anterior até agosto — nov/2025 a ago/2026 contra nov/2024 a ago/2025. Comparar
    /// com os dez meses imediatamente anteriores misturaria safras e a sazonalidade de fim de ano; doze meses
    /// para trás põem cada mês contra ele mesmo.</para>
    /// </summary>
    public JanelaDeCompetencia DoAnoAnterior() => new(Inicial.AddMonths(-12), Final.AddMonths(-12));

    /// <summary>Se a competência (qualquer dia do mês) cai dentro da janela.</summary>
    /// <param name="competencia">O mês a conferir.</param>
    public bool Contem(DateOnly competencia)
    {
        var mes = new DateOnly(competencia.Year, competencia.Month, 1);
        return mes >= Inicial && mes <= Final;
    }

    /// <summary>"nov/2025 a ago/2026" — o intervalo por extenso, para frase e dica.</summary>
    public string Texto => $"{Mes(Inicial)} a {Mes(Final)}";

    /// <summary><c>2025-11-01</c> vira <c>nov/2025</c>, o formato que a tela já usa.</summary>
    /// <param name="competencia">O mês.</param>
    public static string Mes(DateOnly competencia) => $"{Meses3[competencia.Month - 1]}/{competencia.Year}";
}

/// <summary>
/// O ANO FISCAL DA TRACBEL — de <b>novembro a outubro</b>, com o nome do ano em que termina.
///
/// <para><b>Confirmado pelo Ricardo em 24/09/2026</b>, e tornado o <b>período padrão das telas</b> em
/// 27/09/2026: o ano fiscal até hoje (FYTD), comparado com o mesmo trecho do ano fiscal anterior. O FY2026
/// vai de nov/2025 a out/2026. Até aqui a tela de Indicadores já oferecia o ano fiscal como opção do filtro
/// (issue 69, #231), mas o servidor abria nos doze meses fechados e a Visão 360 somava o ano civil.</para>
///
/// <para><b>"Até hoje" é até o último mês FECHADO</b> quando o número é comparado: o faturamento é mensal, e um
/// mês pela metade contra o mesmo mês inteiro do ano anterior erraria para baixo sem avisar — a regra que o
/// filtro de período já seguia. O cartão de realizado da Visão 360, que não compara, soma até o mês em curso e
/// diz que ele está em curso.</para>
///
/// <para><b>Novembro e dezembro são os meses que pegam</b>: neles o ano fiscal vai à frente do civil, e derivar
/// um do outro erra o ano inteiro. Por isso a conta mora aqui, uma vez, e não em cada leitura.</para>
/// </summary>
public static class AnoFiscal
{
    /// <summary>O mês em que o ano fiscal começa: novembro.</summary>
    public const int MesInicial = 11;

    /// <summary>O ano fiscal de uma competência — 2026 para qualquer mês de nov/2025 a out/2026.</summary>
    /// <param name="competencia">O mês (qualquer dia).</param>
    public static int Do(DateOnly competencia) => competencia.Month >= MesInicial ? competencia.Year + 1 : competencia.Year;

    /// <summary>O primeiro mês do ano fiscal que contém a competência.</summary>
    /// <param name="competencia">O mês (qualquer dia).</param>
    public static DateOnly InicioDe(DateOnly competencia) =>
        new(competencia.Month >= MesInicial ? competencia.Year : competencia.Year - 1, MesInicial, 1);

    /// <summary>Os doze meses de um ano fiscal: nov/(N−1) a out/N.</summary>
    /// <param name="anoFiscal">O nome do ano — o ano civil em que ele termina.</param>
    public static JanelaDeCompetencia Inteiro(int anoFiscal) =>
        new(new DateOnly(anoFiscal - 1, MesInicial, 1), new DateOnly(anoFiscal, MesInicial - 1, 1));

    /// <summary>
    /// O ANO FISCAL ATÉ O ÚLTIMO MÊS FECHADO — o período padrão das telas que comparam (27/09/2026).
    ///
    /// <para><b>Em novembro é o ano fiscal que acabou de fechar, inteiro</b>: o último mês fechado é outubro,
    /// que é a última competência daquele ano. Não é caso especial — é a mesma conta.</para>
    /// </summary>
    /// <param name="mesCorrente">O mês corrente, no dia 1 — ele fica de fora, por estar em curso.</param>
    public static JanelaDeCompetencia AteOUltimoMesFechado(DateOnly mesCorrente)
    {
        var ultimoFechado = new DateOnly(mesCorrente.Year, mesCorrente.Month, 1).AddMonths(-1);
        return new JanelaDeCompetencia(InicioDe(ultimoFechado), ultimoFechado);
    }

    /// <summary>"FY2026" — sempre ao lado do intervalo escrito, nunca sozinho (ele se lê como ano civil).</summary>
    /// <param name="anoFiscal">O ano fiscal.</param>
    public static string Nome(int anoFiscal) => $"FY{anoFiscal}";
}
