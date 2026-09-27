using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Dominio.Integracao;

/// <summary>Um código de resultado do Vórtice como a semente o declara.</summary>
/// <param name="Codigo">O código (<c>IV_HISTORICO.Resultado</c>).</param>
/// <param name="Estagio">O maior estágio do funil que o código prova; nulo quando não prova nenhum.</param>
/// <param name="ContaComoContato">Se conta como contato para o último contato (a lista da <c>BI_CARTEIRA_VN</c>).</param>
/// <param name="Fonte">De onde a classificação veio.</param>
public sealed record ItemDaClassificacaoDoVortice(int Codigo, EstagioDoFunil? Estagio, bool ContaComoContato, string Fonte);

/// <summary>
/// O QUE CADA CÓDIGO DE RESULTADO DO VÓRTICE SIGNIFICA PARA O CRM — o estágio do funil que ele prova e se ele conta
/// como contato (decisões de 27/09/2026, documento 52 §2.2).
///
/// <para><b>Uma lista só, lida pelas duas rotinas.</b> A <c>PROCESSOS_VORTICE</c> tira daqui o estágio; a
/// <c>CARTEIRAS_VORTICE</c> tira daqui o último contato. Antes, cada uma teria a sua constante no código, e as duas
/// divergiriam no primeiro código novo.</para>
///
/// <para><b>O estágio guardado é o MAIOR que o código prova.</b> As listas do extrator do BI são aninhadas — a da
/// Cobertura contém a da Negociação, que contém a do Pedido, que contém a do Faturamento —, então "alcançou E" é "tem
/// código de estágio ≥ E". A exceção é 2607/2609/2610: o extrator os põe na Negociação e esquece na Cobertura; aqui,
/// por serem de Negociação, eles também contam na Cobertura (decisão de 27/09/2026).</para>
/// </summary>
public sealed class ClassificacaoDeResultadoDoVortice
{
    private ClassificacaoDeResultadoDoVortice() { }

    /// <summary>Identificador interno — a posição na semente, mais um.</summary>
    public int Id { get; private set; }

    /// <summary>O código de resultado no Vórtice.</summary>
    public int CodigoNaOrigem { get; private set; }

    /// <summary>O maior estágio do funil que o código prova; nulo quando não prova nenhum.</summary>
    public EstagioDoFunil? Estagio { get; private set; }

    /// <summary>Se conta como contato comercial para o último contato do cliente.</summary>
    public bool ContaComoContato { get; private set; }

    /// <summary>De onde a classificação veio — extrator, view, decisão.</summary>
    public string Fonte { get; private set; } = default!;

    /// <summary>Uma linha a partir do item da semente.</summary>
    /// <param name="item">O item.</param>
    public static ClassificacaoDeResultadoDoVortice DaSemente(ItemDaClassificacaoDoVortice item) => new()
    {
        CodigoNaOrigem = item.Codigo,
        Estagio = item.Estagio,
        ContaComoContato = item.ContaComoContato,
        Fonte = item.Fonte
    };

    // =============================================================================================
    // A semente — as listas como as fontes as escrevem
    // =============================================================================================

    /// <summary>A entrada digital do Lead: 1278 "Realizou Contato (Digital)" (extrator, l. 257).</summary>
    public static readonly IReadOnlyList<int> ResultadosDoLead = [1278];

    /// <summary>A entrada digital do Qualificado: 3803 "LEAD Qualificado" (extrator, l. 384).</summary>
    public static readonly IReadOnlyList<int> ResultadosDoQualificado = [3803];

    /// <summary>
    /// Os que provam só a Cobertura — a lista da Cobertura do extrator (l. 520–525) sem os que provam mais.
    /// </summary>
    public static readonly IReadOnlyList<int> ResultadosDaCobertura =
        [250, 252, 254, 255, 260, 263, 265, 267, 299, 300, 304, 305, 306, 307, 570, 1286, 3225, 3227, 3639, 3640, 2605, 1929, 2547, 2553, 2554];

    /// <summary>
    /// Os que provam a Negociação — a lista da Negociação do extrator (l. 666–669) sem os do Pedido e do Faturamento.
    /// 2607, 2609 e 2610 estão aqui, e por isso contam também na Cobertura (decisão de 27/09/2026).
    /// </summary>
    public static readonly IReadOnlyList<int> ResultadosDaNegociacao =
        [2563, 2564, 2565, 2566, 2568, 3234, 3572, 3573, 2612, 3223, 2607, 2609, 2610];

