namespace Tracbel.Crm.Dominio.Processo;

/// <summary>Um processo do Vórtice como a regra do estágio o enxerga.</summary>
/// <param name="Numero">O número do processo (<c>IV_PROCDADO.Processo</c>).</param>
/// <param name="Tipo">O tipo (<c>CodProcesso</c>): 31, 41 ou 50 entram; o resto é ignorado.</param>
/// <param name="NumeroDoDna">O <c>ProcessoDNA</c> — nunca nulo no Vórtice: o processo comum aponta para si.</param>
/// <param name="IncluidoEm">A inclusão (<c>IV_PROCESSO.DtaInclusao</c>, UTC); nula em 3.280 processos (27/09/2026).</param>
/// <param name="PrimeiroAndamentoEm">O primeiro registro do histórico do processo (UTC), qualquer resultado.</param>
public sealed record ProcessoNaRegraDoEstagio(
    long Numero, short Tipo, long NumeroDoDna, DateTime? IncluidoEm, DateTime? PrimeiroAndamentoEm);

/// <summary>Uma linha do histórico do Vórtice como a regra do estágio a enxerga.</summary>
/// <param name="Sequencia">O identificador da linha na origem (<c>SeqHistorico</c>) — o desempate de duas linhas na mesma data.</param>
/// <param name="Processo">O processo da linha.</param>
/// <param name="Resultado">O código de resultado (<c>IV_HISTORICO.Resultado</c>).</param>
/// <param name="RealizadoEm">Quando (UTC).</param>
/// <param name="AcaoGeradora">A ação que gerou a linha — só para o <c>DTA_ETAPA</c> do BI.</param>
public sealed record ResultadoNaRegraDoEstagio(long Sequencia, long Processo, int Resultado, DateTime RealizadoEm, int? AcaoGeradora);

/// <summary>Onde o processo ficou depois da regra.</summary>
public enum SituacaoNaRegraDoEstagio
{
    /// <summary>Alcançou ao menos um estágio.</summary>
    NoFunil = 0,

    /// <summary>Aberto antes da janela: fora do universo, sem registro.</summary>
    ForaDaJanela = 1,

    /// <summary>Sem inclusão e sem histórico nenhum: não há data para situá-lo.</summary>
    SemAbertura = 2,

    /// <summary>Na janela, mas sem resultado aceito — próprio ou herdado.</summary>
    SemResultadoAceito = 3,

    /// <summary>Tem filho DNA: sai, e o filho herda os resultados dele.</summary>
    PaiSubstituidoPeloFilhoDna = 4
}

/// <summary>Um estágio que o processo alcançou.</summary>
/// <param name="Estagio">O estágio.</param>
/// <param name="AlcancadoEm">O primeiro resultado que o alcança (UTC).</param>
/// <param name="ResultadoQueAbriu">O código desse resultado.</param>
/// <param name="NumeroDoProcessoDna">O processo DNA de onde o resultado veio; nulo quando é do próprio processo.</param>
/// <param name="PelaEntradaDigital">
/// Lead: o processo teve o resultado da entrada digital do Lead (1278). Qualificado: teve o do Qualificado (3803).
/// Da Cobertura em diante: entrou por um dos dois. É o que deixa ver o subfunil digital (1278 → 3803 → Cobertura →
/// faturado) sem somar os dois.
/// </param>
/// <param name="UltimaAcaoDaEtapaEm">O <c>DTA_ETAPA</c> do BI; nulo em Lead e Qualificado, como no BI.</param>
public sealed record EstagioApurado(
    EstagioDoFunil Estagio, DateTime AlcancadoEm, int ResultadoQueAbriu, long? NumeroDoProcessoDna, bool PelaEntradaDigital,
    DateTime? UltimaAcaoDaEtapaEm)
{
    /// <summary>Se o estágio foi aberto por um resultado do pai DNA.</summary>
    public bool Herdado => NumeroDoProcessoDna is not null;
}

