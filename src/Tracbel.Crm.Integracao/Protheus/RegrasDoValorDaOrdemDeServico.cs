using System.Globalization;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// A RÉGUA DO VALOR DE UMA ORDEM DE SERVIÇO — a mesma do painel "Pós-Venda (Serviços)" do BI, portada do script do
/// extrator "Pós Vendas Serviços" (abas "02 - Pecas_OS" e "03 - Srv Executado") e das medidas mestras do painel. O
/// script do BI é a especificação de fato do negócio: onde esta régua divergir dele, é defeito daqui.
///
/// <para><b>O nulo segue o Qlik</b>: conta com nulo dá nulo, divisão por zero dá nulo, comparação com nulo é falsa — e o
/// valor nulo não entra na soma. Por isso tudo aqui é <c>decimal?</c> e as comparações passam por
/// <see cref="Maior"/>, <c>Igual</c> e <see cref="Diferente"/>.</para>
/// </summary>
public static class RegrasDoValorDaOrdemDeServico
{
    /// <summary>
    /// O VALOR DAS PEÇAS DE UMA OS — <c>Σ QTD_PECA × (VLR_UNI_PECA − VLR_DESC_UNI_PECA)</c>, a medida "Vlr Peças" do BI.
    ///
    /// <para>As linhas de peça são as DISTINTAS (o <c>LOAD DISTINCT</c> do BI). O desconto da linha
    /// (<c>VLR_DESC</c>) vem repetido em cada requisição da mesma peça: o BI toma o MAIOR por peça (a chave sem a
    /// requisição, <c>CHAVE_PECA_2</c>) e o rateia pela quantidade — o que, somado, desconta o maior uma vez só por
    /// peça. O desconto ausente vale zero (o campo existe desde 04/08/2026 na origem; antes, não havia desconto a
    /// descontar).</para>
    /// </summary>
    /// <param name="itens">Os itens da OS.</param>
    public static decimal ValorDasPecas(IEnumerable<ItemDaOrdemNaOrigem> itens)
    {
        var linhas = itens
            .Where(i => i.EhPeca)
            .Select(i => new LinhaDePeca(ChaveDaPeca(i), i.CodigoDoItem, i.Formula, i.Grupo, i.Quantidade, i.ValorUnitario,
                i.ValorDoDesconto, i.ProdutoRequisitado, i.OrigemParalela))
            .Distinct()
            .ToList();

        var desconto = linhas
            .GroupBy(l => ChaveSemARequisicao(l.Chave), StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var quantidade = g.Sum(l => l.Quantidade ?? 0m);
                    var maior = g.Max(l => l.Desconto) ?? 0m;
                    return quantidade == 0m ? 0m : maior / quantidade;
                },
                StringComparer.Ordinal);

        var total = 0m;
        foreach (var l in linhas)
        {
            if (l.Quantidade is not { } quantidade || l.ValorUnitario is not { } unitario) continue;
            total += quantidade * (unitario - desconto[ChaveSemARequisicao(l.Chave)]);
        }

