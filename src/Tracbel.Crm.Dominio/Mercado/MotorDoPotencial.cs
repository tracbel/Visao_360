using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>Por que o parque ou a demanda anual não saiu (issue 72).</summary>
public enum MotivoSemPotencial
{
    /// <summary>Saiu.</summary>
    Nenhum = 0,

    /// <summary>Nenhuma cultura do recorte tem área divulgada — sigilo do IBGE ou município sem lavoura.</summary>
    SemArea = 1,

    /// <summary>Há área, mas nenhuma cultura com área tem regra vigente de hectares por máquina.</summary>
    SemRegra = 2,

    /// <summary>Há parque, mas o ciclo de renovação não foi informado em alguma parcela (D-P01).</summary>
    SemCicloDeRenovacao = 3
}

/// <summary>
/// UMA CULTURA DENTRO DE UM RECORTE, já com a regra vigente da categoria de máquina em questão.
///
/// <para><b>É a entrada do motor, e ela é burra de propósito:</b> quem lê o banco resolve vigência,
/// catálogo e grupo, e entrega números. O motor não sabe o que é EF, ano da PAM nem data de vigência —
/// é o que permite a calculadora usar exatamente a mesma conta com área digitada à mão.</para>
/// </summary>
/// <param name="CulturaCodigo">O código da cultura no catálogo (issue 165).</param>
/// <param name="CulturaNome">O nome de exibição.</param>
/// <param name="AreaPlantadaHectares">A área plantada da cultura no recorte; nula quando a fonte não divulgou.</param>
/// <param name="HectaresPorMaquina">A regra vigente; nula quando a cultura não tem regra nesta categoria.</param>
/// <param name="AnosDeRenovacao">O ciclo de troca; nulo enquanto ninguém informou (D-P01).</param>
/// <param name="RegraConfirmada">Se a regra foi confirmada pelo comercial; falso acende o selo de estimativa.</param>
/// <param name="GrupoCodigo">O grupo de compartilhamento (issue 160); nulo quando a cultura não divide terra com ninguém.</param>
public sealed record CulturaNoRecorte(
    string CulturaCodigo,
    string CulturaNome,
    decimal? AreaPlantadaHectares,
    decimal? HectaresPorMaquina,
    decimal? AnosDeRenovacao,
    bool RegraConfirmada,
    string? GrupoCodigo = null);

/// <summary>
/// UMA PARCELA DO PARQUE — um grupo que divide a mesma terra, ou uma cultura sozinha.
///
/// <para>A parcela é o que a tela abre quando alguém pergunta "de onde vem este número": ela nomeia a
/// cultura que dimensionou a máquina e as que dividem a terra com ela.</para>
/// </summary>
/// <param name="CulturaCodigo">O código da cultura dominante no catálogo.</param>
/// <param name="Cultura">A cultura dominante — a de maior área, cujos parâmetros valeram.</param>
/// <param name="Compartilhada">As outras culturas da parcela, que não somam área de novo.</param>
/// <param name="AreaUtilHectares">A área que entrou na conta.</param>
/// <param name="Parque">As máquinas que a área comporta.</param>
/// <param name="DemandaAnual">Parque ÷ ciclo de renovação.</param>
/// <param name="Motivo">Por que o parque ou a demanda não saiu, como texto.</param>
public sealed record ParcelaDoParque(
    string CulturaCodigo,
    string Cultura,
    IReadOnlyList<string> Compartilhada,
    decimal? AreaUtilHectares,
    decimal? Parque,
    decimal? DemandaAnual,
    string Motivo);

