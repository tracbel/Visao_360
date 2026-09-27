namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>
/// UMA VENDA DE MÁQUINA DO ART, com o que o casamento com a nota precisa.
/// </summary>
/// <param name="FilialDoFaturamento">O código da filial que faturou (<c>0101NN</c>), igual ao <c>D2_FILIAL</c>; nulo quando o ART não disse.</param>
/// <param name="NumeroDaNota">O número da nota, como o ART o traz; nulo quando não traz.</param>
/// <param name="Data">A data de faturamento (ou, sem ela, a da venda) — a conferência contra a emissão.</param>
/// <param name="CategoriaDeMaquinaId">A categoria da máquina vendida; nula para usado ou linha sem categoria.</param>
public sealed record VendaComNota(string? FilialDoFaturamento, string? NumeroDaNota, DateOnly? Data, int? CategoriaDeMaquinaId);

/// <summary>
/// UM ITEM DE MÁQUINA NUMA NOTA DE VENDA DO PROTHEUS — uma linha da <c>SD2</c> com grupo <c>VEIC</c>.
/// </summary>
/// <param name="Filial">O <c>D2_FILIAL</c>.</param>
/// <param name="Serie">O <c>D2_SERIE</c>.</param>
/// <param name="Documento">O <c>D2_DOC</c>, com os zeros à esquerda da origem.</param>
/// <param name="Item">O <c>D2_ITEM</c>.</param>
/// <param name="Emissao">O <c>D2_EMISSAO</c>.</param>
/// <param name="Quantidade">O <c>D2_QUANT</c>.</param>
/// <param name="Total">O <c>D2_TOTAL</c> — sem IPI e sem ICMS-ST, líquido de desconto.</param>
public sealed record ItemDeMaquinaNaNota(
    string Filial, string Serie, string Documento, string Item, DateOnly Emissao, decimal Quantidade, decimal Total);

/// <summary>O que aconteceu com cada venda no casamento.</summary>
public enum DesfechoDoCasamento
{
    /// <summary>Um item de máquina, e um só, na nota da venda: o preço entra.</summary>
    Casada = 0,

    /// <summary>A venda não traz número de nota ou filial que faturou (venda direta da fábrica, por exemplo).</summary>
    SemNota = 1,

    /// <summary>A máquina não tem categoria — usado, ou linha ainda sem categoria.</summary>
    SemCategoria = 2,

    /// <summary>A nota da venda não tem item de máquina de venda na SD2 (cancelada, remessa, outra filial).</summary>
    SemItemNaNota = 3,

    /// <summary>Mais de um item de máquina possível — duas máquinas na mesma nota, ou o mesmo número em duas séries.</summary>
    Ambigua = 4,

    /// <summary>O item existe, mas a emissão está longe demais da data da venda: é outra nota com o mesmo número.</summary>
    ForaDaData = 5,

    /// <summary>A venda não tem data para conferir contra a emissão.</summary>
    SemData = 6
}

/// <summary>O preço de uma categoria num mês, pronto para gravar.</summary>
/// <param name="CategoriaDeMaquinaId">A categoria.</param>
/// <param name="Mes">O mês da emissão, no dia 1.</param>
/// <param name="Mediana">A mediana do valor unitário.</param>
/// <param name="Menor">O menor valor unitário.</param>
/// <param name="Maior">O maior valor unitário.</param>
/// <param name="Notas">Quantas notas.</param>
public sealed record PrecoDaCategoriaNoMes(int CategoriaDeMaquinaId, DateOnly Mes, decimal Mediana, decimal Menor, decimal Maior, int Notas);

/// <summary>
/// O PREÇO DA MÁQUINA PELA NOTA (issue 70, D-P12, decidida pelo Ricardo em 27/09/2026) — domínio puro, sem banco.
///
/// <para><b>A regra.</b> Cada venda do ART aponta a nota pela filial que faturou e pelo número. Na <c>SD2</c>, a
/// mesma filial e o mesmo número (sem os zeros à esquerda que o Protheus guarda) dão os itens de máquina da nota.
/// Com <b>um item só</b>, emitido perto da data da venda, o valor unitário é o preço daquela máquina — e a
/// categoria vem da venda.</para>
///
/// <para><b>Na dúvida, fica de fora, contado.</b> Duas máquinas na mesma nota, o mesmo número em duas séries, uma
/// emissão a meses da venda: qualquer escolha ali seria chute sobre qual valor é de qual máquina. O que sai é
/// menos preço e visível, e nunca um preço de outra máquina.</para>
/// </summary>
public static class PrecoDaMaquinaPelaNota
{
    /// <summary>
    /// A DISTÂNCIA ACEITA ENTRE A DATA DA VENDA E A EMISSÃO DA NOTA. Número de nota se repete — por série, e ao longo
    /// dos anos —, e é a data que separa a nota da venda de outra com o mesmo número. Quarenta e cinco dias cobrem o
    /// faturamento que o ART registra depois da emissão sem aceitar a nota do ano seguinte.
    /// </summary>
    public const int DiasDeTolerancia = 45;

