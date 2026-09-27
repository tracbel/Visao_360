namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>Por que um dos quatro números de decisão não saiu.</summary>
public enum MotivoSemNumeroDeDecisao
{
    /// <summary>Saiu.</summary>
    Nenhum = 0,

    /// <summary>Não há demanda anual — o motor não a produziu, e ela é o denominador ou a base.</summary>
    SemDemandaAnual = 1,

    /// <summary>Não há vendas da Tracbel em UNIDADES para o recorte (issue 69).</summary>
    SemVendasEmUnidades = 2,

    /// <summary>Nenhuma categoria tem preço de referência (issue 70).</summary>
    SemPrecoDeMaquina = 3
}

/// <summary>
/// Um dos quatro números de decisão: o valor, ou o motivo de não haver valor.
/// </summary>
/// <param name="Valor">O número; nulo quando não há.</param>
/// <param name="Motivo">Um <see cref="MotivoSemNumeroDeDecisao"/> como texto.</param>
/// <param name="Frase">
/// A ausência explicada, pronta para a tela; vazia quando o número saiu.
///
/// <para><b>Ela vem do servidor de propósito.</b> A alternativa — mandar só o código e o front montar a
/// frase — devolveria a redação para o TypeScript, que é de onde ela está saindo. Com a frase aqui, a
/// página e a ficha do município dizem a mesma coisa porque é literalmente o mesmo texto, e ele tem teste.</para>
/// </param>
public sealed record NumeroDeDecisao(decimal? Valor, string Motivo, string Frase)
{
    /// <summary>O número que saiu.</summary>
    /// <param name="valor">O valor.</param>
    public static NumeroDeDecisao De(decimal valor) =>
        new(valor, nameof(MotivoSemNumeroDeDecisao.Nenhum), string.Empty);

    /// <summary>A ausência, com o motivo e a frase.</summary>
    /// <param name="motivo">Por que não saiu.</param>
    /// <param name="nome">Como a frase chama este número — "a captura", "o mercado anual".</param>
    public static NumeroDeDecisao Sem(MotivoSemNumeroDeDecisao motivo, string nome) =>
        new(null, motivo.ToString(), DecisaoDoMercado.Frase(motivo.ToString(), nome));
}

/// <summary>
/// O MERCADO ANUAL, que é o único dos quatro que pode sair PELA METADE.
/// </summary>
/// <param name="Valor">A soma das categorias que têm preço; nulo quando nenhuma tem.</param>
/// <param name="Motivo">Um <see cref="MotivoSemNumeroDeDecisao"/> como texto.</param>
/// <param name="Frase">A ausência explicada; vazia quando saiu.</param>
/// <param name="Parcial">Se alguma categoria ficou de fora por não ter preço.</param>
/// <param name="CategoriasSemPreco">Quais ficaram de fora, pelo nome.</param>
public sealed record MercadoAnual(
    decimal? Valor,
    string Motivo,
    string Frase,
    bool Parcial,
    IReadOnlyList<string> CategoriasSemPreco);

/// <summary>A demanda de uma categoria e o preço de referência dela, quando houver.</summary>
/// <param name="Categoria">O nome da categoria, para dizer qual ficou de fora.</param>
/// <param name="DemandaAnual">A demanda anual daquela categoria, em máquinas.</param>
/// <param name="PrecoDeReferencia">O preço de referência daquela categoria (issue 70); nulo quando não há.</param>
public sealed record DemandaDaCategoria(string Categoria, decimal DemandaAnual, decimal? PrecoDeReferencia);