/// <summary>
/// O POTENCIAL ESTRUTURAL DE UM RECORTE numa categoria de máquina.
/// </summary>
/// <param name="Parque">As máquinas que a área do recorte comporta; nulo com motivo.</param>
/// <param name="DemandaAnual">Quantas máquinas por ano o parque pede; nula com motivo.</param>
/// <param name="AreaUtilHectares">A área que entrou na conta, já sem a terra contada duas vezes.</param>
/// <param name="Estimativa">Se alguma regra usada ainda não foi confirmada pelo comercial (D-P01).</param>
/// <param name="MotivoSemParque">Por que o parque não saiu, como texto; <c>Nenhum</c> quando saiu.</param>
/// <param name="MotivoSemDemanda">Por que a demanda anual não saiu, como texto.</param>
/// <param name="CulturasSemCiclo">As culturas dominantes que não têm ciclo de renovação informado.</param>
/// <param name="Parcelas">O detalhe, uma linha por grupo ou cultura solta.</param>
public sealed record PotencialDoRecorte(
    decimal? Parque,
    decimal? DemandaAnual,
    decimal? AreaUtilHectares,
    bool Estimativa,
    string MotivoSemParque,
    string MotivoSemDemanda,
    IReadOnlyList<string> CulturasSemCiclo,
    IReadOnlyList<ParcelaDoParque> Parcelas);

/// <summary>
/// AS MEDIDAS DA LAVOURA DE UM RECORTE — o que se compara com São Paulo.
///
/// <para><b>A quantidade é de UMA cultura</b>, nunca um total: o IBGE publica cada produto na unidade
/// dele (<see cref="UnidadesDaPam"/>), e somar tonelada com mil frutos daria um número sem unidade.</para>
/// </summary>
/// <param name="AreaPlantadaHectares">Área plantada.</param>
/// <param name="AreaColhidaHectares">Área colhida.</param>
/// <param name="QuantidadeProduzida">Quantidade produzida, na unidade do produto.</param>
/// <param name="ValorDaProducaoMilReais">Valor da produção, em mil reais.</param>
public sealed record MedidasDaLavoura(
    decimal? AreaPlantadaHectares,
    decimal? AreaColhidaHectares,
    decimal? QuantidadeProduzida,
    decimal? ValorDaProducaoMilReais);

/// <summary>
/// A RELEVÂNCIA DE UM RECORTE DENTRO DE SÃO PAULO — que fatia do estado está aqui, e se a terra daqui
/// rende mais ou menos que a média do estado.
/// </summary>
/// <param name="FatiaDaAreaPlantada">Percentual da área plantada do estado.</param>
/// <param name="FatiaDaAreaColhida">Percentual da área colhida do estado.</param>
/// <param name="FatiaDaQuantidade">Percentual da quantidade produzida do estado.</param>
/// <param name="FatiaDoValor">Percentual do valor da produção do estado.</param>
/// <param name="ProdutividadeDoRecorte">Quantidade ÷ área colhida aqui, na unidade do produto.</param>
/// <param name="ProdutividadeNoEstado">A mesma conta em São Paulo inteiro.</param>
/// <param name="RazaoDeProdutividade">Aqui ÷ estado: 1,10 é "10% acima da média de São Paulo".</param>
public sealed record RelevanciaNoEstado(
    decimal? FatiaDaAreaPlantada,
    decimal? FatiaDaAreaColhida,
    decimal? FatiaDaQuantidade,
    decimal? FatiaDoValor,
    decimal? ProdutividadeDoRecorte,
    decimal? ProdutividadeNoEstado,
    decimal? RazaoDeProdutividade);

