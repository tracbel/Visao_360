using System.Globalization;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>Por que o fator de ciclo não saiu (issue 74).</summary>
public enum MotivoSemFator
{
    /// <summary>Saiu.</summary>
    Nenhum = 0,

    /// <summary>Não há parâmetro vigente na data.</summary>
    SemParametroVigente = 1,

    /// <summary>Os pesos dos indicadores não foram registrados (D-P05).</summary>
    SemPesos = 2
}

/// <summary>
/// O FATOR DE CICLO DE MERCADO, com a parcela de cada sensibilidade.
///
/// <para><b>1,00 é neutro</b>: o mercado não antecipa nem adia nada, e a demanda ajustada é igual à
/// estrutural. Acima de 1, o momento puxa a renovação para frente; abaixo, empurra para trás.</para>
/// </summary>
/// <param name="Fator">O fator, já dentro dos limites; nulo com motivo.</param>
/// <param name="ParcelaDePreco">Quanto o preço e a rentabilidade contribuíram, em fração.</param>
/// <param name="ParcelaDaPercepcao">Quanto a percepção do gestor contribuiu.</param>
/// <param name="ParcelaDeCredito">Quanto o crédito contribuiu.</param>
/// <param name="FatorSemLimite">O fator antes do corte — para a tela poder dizer que houve corte.</param>
/// <param name="CortadoPeloLimite">Se o limite mínimo ou máximo mudou o número.</param>
/// <param name="IndicadoresUsados">Quantas das três sensibilidades tinham dado; zero é fator neutro.</param>
/// <param name="Estimativa">Se algum parâmetro usado ainda está marcado como a confirmar.</param>
/// <param name="Motivo">Por que não saiu, como texto; <c>Nenhum</c> quando saiu.</param>
public sealed record FatorDoCiclo(
    decimal? Fator,
    decimal? ParcelaDePreco,
    decimal? ParcelaDaPercepcao,
    decimal? ParcelaDeCredito,
    decimal? FatorSemLimite,
    bool CortadoPeloLimite,
    int IndicadoresUsados,
    bool Estimativa,
    string Motivo);

/// <summary>
/// UM CENÁRIO — o mesmo modelo com as sensibilidades na borda da faixa em que estão.
/// </summary>
/// <param name="Nome">Conservador, Moderado ou Otimista.</param>
/// <param name="Fator">O fator do cenário.</param>
/// <param name="DemandaAjustada">Demanda estrutural × fator.</param>
/// <param name="VariacaoPercentual">Quanto a ajustada difere da estrutural, em pontos percentuais.</param>
public sealed record CenarioDoPotencial(
    string Nome, decimal? Fator, decimal? DemandaAjustada, decimal? VariacaoPercentual);

/// <summary>
/// A DEMANDA ESTRUTURAL, A AJUSTADA E OS TRÊS CENÁRIOS, lado a lado.
/// </summary>
/// <param name="DemandaEstrutural">O que a área comporta por ano (issue 72), sem ciclo de mercado.</param>
/// <param name="Fator">O fator aplicado, com o detalhe de cada parcela.</param>
/// <param name="DemandaAjustada">Estrutural × fator; nula quando o fator não saiu.</param>
/// <param name="VariacaoPercentual">Quanto a ajustada difere da estrutural.</param>
/// <param name="Cenarios">Conservador, moderado e otimista, nesta ordem.</param>
/// <param name="Frase">O que a tela mostra ao lado do número, ou no lugar dele.</param>
public sealed record PotencialAjustado(
    decimal? DemandaEstrutural,
    FatorDoCiclo Fator,
    decimal? DemandaAjustada,
    decimal? VariacaoPercentual,
    IReadOnlyList<CenarioDoPotencial> Cenarios,
    string Frase);

