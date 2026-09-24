using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>Por que o preço implícito de um ano não saiu.</summary>
public enum MotivoSemPrecoImplicito
{
    /// <summary>Saiu.</summary>
    Nenhum = 0,

    /// <summary>O IBGE não divulgou o valor da produção daquele ano — sigilo ou ausência.</summary>
    SemValorDaProducao = 1,

    /// <summary>O IBGE não divulgou a quantidade produzida.</summary>
    SemQuantidadeProduzida = 2,

    /// <summary>A quantidade é zero: não houve colheita, e dividir por ela não dá preço nenhum.</summary>
    SemColheita = 3
}

/// <summary>O preço implícito de um produto num município e num ano.</summary>
/// <param name="Ano">O ano da safra.</param>
/// <param name="PrecoPorUnidade">Reais por unidade da quantidade; nulo com motivo.</param>
/// <param name="Unidade">A unidade do denominador — "tonelada", "mil frutos", "mil cachos".</param>
/// <param name="ValorDaProducaoMilReais">O numerador, como o IBGE publica.</param>
/// <param name="QuantidadeProduzida">O denominador, na unidade acima.</param>
/// <param name="Motivo">Um <see cref="MotivoSemPrecoImplicito"/> como texto.</param>
public sealed record PrecoImplicitoNoAno(
    short Ano,
    decimal? PrecoPorUnidade,
    string Unidade,
    decimal? ValorDaProducaoMilReais,
    decimal? QuantidadeProduzida,
    string Motivo);

/// <summary>Uma média plurianual da série, ou o motivo de ela não fechar.</summary>
/// <param name="Anos">Quantos anos ela deveria cobrir — 3 ou 5.</param>
/// <param name="Preco">A média; nula quando falta algum ano.</param>
/// <param name="AnosFaltando">Os anos que faltaram, para a tela dizer quais.</param>
public sealed record MediaPlurianual(int Anos, decimal? Preco, IReadOnlyList<short> AnosFaltando);

/// <summary>
/// O PREÇO IMPLÍCITO DA PAM (issue 198) — domínio puro, sem banco.
///
/// <para><b>O dado já está no banco e ninguém o estava lendo.</b> O IBGE define o valor da produção da PAM
/// como <i>"a média ponderada das informações de quantidade e preço médio corrente pago ao produtor"</i>.
/// Logo <c>valor × 1000 ÷ quantidade</c> devolve o <b>preço médio recebido pelo produtor naquele ano</b>,
/// por município — e a issue 156 guarda as duas colunas desde 2010.</para>
///
/// <para><b>Por que isso importa</b> [M 22/09/2026]: nenhuma fonte aberta publica série mensal longa de
/// preço recebido em São Paulo. A CONAB é janela de 12 meses e está carregada desde 09/2025 — o índice de
/// momento, que é 12 meses contra os 12 anteriores, só fecha em <b>09/2027</b>. Esta série derruba essa
/// parede e é a única com granularidade <b>municipal</b>: a CONAB só publica por UF.</para>
///
/// <para><b>NÃO SE EMENDA À SÉRIE DA CONAB.</b> São conceitos da mesma família e não intercambiáveis:
/// comparando 2025 em SP, a distância vai de <b>+1,1% na soja a −28,4% no amendoim</b>. As duas aparecem
/// lado a lado, cada uma rotulada. Emendá-las produziria uma série com degrau artificial na virada.</para>
///
/// <para><b>Nada aqui é gravado.</b> O preço é derivado de duas colunas que já existem; guardá-lo criaria
/// uma terceira verdade para o mesmo número — a mesma recusa que o modelo já fez com o rendimento médio.</para>
///
/// <para><b>Valor nominal, do ano, sem deflator.</b> Quem compara 2010 com 2024 precisa saber disso, e a
/// tela diz. Deflacionar exigiria escolher um índice, e essa escolha não foi feita por ninguém.</para>
/// </summary>
public static class PrecoImplicitoDaPam
{
    /// <summary>Mil reais para reais — é assim que o IBGE publica o valor da produção.</summary>
    private const decimal ReaisPorMilReais = 1_000m;

