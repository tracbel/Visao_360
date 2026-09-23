using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>Por que um indicador não saiu (issue 73).</summary>
public enum MotivoSemIndicador
{
    /// <summary>Saiu.</summary>
    Nenhum = 0,

    /// <summary>Não há meses suficientes para formar as duas janelas.</summary>
    SerieCurta = 1,

    /// <summary>A janela anterior está zerada — não há do que variar.</summary>
    SemBaseDeComparacao = 2,

    /// <summary>A fonte não publica esta cultura, ou a cultura não declara onde procurar.</summary>
    SemFonte = 3,

    /// <summary>Falta um parâmetro que ninguém decidiu.</summary>
    SemParametro = 4
}

/// <summary>
/// UM ÍNDICE DE MOMENTO, com o rastro das duas janelas que o formaram.
///
/// <para><b>1,00 é "igual à janela anterior".</b> 1,15 é "15% acima". O índice não tem unidade: ele é
/// uma razão entre duas médias da mesma coisa.</para>
/// </summary>
/// <param name="Indice">A razão entre a janela recente e a anterior; nulo com motivo.</param>
/// <param name="Faixa">A faixa de mercado do índice, como texto; nula quando o índice não saiu.</param>
/// <param name="MediaRecente">A média da janela recente.</param>
/// <param name="MediaAnterior">A média da janela anterior.</param>
/// <param name="MesesRecentes">Quantos meses entraram na janela recente.</param>
/// <param name="MesesAnteriores">Quantos meses entraram na anterior.</param>
/// <param name="Motivo">Por que não saiu, como texto; <c>Nenhum</c> quando saiu.</param>
public sealed record IndiceDeMomento(
    decimal? Indice,
    string? Faixa,
    decimal? MediaRecente,
    decimal? MediaAnterior,
    int MesesRecentes,
    int MesesAnteriores,
    string Motivo);

/// <summary>
/// AS DUAS JANELAS DO CRÉDITO — a recente e a anterior, em quantidade de contratos e em valor.
/// </summary>
/// <param name="Contratos">Contratos da janela recente.</param>
/// <param name="Valor">Valor financiado na janela recente.</param>
/// <param name="ContratosAnteriores">Contratos da janela anterior.</param>
/// <param name="ValorAnterior">Valor financiado na janela anterior.</param>
public sealed record JanelasDoCredito(int Contratos, decimal Valor, int ContratosAnteriores, decimal ValorAnterior);

/// <summary>
/// O ÍNDICE DE CRÉDITO DE UM RECORTE, com o aviso de base pequena.
/// </summary>
/// <param name="Indice">O índice composto; nulo com motivo.</param>
/// <param name="Faixa">A faixa de mercado, como texto.</param>
/// <param name="IndiceDeContratos">A parcela da quantidade.</param>
/// <param name="IndiceDeValor">A parcela do valor.</param>
/// <param name="Contratos">Os contratos da janela recente — o tamanho da base.</param>
/// <param name="ContratosAnteriores">Os da janela anterior.</param>
/// <param name="BasePequena">Se a base é pequena demais para o índice ser lido como tendência.</param>
/// <param name="Motivo">Por que não saiu, como texto.</param>
public sealed record IndiceDeCredito(
    decimal? Indice,
    string? Faixa,
    decimal? IndiceDeContratos,
    decimal? IndiceDeValor,
    int Contratos,
    int ContratosAnteriores,
    bool BasePequena,
    string Motivo);