/// <summary>O que a regra concluiu sobre um processo.</summary>
/// <param name="Numero">O processo.</param>
/// <param name="Tipo">31, 41 ou 50.</param>
/// <param name="AbertoEm">A abertura (UTC), quando há.</param>
/// <param name="AberturaDeduzida">Se a abertura veio do primeiro andamento.</param>
/// <param name="Situacao">No funil, ou por que não.</param>
/// <param name="Estagios">Os estágios alcançados, na ordem.</param>
/// <param name="ResultadosComDataFutura">Quantos resultados foram recusados por data futura.</param>
public sealed record ApuracaoDoProcesso(
    long Numero, short Tipo, DateTime? AbertoEm, bool AberturaDeduzida, SituacaoNaRegraDoEstagio Situacao,
    IReadOnlyList<EstagioApurado> Estagios, int ResultadosComDataFutura);

/// <summary>
/// A REGRA DO ESTÁGIO DO FUNIL — pura, sem banco, sem relógio próprio (decisões de 27/09/2026, documento 52 §3).
///
/// <para><b>A especificação é o extrator do BI</b> (<c>(Extrator) Funil de Vendas - Tracbel Agro.qvs</c>, l. 216–1001):
/// o estágio vem do CÓDIGO DE RESULTADO do <c>IV_HISTORICO</c>, e não de campo de fase do processo. O que é diferente
/// do BI, e por decisão:</para>
/// <list type="number">
/// <item><b>Cumulativo, inclusive Lead e Qualificado.</b> O processo alcança o estágio E quando tem um resultado aceito
/// de estágio ≥ E. No BI, Lead e Qualificado não são superconjunto da Cobertura, e Qualificado → Cobertura dava 453%
/// (medido em 27/09/2026).</item>
/// <item><b>A data do estágio é o PRIMEIRO resultado que o alcança.</b> O "mais recente" do BI
/// (<c>MAX(SeqHistorico)</c>) só servia para saber se alcançou alguma vez.</item>
/// <item><b>A regra DNA, escrita.</b> O pai que tem filho DNA (31/41/50) sai; o filho herda os resultados do pai, com a
/// data mínima. A herança é transitiva — o neto herda do filho, que herdou do pai — e só o último da cadeia fica. O
/// pai de outro tipo não transmite nada: o BI filtra o histórico por 31/41/50.</item>
/// <item><b>Janela única desde 01/11/2023</b> (o início do FY24), pela abertura: <c>COALESCE(DtaInclusao, 1º
/// andamento)</c>. O BI tinha duas janelas (2024 e 2026).</item>
/// <item><b>Data futura é recusada</b>: resultado depois de agora + 1 dia é digitação (há datas em 2103 no Vórtice).</item>
/// <item><b>Uma linha por processo por estágio</b> — sem o fan-out por departamento do BI.</item>
/// </list>
/// </summary>
public static class RegraDoEstagio
{
    /// <summary>Os tipos de processo do funil: 31 (prospecção), 41 (venda de máquina) e 50 (venda de equipamento).</summary>
    public static readonly IReadOnlySet<short> TiposDoFunil = new HashSet<short> { 31, 41, 50 };

    /// <summary>
    /// O COMEÇO DA JANELA ÚNICA: 01/11/2023, 00:00 em São Paulo (03:00 UTC) — o primeiro dia do FY24. Trocar por
    /// 01/01/2024 não muda nenhum estágio (medido em 27/09/2026: os 2.071 processos de nov–dez/2023 são do tipo 31 e
    /// nenhum tem resultado aceito).
    /// </summary>
    public static readonly DateTime InicioDaJanela = new(2023, 11, 1, 3, 0, 0, DateTimeKind.Utc);

    /// <summary>Quanto além de agora uma data de resultado ainda é aceita.</summary>
    public static readonly TimeSpan ToleranciaDeDataFutura = TimeSpan.FromDays(1);

    /// <summary>
    /// AS AÇÕES GERADORAS DE CADA ETAPA — o <c>DTA_ETAPA</c> do BI (l. 483, 630, 767 e 898 do extrator). Lead e
    /// Qualificado não têm: no BI, a coluna é nula para eles.
    /// </summary>
    public static readonly IReadOnlyDictionary<EstagioDoFunil, IReadOnlySet<int>> AcoesDaEtapa =
        new Dictionary<EstagioDoFunil, IReadOnlySet<int>>
        {
            [EstagioDoFunil.Cobertura] = new HashSet<int> { 50, 54, 608, 767 },
            [EstagioDoFunil.Negociacao] = new HashSet<int> { 609, 614, 767, 768, 841 },
            [EstagioDoFunil.Pedido] = new HashSet<int> { 609, 768, 769 },
            [EstagioDoFunil.Faturamento] = new HashSet<int> { 597, 808, 823 }
        };