/// <summary>
/// O QUE A CAPTURA CONTOU — as máquinas vendidas das categorias que têm demanda, e o que ficou de fora.
///
/// <para><b>Os dois lados da razão são da mesma máquina.</b> A demanda sai das regras de potencial, e a
/// regra é por categoria (D-P01): com regra só de trator, a demanda é de trator. Dividir por ela também a
/// colheitadeira, o pulverizador e a colhedora de cana que o ART traz inflaria a captura — seria pôr a
/// venda de colheitadeira contra a demanda de trator.</para>
///
/// <para><b>O que fica de fora não some:</b> sai contado, com a frase que diz por quê. Quando outra
/// categoria ganhar regra, ela entra sozinha — a lista vem da demanda, e não de uma constante.</para>
/// </summary>
/// <param name="Unidades">As máquinas vendidas nas categorias com demanda — o numerador da captura e da oportunidade.</param>
/// <param name="Categorias">O nome das categorias que entraram na conta, em ordem alfabética.</param>
/// <param name="UnidadesForaDaConta">As que ficaram de fora: de categoria sem demanda, de linha sem categoria ou sem classificação.</param>
/// <param name="Frase">A conta por extenso, pronta para a tela — a mesma no cartão e na aba.</param>
public sealed record BaseDaCaptura(int Unidades, IReadOnlyList<string> Categorias, int UnidadesForaDaConta, string Frase)
{
    private static readonly System.Globalization.CultureInfo PtBr = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Monta a base; nula quando a fonte das unidades não trouxe venda nenhuma.</summary>
    /// <param name="unidadesVendidas">O total de máquinas vendidas no recorte; nulo quando a fonte não existe.</param>
    /// <param name="vendidasPorCategoria">As máquinas vendidas por categoria, pelo código da categoria.</param>
    /// <param name="categoriasComDemanda">As categorias que têm demanda anual no recorte: código e nome.</param>
    /// <param name="mesesDoPeriodo">Quantos meses o período tem — a demanda do outro lado é proporcional a eles.</param>
    public static BaseDaCaptura? Montar(
        int? unidadesVendidas,
        IEnumerable<(string Codigo, int Unidades)> vendidasPorCategoria,
        IEnumerable<(string Codigo, string Nome)> categoriasComDemanda,
        int mesesDoPeriodo = 12)
    {
        // AUSÊNCIA DE CARGA NÃO É VENDA ZERO: sem a fonte, não há base — e a captura diz que falta a fonte.
        if (unidadesVendidas is not { } total) return null;

        var comDemanda = categoriasComDemanda.DistinctBy(c => c.Codigo, StringComparer.Ordinal).ToList();
        var codigos = comDemanda.Select(c => c.Codigo).ToHashSet(StringComparer.Ordinal);
        var naConta = vendidasPorCategoria.Where(v => codigos.Contains(v.Codigo)).Sum(v => v.Unidades);

        // ORDEM ESTÁVEL: a lista vai para a tela e para o teste, e não pode depender de quem chamou.
        var nomes = comDemanda.Select(c => c.Nome).Order(StringComparer.Ordinal).ToList();

        return new BaseDaCaptura(naConta, nomes, total - naConta, FraseDa(naConta, nomes, total - naConta, total, mesesDoPeriodo));
    }

    private static string FraseDa(int naConta, IReadOnlyList<string> categorias, int fora, int total, int meses)
    {
        if (categorias.Count == 0)
            return string.Format(PtBr,
                "Nenhuma categoria tem demanda estimada neste recorte, e a captura não tem contra o que ler as {0:N0} {1}.",
                total, total == 1 ? "máquina vendida" : "máquinas vendidas");

        var quais = categorias.Count == 1
            ? $"da categoria {categorias[0]}"
            : $"das categorias {string.Join(", ", categorias.Take(categorias.Count - 1))} e {categorias[^1]}";

        // A DEMANDA DO PERÍODO (decisão do Ricardo de 27/09/2026): a anual, proporcional aos meses. Com o ano
        // inteiro a frase é a de sempre — e o número é o da planilha ("Captura FY25").
        var doPeriodo = meses switch
        {
            12 => string.Empty,
            1 => " para o mês do período (a anual × 1/12)",
            _ => string.Format(PtBr, " para os {0} meses do período (a anual × {0}/12)", meses)
        };

        var conta = string.Format(PtBr,
            "A conta deste recorte: {0:N0} {1} {2} ÷ a demanda {3} {4}{5}.",
            naConta, naConta == 1 ? "máquina vendida" : "máquinas vendidas", quais,
            meses == 12 ? "anual estimada" : "estimada",
            categorias.Count == 1 ? "da mesma categoria" : "das mesmas categorias",
            doPeriodo);

        if (fora == 0) return conta;

        return conta + string.Format(PtBr,
            " {0:N0} {1} de fora — de categoria que ainda não tem regra de potencial, de linha sem categoria ou " +
            "sem classificação —, porque não há demanda delas do outro lado da conta.",
            fora, fora == 1 ? "máquina fica" : "máquinas ficam");
    }
}