/// <summary>
/// O MOTOR DO POTENCIAL ESTRUTURAL (issue 72) — parque de máquinas, demanda anual e relevância dentro de
/// São Paulo, a partir dos parâmetros vigentes.
///
/// <para><b>Domínio puro, sem banco.</b> Quem lê o banco resolve vigência, catálogo e grupo e entrega
/// números; o motor faz a conta. É o que permite a <b>calculadora</b> (issue 161) devolver, para a área
/// medida de um município, exatamente o número que o mapa mostra — não é "a mesma fórmula reescrita", é a
/// mesma função.</para>
///
/// <para><b>O que ele faz, em uma linha:</b> parque = área útil ÷ hectares por máquina; demanda anual =
/// parque ÷ ciclo de renovação. A área útil já vem sem a terra contada duas vezes, porque o motor agrupa
/// pelo grupo de compartilhamento (issue 160) antes de dividir.</para>
///
/// <para><b>O que ele não faz:</b> fator de ciclo de mercado e cenários (issue 74) — aqui não entra preço,
/// crédito nem percepção do gestor. O resultado é <b>necessidade teórica de frota</b>, e não venda.</para>
///
/// <para><b>O selo de estimativa.</b> Enquanto a regra usada estiver "a confirmar" (D-P01), todo número que
/// sai dela carrega <see cref="PotencialDoRecorte.Estimativa"/>, e a tela diz isso ao lado do número. A
/// regra de hoje é o exemplo do gerente comercial — um trator 3036N a cada 10 hectares de café —, e ela não
/// foi confirmada por ninguém.</para>
/// </summary>
public static class MotorDoPotencial
{
    /// <summary>
    /// O POTENCIAL DE UM RECORTE numa categoria de máquina.
    ///
    /// <para><b>Cada grupo de compartilhamento vira uma parcela, e cada cultura solta vira a sua.</b> É
    /// dentro da parcela que a terra deixa de ser contada duas vezes (issue 160): a área útil é a maior
    /// das áreas do grupo, com os parâmetros da dona dessa área.</para>
    ///
    /// <para><b>A demanda anual só sai completa.</b> Se uma parcela tem parque mas não tem ciclo de
    /// renovação, a demanda do recorte fica vazia e as culturas sem ciclo são nomeadas — somar só as que
    /// têm daria um total menor que o real, com cara de completo.</para>
    ///
    /// <para><b>A ordem não depende de quem chama:</b> as culturas de cada parcela são ordenadas pelo
    /// código antes da conta, e as parcelas pelo código da dominante. Empate de área fica com o primeiro
    /// código, sempre o mesmo.</para>
    /// </summary>
    /// <param name="culturas">As culturas do recorte, com área, regra e grupo.</param>
    public static PotencialDoRecorte Potencial(IReadOnlyList<CulturaNoRecorte> culturas)
    {
        var parcelas = new List<ParcelaDoParque>();

        // ESTIMATIVA É SOBRE A REGRA QUE FOI USADA. Só a dominante de cada parcela com parque dimensionou
        // máquina; a regra de uma cultura que ficou de fora não torna estimativa um número que não a usou.
        var estimativa = false;

        foreach (var grupo in Agrupar(culturas))
        {
            var entrada = grupo
                .Select(c => new AreaDaCulturaNoGrupo(
                    c.CulturaCodigo, c.CulturaNome, c.AreaPlantadaHectares, c.HectaresPorMaquina, c.AnosDeRenovacao))
                .ToList();

            var parque = PotencialEstrutural.Parque(entrada);
            var dominante = grupo.FirstOrDefault(c => c.CulturaCodigo == parque.CulturaDominanteCodigo) ?? grupo[0];

            if (parque.Maquinas is not null && !dominante.RegraConfirmada) estimativa = true;

            parcelas.Add(new ParcelaDoParque(
                dominante.CulturaCodigo,
                dominante.CulturaNome,
                parque.Compartilhada,
                parque.AreaUtilHectares,
                parque.Maquinas,
                parque.DemandaAnual,
                MotivoDaParcela(parque).ToString()));
        }

        parcelas = [.. parcelas.OrderBy(p => p.CulturaCodigo, StringComparer.Ordinal)];

        var comParque = parcelas.Where(p => p.Parque is not null).ToList();

        decimal? total = comParque.Count == 0 ? null : comParque.Sum(p => p.Parque!.Value);
        decimal? areaUtil = comParque.Count == 0 ? null : comParque.Sum(p => p.AreaUtilHectares ?? 0m);

        // O CICLO FALTA NA DOMINANTE, e é ela que dimensiona a troca. Quem não gerou parque não entra na
        // lista: cultura sem área nenhuma não deve um ciclo a ninguém.
        var semCiclo = comParque.Where(p => p.DemandaAnual is null).Select(p => p.Cultura).Distinct().ToList();

        decimal? demanda = total is null || semCiclo.Count > 0 ? null : comParque.Sum(p => p.DemandaAnual!.Value);

        var motivoDoParque = total is not null ? MotivoSemPotencial.Nenhum
            : culturas.All(c => c.AreaPlantadaHectares is not > 0) ? MotivoSemPotencial.SemArea
            : MotivoSemPotencial.SemRegra;

        var motivoDaDemanda = demanda is not null ? MotivoSemPotencial.Nenhum
            : total is null ? motivoDoParque
            : MotivoSemPotencial.SemCicloDeRenovacao;

        return new PotencialDoRecorte(
            total, demanda, areaUtil, estimativa,
            motivoDoParque.ToString(), motivoDaDemanda.ToString(), semCiclo, parcelas);
    }