/// <summary>
/// O FATOR DE CICLO DE MERCADO E OS TRÊS CENÁRIOS (issue 74) — o que antecipa ou adia a renovação.
///
/// <para><b>A demanda estrutural diz o que a área comporta</b> (issue 72); o fator diz se o momento do
/// mercado puxa essa renovação para frente ou empurra para trás. <b>Demanda ajustada = estrutural ×
/// fator</b>.</para>
///
/// <para><b>A forma é a do protótipo:</b>
/// <c>fator = limite[(1 + a·z_preço + d·z_percepção) × (1 + b·z_crédito)]</c>. Preço e percepção somam
/// dentro do mesmo parêntese porque são a leitura do <b>produtor</b> — quanto a lavoura rende e o que o
/// gestor vê na rua; o crédito multiplica porque é a <b>condição de financiar</b>, que age sobre o
/// conjunto: sem crédito, nem a melhor safra vira máquina.</para>
///
/// <para><b>O termo de troca não entra</b> (D-P05, 23/09/2026): ele precisa do preço de máquina (#70),
/// que é dado interno e não existe no CRM. No lugar dele, a sensibilidade de <b>preço e
/// rentabilidade</b>, que é o que o pedido descreve.</para>
///
/// <para><b>Sem índice nenhum, o fator é 1</b> — o critério de aceite da issue. Indicador ausente vale
/// <b>zero desvio</b>, e não "fator indeterminado": quem não tem preço carregado não deve ver a demanda
/// sumir, deve ver a demanda estrutural.</para>
///
/// <para><b>Sem os pesos, não há fator.</b> Peso é decisão (D-P05), não conta: sem ele o resultado sai
/// vazio com o motivo, e a tela mostra a demanda estrutural sozinha.</para>
/// </summary>
public static class FatorDeCiclo
{
    /// <summary>
    /// A CULTURA DAS FRASES É A DO LEITOR, E NÃO A DO SERVIDOR.
    ///
    /// <para>Uma frase em português que diz "o fator foi 3.60" está errada, e foi o CI que pegou: o
    /// runner roda em cultura invariante e a estação em pt-BR, então o mesmo código escrevia números
    /// diferentes nos dois. Quem lê a frase é o comercial brasileiro — a vírgula não é preferência do
    /// processo, é parte do texto.</para>
    /// </summary>
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Os nomes dos três cenários, na ordem em que a tela os mostra.</summary>
    public const string Conservador = "Conservador";

    /// <summary>O cenário do fator calculado.</summary>
    public const string Moderado = "Moderado";

    /// <summary>O cenário otimista.</summary>
    public const string Otimista = "Otimista";

    /// <summary>
    /// O FATOR DE CICLO de um recorte.
    ///
    /// <para><b>Os índices chegam em torno de 1</b> (1,20 é "20% acima da janela anterior"), e o desvio
    /// que entra na conta é <c>índice − 1</c>. O protótipo usa <c>(índice − 100)/100</c> porque lá o
    /// índice é base 100 — é a mesma fração.</para>
    ///
    /// <para><b>A percepção entra como fração do percentual</b> (<c>percepção ÷ 100</c>): +5 significa
    /// "5% a mais". Com o peso da percepção em 1,0, o rótulo "−5% a +5%" (D-P04) é literal; com 0,4, o
    /// mesmo +5 moveria o fator em 2%. O peso é parâmetro com vigência — quem decide é o Administrador,
    /// não este arquivo.</para>
    ///
    /// <para><b>O efeito da percepção é amplificado pelo crédito</b>, e isso é consequência da forma do
    /// protótipo, não um descuido: ela mora dentro do parêntese do produtor, que o termo de crédito
    /// multiplica. Com crédito neutro, +5% move o fator em exatamente 5%; com o crédito 20% acima, move
    /// 5,5%. É defensável — o otimismo do gestor vale mais quando há crédito para financiá-lo —, mas
    /// quem lê o número precisa saber que o "±5%" é exato só no crédito neutro.</para>
    /// </summary>
    /// <param name="indiceDePreco">O momento de preço, em torno de 1; nulo é desvio zero.</param>
    /// <param name="indiceDeCredito">O índice de crédito, em torno de 1; nulo é desvio zero.</param>
    /// <param name="percepcaoPercentual">A percepção do gestor, em pontos percentuais; nula é zero.</param>
    /// <param name="parametro">Os parâmetros vigentes na data.</param>
    /// <param name="estimativa">Se o que alimenta o fator ainda é regra a confirmar.</param>
    public static FatorDoCiclo Fator(
        decimal? indiceDePreco,
        decimal? indiceDeCredito,
        decimal? percepcaoPercentual,
        ParametroDoPotencial? parametro,
        bool estimativa = false)
    {
        var usados = (indiceDePreco is not null ? 1 : 0)
                     + (indiceDeCredito is not null ? 1 : 0)
                     + (percepcaoPercentual is not null ? 1 : 0);

        if (parametro is null)
            return new FatorDoCiclo(null, null, null, null, null, false, usados, estimativa,
                nameof(MotivoSemFator.SemParametroVigente));

        // PESO É DECISÃO, NÃO CONTA. Sem os três registrados (D-P05), não há fator — e a tela mostra a
        // demanda estrutural sozinha, em vez de uma ajustada por pesos que ninguém escolheu.
        if (parametro.PesoDoIndicadorDePreco is not { } pesoDoPreco
            || parametro.PesoDoIndicadorComercial is not { } pesoDaPercepcao
            || parametro.PesoDoIndicadorDeCredito is not { } pesoDoCredito)
            return new FatorDoCiclo(null, null, null, null, null, false, usados, estimativa,
                nameof(MotivoSemFator.SemPesos));

        // INDICADOR AUSENTE É DESVIO ZERO, e não fator indeterminado: quem não tem preço carregado deve
        // ver a demanda estrutural, não a demanda sumindo. É o aceite "sem índices, o fator é 1".
        var zPreco = (indiceDePreco ?? 1m) - 1m;
        var zCredito = (indiceDeCredito ?? 1m) - 1m;
        var zPercepcao = (percepcaoPercentual ?? 0m) / 100m;

        var doProdutor = pesoDoPreco * zPreco;
        var doGestor = pesoDaPercepcao * zPercepcao;
        var doCredito = pesoDoCredito * zCredito;

        var semLimite = (1m + doProdutor + doGestor) * (1m + doCredito);

        var fator = semLimite;
        var cortado = false;

        // OS LIMITES ANDAM JUNTOS (a entidade não aceita um só). Sem eles, o fator sai sem corte — e a
        // frase diz isso, em vez de o número aparecer como se tivesse sido contido.
        if (parametro.FatorMinimo is { } minimo && parametro.FatorMaximo is { } maximo)
        {
            fator = Math.Clamp(semLimite, minimo, maximo);
            cortado = fator != semLimite;
        }

        return new FatorDoCiclo(
            fator, doProdutor, doGestor, doCredito, semLimite, cortado, usados, estimativa,
            nameof(MotivoSemFator.Nenhum));
    }