/// <summary>Os quatro números de decisão de um recorte (documento 50, §4.1).</summary>
/// <param name="DemandaAnual">Quantas máquinas o recorte renova por ano.</param>
/// <param name="MercadoAnual">Quanto isso vale em reais.</param>
/// <param name="CapturaPercentual">Que fatia da demanda a Tracbel leva, em pontos percentuais.</param>
/// <param name="Oportunidade">Quantas máquinas da demanda ajustada ainda não foram capturadas.</param>
/// <param name="BaseDaCaptura">O que a captura e a oportunidade contaram; nula quando a fonte das unidades não existe.</param>
public sealed record NumerosDeDecisao(
    NumeroDeDecisao DemandaAnual,
    MercadoAnual MercadoAnual,
    NumeroDeDecisao CapturaPercentual,
    NumeroDeDecisao Oportunidade,
    BaseDaCaptura? BaseDaCaptura = null);

/// <summary>
/// OS QUATRO NÚMEROS DE DECISÃO (documento 50, §4.1) — domínio puro, sem banco.
///
/// <para><b>Por que isto existe.</b> Os três números que ainda não têm dado — mercado anual, captura e
/// oportunidade — eram <c>valor={null}</c> escrito no TypeScript, e o motivo de cada um era uma constante
/// repetida em <b>três arquivos do front</b>: os quatro cartões do topo, a ficha do município e o bloco
/// Performance. Dezesseis lugares dizendo a mesma coisa, e nenhum deles testável. No dia em que a issue 69
/// trouxer as unidades, alguém teria de achar os dezesseis.</para>
///
/// <para><b>Nenhum número muda com esta classe.</b> Ela não inventa dado: com o banco de hoje os três saem
/// exatamente como saíam, vazios — só que o motivo passa a vir da API, com a mesma regra para a página e
/// para a ficha, e com teste.</para>
///
/// <para><b>Captura, e não market share</b> (issue 162): enquanto o denominador for a demanda ESTIMADA pelo
/// motor, o nome é captura. Share exigiria o total vendido por todos os fabricantes.</para>
/// </summary>
public static class DecisaoDoMercado
{
    /// <summary>
    /// Monta os quatro números.
    /// </summary>
    /// <param name="demandaAnual">A demanda estrutural do recorte, do motor; nula quando ele não a produziu.</param>
    /// <param name="demandaAjustada">A demanda depois do fator de ciclo; nula quando não há fator.</param>
    /// <param name="vendasEmUnidades">
    /// As máquinas que a Tracbel vendeu no recorte (issue 69) NAS CATEGORIAS QUE TÊM DEMANDA — o
    /// <see cref="BaseDaCaptura.Unidades"/>, e não o total do ART; nulo quando a fonte não existe.
    /// </param>
    /// <param name="porCategoria">A demanda e o preço de cada categoria (issue 70). Lista vazia = não há composição.</param>
    /// <param name="mesesDoPeriodo">
    /// Quantos meses o período das vendas tem. A captura e a oportunidade usam a demanda DO PERÍODO — a anual ×
    /// meses/12 (decisão do Ricardo de 27/09/2026); a demanda anual e o mercado anual continuam anuais.
    /// </param>
    public static NumerosDeDecisao Calcular(
        decimal? demandaAnual,
        decimal? demandaAjustada,
        int? vendasEmUnidades,
        IReadOnlyList<DemandaDaCategoria> porCategoria,
        int mesesDoPeriodo = 12)
    {
        var demanda = demandaAnual is { } valor
            ? NumeroDeDecisao.De(valor)
            : NumeroDeDecisao.Sem(MotivoSemNumeroDeDecisao.SemDemandaAnual, "a demanda anual");

        return new NumerosDeDecisao(
            demanda,
            Mercado(demandaAnual, porCategoria),
            Captura(demandaAnual, vendasEmUnidades, mesesDoPeriodo),
            Oportunidade(demandaAjustada, vendasEmUnidades, mesesDoPeriodo));
    }