    /// <summary>
    /// O RECORTE MAIOR É A SOMA DOS MUNICÍPIOS — a loja, a região da ADR e São Paulo inteiro.
    ///
    /// <para><b>E não o motor rodado sobre as áreas somadas, que daria outro número.</b> O
    /// compartilhamento acontece no chão: é o talhão do município que é o mesmo para a soja e para o
    /// milho safrinha. Aplicar "a área útil é a maior" sobre a soja da região inteira contra o milho da
    /// região inteira afirmaria que toda a soja de Ribeirão Preto e todo o milho de Franca são a mesma
    /// terra — o que não são.</para>
    ///
    /// <para><b>E é o que faz a conta fechar na tela:</b> o total do cabeçalho é a soma da coluna, e quem
    /// conferir no papel chega no mesmo número.</para>
    ///
    /// <para><b>As parcelas se juntam por cultura</b>, para a região poder dizer de onde vem o parque
    /// dela. Quem compartilha terra num município e não em outro aparece nas duas listas de
    /// compartilhamento, sem repetir nome.</para>
    /// </summary>
    /// <param name="recortes">Os recortes menores — tipicamente um por município.</param>
    public static PotencialDoRecorte Somar(IReadOnlyList<PotencialDoRecorte> recortes) =>
        Juntar(recortes, aMesmaTerra: false);

    /// <summary>
    /// AS CATEGORIAS DE MÁQUINA SE SOBREPÕEM NO MESMO CHÃO — o trator e a colheitadeira passam no mesmo
    /// talhão.
    ///
    /// <para><b>As máquinas somam; a terra, não.</b> Um hectare de soja pede um trator a cada tantos
    /// hectares <b>e</b> uma colheitadeira a cada outros tantos: são duas máquinas, e as duas contam. Mas
    /// somar a área útil das duas categorias diria que o município tem o dobro da lavoura que tem — e é
    /// exatamente o erro que a issue 160 conserta entre culturas, repetido entre categorias.</para>
    ///
    /// <para>Por isso a área de uma cultura entra <b>uma vez</b>, pela maior que qualquer categoria usou —
    /// a mesma regra do grupo de compartilhamento, pela mesma razão física.</para>
    /// </summary>
    /// <param name="categorias">O resultado de cada categoria de máquina no MESMO recorte.</param>
    public static PotencialDoRecorte Sobrepor(IReadOnlyList<PotencialDoRecorte> categorias) =>
        Juntar(categorias, aMesmaTerra: true);