/// <summary>
/// OS INDICADORES QUE ANTECIPAM OU ADIAM A RENOVAÇÃO (issue 73) — momento de preço, crédito e a margem
/// na unidade em que o produtor pensa.
///
/// <para><b>Domínio puro, sem banco.</b> Quem lê o banco entrega séries e contagens; aqui só há conta —
/// que é o que permite conferir cada indicador contra a planilha sem subir aplicação nenhuma.</para>
///
/// <para><b>A janela é 12 contra 12, e inclui o último mês</b> (D-P02, decidida em 21/09/2026): os
/// últimos 12 meses divididos pelos 12 anteriores. O protótipo tem quatro variantes — último ÷ média no
/// café, 1 contra 12 e 6 contra 6 na cana, e na laranja uma janela <b>deslocada um mês</b>, sem o mês
/// mais recente. A deslocada é a que o aceite da issue proíbe: "a janela de 12 meses inclui o último
/// mês", porque descartar o mês mais novo é justamente perder a virada que o indicador existe para ver.</para>
///
/// <para><b>O termo de troca não está aqui</b>, e é decisão registrada (D-P05, 23/09/2026): ele precisa
/// do preço de máquina (#70), que é dado interno e ainda não existe no CRM. As três sensibilidades do
/// fator de ciclo são preço/rentabilidade, crédito e percepção.</para>
/// </summary>
public static class IndicadoresDeMercado
{
    /// <summary>
    /// O MOMENTO DE PREÇO: a média dos meses recentes dividida pela média dos anteriores.
    ///
    /// <para><b>As duas janelas precisam estar cheias.</b> Comparar 12 meses com 7 daria uma variação que
    /// mede o tamanho da janela, e não o preço. Série curta sai vazia com o motivo — e a tela diz quantos
    /// meses faltam, em vez de mostrar um número que parece pronto.</para>
    ///
    /// <para><b>Média simples, e não ponderada por volume.</b> O que se pergunta é "o preço está melhor
    /// do que estava?", e não "quanto o produtor faturou": ponderar por safra misturaria a resposta com
    /// o tamanho da colheita.</para>
    ///
    /// <para><b>Base zero não vira índice.</b> Dividir por zero não é "subiu infinito": é "não havia do
    /// que variar".</para>
    /// </summary>
    /// <param name="recentes">Os preços da janela recente, um por mês.</param>
    /// <param name="anteriores">Os preços da janela anterior, um por mês.</param>
    /// <param name="mesesPorJanela">Quantos meses cada janela precisa ter cheios.</param>
    /// <param name="parametro">Os parâmetros vigentes, que dão as faixas; nulo devolve o índice sem faixa.</param>
    public static IndiceDeMomento MomentoDePreco(
        IReadOnlyList<decimal> recentes,
        IReadOnlyList<decimal> anteriores,
        short mesesPorJanela,
        ParametroDoPotencial? parametro)
    {
        if (recentes.Count < mesesPorJanela || anteriores.Count < mesesPorJanela)
            return new IndiceDeMomento(
                null, null,
                recentes.Count > 0 ? recentes.Average() : null,
                anteriores.Count > 0 ? anteriores.Average() : null,
                recentes.Count, anteriores.Count,
                nameof(MotivoSemIndicador.SerieCurta));

        var media = recentes.Average();
        var mediaAnterior = anteriores.Average();

        if (mediaAnterior <= 0)
            return new IndiceDeMomento(
                null, null, media, mediaAnterior, recentes.Count, anteriores.Count,
                nameof(MotivoSemIndicador.SemBaseDeComparacao));

        var indice = media / mediaAnterior;

        return new IndiceDeMomento(
            indice,
            parametro?.FaixaDe(indice).ToString(),
            media,
            mediaAnterior,
            recentes.Count,
            anteriores.Count,
            nameof(MotivoSemIndicador.Nenhum));
    }

    /// <summary>
    /// O ÍNDICE DE CRÉDITO: a quantidade de contratos e o valor financiado, compostos pelo peso vigente.
    ///
    /// <para><b>Por que dois componentes, e não só o valor.</b> O valor sobe com a inflação e com o preço
    /// da máquina, sem nenhum produtor a mais ter financiado; a quantidade de contratos conta pessoas. O
    /// peso decidido (D-P03) é 70% contratos e 30% valor — "quantos estão comprando" pesa mais que
    /// "quanto está sendo gasto".</para>
    ///
    /// <para><b>Município com poucos contratos não gera índice extremo</b>, que é o aceite da issue. Aqui
    /// isso é resolvido <b>dizendo</b>, e não corrigindo em silêncio: abaixo do mínimo, o índice sai com
    /// <see cref="IndiceDeCredito.BasePequena"/> ligado e a tela o mostra como base pequena, com a
    /// contagem ao lado. De 2 contratos para 4 é "+100%", e nenhuma suavização faz esse número virar
    /// informação — o que ele precisa é de contexto, não de maquiagem.</para>
    ///
    /// <para><b>O mínimo é parâmetro, e ele ainda não foi decidido</b> (D-P03): sem ele, nada é marcado,
    /// e a contagem de contratos continua visível para quem lê julgar. Inventar um limiar aqui seria
    /// escolher, no código, o que conta como "poucos".</para>
    /// </summary>
    /// <param name="janelas">As duas janelas, em contratos e em valor.</param>
    /// <param name="pesoDosContratos">O peso da quantidade, de 0 a 1; o valor pesa o resto.</param>
    /// <param name="contratosMinimos">Abaixo disto, a base é pequena; nulo não marca nada.</param>
    /// <param name="parametro">Os parâmetros vigentes, que dão as faixas.</param>
    public static IndiceDeCredito Credito(
        JanelasDoCredito janelas,
        decimal pesoDosContratos,
        int? contratosMinimos,
        ParametroDoPotencial? parametro)
    {
        var basePequena = contratosMinimos is { } minimo && janelas.Contratos < minimo;

        if (janelas.ContratosAnteriores <= 0 || janelas.ValorAnterior <= 0)
            return new IndiceDeCredito(
                null, null, null, null, janelas.Contratos, janelas.ContratosAnteriores, basePequena,
                nameof(MotivoSemIndicador.SemBaseDeComparacao));

        var deContratos = (decimal)janelas.Contratos / janelas.ContratosAnteriores;
        var deValor = janelas.Valor / janelas.ValorAnterior;

        var indice = (pesoDosContratos * deContratos) + ((1m - pesoDosContratos) * deValor);

        return new IndiceDeCredito(
            indice,
            parametro?.FaixaDe(indice).ToString(),
            deContratos,
            deValor,
            janelas.Contratos,
            janelas.ContratosAnteriores,
            basePequena,
            nameof(MotivoSemIndicador.Nenhum));
    }