    /// <summary>
    /// A DEMANDA DO PERÍODO — a anual proporcional aos meses: <c>anual × meses / 12</c>.
    ///
    /// <para><b>Por que proporcional</b> (decisão do Ricardo de 27/09/2026): as vendas de dez meses divididas pela
    /// demanda de doze dariam uma captura cinco sextos do que ela é, e a oportunidade cresceria só por o ano
    /// ainda não ter acabado. Com o ano fiscal inteiro, o fator é 1 e o número é o da planilha ("Captura FY25").
    /// A sazonalidade não entra: a demanda estimada é anual, e ratear por mês de safra seria uma regra que
    /// ninguém decidiu.</para>
    /// </summary>
    /// <param name="anual">A demanda de um ano.</param>
    /// <param name="meses">Os meses do período.</param>
    public static decimal DemandaDoPeriodo(decimal anual, int meses) => anual * meses / 12m;

    /// <summary>
    /// MERCADO ANUAL — a soma, POR CATEGORIA, de <c>demanda da categoria × preço daquela categoria</c>.
    ///
    /// <para><b>Nunca é demanda total × um preço genérico</b> (documento 50, §4.1). O trator de R$ 300 mil
    /// da planilha vale para a categoria dele; aplicá-lo à demanda inteira mistura colhedora com trator
    /// compacto e produz um número que parece preciso e não é.</para>
    ///
    /// <para><b>Categoria sem preço não é somada como zero, e também não derruba o total</b>: o que sai é a
    /// soma das que têm, <b>marcada como parcial</b>, com o nome das que ficaram de fora. Completar em
    /// silêncio seria afirmar que a colheitadeira não vale nada.</para>
    /// </summary>
    private static MercadoAnual Mercado(decimal? demandaAnual, IReadOnlyList<DemandaDaCategoria> porCategoria)
    {
        const string nome = "o mercado anual";

        if (demandaAnual is null)
            return new MercadoAnual(
                null,
                nameof(MotivoSemNumeroDeDecisao.SemDemandaAnual),
                Frase(nameof(MotivoSemNumeroDeDecisao.SemDemandaAnual), nome),
                false,
                []);

        var comPreco = porCategoria.Where(c => c.PrecoDeReferencia is not null).ToList();

        // ORDEM ESTÁVEL no nome das que faltam: a lista vai para a tela e para o teste, e uma ordem que
        // depende de quem chamou faz a mesma leitura mudar de texto sem nada ter mudado.
        var semPreco = porCategoria
            .Where(c => c.PrecoDeReferencia is null)
            .Select(c => c.Categoria)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        if (comPreco.Count == 0)
            return new MercadoAnual(
                null,
                nameof(MotivoSemNumeroDeDecisao.SemPrecoDeMaquina),
                Frase(nameof(MotivoSemNumeroDeDecisao.SemPrecoDeMaquina), nome),
                false,
                semPreco);

        var total = comPreco.Sum(c => c.DemandaAnual * c.PrecoDeReferencia!.Value);

        return new MercadoAnual(
            total, nameof(MotivoSemNumeroDeDecisao.Nenhum), string.Empty, semPreco.Count > 0, semPreco);
    }