    /// <summary>Os que o extrator esquece na Cobertura e a decisão de 27/09/2026 põe lá.</summary>
    public static readonly IReadOnlyList<int> NegociacaoQueOExtratorEsqueceNaCobertura = [2607, 2609, 2610];

    /// <summary>Os que provam o Pedido: 2548, 3231, 3232, 3239 "Venda Aprovada" e 3663 (extrator, l. 803).</summary>
    public static readonly IReadOnlyList<int> ResultadosDoPedido = [2548, 3231, 3232, 3239, 3663];

    /// <summary>Os que provam o Faturamento (extrator, l. 936).</summary>
    public static readonly IReadOnlyList<int> ResultadosDoFaturamento = [2529, 2530, 3440, 3494];

    /// <summary>
    /// OS QUE CONTAM COMO CONTATO — a lista da view <c>BI_CARTEIRA_VN</c> (l. 86–87), código a código e na ordem da
    /// view: 53 códigos distintos, não 55 (conferido em 27/09/2026). É a mesma lista da constante
    /// <c>ResultadosQueContamComoContato</c> do PR #244; quando ele entrar, um teste prende as duas.
    /// </summary>
    public static readonly IReadOnlyList<int> ResultadosQueContamComoContato =
    [
        250, 252, 254, 255, 257, 258, 260, 263, 265, 267, 299, 300, 302, 304, 305,
        306, 307, 308, 570, 1286, 3223, 3225, 3227, 3226, 3231, 3232, 3663, 3234, 3235, 3236, 3639, 3640, 2605, 1929,
        2612, 2622, 1175, 2547, 2553, 2554, 2555, 2548, 2549, 2550, 2563, 2564, 2565, 2566, 2568, 3572, 3573, 3575, 3576
    ];

    private const string DaView = "BI_CARTEIRA_VN (l. 86–87)";

    /// <summary>
    /// A SEMENTE, na ordem que a migração grava (<c>Id</c> = posição + 1): os códigos de estágio, do Lead ao
    /// Faturamento, e depois os que só contam como contato, na ordem da view. Código novo entra no fim.
    /// </summary>
    public static readonly IReadOnlyList<ItemDaClassificacaoDoVortice> Semente = MontarSemente();

    private static List<ItemDaClassificacaoDoVortice> MontarSemente()
    {
        var contato = ResultadosQueContamComoContato.ToHashSet();
        var itens = new List<ItemDaClassificacaoDoVortice>();

        void Estagio(IEnumerable<int> codigos, EstagioDoFunil estagio, string doExtrator)
        {
            foreach (var codigo in codigos)
            {
                var fonte = $"extrator do funil: {doExtrator}";
                if (NegociacaoQueOExtratorEsqueceNaCobertura.Contains(codigo)) fonte += "; também Cobertura (decisão de 27/09/2026)";
                if (contato.Contains(codigo)) fonte += "; " + DaView;
                itens.Add(new ItemDaClassificacaoDoVortice(codigo, estagio, contato.Contains(codigo), fonte));
            }
        }

        Estagio(ResultadosDoLead, EstagioDoFunil.Lead, "Lead (l. 257)");
        Estagio(ResultadosDoQualificado, EstagioDoFunil.Qualificado, "Qualificado (l. 384)");
        Estagio(ResultadosDaCobertura, EstagioDoFunil.Cobertura, "Cobertura (l. 520–525)");
        Estagio(ResultadosDaNegociacao, EstagioDoFunil.Negociacao, "Negociação (l. 666–669)");
        Estagio(ResultadosDoPedido, EstagioDoFunil.Pedido, "Pedido (l. 803)");
        Estagio(ResultadosDoFaturamento, EstagioDoFunil.Faturamento, "Faturamento (l. 936)");

        var comEstagio = itens.Select(i => i.Codigo).ToHashSet();
        foreach (var codigo in ResultadosQueContamComoContato.Where(c => !comEstagio.Contains(c)))
            itens.Add(new ItemDaClassificacaoDoVortice(codigo, null, true, DaView));

        return itens;
    }
}