    /// <summary>
    /// A MARGEM NA UNIDADE EM QUE O PRODUTOR PENSA — por saca, por caixa, por tonelada.
    ///
    /// <para><b>Por que ela não é a margem por hectare dividida por nada.</b> A margem por hectare
    /// responde "a terra paga a conta?"; esta responde "cada saca que eu vendo sobra quanto?". A ponte
    /// entre as duas é a produtividade: <c>margem/ha ÷ (kg/ha ÷ kg por unidade)</c>, que é
    /// <c>margem/ha × kg por unidade ÷ kg/ha</c>.</para>
    ///
    /// <para><b>Produtividade zero não vira margem infinita</b>: sem colheita não há unidade vendida, e
    /// a conta não existe.</para>
    /// </summary>
    /// <param name="margemPorHectare">A margem por hectare (issue 159).</param>
    /// <param name="produtividadeKgPorHa">Quilos por hectare colhido.</param>
    /// <param name="quilosPorUnidade">Quantos quilos tem a unidade comercial da cultura (catálogo, issue 165).</param>
    public static decimal? MargemPorUnidade(
        decimal? margemPorHectare, decimal? produtividadeKgPorHa, decimal quilosPorUnidade) =>
        margemPorHectare is { } margem && produtividadeKgPorHa is > 0 && quilosPorUnidade > 0
            ? margem * quilosPorUnidade / produtividadeKgPorHa.Value
            : null;

    /// <summary>
    /// A FRASE QUE A TELA MOSTRA NO LUGAR DO ÍNDICE.
    ///
    /// <para>Vazia quando o índice saiu e a base não é pequena — a tela não põe rodapé para dizer que
    /// está tudo bem.</para>
    /// </summary>
    /// <param name="motivo">O motivo, como o indicador o devolveu.</param>
    /// <param name="mesesFaltando">Quantos meses faltam para fechar as janelas, quando a série é curta.</param>
    /// <param name="contratos">Os contratos da janela recente, quando a base é pequena.</param>
    public static string Frase(string motivo, int mesesFaltando = 0, int? contratos = null) =>
        motivo switch
        {
            nameof(MotivoSemIndicador.SerieCurta) when mesesFaltando > 0 =>
                $"faltam {mesesFaltando} {(mesesFaltando == 1 ? "mês" : "meses")} de preço para fechar as duas janelas",
            nameof(MotivoSemIndicador.SerieCurta) =>
                "a série de preço ainda não tem as duas janelas cheias",
            nameof(MotivoSemIndicador.SemBaseDeComparacao) =>
                "a janela anterior está zerada — não há do que variar",
            nameof(MotivoSemIndicador.SemFonte) =>
                "nenhuma fonte publica esta cultura, ou ela não declara onde procurar o preço",
            nameof(MotivoSemIndicador.SemParametro) =>
                "falta um parâmetro que ainda não foi decidido",
            _ when contratos is { } quantos =>
                $"base pequena: {quantos} {(quantos == 1 ? "contrato" : "contratos")} na janela — leia a variação com cuidado",
            _ => string.Empty
        };
}