    /// <summary>
    /// CAPTURA — vendas da Tracbel em unidades ÷ a demanda DO PERÍODO (<see cref="DemandaDoPeriodo"/>), em pontos
    /// percentuais.
    ///
    /// <para>Os dois lados em MÁQUINAS: o faturamento em reais não serve de numerador para uma demanda
    /// medida em máquinas (issue 69).</para>
    /// </summary>
    private static NumeroDeDecisao Captura(decimal? demandaAnual, int? vendasEmUnidades, int meses)
    {
        const string nome = "a captura";

        if (vendasEmUnidades is null) return NumeroDeDecisao.Sem(MotivoSemNumeroDeDecisao.SemVendasEmUnidades, nome);
        if (demandaAnual is not > 0 || meses <= 0) return NumeroDeDecisao.Sem(MotivoSemNumeroDeDecisao.SemDemandaAnual, nome);

        return NumeroDeDecisao.De(vendasEmUnidades.Value / DemandaDoPeriodo(demandaAnual.Value, meses) * 100m);
    }

    /// <summary>
    /// OPORTUNIDADE — <c>max(0, demanda ajustada do período − vendas)</c>, a regra da issue 162, com a demanda
    /// proporcional aos meses (<see cref="DemandaDoPeriodo"/>, decisão de 27/09/2026).
    ///
    /// <para><b>Nunca abaixo de zero.</b> Vender mais do que a demanda estimada não é oportunidade
    /// negativa: é a estimativa tendo ficado curta, e o número certo ali é zero.</para>
    ///
    /// <para>A base é a demanda <b>ajustada</b> pelo momento do mercado, e não a estrutural — é o que
    /// sobra no mercado de hoje, e não no de um ano médio.</para>
    /// </summary>
    private static NumeroDeDecisao Oportunidade(decimal? demandaAjustada, int? vendasEmUnidades, int meses)
    {
        const string nome = "a oportunidade";

        if (vendasEmUnidades is null) return NumeroDeDecisao.Sem(MotivoSemNumeroDeDecisao.SemVendasEmUnidades, nome);
        if (demandaAjustada is null || meses <= 0) return NumeroDeDecisao.Sem(MotivoSemNumeroDeDecisao.SemDemandaAnual, nome);

        return NumeroDeDecisao.De(Math.Max(0m, DemandaDoPeriodo(demandaAjustada.Value, meses) - vendasEmUnidades.Value));
    }

    /// <summary>
    /// O motivo na língua de quem lê, com a issue que o destrava — a mesma frase para a página e para a
    /// ficha do município, que até aqui tinham duas redações cada.
    /// </summary>
    /// <param name="motivo">O motivo, como texto.</param>
    /// <param name="numero">O nome do número, para a frase dizer de qual se fala.</param>
    public static string Frase(string motivo, string numero) => motivo switch
    {
        nameof(MotivoSemNumeroDeDecisao.SemDemandaAnual) =>
            $"Sem a demanda anual não há base para {numero}. Falta o ciclo de renovação por cultura: a decisão D-P01 " +
            "(issue 63) fixa cultura, categoria, hectares por máquina e anos de renovação — as quatro juntas.",

        // O TEXTO NÃO AFIRMA O ESTADO DO SERVIÇO. Ele dizia "o serviço está desligado para o ajuste dos
        // dados" — e continuou dizendo depois que o serviço voltou e trouxe milhares de vendas. O que a
        // tela sabe é só o que a consulta alcançou.
        nameof(MotivoSemNumeroDeDecisao.SemVendasEmUnidades) =>
            $"As vendas da Tracbel em MÁQUINAS não chegaram a esta consulta, e {numero} precisa delas. O faturamento " +
            "em reais que já existe não serve de numerador para uma demanda medida em máquinas (issue 69). A fonte " +
            "está decidida — é o ART (D-P08, 24/09/2026) —, e nenhuma venda dele está ao alcance deste recorte: a " +
            "carga não rodou, ou não trouxe venda para cá.",

        nameof(MotivoSemNumeroDeDecisao.SemPrecoDeMaquina) =>
            $"Não há preço de referência de máquina no CRM, e {numero} é a demanda de cada categoria multiplicada pelo " +
            "preço DAQUELA categoria (issue 70). Um preço genérico aplicado à demanda inteira misturaria colhedora " +
            "com trator compacto.",

        _ => $"Não foi possível apurar {numero} neste recorte."
    };