    /// <summary>Todas as ações geradoras que a regra usa — as que a leitura do histórico precisa trazer.</summary>
    public static IReadOnlyCollection<int> TodasAsAcoesDaEtapa =>
        AcoesDaEtapa.Values.SelectMany(a => a).Distinct().Order().ToList();

    /// <summary>
    /// Apura o funil.
    /// </summary>
    /// <param name="processos">Os processos lidos — todos os 31/41/50, sem janela: o pai antigo transmite ao filho novo.</param>
    /// <param name="historico">As linhas do histórico com resultado classificado ou ação geradora de etapa.</param>
    /// <param name="estagioPorResultado">O maior estágio que cada código de resultado prova (a classificação).</param>
    /// <param name="agora">O instante da apuração (UTC) — o limite da data futura.</param>
    public static IReadOnlyList<ApuracaoDoProcesso> Apurar(
        IEnumerable<ProcessoNaRegraDoEstagio> processos,
        IEnumerable<ResultadoNaRegraDoEstagio> historico,
        IReadOnlyDictionary<int, EstagioDoFunil> estagioPorResultado,
        DateTime agora)
    {
        var porNumero = new Dictionary<long, ProcessoNaRegraDoEstagio>();
        foreach (var processo in processos)
            if (TiposDoFunil.Contains(processo.Tipo)) porNumero.TryAdd(processo.Numero, processo);

        var limite = agora + ToleranciaDeDataFutura;
        var aceitos = new Dictionary<long, List<ResultadoNaRegraDoEstagio>>();
        var acoes = new Dictionary<long, List<ResultadoNaRegraDoEstagio>>();
        var futuros = new Dictionary<long, int>();

        foreach (var linha in historico)
        {
            if (!porNumero.ContainsKey(linha.Processo)) continue;

            if (linha.RealizadoEm > limite)
            {
                futuros[linha.Processo] = futuros.GetValueOrDefault(linha.Processo) + 1;
                continue;
            }

            if (estagioPorResultado.ContainsKey(linha.Resultado)) Acrescentar(aceitos, linha);
            if (linha.AcaoGeradora is not null) Acrescentar(acoes, linha);
        }

        // O PAI COM FILHO DNA — o filho precisa ser do funil; o de outro tipo não substitui ninguém.
        var paisComFilho = porNumero.Values
            .Where(p => p.NumeroDoDna != p.Numero && porNumero.ContainsKey(p.NumeroDoDna))
            .Select(p => p.NumeroDoDna)
            .ToHashSet();

        var apuracoes = new List<ApuracaoDoProcesso>(porNumero.Count);
        foreach (var processo in porNumero.Values)
        {
            var linhagem = Linhagem(processo, porNumero, aceitos);
            // A ABERTURA: a inclusão; nula, o primeiro andamento; sem andamento próprio, o primeiro resultado herdado —
            // é o COALESCE do BI, que usa a data do histórico quando a inclusão falta.
            var abertoEm = processo.IncluidoEm ?? processo.PrimeiroAndamentoEm ?? linhagem.Min(r => (DateTime?)r.RealizadoEm);
            var deduzida = processo.IncluidoEm is null;
            var comFutura = futuros.GetValueOrDefault(processo.Numero);

            ApuracaoDoProcesso Sem(SituacaoNaRegraDoEstagio situacao) =>
                new(processo.Numero, processo.Tipo, abertoEm, deduzida, situacao, [], comFutura);

            if (abertoEm is null) { apuracoes.Add(Sem(SituacaoNaRegraDoEstagio.SemAbertura)); continue; }
            if (abertoEm < InicioDaJanela) { apuracoes.Add(Sem(SituacaoNaRegraDoEstagio.ForaDaJanela)); continue; }
            if (paisComFilho.Contains(processo.Numero)) { apuracoes.Add(Sem(SituacaoNaRegraDoEstagio.PaiSubstituidoPeloFilhoDna)); continue; }

            var estagios = Estagios(processo.Numero, linhagem, estagioPorResultado, acoes.GetValueOrDefault(processo.Numero));
            apuracoes.Add(estagios.Count == 0
                ? Sem(SituacaoNaRegraDoEstagio.SemResultadoAceito)
                : new ApuracaoDoProcesso(processo.Numero, processo.Tipo, abertoEm, deduzida, SituacaoNaRegraDoEstagio.NoFunil, estagios, comFutura));
        }

        return apuracoes;
    }

