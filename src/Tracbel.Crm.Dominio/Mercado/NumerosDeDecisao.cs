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

/// <summary>Os quatro números de decisão de um recorte (documento 50, §4.1).</summary>
/// <param name="DemandaAnual">Quantas máquinas o recorte renova por ano.</param>
/// <param name="MercadoAnual">Quanto isso vale em reais.</param>
/// <param name="CapturaPercentual">Que fatia da demanda a Tracbel leva, em pontos percentuais.</param>
/// <param name="Oportunidade">Quantas máquinas da demanda ajustada ainda não foram capturadas.</param>
public sealed record NumerosDeDecisao(
    NumeroDeDecisao DemandaAnual,
    MercadoAnual MercadoAnual,
    NumeroDeDecisao CapturaPercentual,
    NumeroDeDecisao Oportunidade);

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
    /// <param name="vendasEmUnidades">As máquinas que a Tracbel vendeu no recorte (issue 69); nulo quando a fonte não existe.</param>
    /// <param name="porCategoria">A demanda e o preço de cada categoria (issue 70). Lista vazia = não há composição.</param>
    public static NumerosDeDecisao Calcular(
        decimal? demandaAnual,
        decimal? demandaAjustada,
        int? vendasEmUnidades,
        IReadOnlyList<DemandaDaCategoria> porCategoria)
    {
        var demanda = demandaAnual is { } valor
            ? NumeroDeDecisao.De(valor)
            : NumeroDeDecisao.Sem(MotivoSemNumeroDeDecisao.SemDemandaAnual, "a demanda anual");

        return new NumerosDeDecisao(
            demanda,
            Mercado(demandaAnual, porCategoria),
            Captura(demandaAnual, vendasEmUnidades),
            Oportunidade(demandaAjustada, vendasEmUnidades));
    }

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
    /// CAPTURA — vendas da Tracbel em unidades ÷ demanda anual, em pontos percentuais.
    ///
    /// <para>Os dois lados em MÁQUINAS: o faturamento em reais não serve de numerador para uma demanda
    /// medida em máquinas (issue 69).</para>
    /// </summary>
    private static NumeroDeDecisao Captura(decimal? demandaAnual, int? vendasEmUnidades)
    {
        const string nome = "a captura";

        if (vendasEmUnidades is null) return NumeroDeDecisao.Sem(MotivoSemNumeroDeDecisao.SemVendasEmUnidades, nome);
        if (demandaAnual is not > 0) return NumeroDeDecisao.Sem(MotivoSemNumeroDeDecisao.SemDemandaAnual, nome);

        return NumeroDeDecisao.De(vendasEmUnidades.Value / demandaAnual.Value * 100m);
    }

    /// <summary>
    /// OPORTUNIDADE — <c>max(0, demanda ajustada − vendas)</c>, a regra da issue 162.
    ///
    /// <para><b>Nunca abaixo de zero.</b> Vender mais do que a demanda estimada não é oportunidade
    /// negativa: é a estimativa tendo ficado curta, e o número certo ali é zero.</para>
    ///
    /// <para>A base é a demanda <b>ajustada</b> pelo momento do mercado, e não a estrutural — é o que
    /// sobra no mercado de hoje, e não no de um ano médio.</para>
    /// </summary>
    private static NumeroDeDecisao Oportunidade(decimal? demandaAjustada, int? vendasEmUnidades)
    {
        const string nome = "a oportunidade";

        if (vendasEmUnidades is null) return NumeroDeDecisao.Sem(MotivoSemNumeroDeDecisao.SemVendasEmUnidades, nome);
        if (demandaAjustada is null) return NumeroDeDecisao.Sem(MotivoSemNumeroDeDecisao.SemDemandaAnual, nome);

        return NumeroDeDecisao.De(Math.Max(0m, demandaAjustada.Value - vendasEmUnidades.Value));
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

        nameof(MotivoSemNumeroDeDecisao.SemVendasEmUnidades) =>
            $"As vendas da Tracbel em MÁQUINAS não estão carregadas, e {numero} precisa delas. O faturamento em reais " +
            "que já existe não serve de numerador para uma demanda medida em máquinas (issue 69). A fonte canônica " +
            "ainda não foi escolhida — é a decisão D-P08.",

        nameof(MotivoSemNumeroDeDecisao.SemPrecoDeMaquina) =>
            $"Não há preço de referência de máquina no CRM, e {numero} é a demanda de cada categoria multiplicada pelo " +
            "preço DAQUELA categoria (issue 70). Um preço genérico aplicado à demanda inteira misturaria colhedora " +
            "com trator compacto.",

        _ => $"Não foi possível apurar {numero} neste recorte."
    };
}