    /// <summary>
    /// O preço implícito de um ano.
    /// </summary>
    /// <param name="produtoCodigoIbge">O produto na classificação 782 — decide a unidade.</param>
    /// <param name="ano">O ano da safra — decide a unidade junto com o produto.</param>
    /// <param name="valorDaProducaoMilReais">Variável 215, em mil reais.</param>
    /// <param name="quantidadeProduzida">Variável 214, na unidade do produto naquele ano.</param>
    public static PrecoImplicitoNoAno De(
        int produtoCodigoIbge, short ano, decimal? valorDaProducaoMilReais, decimal? quantidadeProduzida)
    {
        // A UNIDADE VEM DO PRODUTO E DO ANO (issue 152), e não é tonelada em todos: abacaxi e coco são mil
        // frutos, e as frutas da nota 2 só passam a tonelada em 2001. Um preço "por tonelada" nesses casos
        // seria mentira — e é por isso que ela sai junto do número, nunca escrita na tela.
        var unidade = UnidadesDaPam.DaQuantidade(produtoCodigoIbge, ano).Nome;

        // AUSÊNCIA NÃO É ZERO, e os três motivos são diferentes: o IBGE não divulgar o valor, não divulgar a
        // quantidade, e a quantidade ser ZERO são três fatos distintos. O terceiro é medição — não houve
        // colheita —, e mesmo assim não produz preço: dividir por zero não dá "preço zero", dá nada.
        if (valorDaProducaoMilReais is null)
            return Sem(ano, unidade, valorDaProducaoMilReais, quantidadeProduzida, MotivoSemPrecoImplicito.SemValorDaProducao);

        if (quantidadeProduzida is null)
            return Sem(ano, unidade, valorDaProducaoMilReais, quantidadeProduzida, MotivoSemPrecoImplicito.SemQuantidadeProduzida);

        if (quantidadeProduzida.Value == 0m)
            return Sem(ano, unidade, valorDaProducaoMilReais, quantidadeProduzida, MotivoSemPrecoImplicito.SemColheita);

        return new PrecoImplicitoNoAno(
            ano,
            valorDaProducaoMilReais.Value * ReaisPorMilReais / quantidadeProduzida.Value,
            unidade,
            valorDaProducaoMilReais,
            quantidadeProduzida,
            nameof(MotivoSemPrecoImplicito.Nenhum));
    }

    /// <summary>
    /// A MÉDIA DE N ANOS — só quando TODOS os N anos têm preço.
    ///
    /// <para><b>Média de série furada é a armadilha desta issue.</b> Se 2022 não tem preço e a média de três
    /// anos for feita com 2023 e 2024, o número sai e ninguém percebe que ele é de dois anos: uma média de
    /// dois anos apresentada como de três é mais enganosa que ausência nenhuma, porque parece completa.
    /// Aqui, faltando um ano a média não sai — e a tela diz <b>quais</b> anos faltaram.</para>
    ///
    /// <para>A janela é a dos N anos mais recentes DA SÉRIE PEDIDA, terminando no último ano informado.</para>
    /// </summary>
    /// <param name="serie">Os anos, em qualquer ordem.</param>
    /// <param name="anos">O tamanho da janela — 3 ou 5.</param>
    public static MediaPlurianual Media(IReadOnlyList<PrecoImplicitoNoAno> serie, int anos)
    {
        if (anos <= 0) throw new ArgumentOutOfRangeException(nameof(anos), "A janela da média é de um ano ou mais.");
        if (serie.Count == 0) return new MediaPlurianual(anos, null, []);

        var ultimo = serie.Max(p => p.Ano);
        var janela = Enumerable.Range(0, anos).Select(i => (short)(ultimo - i)).ToList();

        var porAno = serie
            .Where(p => p.PrecoPorUnidade is not null)
            .ToDictionary(p => p.Ano, p => p.PrecoPorUnidade!.Value);

        var faltando = janela.Where(a => !porAno.ContainsKey(a)).OrderBy(a => a).ToList();

        return faltando.Count > 0
            ? new MediaPlurianual(anos, null, faltando)
            : new MediaPlurianual(anos, janela.Sum(a => porAno[a]) / anos, []);
    }

    /// <summary>Por que o ano não tem preço, na língua de quem lê.</summary>
    /// <param name="motivo">O motivo, como texto.</param>
    public static string Frase(string motivo) => motivo switch
    {
        nameof(MotivoSemPrecoImplicito.SemValorDaProducao) =>
            "O IBGE não divulgou o valor da produção deste ano — pode ser sigilo, quando poucos produtores o " +
            "compõem. Sigilo não é zero, e sem o numerador não há preço.",

        nameof(MotivoSemPrecoImplicito.SemQuantidadeProduzida) =>
            "O IBGE não divulgou a quantidade produzida deste ano, e ela é o denominador do preço.",

        nameof(MotivoSemPrecoImplicito.SemColheita) =>
            "A quantidade produzida é zero neste ano: não houve colheita. Isso é medição, e mesmo assim não " +
            "produz preço — dividir por zero não dá \"preço zero\", dá nada.",

        _ => "Não foi possível apurar o preço implícito deste ano."
    };

    private static PrecoImplicitoNoAno Sem(
        short ano, string unidade, decimal? valor, decimal? quantidade, MotivoSemPrecoImplicito motivo) =>
        new(ano, null, unidade, valor, quantidade, motivo.ToString());
}