    /// <summary>
    /// A DEMANDA AJUSTADA E OS TRÊS CENÁRIOS.
    ///
    /// <para><b>O moderado é o fator calculado</b>; o conservador e o otimista são o mesmo modelo com
    /// <b>cada</b> índice levado à <b>borda da faixa em que ele já está</b> (D-P05). Um índice de preço
    /// de 1,28 está na faixa "aquecido" (de 1,20 a 1,40) e um de crédito de 1,00 está na intermediária
    /// (de 1,00 a 1,20): o conservador lê 1,20 e 1,00, o otimista lê 1,40 e 1,20. Os dois se movem — não
    /// é uma variação inventada, é o intervalo que a própria classificação já usa.</para>
    ///
    /// <para><b>As bordas abertas ficam onde estão.</b> A faixa de retração não tem piso e a de
    /// superaquecimento não tem teto; empurrar o índice para zero ou para o infinito seria inventar um
    /// cenário que a classificação não descreve. Nesses casos o cenário coincide com o moderado.</para>
    ///
    /// <para><b>A ordem é garantida por construção:</b> conservador é o menor entre a borda de baixo e o
    /// moderado, otimista é o maior entre a de cima e o moderado. Não é maquiagem — "conservador" quer
    /// dizer "o pessimista dos dois", e é essa a definição.</para>
    ///
    /// <para><b>A percepção do gestor não varia entre cenários</b>, de propósito: ela não é uma faixa de
    /// mercado, é a opinião de uma pessoa sobre aquele município. Um cenário que a mexe estaria simulando
    /// o gestor mudando de ideia, e não o mercado mudando.</para>
    /// </summary>
    /// <param name="demandaEstrutural">A demanda anual do motor (issue 72).</param>
    /// <param name="indiceDePreco">O momento de preço.</param>
    /// <param name="indiceDeCredito">O índice de crédito.</param>
    /// <param name="percepcaoPercentual">A percepção do gestor, em pontos percentuais.</param>
    /// <param name="parametro">Os parâmetros vigentes na data.</param>
    /// <param name="estimativa">Se o que alimenta o fator ainda é regra a confirmar.</param>
    public static PotencialAjustado Ajustar(
        decimal? demandaEstrutural,
        decimal? indiceDePreco,
        decimal? indiceDeCredito,
        decimal? percepcaoPercentual,
        ParametroDoPotencial? parametro,
        bool estimativa = false)
    {
        var fator = Fator(indiceDePreco, indiceDeCredito, percepcaoPercentual, parametro, estimativa);

        decimal? ajustada = demandaEstrutural is { } estrutural && fator.Fator is { } f ? estrutural * f : null;

        decimal? variacao = demandaEstrutural is > 0 && ajustada is { } a
            ? (a - demandaEstrutural.Value) / demandaEstrutural.Value * 100m
            : null;

        var cenarios = Cenarios(demandaEstrutural, indiceDePreco, indiceDeCredito, percepcaoPercentual, parametro, fator, estimativa);

        return new PotencialAjustado(demandaEstrutural, fator, ajustada, variacao, cenarios, Frase(fator));
    }