    private static PotencialDoRecorte Juntar(IReadOnlyList<PotencialDoRecorte> recortes, bool aMesmaTerra)
    {
        var parcelas = recortes
            .SelectMany(r => r.Parcelas)
            .Where(p => p.Parque is not null)
            .GroupBy(p => p.CulturaCodigo, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g =>
            {
                var comCiclo = g.All(p => p.DemandaAnual is not null);
                return new ParcelaDoParque(
                    g.Key,
                    g.First().Cultura,
                    [.. g.SelectMany(p => p.Compartilhada).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
                    aMesmaTerra ? g.Max(p => p.AreaUtilHectares ?? 0m) : g.Sum(p => p.AreaUtilHectares ?? 0m),
                    g.Sum(p => p.Parque!.Value),
                    comCiclo ? g.Sum(p => p.DemandaAnual!.Value) : null,
                    (comCiclo ? MotivoSemPotencial.Nenhum : MotivoSemPotencial.SemCicloDeRenovacao).ToString());
            })
            .ToList();

        decimal? total = parcelas.Count == 0 ? null : parcelas.Sum(p => p.Parque!.Value);
        decimal? areaUtil = parcelas.Count == 0 ? null : parcelas.Sum(p => p.AreaUtilHectares ?? 0m);

        var semCiclo = parcelas.Where(p => p.DemandaAnual is null).Select(p => p.Cultura).ToList();
        decimal? demanda = total is null || semCiclo.Count > 0 ? null : parcelas.Sum(p => p.DemandaAnual!.Value);

        // SEM PARQUE EM LUGAR NENHUM, o motivo do recorte maior é o dos menores — "sem regra" ganha de
        // "sem área", porque quem tem área e não tem regra precisa ouvir o que falta de verdade.
        var motivoDoParque = total is not null ? MotivoSemPotencial.Nenhum
            : recortes.Any(r => r.MotivoSemParque == nameof(MotivoSemPotencial.SemRegra)) ? MotivoSemPotencial.SemRegra
            : MotivoSemPotencial.SemArea;

        var motivoDaDemanda = demanda is not null ? MotivoSemPotencial.Nenhum
            : total is null ? motivoDoParque
            : MotivoSemPotencial.SemCicloDeRenovacao;

        return new PotencialDoRecorte(
            total, demanda, areaUtil, recortes.Any(r => r.Estimativa),
            motivoDoParque.ToString(), motivoDaDemanda.ToString(), semCiclo, parcelas);
    }

    /// <summary>
    /// A CALCULADORA (issue 72, tela na 161): a mesma conta do mapa, com a área que alguém digitou no
    /// lugar da área medida.
    ///
    /// <para><b>Por que ela é esta função, e não outra.</b> O critério de aceite da calculadora é "a mesma
    /// área de um município devolve o mesmo resultado que o motor gravou para ele". Uma segunda fórmula,
    /// por mais fiel que nascesse, passaria a divergir no dia em que uma das duas mudasse. Aqui a
    /// simulação só troca o número da área e chama <see cref="Potencial"/> — se as áreas informadas forem
    /// as medidas, o resultado é idêntico por construção.</para>
    ///
    /// <para><b>Cultura que a simulação não menciona fica como está</b>, com a área medida. É o que
    /// permite perguntar "e se este município tivesse 5.000 ha de soja?" sem apagar o café que ele tem.
    /// Para simular <b>só</b> a cultura informada, quem chama passa o recorte com aquela cultura.</para>
    ///
    /// <para><b>Código que não existe no recorte é ignorado em silêncio?</b> Não: ele entra como cultura
    /// nova sem regra, e o resultado diz "sem regra" — simular uma cultura que ninguém parametrizou não
    /// pode devolver máquina nenhuma sem explicar por quê.</para>
    /// </summary>
    /// <param name="culturas">O recorte de partida, com as áreas medidas.</param>
    /// <param name="areasInformadas">A área digitada de cada cultura, pelo código do catálogo.</param>
    public static PotencialDoRecorte Simular(
        IReadOnlyList<CulturaNoRecorte> culturas, IReadOnlyDictionary<string, decimal> areasInformadas)
    {
        var simulado = culturas
            .Select(c => areasInformadas.TryGetValue(c.CulturaCodigo, out var area)
                ? c with { AreaPlantadaHectares = area }
                : c)
            .ToList();

        var novas = areasInformadas.Keys
            .Where(codigo => !culturas.Any(c => string.Equals(c.CulturaCodigo, codigo, StringComparison.Ordinal)))
            .OrderBy(codigo => codigo, StringComparer.Ordinal)
            .Select(codigo => new CulturaNoRecorte(codigo, codigo, areasInformadas[codigo], null, null, false));

        return Potencial([.. simulado, .. novas]);
    }

    /// <summary>
    /// A RELEVÂNCIA DE UM RECORTE DENTRO DE SÃO PAULO.
    ///
    /// <para><b>O denominador é o que o IBGE publica para a UF</b>, e não a soma dos municípios (issue
    /// 155): o município sigiloso entra no total do estado sem aparecer embaixo, e somar os municípios
    /// inflaria a fatia da região justamente onde há sigilo.</para>
    ///
    /// <para><b>Denominador zero não vira 100%, vira vazio.</b> Dividir por zero não é "tudo": é "não dá
    /// para dizer". Numerador zero com denominador de pé, sim, é 0% — e isso é informação.</para>
    ///
    /// <para><b>A razão de produtividade só faz sentido na mesma unidade</b>, e faz: os dois lados vêm do
    /// mesmo produto da PAM. 1,10 é "a terra daqui rende 10% acima da média do estado".</para>
    /// </summary>
    /// <param name="recorte">As medidas do recorte.</param>
    /// <param name="estado">As medidas de São Paulo, como o IBGE publica.</param>
    public static RelevanciaNoEstado Relevancia(MedidasDaLavoura recorte, MedidasDaLavoura estado)
    {
        var aqui = UnidadesDaPam.Produtividade(recorte.QuantidadeProduzida, recorte.AreaColhidaHectares);
        var emSp = UnidadesDaPam.Produtividade(estado.QuantidadeProduzida, estado.AreaColhidaHectares);

        return new RelevanciaNoEstado(
            Fatia(recorte.AreaPlantadaHectares, estado.AreaPlantadaHectares),
            Fatia(recorte.AreaColhidaHectares, estado.AreaColhidaHectares),
            Fatia(recorte.QuantidadeProduzida, estado.QuantidadeProduzida),
            Fatia(recorte.ValorDaProducaoMilReais, estado.ValorDaProducaoMilReais),
            aqui,
            emSp,
            aqui is { } a && emSp is > 0 ? a / emSp.Value : null);
    }

    /// <summary>
    /// A FRASE QUE A TELA MOSTRA NO LUGAR DO NÚMERO, ou ao lado dele.
    ///
    /// <para>Vazia quando o número saiu e a regra está confirmada — a tela não põe rodapé para dizer que
    /// está tudo bem.</para>
    /// </summary>
    /// <param name="potencial">O resultado do motor.</param>
    public static string Frase(PotencialDoRecorte potencial)
    {
        var motivo = Enum.Parse<MotivoSemPotencial>(potencial.MotivoSemParque);

        var frase = motivo switch
        {
            MotivoSemPotencial.SemArea =>
                "sem área plantada divulgada aqui — o IBGE mantém em sigilo o município com poucos produtores",
            MotivoSemPotencial.SemRegra =>
                "há área plantada, mas nenhuma cultura daqui tem regra de hectares por máquina vigente",
            _ when potencial.CulturasSemCiclo.Count > 0 =>
                $"o parque saiu; a demanda anual não, porque falta o ciclo de renovação de {Juntar(potencial.CulturasSemCiclo)}",
            _ => string.Empty
        };

        if (!potencial.Estimativa) return frase;

        const string selo = "estimativa: a regra de hectares por máquina ainda não foi confirmada pelo comercial";

        return frase.Length == 0 ? selo : $"{selo}; {frase}";
    }

    /// <summary>
    /// As parcelas do recorte: um grupo de compartilhamento por vez, e cada cultura solta na sua.
    ///
    /// <para>Ordenar aqui, e não deixar a ordem do banco decidir, é o que faz o empate de área ser
    /// sempre resolvido do mesmo jeito.</para>
    /// </summary>
    private static List<List<CulturaNoRecorte>> Agrupar(IReadOnlyList<CulturaNoRecorte> culturas)
    {
        var ordenadas = culturas.OrderBy(c => c.CulturaCodigo, StringComparer.Ordinal).ToList();

        var grupos = ordenadas
            .Where(c => !string.IsNullOrWhiteSpace(c.GrupoCodigo))
            .GroupBy(c => c.GrupoCodigo!, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => g.ToList());

        var soltas = ordenadas
            .Where(c => string.IsNullOrWhiteSpace(c.GrupoCodigo))
            .Select(c => new List<CulturaNoRecorte> { c });

        return [.. grupos, .. soltas];
    }

    private static MotivoSemPotencial MotivoDaParcela(ParqueDoGrupo parque) =>
        parque.AreaUtilHectares is null ? MotivoSemPotencial.SemArea
        : parque.Maquinas is null ? MotivoSemPotencial.SemRegra
        : parque.DemandaAnual is null ? MotivoSemPotencial.SemCicloDeRenovacao
        : MotivoSemPotencial.Nenhum;

    private static decimal? Fatia(decimal? parte, decimal? todo) =>
        parte is { } p && todo is > 0 ? p / todo.Value * 100m : null;

    private static string Juntar(IReadOnlyList<string> nomes) =>
        nomes.Count == 1 ? nomes[0] : $"{string.Join(", ", nomes.Take(nomes.Count - 1))} e {nomes[^1]}";
}