        return total;
    }

    /// <summary>
    /// A <c>CHAVE_PECA</c> do BI: <c>FILIAL|NUMERO_OS|TIPO_TEMPO|COD_ITEM|NOSS_NUM_REQ</c> — o nulo vira texto vazio, como
    /// na concatenação do Qlik.
    /// </summary>
    /// <param name="item">O item.</param>
    public static string ChaveDaPeca(ItemDaOrdemNaOrigem item) =>
        string.Join('|', item.Filial, item.NumeroOs, item.TipoDeTempo ?? string.Empty, item.CodigoDoItem ?? string.Empty,
            item.Requisicao ?? string.Empty);

    /// <summary>
    /// A <c>CHAVE_PECA_2</c> do BI: <c>REPLACE(CHAVE_PECA, RIGHT(CHAVE_PECA, 9), '')</c> — a chave sem a requisição
    /// (a barra e os oito dígitos). Portada como está, inclusive o <c>REPLACE</c> que tira toda ocorrência do sufixo.
    /// </summary>
    /// <param name="chave">A <c>CHAVE_PECA</c>.</param>
    public static string ChaveSemARequisicao(string chave)
    {
        var sufixo = chave.Length >= 9 ? chave[^9..] : chave;
        return sufixo.Length == 0 ? chave : chave.Replace(sufixo, string.Empty, StringComparison.Ordinal);
    }

    /// <summary>
    /// O "VALOR SERVIÇO C/DESC" DE UM SERVIÇO EXECUTADO — a coluna do BI que o painel soma (aba "03 - Srv Executado").
    /// RAT (rateio), FDLI (não contabilizado no faturamento) e RSF (não gera receita, desde 16/06/2025) valem zero; km de
    /// socorro é km vendido × valor do km; serviço de terceiro é o valor dele; o resto é tempo vendido × valor da hora,
    /// com o desconto rateado pelo tempo. Nulo quando a conta do BI dá nulo — e então não entra na soma.
    /// </summary>
    /// <param name="s">O serviço executado.</param>
    public static decimal? ValorDoServico(ServicoExecutadoNaOrigem s)
    {
        if (Igual(s.TpServico, "RAT") || Igual(s.Vo4TipTem, "FDLI") || Igual(s.Vo4TipTem, "RSF")) return 0m;

        var unitario = ValorUnitario(s);
        var desconto = Desconto(s);
        return Prestacao(s) switch
        {
            TipoDePrestacao.Km => KmVendido(s) * unitario - desconto,
            TipoDePrestacao.ServicoDeTerceiro => StVendido(s) * unitario - desconto,
            _ => TempoVendido(s) * unitario - Dividir(desconto, TempoVendidoTotal(s)) * TempoVendido(s)
        };
    }

    // =============================================================================================
    // As colunas intermediárias do BI, na ordem do script
    // =============================================================================================

    /// <summary>O <c>Tipo_Prestacao</c> — só o que muda a conta do valor; a ordem dos testes é a do script.</summary>
    private static TipoDePrestacao Prestacao(ServicoExecutadoNaOrigem s)
    {
        if (Contem(s.DescTpTpo, "PECA") || Contem(s.DescTpTpo, "PEÇA")) return TipoDePrestacao.SomentePecas;
        if (Igual(s.VokIncMob, "5")) return TipoDePrestacao.Km;
        if (Igual(s.TpServico, "DGR")) return TipoDePrestacao.KmGarantia;
        if (Igual(s.TpServico, "SA") || Contem(s.CodServico, "SERVICE")) return TipoDePrestacao.ServiceAdvisor;
        if (Igual(s.TpServico, "ST") || Igual(s.VokIncMob, "2")) return TipoDePrestacao.ServicoDeTerceiro;
        return TipoDePrestacao.Outros;
    }

    /// <summary>O <c>Valor_Unitario_Srv</c>.</summary>
    private static decimal? ValorUnitario(ServicoExecutadoNaOrigem s)
    {
        if (Igual(s.VokIncMob, "5"))
        {
            if (Maior(s.VscKilRod, 0m))
            {
                if (Igual(s.VoiSitTpo, "3"))
                    return Igual(s.Vo4ValInt, 0.01m) ? s.Vo4ValInt : Dividir(s.Vo4ValInt, s.Vo4KilRod);

                var esquerda = Piso(Dividir(s.VscValBru, s.VscKilRod) * s.Vo4KilRod);
                var direita = Piso(s.VscValBru);
                if (esquerda < direita)
                    return Diferente(s.Vo4PreKil, 0m) ? s.Vo4PreKil
                        : Diferente(s.Vo4ValHor, 0m) ? s.Vo4ValHor
                        : Dividir(s.VscValBru, s.VscKilRod);
                return Dividir(s.VscValBru, s.VscKilRod);
            }

            if (Maior(s.Vz1ValBru, 0m))
                return Maior(s.Vo4KilRod, 0m) ? Dividir(s.Vz1ValBru, s.Vo4KilRod) : s.Vz1ValBru;

            if (s.Vo4PreKil is not null && s.Vo4PreKil < s.Vo4ValHor)
                return Igual(s.Vo4PreKil, 0m) && Diferente(s.VokPreKil, 0m) ? s.VokPreKil : s.Vo4PreKil;
            return s.Vo4ValHor;
        }

        if (Igual(s.TpServico, "ST") || Igual(s.VokIncMob, "2"))
            return Maior(s.VscValBru, 0m) ? s.VscValBru
                : Maior(s.Vz1ValUni, 0m) ? s.Vz1ValUni
                : s.Vo4ValVen;

        if (Maior(s.VscValBru, 0m)) return Dividir(s.VscValBru, s.TemCob);

        if (Maior(s.Vz1ValBru, 0m))
        {
            decimal? divisor;
            if (Igual(s.TpServico, "HI") && (Igual(s.TemVen, 0m) || (Maior(s.TemPad, 0m) && Diferente(s.TemVen, s.TemPad))))
                divisor = s.TemPadTotal;
            else if (Igual(s.TpServico, "HT"))
                divisor = s.TemTraTotal is not null && Maior(s.TemTraTotal, 0m) ? s.TemTraTotal : 1m;
            else
                divisor = s.TemCobTotal;
            return Dividir(s.Vz1ValBru, divisor);
        }

        return Igual(s.VoiSitTpo, "3") ? s.Vo4ValInt : s.Vo4ValHor;
    }

    /// <summary>O <c>Tempo_Vendido</c>.</summary>
    private static decimal? TempoVendido(ServicoExecutadoNaOrigem s)
    {
        if (Igual(s.VokIncMob, "5") || Igual(s.TpServico, "ST")) return 0m;

        if (Igual(s.TpServico, "HT"))
            return s.TemTra is not null && Maior(s.TemTra, 0m) ? s.TemTra
                : s.TemTraTotal is not null && Maior(s.TemTraTotal, 0m) ? 0m
                : s.TemPad;

        if (Igual(s.TpServico, "HI") && (Igual(s.TemVen, 0m) || (Maior(s.TemPad, 0m) && Diferente(s.TemVen, s.TemPad))))
            return Maior(s.TemCob, 0m) && Diferente(s.TemCob, s.TemPad) ? s.TemCob : s.TemPad;

        if (Maior(s.TemCob, 0m)) return s.TemCob;
        if (s.TemCobTotal is not null && Maior(s.TemCobTotal, 0m)) return 0m;
        if (s.TemVen is not null && Maior(s.TemVen, 0m)) return s.TemVen;
        if (s.TemVenTotal is not null && Maior(s.TemVenTotal, 0m)) return 0m;
        return s.TemPad;
    }

    /// <summary>O <c>Tempo_Vendido_Total</c>.</summary>
    private static decimal? TempoVendidoTotal(ServicoExecutadoNaOrigem s)
    {
        if (Igual(s.VokIncMob, "5") || Igual(s.TpServico, "ST")) return 0m;

        if (Igual(s.TpServico, "HT"))
            return s.TemTraTotal is not null && Maior(s.TemTraTotal, 0m) ? s.TemTraTotal : s.TemPadTotal;

        if (Igual(s.TpServico, "HI")
            && (Igual(s.TemVenTotal, 0m) || (Maior(s.TemPadTotal, 0m) && Diferente(s.TemVenTotal, s.TemPadTotal))))
            return Maior(s.TemCobTotal, 0m) && Diferente(s.TemCobTotal, s.TemPadTotal) ? s.TemCobTotal : s.TemPadTotal;

        if (Maior(s.TemCobTotal, 0m)) return s.TemCobTotal;
        return Igual(s.TemVenTotal, 0m) ? s.TemPadTotal : s.TemVenTotal;
    }

    /// <summary>O <c>St_Vendido</c>.</summary>
    private static decimal? StVendido(ServicoExecutadoNaOrigem s) =>
        Igual(s.TpServico, "ST") || Igual(s.VokIncMob, "2") ? 1m : 0m;

    /// <summary>O <c>Km_Vendido</c> — o km vendido por R$ 0,01 conta uma vez só (regra do BI de 13/10/2025).</summary>
    private static decimal? KmVendido(ServicoExecutadoNaOrigem s) =>
        Igual(s.VokIncMob, "5") ? (Igual(s.Vo4ValInt, 0.01m) ? 1m : s.Vo4KilRod) : 0m;

    /// <summary>O <c>Desconto_Srv</c>.</summary>
    private static decimal? Desconto(ServicoExecutadoNaOrigem s)
    {
        if (Igual(s.VokIncMob, "5") && Maior(s.VscKilRod, 0m) && Igual(s.VoiSitTpo, "3") && Igual(s.Vo4ValInt, 0.01m)) return 0m;
        if (s.Vz1ValDes is null || Igual(s.Vz1ValDes, 0m))
            return s.VscValDes is null || Igual(s.VscValDes, 0m) ? s.Vo4ValDes : s.VscValDes;
        return s.Vz1ValDes;
    }

    // =============================================================================================
    // O nulo do Qlik
    // =============================================================================================

    /// <summary><c>a &gt; b</c> no Qlik: falso quando um dos dois é nulo.</summary>
    private static bool Maior(decimal? a, decimal b) => a is { } valor && valor > b;

    /// <summary><c>a = b</c> no Qlik: falso quando um dos dois é nulo.</summary>
    private static bool Igual(decimal? a, decimal b) => a is { } valor && valor == b;

    /// <summary><c>a &lt;&gt; b</c> no Qlik: falso quando um dos dois é nulo — diferente do <c>!=</c> do C#.</summary>
    private static bool Diferente(decimal? a, decimal? b) => a is { } x && b is { } y && x != y;

    /// <summary><c>a = 'texto'</c> no Qlik, sobre o valor já aparado.</summary>
    private static bool Igual(string? a, string b) => a is not null && string.Equals(a.Trim(), b, StringComparison.OrdinalIgnoreCase);

    /// <summary><c>a LIKE '%texto%'</c> no Qlik — sem distinguir maiúscula.</summary>
    private static bool Contem(string? a, string trecho) => a is not null && a.Contains(trecho, StringComparison.OrdinalIgnoreCase);

    /// <summary>A divisão do Qlik: nula com nulo ou com divisor zero.</summary>
    private static decimal? Dividir(decimal? a, decimal? b) => a is { } x && b is { } y && y != 0m ? x / y : null;

    /// <summary>O <c>floor</c> do Qlik.</summary>
    private static decimal? Piso(decimal? a) => a is { } x ? Math.Floor(x) : null;

    private enum TipoDePrestacao { SomentePecas, Km, KmGarantia, ServiceAdvisor, ServicoDeTerceiro, Outros }

    private sealed record LinhaDePeca(
        string Chave, string? Codigo, string? Formula, string? Grupo, decimal? Quantidade, decimal? ValorUnitario, decimal? Desconto,
        string? ProdutoRequisitado, string? OrigemParalela);
}