    /// <summary>
    /// A FRASE QUE A TELA MOSTRA — vazia quando o fator saiu inteiro e nada precisa de ressalva.
    /// </summary>
    /// <param name="fator">O fator calculado.</param>
    public static string Frase(FatorDoCiclo fator)
    {
        var partes = new List<string>();

        if (fator.Motivo == nameof(MotivoSemFator.SemParametroVigente))
            return "não há parâmetro do potencial vigente nesta data — a demanda sai sem ajuste de mercado";

        if (fator.Motivo == nameof(MotivoSemFator.SemPesos))
            return "os pesos dos três indicadores ainda não foram decididos (D-P05) — a demanda sai sem ajuste de mercado";

        if (fator.Estimativa)
            partes.Add("estimativa: os pesos do fator são os medidos no protótipo, ainda a confirmar");

        if (fator.IndicadoresUsados == 0)
            partes.Add("nenhum indicador de mercado tem dado aqui — o fator é neutro, e a demanda é a estrutural");
        else if (fator.IndicadoresUsados < 3)
            partes.Add($"{fator.IndicadoresUsados} de 3 indicadores com dado — os ausentes entram como neutros");

        if (fator.CortadoPeloLimite && fator.FatorSemLimite is { } sem && fator.Fator is { } com)
            partes.Add(string.Format(
                PtBr, "o fator calculado foi {0:0.00} e o limite o trouxe para {1:0.00}", sem, com));

        return string.Join("; ", partes);
    }

    /// <summary>
    /// Os três cenários, do conservador ao otimista.
    /// </summary>
    private static List<CenarioDoPotencial> Cenarios(
        decimal? demandaEstrutural,
        decimal? indiceDePreco,
        decimal? indiceDeCredito,
        decimal? percepcaoPercentual,
        ParametroDoPotencial? parametro,
        FatorDoCiclo moderado,
        bool estimativa)
    {
        CenarioDoPotencial Montar(string nome, decimal? valor) =>
            new(nome, valor,
                demandaEstrutural is { } e && valor is { } f ? e * f : null,
                demandaEstrutural is > 0 && valor is { } v
                    ? (demandaEstrutural.Value * v - demandaEstrutural.Value) / demandaEstrutural.Value * 100m
                    : null);

        if (parametro is null || moderado.Fator is not { } doModerado)
            return [Montar(Conservador, null), Montar(Moderado, null), Montar(Otimista, null)];

        var piso = Fator(
            Borda(indiceDePreco, parametro, paraBaixo: true),
            Borda(indiceDeCredito, parametro, paraBaixo: true),
            percepcaoPercentual, parametro, estimativa).Fator;

        var teto = Fator(
            Borda(indiceDePreco, parametro, paraBaixo: false),
            Borda(indiceDeCredito, parametro, paraBaixo: false),
            percepcaoPercentual, parametro, estimativa).Fator;

        return
        [
            Montar(Conservador, piso is { } p ? Math.Min(p, doModerado) : doModerado),
            Montar(Moderado, doModerado),
            Montar(Otimista, teto is { } t ? Math.Max(t, doModerado) : doModerado)
        ];
    }

    /// <summary>
    /// A borda da faixa em que o índice já está. Bordas abertas — abaixo da retração e acima do
    /// superaquecimento — devolvem o próprio índice: empurrá-lo para zero ou para o infinito seria
    /// inventar um cenário que a classificação não descreve.
    /// </summary>
    private static decimal? Borda(decimal? indice, ParametroDoPotencial parametro, bool paraBaixo) =>
        indice is not { } i
            ? (decimal?)null
            : parametro.FaixaDe(i) switch
            {
                FaixaDeMercado.Retraido => paraBaixo ? i : parametro.LimiteDeRetracao,
                FaixaDeMercado.Intermediaria => paraBaixo ? parametro.LimiteDeRetracao : parametro.LimiteDeAquecimento,
                FaixaDeMercado.Aquecido => paraBaixo ? parametro.LimiteDeAquecimento : parametro.LimiteDeSuperaquecimento,
                _ => paraBaixo ? parametro.LimiteDeSuperaquecimento : i
            };
}