    /// <summary>
    /// OS QUATRO NÚMEROS NO MESMO TRECHO DO ANO ANTERIOR — a base do "vs. ano anterior" dos cartões do topo
    /// (decisão de 27/09/2026).
    ///
    /// <para><b>Só a captura e a oportunidade têm ano anterior</b>, porque só elas dependem das vendas do
    /// período. A demanda e o mercado anual são ESTRUTURAIS — área da PAM e regra de hoje —, e são os mesmos
    /// nos dois trechos: uma variação de 0% ali afirmaria uma estabilidade que ninguém mediu. Elas saem sem
    /// número, com o motivo.</para>
    ///
    /// <para><b>A mesma base dos dois lados</b> (D-P01): a captura anterior conta as máquinas das MESMAS
    /// categorias que têm demanda hoje, contra a MESMA demanda — a do período, proporcional aos mesmos meses (o
    /// trecho anterior tem o mesmo tamanho). O que muda entre os dois números é só a venda — e é exatamente o
    /// que a comparação quer medir.</para>
    /// </summary>
    /// <param name="atual">Os quatro números do período pedido.</param>
    /// <param name="demandaAnual">A demanda estrutural do recorte — a mesma dos dois lados.</param>
    /// <param name="demandaAjustada">A demanda ajustada pelo momento — a mesma dos dois lados.</param>
    /// <param name="baseAnterior">
    /// As máquinas do ano anterior nas categorias com demanda; nula quando a fonte das unidades não cobre o
    /// ano anterior inteiro.
    /// </param>
    /// <param name="motivoSemBaseAnterior">Por que a base anterior falta — a frase da cobertura do ART.</param>
    /// <param name="mesesDoPeriodo">Os meses do período — os mesmos do trecho anterior.</param>
    public static ComparacaoComOAnoAnterior CompararComOAnoAnterior(
        NumerosDeDecisao atual,
        decimal? demandaAnual,
        decimal? demandaAjustada,
        BaseDaCaptura? baseAnterior,
        string? motivoSemBaseAnterior,
        int mesesDoPeriodo = 12)
    {
        const string estrutural =
            "é estrutural: sai da área plantada da PAM e das regras de potencial, e não das vendas do período — é a " +
            "mesma nos dois trechos do ano. Compará-la com a PAM do ano anterior pediria rodar o motor sobre outro ano, " +
            "e esta leitura não faz isso.";

        NumeroNoAnoAnterior DoPeriodo(NumeroDeDecisao doAtual, string nome, Func<int, NumeroDeDecisao> calcular)
        {
            if (doAtual.Valor is null)
                return NumeroNoAnoAnterior.Sem(MotivoSemComparacao.SemNumeroNoPeriodo,
                    $"Sem {nome} do período não há variação a calcular — o motivo está no próprio número.");

            if (baseAnterior is null)
                return NumeroNoAnoAnterior.Sem(MotivoSemComparacao.AnoAnteriorSemVendas,
                    motivoSemBaseAnterior ?? $"As vendas em máquinas do ano anterior não chegaram a esta consulta, e {nome} precisa delas.");

            var anterior = calcular(baseAnterior.Unidades);
            return anterior.Valor is { } valor
                ? new NumeroNoAnoAnterior(valor, nameof(MotivoSemComparacao.Nenhum), string.Empty)
                : NumeroNoAnoAnterior.Sem(MotivoSemComparacao.SemNumeroNoPeriodo, anterior.Frase);
        }

        return new ComparacaoComOAnoAnterior(
            NumeroNoAnoAnterior.Sem(MotivoSemComparacao.NumeroEstrutural, $"A demanda anual {estrutural}"),
            atual.MercadoAnual.Valor is null
                ? NumeroNoAnoAnterior.Sem(MotivoSemComparacao.SemNumeroNoPeriodo,
                    "Sem o mercado anual do período não há variação a calcular — o motivo está no próprio número.")
                : NumeroNoAnoAnterior.Sem(MotivoSemComparacao.NumeroEstrutural,
                    $"O mercado anual é a demanda vezes o preço de cada categoria, e a demanda {estrutural}"),
            DoPeriodo(atual.CapturaPercentual, "a captura", unidades => Captura(demandaAnual, unidades, mesesDoPeriodo)),
            DoPeriodo(atual.Oportunidade, "a oportunidade", unidades => Oportunidade(demandaAjustada, unidades, mesesDoPeriodo)),
            baseAnterior);
    }
}

