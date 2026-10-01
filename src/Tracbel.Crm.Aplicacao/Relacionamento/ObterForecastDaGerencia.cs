using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Relacionamento;

/// <summary>
/// O FORECAST DA GERÊNCIA de um mês (28/09/2026) — o que <c>GET /api/v1/relatorios/forecast</c> devolve.
/// </summary>
/// <param name="Competencia">O mês, no dia 1.</param>
/// <param name="Texto">"set/2026".</param>
/// <param name="MesesDisponiveis">Os meses que têm forecast — é entre eles que a tela troca.</param>
/// <param name="Alcance"><c>Organizacao</c> quando quem lê está em "Todas as filiais" e o PO e o realizado são da organização
/// inteira; <c>Filiais</c> quando são só das filiais escolhidas.</param>
/// <param name="Gestores">Cada gestor, linha a linha: PO do time, forecast, best guess e realizado do time.</param>
/// <param name="Total">A soma, linha a linha — com as vendas sem gestor.</param>
/// <param name="VendasSemGestor">As vendas do mês cujo vendedor não está no de-para: entram no total e em gestor nenhum.</param>
/// <param name="LidoEm">A última leitura do forecast (UTC).</param>
/// <param name="GeradoNaOrigemEm">Quando a API gerou o cadastro nessa leitura (UTC).</param>
/// <param name="MetricasSemDado">O que o número não diz.</param>
public sealed record RelatorioDoForecast(
    DateOnly Competencia,
    string Texto,
    IReadOnlyList<DateOnly> MesesDisponiveis,
    string Alcance,
    IReadOnlyList<ForecastDoGestor> Gestores,
    IReadOnlyList<LinhaDoForecast> Total,
    int VendasSemGestor,
    DateTime? LidoEm,
    DateTime? GeradoNaOrigemEm,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>
/// O FORECAST DA GERÊNCIA (decisão do Ricardo em 28/09/2026) — a previsão de cada gestor da API Gestão de Negócios (Forecast,
/// revisto todo mês; Best Guess, toda segunda), ao lado do PO (a meta do time) e do realizado do time, no mês. É a tela
/// "Forecast Gerência" da GN, com o realizado que o CRM tem.
///
/// <para><b>É da gerência.</b> Pede <c>Meta.Ler</c> a partir do alcance da filial: quem só vê a própria meta não vê a previsão
/// dos gestores. O PO e o realizado são das filiais escolhidas no seletor; o forecast é do gestor inteiro, mas só dos gestores
/// dessas filiais (01/10/2026) — quando a escolha não é "Todas as filiais", a resposta diz, e conta os que ficaram fora.</para>
///
/// <para><b>O mês padrão é o corrente</b> (de São Paulo), quando tem forecast; senão, o mais recente com forecast.</para>
/// </summary>
/// <param name="repositorio">Onde o forecast é apurado.</param>
/// <param name="acesso">Quem pergunta.</param>
/// <param name="relogio">O relógio.</param>
public sealed class ObterForecastDaGerencia(IRepositorioDoForecast repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    private static readonly CultureInfo Portugues = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Executa a apuração.</summary>
    /// <param name="competencia">O mês (<c>AAAA-MM</c> ou <c>AAAA-MM-DD</c>); vazio é o padrão.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<RelatorioDoForecast>>> ExecutarAsync(string? competencia, CancellationToken ct)
    {
        var profundidade = acesso.Atual.ProfundidadeDe(Permissoes.MetaLer);
        if (profundidade < Profundidade.Empresa)
            return Resultado<ComProcedencia<RelatorioDoForecast>>.SemPermissao(
                $"O forecast é da gerência: ele pede '{Permissoes.MetaLer}' ({Permissoes.Catalogo[Permissoes.MetaLer]}) a partir da filial inteira.");

        DateOnly? pedido = null;
        if (!string.IsNullOrWhiteSpace(competencia))
        {
            var bruto = competencia.Trim();
            if (!DateOnly.TryParseExact(bruto, bruto.Length == 7 ? "yyyy-MM" : "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
                || data.Year is < 2020 or > 2100)
            {
                var erros = new ColetorDeErros();
                erros.Registrar("competencia", "Informe o mês como AAAA-MM.", competencia);
                return erros.Recusar<ComProcedencia<RelatorioDoForecast>>("A consulta tem parâmetros que não valem.");
            }

            pedido = new DateOnly(data.Year, data.Month, 1);
        }

        var meses = await repositorio.MesesComForecastAsync(ct);
        var corrente = AnoFiscal.MesCorrenteEmSaoPaulo(relogio.Agora);
        var mes = pedido ?? MesPadrao(meses, corrente);

        // A ORGANIZAÇÃO É "TODAS AS FILIAIS" NO SELETOR — e não a profundidade: a diretoria lê a meta na filial e vê todas pela
        // escolha no seletor; o administrador, com a profundidade da organização, olhando uma filial, vê só ela. Fora dela, o
        // forecast é só dos gestores das filiais escolhidas — as mesmas que o filtro global deixa ver.
        var organizacao = acesso.Atual.VeTodasAsFiliais;
        IReadOnlySet<int>? filiais = organizacao ? null : new HashSet<int>(acesso.Atual.EmpresasVisiveis) { acesso.Atual.EmpresaId };

        var apurado = await repositorio.ApurarAsync(mes, ct, filiais);

        var resposta = new RelatorioDoForecast(
            apurado.Competencia,
            JanelaDeCompetencia.Mes(apurado.Competencia),
            meses,
            organizacao ? "Organizacao" : "Filiais",
            apurado.Gestores,
            apurado.Total,
            apurado.VendasSemGestor,
            apurado.LidoEm,
            apurado.GeradoNaOrigemEm,
            Lacunas(apurado, organizacao, meses));

        return Resultado<ComProcedencia<RelatorioDoForecast>>.Ok(
            new ComProcedencia<RelatorioDoForecast>(resposta, ProcedenciaDaLeitura(apurado.LidoEm, relogio.Agora)));
    }

    /// <summary>Mais do que isto sem leitura da GN, e a tela avisa: a rotina lê de hora em hora.</summary>
    public static readonly TimeSpan IdadeQueDesatualiza = TimeSpan.FromHours(3);

    private const string ObjetoDaProcedencia =
        "organizacao.ForecastDaGerencia · organizacao.GestorDoConsultor (API Gestão de Negócios) · organizacao.MetaDeVenda · frota.VendaDeMaquina (ART)";

    /// <summary>
    /// "DADOS ATUALIZADOS EM" É A HORA DA LEITURA DA GN (01/10/2026), e não a da consulta ao nosso banco: em 01/10 a tela disse
    /// 11:32 com o forecast lido às 06:00, antes de cinco dos sete gestores digitarem o mês. Sem leitura, fica a hora da
    /// consulta — e a lacuna <c>forecastNaoLido</c> diz o resto.
    /// </summary>
    /// <param name="lidoEm">A última leitura do forecast (UTC).</param>
    /// <param name="agora">Agora (UTC).</param>
    public static Procedencia ProcedenciaDaLeitura(DateTime? lidoEm, DateTime agora)
    {
        if (lidoEm is not { } lido) return Procedencias.DoNossoBanco(ObjetoDaProcedencia, agora);

        var velho = agora - lido > IdadeQueDesatualiza;
        return new Procedencia(
            Procedencias.SistemaProprio,
            ObjetoDaProcedencia,
            lido,
            null,
            velho,
            velho
                ? Texto($"leu o forecast da Gestão de Negócios pela última vez em {lido + FusoDeSaoPaulo:dd/MM/yyyy 'às' HH:mm}, há mais de {IdadeQueDesatualiza.TotalHours:N0} horas: o que os gestores digitaram depois ainda não está aqui. A leitura é da rotina \"Metas de venda (Gestão de Negócios)\", em Configurações › Integrações.")
                : null);
    }

    private static readonly TimeSpan FusoDeSaoPaulo = TimeSpan.FromHours(-3);

    /// <summary>
    /// O mês que a tela abre: o corrente, quando tem forecast; senão, o mais recente com forecast até ele; senão, o mais
    /// recente de todos (forecast de mês futuro); sem forecast nenhum, o corrente.
    /// </summary>
    /// <param name="meses">Os meses com forecast, em ordem.</param>
    /// <param name="corrente">O mês corrente em São Paulo.</param>
    public static DateOnly MesPadrao(IReadOnlyList<DateOnly> meses, DateOnly corrente)
    {
        if (meses.Contains(corrente)) return corrente;
        var anteriores = meses.Where(m => m < corrente).ToList();
        if (anteriores.Count > 0) return anteriores.Max();
        return meses.Count > 0 ? meses[^1] : corrente;
    }

    private static string Texto(FormattableString texto) => texto.ToString(Portugues);

    /// <summary>O que o número não diz — cada frase com a medida que a sustenta.</summary>
    private static List<MetricaSemDado> Lacunas(ForecastApurado a, bool organizacao, IReadOnlyList<DateOnly> meses)
    {
        var lacunas = new List<MetricaSemDado>();

        if (a.LidoEm is null)
            lacunas.Add(new MetricaSemDado("forecastNaoLido",
                "O forecast da API Gestão de Negócios ainda não foi lido: ele vem da rotina \"Metas de venda (Gestão de Negócios)\", no " +
                "modo do planejamento. Sem a leitura, a previsão fica vazia — e não zero."));
        else if (!meses.Contains(a.Competencia))
            lacunas.Add(new MetricaSemDado("semForecastNoMes",
                Texto($"Nenhum gestor informou forecast para {JanelaDeCompetencia.Mes(a.Competencia)}: a coluna fica vazia, e não com zero.")));
        else
        {
            // O QUE A GN AINDA NÃO TEM (01/10/2026): no dia 1º, 37 linhas de forecast e nenhum best guess — o best guess vem
            // depois, e a tela dizia só "nenhum gestor informou", sem dizer até quando.
            var linhas = a.Gestores.SelectMany(g => g.Linhas).ToList();
            if (linhas.Count > 0 && linhas.All(l => l.BestGuess is null))
                lacunas.Add(new MetricaSemDado("semBestGuessNoMes",
                    Texto($"Nenhum gestor informou o best guess de {JanelaDeCompetencia.Mes(a.Competencia)} na Gestão de Negócios até a última leitura: a coluna fica vazia, e não com zero.")));

            var semForecast = a.Gestores.Count(g => g.Linhas.Any(l => l.Meta > 0) && g.Linhas.All(l => l.Forecast is null));
            if (semForecast > 0)
                lacunas.Add(new MetricaSemDado("gestoresSemForecast",
                    Texto($"{semForecast:N0} gestor(es) com PG em {JanelaDeCompetencia.Mes(a.Competencia)} não informaram forecast na Gestão de Negócios: o forecast deles fica vazio, e não com zero.")));
        }

        if (!organizacao)
            lacunas.Add(new MetricaSemDado("alcanceDasFiliais",
                "O PO e o realizado são só da filial escolhida; o forecast é o do gestor inteiro, que atravessa filiais. A comparação exata é em \"Todas as filiais\"."));

        if (a.GestoresForaDoRecorte > 0)
            lacunas.Add(new MetricaSemDado("gestoresForaDoRecorte",
                Texto($"{a.GestoresForaDoRecorte:N0} gestor(es) com forecast em {JanelaDeCompetencia.Mes(a.Competencia)} ficam fora da filial escolhida: nenhum consultor do time é dela, e o time não tem meta nem venda nela. O forecast da gerência inteira está em \"Todas as filiais\".")));

        if (a.VendasSemGestor > 0)
            lacunas.Add(new MetricaSemDado("vendasSemGestor",
                Texto($"{a.VendasSemGestor:N0} venda(s) do mês têm vendedor fora do de-para de consultores (ou sem vendedor no ART): entram no total e em gestor nenhum.")));

        lacunas.Add(new MetricaSemDado("observacao",
            "A observação que o gestor escreve na GN não é lida pelo CRM: é texto livre e pode citar cliente pelo nome."));

        return lacunas;
    }
}
