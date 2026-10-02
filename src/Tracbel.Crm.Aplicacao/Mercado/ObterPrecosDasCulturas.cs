using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Aplicacao.Potencial;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Mercado;

/// <summary>
/// O PREÇO DE COMMODITIES — a variação do preço de cada cultura em quatro horizontes (o mês, o trimestre, o semestre e os 12
/// meses), o preço médio de cada janela e a série histórica (issue 260, a aba "Preço de Commodities" do protótipo da pasta
/// 360).
///
/// <para><b>A conta é a do momento de preço do CRM</b> (<see cref="IndicadoresDeMercado.MomentoDePreco"/>): a média dos
/// últimos N meses sobre a dos N anteriores, com as duas janelas cheias. O R12 é o PRÓPRIO momento dos Indicadores — inclusive
/// o recurso ao preço anual da PAM onde a série mensal ainda não fecha 24 meses (decisão de 27/09/2026) —, para esta tela e o
/// fator de ciclo nunca discordarem.</para>
///
/// <para><b>As faixas são as do CRM</b> (retraído, intermediária, aquecido, superaquecido) nos quatro horizontes, e não os
/// cortes de ±10% e ±20% do protótipo.</para>
/// </summary>
public sealed class ObterPrecosDasCulturas(
    IRepositorioDosPrecosDasCulturas precos,
    IRepositorioDeIndicadoresDeMercado mercado,
    IRepositorioDeParametrosDoPotencial parametros,
    IRelogio relogio)
{
    /// <summary>Os horizontes de comparação, em meses: o mês, R3, R6 e R12.</summary>
    public static readonly IReadOnlyList<int> Horizontes = [1, 3, 6, 12];

    /// <summary>Executa a leitura.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PrecosDasCulturas>>> ExecutarAsync(CancellationToken ct)
    {
        var hoje = ParametroComVigencia.HojeNoBrasil(relogio.Agora);
        var vigente = ParametroComVigencia.VigenteEm(await parametros.ListarGeraisAsync(ct), hoje);
        var series = await precos.LerSeriesAsync(ct);
        var momento = await mercado.LerAsync(hoje, null, ct);

        var culturas = series
            .Select(s =>
            {
                var valores = s.Meses.Select(m => m.Valor).ToList();
                var horizontes = Horizontes
                    .Select(meses =>
                    {
                        // O R12 É O MOMENTO DOS INDICADORES, e não uma conta à parte: é ele que entra no fator de ciclo.
                        if (meses == 12 && momento.PrecoPorCultura.TryGetValue(s.Codigo, out var doCiclo))
                            return new HorizonteDoPreco(meses, doCiclo);

                        var recentes = valores.TakeLast(meses).ToList();
                        var anteriores = valores.SkipLast(meses).TakeLast(meses).ToList();
                        return new HorizonteDoPreco(meses, IndicadoresDeMercado.MomentoDePreco(recentes, anteriores, (short)meses, vigente));
                    })
                    .ToList();

                return new PrecoDaCultura(
                    s.Codigo,
                    s.Nome,
                    s.Fonte,
                    s.Unidade,
                    s.Meses.Count > 0 ? s.Meses[^1].Mes : null,
                    s.Meses.Count > 0 ? s.Meses[^1].Valor : null,
                    horizontes,
                    s.Meses);
            })
            .ToList();

        var resultado = new PrecosDasCulturas(momento.UltimoMesDePreco, culturas, Lacunas(culturas, vigente));

        return Resultado<ComProcedencia<PrecosDasCulturas>>.Ok(
            ComProcedencia<PrecosDasCulturas>.DoNossoBanco(
                resultado,
                "preços mensais da CONAB e o ATR da Socicana · preço implícito anual da PAM onde a série mensal não fecha o R12",
                relogio));
    }

    private static List<MetricaSemDado> Lacunas(List<PrecoDaCultura> culturas, ParametroDoPotencial? vigente)
    {
        var lacunas = new List<MetricaSemDado>
        {
            new("faixas",
                "A situação de cada horizonte é a faixa do momento do CRM (retraído abaixo de 1, intermediária, aquecido, superaquecido), e não os cortes de ±10% e ±20% do protótipo. Num horizonte curto, uma faixa extrema é rara: 20% em um mês é muito."),
            new("custo",
                "É o momento econômico pelo PREÇO, sem custo — como o protótipo. A margem por cultura está na aba Rentabilidade dos Indicadores Geográficos.")
        };

        var anuais = culturas.Where(c => c.Horizontes.Any(h => h.Meses == 12 && h.Indice.Serie == nameof(SerieDoIndiceDePreco.AnualPam))).Select(c => c.Nome).ToList();
        if (anuais.Count > 0)
            lacunas.Add(new MetricaSemDado(
                "r12Anual",
                $"O R12 de {string.Join(", ", anuais)} é o preço anual da PAM (o último ano contra o anterior): a série mensal da CONAB no CRM ainda não tem 24 meses. Ele volta a ser o mensal sozinho quando a série fechar."));

        if (vigente is null)
            lacunas.Add(new MetricaSemDado("faixa", "Não há parâmetros do potencial vigentes: as variações aparecem, mas a faixa não."));

        return lacunas;
    }
}

/// <summary>O preço de commodities consultado.</summary>
/// <param name="UltimoMesDePreco">O mês mais recente com preço entre todas as culturas.</param>
/// <param name="Culturas">Cada cultura, com os quatro horizontes e a série.</param>
/// <param name="Lacunas">O que a leitura não afirma, com o motivo.</param>
public sealed record PrecosDasCulturas(DateOnly? UltimoMesDePreco, IReadOnlyList<PrecoDaCultura> Culturas, IReadOnlyList<MetricaSemDado> Lacunas);

/// <summary>Uma cultura na tela de preços.</summary>
/// <param name="Codigo">O código da cultura.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Fonte">A fonte da série.</param>
/// <param name="Unidade">A unidade em que a fonte publica.</param>
/// <param name="UltimoMes">O mês mais recente da série.</param>
/// <param name="UltimoValor">O valor nele.</param>
/// <param name="Horizontes">Mês, R3, R6 e R12, cada um com as duas médias, o índice e a faixa.</param>
/// <param name="Serie">A série mensal inteira.</param>
public sealed record PrecoDaCultura(
    string Codigo,
    string Nome,
    string Fonte,
    string Unidade,
    DateOnly? UltimoMes,
    decimal? UltimoValor,
    IReadOnlyList<HorizonteDoPreco> Horizontes,
    IReadOnlyList<PrecoDaCulturaNoMes> Serie);

/// <summary>Um horizonte de comparação.</summary>
/// <param name="Meses">1, 3, 6 ou 12.</param>
/// <param name="Indice">A média recente, a anterior, o índice, a faixa e o motivo quando não saiu.</param>
public sealed record HorizonteDoPreco(int Meses, IndiceDeMomento Indice);