/// <summary>Por que um número de decisão não tem o valor do mesmo trecho do ano anterior.</summary>
public enum MotivoSemComparacao
{
    /// <summary>Tem.</summary>
    Nenhum = 0,

    /// <summary>O número não depende do período: é estrutural, e seria o mesmo nos dois lados.</summary>
    NumeroEstrutural = 1,

    /// <summary>O número do período atual não saiu — sem ele não há variação.</summary>
    SemNumeroNoPeriodo = 2,

    /// <summary>A fonte das unidades não cobre o ano anterior inteiro.</summary>
    AnoAnteriorSemVendas = 3
}

/// <summary>Um número de decisão no mesmo trecho do ano anterior — ou por que ele não existe.</summary>
/// <param name="Valor">O número; nulo quando não há.</param>
/// <param name="Motivo">Um <see cref="MotivoSemComparacao"/> como texto.</param>
/// <param name="Frase">A ausência explicada, pronta para a dica; vazia quando o número saiu.</param>
public sealed record NumeroNoAnoAnterior(decimal? Valor, string Motivo, string Frase)
{
    /// <summary>A ausência, com o motivo e a frase.</summary>
    /// <param name="motivo">Por que não há.</param>
    /// <param name="frase">A frase.</param>
    public static NumeroNoAnoAnterior Sem(MotivoSemComparacao motivo, string frase) => new(null, motivo.ToString(), frase);
}

/// <summary>
/// OS QUATRO NÚMEROS NO MESMO TRECHO DO ANO ANTERIOR — a variação que a maquete põe embaixo de cada um
/// ("↑ +2 p.p. vs. ano anterior (16%)"). A tela calcula a diferença; o servidor diz o número de antes, ou por
/// que ele não existe.
/// </summary>
/// <param name="DemandaAnual">Estrutural — sem ano anterior, com o motivo.</param>
/// <param name="MercadoAnual">Estrutural — sem ano anterior, com o motivo.</param>
/// <param name="CapturaPercentual">A captura do ano anterior, em pontos percentuais, contra a mesma demanda.</param>
/// <param name="Oportunidade">A oportunidade do ano anterior, em máquinas, contra a mesma demanda ajustada.</param>
/// <param name="BaseDaCaptura">O numerador do ano anterior, com a mesma lista de categorias; nulo sem cobertura.</param>
public sealed record ComparacaoComOAnoAnterior(
    NumeroNoAnoAnterior DemandaAnual,
    NumeroNoAnoAnterior MercadoAnual,
    NumeroNoAnoAnterior CapturaPercentual,
    NumeroNoAnoAnterior Oportunidade,
    BaseDaCaptura? BaseDaCaptura);