    /// <summary>
    /// Os resultados aceitos do processo e de toda a cadeia DNA acima dele — o pai, o avô —, parando no primeiro que
    /// não é do funil. A guarda de ciclo existe porque o dado é do Vórtice: dois processos apontando um para o outro
    /// não travam a rotina.
    /// </summary>
    private static List<ResultadoNaRegraDoEstagio> Linhagem(
        ProcessoNaRegraDoEstagio processo, IReadOnlyDictionary<long, ProcessoNaRegraDoEstagio> porNumero,
        IReadOnlyDictionary<long, List<ResultadoNaRegraDoEstagio>> aceitos)
    {
        var linhagem = new List<ResultadoNaRegraDoEstagio>();
        var vistos = new HashSet<long>();
        var atual = processo;

        while (vistos.Add(atual.Numero))
        {
            if (aceitos.TryGetValue(atual.Numero, out var proprios)) linhagem.AddRange(proprios);
            if (atual.NumeroDoDna == atual.Numero || !porNumero.TryGetValue(atual.NumeroDoDna, out var pai)) break;
            atual = pai;
        }

        return linhagem;
    }

    private static List<EstagioApurado> Estagios(
        long numero, IReadOnlyList<ResultadoNaRegraDoEstagio> linhagem, IReadOnlyDictionary<int, EstagioDoFunil> estagioPorResultado,
        IReadOnlyList<ResultadoNaRegraDoEstagio>? acoesProprias)
    {
        var estagios = new List<EstagioApurado>();
        if (linhagem.Count == 0) return estagios;

        // A ENTRADA DIGITAL é o resultado que prova só Lead (1278) ou só Qualificado (3803): o que fica abaixo da
        // Cobertura. A classificação diz quais são, e não uma lista escrita aqui.
        var teveEntradaDoLead = linhagem.Any(r => estagioPorResultado[r.Resultado] == EstagioDoFunil.Lead);
        var teveEntradaDoQualificado = linhagem.Any(r => estagioPorResultado[r.Resultado] == EstagioDoFunil.Qualificado);

        foreach (var estagio in Enum.GetValues<EstagioDoFunil>())
        {
            var primeiro = linhagem
                .Where(r => estagioPorResultado[r.Resultado] >= estagio)
                .OrderBy(r => r.RealizadoEm).ThenBy(r => r.Sequencia)
                .FirstOrDefault();
            if (primeiro is null) break;

            var digital = estagio switch
            {
                EstagioDoFunil.Lead => teveEntradaDoLead,
                EstagioDoFunil.Qualificado => teveEntradaDoQualificado,
                _ => teveEntradaDoLead || teveEntradaDoQualificado
            };

            DateTime? ultimaAcao = null;
            if (acoesProprias is not null && AcoesDaEtapa.TryGetValue(estagio, out var daEtapa))
            {
                var dela = acoesProprias.Where(a => daEtapa.Contains(a.AcaoGeradora!.Value)).ToList();
                if (dela.Count > 0) ultimaAcao = dela.Max(a => a.RealizadoEm);
            }

            estagios.Add(new EstagioApurado(
                estagio, primeiro.RealizadoEm, primeiro.Resultado, primeiro.Processo == numero ? null : primeiro.Processo,
                digital, ultimaAcao));
        }

        return estagios;
    }

    private static void Acrescentar(Dictionary<long, List<ResultadoNaRegraDoEstagio>> mapa, ResultadoNaRegraDoEstagio linha)
    {
        if (!mapa.TryGetValue(linha.Processo, out var lista)) mapa[linha.Processo] = lista = [];
        lista.Add(linha);
    }
}