    /// <summary>
    /// O número da nota como chave: só os dígitos, sem os zeros à esquerda. O Protheus guarda <c>000048351</c>, e o
    /// ART traz o número (<c>48351</c>); os dois viram <c>48351</c>. Sem dígito nenhum, não há número.
    /// </summary>
    /// <param name="numero">O número como a fonte o escreve.</param>
    public static string? NumeroNormalizado(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero)) return null;

        var digitos = new string([.. numero.Where(char.IsAsciiDigit)]).TrimStart('0');
        return digitos.Length == 0 ? null : digitos;
    }

    /// <summary>
    /// Casa cada venda com o item de máquina da nota dela e devolve o valor unitário de cada casamento, com a
    /// contagem de cada desfecho.
    /// </summary>
    /// <param name="vendas">As vendas do ART.</param>
    /// <param name="itens">Os itens de máquina das notas de venda do Protheus.</param>
    public static (IReadOnlyList<(int CategoriaDeMaquinaId, DateOnly Emissao, decimal ValorUnitario)> Precos,
        IReadOnlyDictionary<DesfechoDoCasamento, int> Desfechos) Casar(
        IEnumerable<VendaComNota> vendas, IEnumerable<ItemDeMaquinaNaNota> itens)
    {
        var porNota = itens
            .Where(i => i.Quantidade > 0 && i.Total > 0 && NumeroNormalizado(i.Documento) is not null)
            .ToLookup(i => (Filial: i.Filial.Trim(), Numero: NumeroNormalizado(i.Documento)!));

        var precos = new List<(int, DateOnly, decimal)>();
        var desfechos = Enum.GetValues<DesfechoDoCasamento>().ToDictionary(d => d, _ => 0);

        // O MESMO ITEM NÃO ENTRA DUAS VEZES: duas vendas do ART que apontem a mesma nota (duplicata na origem)
        // contariam o mesmo preço em dobro.
        var usados = new HashSet<(string, string, string, string)>();

        foreach (var venda in vendas)
        {
            var desfecho = Desfecho(venda, porNota, out var item);

            if (desfecho == DesfechoDoCasamento.Casada
                && !usados.Add((item!.Filial.Trim(), item.Serie.Trim(), item.Documento.Trim(), item.Item.Trim())))
                desfecho = DesfechoDoCasamento.Ambigua;

            desfechos[desfecho]++;

            if (desfecho == DesfechoDoCasamento.Casada)
                precos.Add((venda.CategoriaDeMaquinaId!.Value, item!.Emissao, decimal.Round(item.Total / item.Quantidade, 2)));
        }

        return (precos, desfechos);
    }

    private static DesfechoDoCasamento Desfecho(
        VendaComNota venda,
        ILookup<(string Filial, string Numero), ItemDeMaquinaNaNota> porNota,
        out ItemDeMaquinaNaNota? item)
    {
        item = null;

        if (venda.CategoriaDeMaquinaId is null) return DesfechoDoCasamento.SemCategoria;

        var numero = NumeroNormalizado(venda.NumeroDaNota);
        if (numero is null || string.IsNullOrWhiteSpace(venda.FilialDoFaturamento)) return DesfechoDoCasamento.SemNota;
        if (venda.Data is not { } data) return DesfechoDoCasamento.SemData;

        var daNota = porNota[(venda.FilialDoFaturamento.Trim(), numero)].ToList();
        if (daNota.Count == 0) return DesfechoDoCasamento.SemItemNaNota;

        var perto = daNota.Where(i => Math.Abs(i.Emissao.DayNumber - data.DayNumber) <= DiasDeTolerancia).ToList();
        if (perto.Count == 0) return DesfechoDoCasamento.ForaDaData;
        if (perto.Count > 1) return DesfechoDoCasamento.Ambigua;

        item = perto[0];
        return DesfechoDoCasamento.Casada;
    }

    /// <summary>
    /// A MEDIANA: o valor do meio, ou a média dos dois do meio quando a quantidade é par.
    /// </summary>
    /// <param name="valores">Os valores; ao menos um.</param>
    /// <exception cref="ArgumentException">Sem valor nenhum.</exception>
    public static decimal Mediana(IReadOnlyCollection<decimal> valores)
    {
        if (valores.Count == 0) throw new ArgumentException("Mediana de nada não existe.", nameof(valores));

        var ordenados = valores.Order().ToList();
        var meio = ordenados.Count / 2;
        return ordenados.Count % 2 == 1 ? ordenados[meio] : (ordenados[meio - 1] + ordenados[meio]) / 2m;
    }

    /// <summary>O preço de cada categoria em cada mês de emissão: mediana, menor, maior e quantas notas.</summary>
    /// <param name="precos">Os valores unitários casados.</param>
    public static IReadOnlyList<PrecoDaCategoriaNoMes> PorMes(
        IEnumerable<(int CategoriaDeMaquinaId, DateOnly Emissao, decimal ValorUnitario)> precos) =>
    [
        .. precos
            .GroupBy(p => (p.CategoriaDeMaquinaId, Mes: new DateOnly(p.Emissao.Year, p.Emissao.Month, 1)))
            .OrderBy(g => g.Key.CategoriaDeMaquinaId).ThenBy(g => g.Key.Mes)
            .Select(g =>
            {
                var valores = g.Select(p => p.ValorUnitario).ToList();
                return new PrecoDaCategoriaNoMes(
                    g.Key.CategoriaDeMaquinaId, g.Key.Mes, decimal.Round(Mediana(valores), 2), valores.Min(), valores.Max(), valores.Count);
            })
    ];
}
