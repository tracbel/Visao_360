using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A RENTABILIDADE POR CULTURA (issue 159): junta o que vivia em três telas separadas — a produtividade
/// da PAM, o preço da CONAB e o custo da CONAB — e devolve a margem com a competência de cada parte.
///
/// <para><b>Tudo no nível de São Paulo.</b> O custo da CONAB é por local de referência, e não por
/// município; misturar uma produtividade municipal com um custo estadual daria uma margem que não
/// existe em lugar nenhum. A margem por município vem quando houver custo por município.</para>
///
/// <para><b>O ano manda.</b> A produtividade é a do ano mais recente da PAM daquela cultura; o preço é a
/// média dos meses <b>daquele mesmo ano</b> que o CRM tem; o custo é a safra mais recente até ali. Sem
/// essa amarração, a margem misturaria a produtividade de um ano com o preço de outro — que é o defeito
/// que o documento 49 aponta na planilha do comercial.</para>
///
/// <para><b>O catálogo é pequeno e as séries também</b> (alguns milhares de linhas no total): a leitura
/// traz tudo e cruza em memória, em vez de espalhar seis consultas por cultura.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDeRentabilidade(CrmDbContext contexto) : IRepositorioDeRentabilidade
{
    private const int CodigoDeSaoPaulo = 35;

    /// <summary>Uma medida da PAM no estado, do produto e do ano.</summary>
    private sealed record MedidaDaPam(int Produto, short Ano, decimal? Quantidade, decimal? AreaColhida, decimal? ValorMilReais);

    /// <summary>Um preço mensal de uma fonte.</summary>
    private sealed record PrecoMensal(string Fonte, string Codigo, DateOnly Mes, decimal Valor);

    /// <summary>Uma safra de uma série de custo.</summary>
    private sealed record CustoDaSafra(string Cultura, string Local, short Safra, decimal Operacional, decimal? Total);

    /// <inheritdoc />
    public async Task<IReadOnlyList<RentabilidadeDaCultura>> LerAsync(CancellationToken ct)
    {
        var culturas = await contexto.Culturas.AsNoTracking()
            .Where(c => c.EstaAtiva).OrderBy(c => c.Nome).ToListAsync(ct);

        if (culturas.Count == 0) return [];

        // SÓ OS PRODUTOS QUE ENTRAM NA SOMA: o café tem "Total", "Arábica" e "Canephora", e somar os
        // três dobraria a área. A marca vem do catálogo (issue 165), não de uma lista aqui.
        var vinculos = await contexto.ProdutosDaPamNasCulturas.AsNoTracking()
            .Where(p => p.EntraNaSomaDaLavoura).ToListAsync(ct);

        var produtos = vinculos.Select(v => v.ProdutoCodigoIbge).Distinct().ToList();

        var daPam = await contexto.ProducoesAgricolasNosEstados.AsNoTracking()
            .Where(p => p.EstadoCodigoIbge == CodigoDeSaoPaulo && produtos.Contains(p.ProdutoCodigoIbge))
            .Select(p => new MedidaDaPam(p.ProdutoCodigoIbge, p.Ano, p.QuantidadeProduzida, p.AreaColhidaHectares, p.ValorDaProducaoMilReais))
            .ToListAsync(ct);

        var precos = await contexto.CotacoesDeProdutos.AsNoTracking()
            .Select(c => new PrecoMensal(c.Fonte, c.CodigoNaFonte, c.Mes, c.ValorEmReais))
            .ToListAsync(ct);

        var custos = await contexto.CustosDeProducao.AsNoTracking()
            .Select(c => new CustoDaSafra(c.Cultura, c.Local, c.Safra, c.CustoOperacionalHa, c.CustoTotalHa))
            .ToListAsync(ct);

        var porCultura = vinculos.ToLookup(v => v.CulturaId);

        return [.. culturas.Select(c => Montar(c, [.. porCultura[c.Id]], daPam, precos, custos))];
    }

    private static RentabilidadeDaCultura Montar(
        Cultura cultura,
        IReadOnlyList<ProdutoDaPamNaCultura> vinculos,
        IReadOnlyList<MedidaDaPam> daPam,
        IReadOnlyList<PrecoMensal> precos,
        IReadOnlyList<CustoDaSafra> custos)
    {
        var codigos = vinculos.Select(v => v.ProdutoCodigoIbge).ToHashSet();
        var linhas = daPam.Where(p => codigos.Contains(p.Produto)).ToList();

        // O ANO MAIS RECENTE DA CULTURA, e não o mais recente da PAM inteira: cada cultura tem o seu.
        short? ano = linhas.Count == 0 ? null : linhas.Max(p => p.Ano);

        var produtividade = ano is { } a ? ProdutividadeEmKg(linhas, a) : null;

        // ÁREA COLHIDA ZERO NÃO É ÁREA: ela vira nulo, e a margem total não sai — em vez de sair zero,
        // que diria "não há margem" quando o certo é "não se sabe".
        decimal? areaColhida = null;
        if (ano is { } anoDaArea)
        {
            var soma = linhas.Where(p => p.Ano == anoDaArea).Sum(p => p.AreaColhida ?? 0m);
            if (soma > 0) areaColhida = soma;
        }

        var (preco, meses) = PrecoMedio(cultura, precos, ano);
        var receita = Rentabilidade.ReceitaPorHectare(produtividade, preco);

        var custo = CustoDaReferencia(cultura, custos, ano);
        var custoPorHa = NaCamada(cultura, custo);

        // A SAFRA ANTERIOR DA SÉRIE, e não a do ano anterior da PAM — ver RentabilidadeDaCultura.SafraAnteriorDoCusto.
        var custoAnterior = custo is null ? null : CustoDaReferencia(cultura, custos, (short)(custo.Safra - 1));

        var margem = Rentabilidade.MargemPorHectare(receita, custoPorHa);

        var motivo = Rentabilidade.PorQueNaoSaiu(
            cultura.LocalDeReferenciaDoCusto is not null,
            cultura.CamadaDeCustoDaMargem is not null,
            custo is not null,
            custoPorHa,
            produtividade,
            preco);

        return new RentabilidadeDaCultura(
            cultura.Codigo,
            cultura.Nome,
            cultura.UnidadeComercial,
            ano,
            produtividade,
            preco,
            meses,
            receita,
            cultura.LocalDeReferenciaDoCusto,
            cultura.CamadaDeCustoDaMargem?.ToString(),
            custo?.Safra,
            custoPorHa,
            margem,
            IndicadoresDeMercado.MargemPorUnidade(margem, produtividade, cultura.QuilosPorUnidade),
            areaColhida,
            Rentabilidade.MargemTotal(margem, areaColhida),
            motivo.ToString(),
            Rentabilidade.Frase(motivo, cultura.Nome),
            Tendencia(cultura, linhas, custos, ano),
            custoAnterior?.Safra,
            NaCamada(cultura, custoAnterior));
    }

    /// <summary>O custo por hectare de uma safra na camada que a cultura escolheu; nulo sem safra ou sem camada.</summary>
    private static decimal? NaCamada(Cultura cultura, CustoDaSafra? c) =>
        c is null || cultura.CamadaDeCustoDaMargem is not { } camada
            ? null
            : camada == CamadaDoCusto.Operacional ? c.Operacional : c.Total;

    /// <summary>
    /// A TENDÊNCIA: o ano mais recente contra o anterior, os dois pelo preço recebido da PAM (valor ÷ quantidade) e pelo
    /// custo da safra mais recente até cada ano. Ver <see cref="TendenciaDaRentabilidade"/>.
    /// </summary>
    private static TendenciaDaRentabilidade? Tendencia(
        Cultura cultura, IReadOnlyList<MedidaDaPam> linhas, IReadOnlyList<CustoDaSafra> custos, short? ano)
    {
        if (ano is not { } a) return null;
        var anterior = (short)(a - 1);
        if (!linhas.Any(p => p.Ano == anterior)) return null;

        var custo = CustoDaReferencia(cultura, custos, a);
        var custoAnterior = CustoDaReferencia(cultura, custos, anterior);

        return new TendenciaDaRentabilidade(
            a,
            anterior,
            MargemPelaPam(linhas, a, NaCamada(cultura, custo)),
            MargemPelaPam(linhas, anterior, NaCamada(cultura, custoAnterior)),
            custo?.Safra,
            NaCamada(cultura, custo),
            custoAnterior?.Safra,
            NaCamada(cultura, custoAnterior));
    }

    /// <summary>
    /// A margem de um ano pela receita da PAM — valor da produção ÷ área colhida — menos o custo. É a receita que o
    /// produtor recebeu, sem precisar do preço em R$/kg: o valor da PAM já é quantidade × preço recebido.
    /// </summary>
    private static decimal? MargemPelaPam(IReadOnlyList<MedidaDaPam> linhas, short ano, decimal? custoPorHa)
    {
        var doAno = linhas.Where(p => p.Ano == ano).ToList();
        var area = doAno.Sum(p => p.AreaColhida ?? 0m);
        var valor = doAno.Sum(p => p.ValorMilReais ?? 0m);
        if (area <= 0 || valor <= 0 || custoPorHa is not { } custo) return null;

        return decimal.Round(valor * 1_000m / area - custo, 2);
    }

    /// <summary>
    /// A produtividade em QUILOS por hectare colhido.
    ///
    /// <para><b>Só quando a quantidade é massa.</b> O IBGE publica abacaxi e coco em mil frutos (issue
    /// 152): multiplicar isso por um preço em R$/kg daria um número sem significado. Nesses casos a
    /// produtividade sai nula, e a margem não sai.</para>
    /// </summary>
    private static decimal? ProdutividadeEmKg(IReadOnlyList<MedidaDaPam> linhas, short ano)
    {
        var doAno = linhas.Where(p => p.Ano == ano).ToList();
        if (doAno.Any(p => !UnidadesDaPam.DaQuantidade(p.Produto, ano).EhMassa)) return null;

        var quantidade = doAno.Sum(p => p.Quantidade ?? 0m);
        var area = doAno.Sum(p => p.AreaColhida ?? 0m);

        // A quantidade da PAM vem em TONELADAS; o preço da CONAB, em R$/kg.
        return area > 0 && quantidade > 0 ? quantidade * 1_000m / area : null;
    }

    /// <summary>
    /// O preço médio dos meses do ano da produtividade, na fonte que a cultura declara.
    ///
    /// <para>Sem ano da PAM, ou sem fonte de preço no catálogo, não há média — e a contagem de meses
    /// volta zero, que é o que a tela mostra ao lado do número.</para>
    /// </summary>
    private static (decimal? Preco, int Meses) PrecoMedio(
        Cultura cultura, IReadOnlyList<PrecoMensal> precos, short? ano)
    {
        if (ano is not { } a || cultura.FonteDoPreco is not { } fonte || cultura.ProdutoDoPreco is not { } codigo)
            return (null, 0);

        var doAno = precos
            .Where(p => p.Mes.Year == a
                        && string.Equals(p.Fonte, fonte, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(p.Codigo, codigo, StringComparison.Ordinal))
            .ToList();

        return doAno.Count == 0 ? (null, 0) : (doAno.Average(p => p.Valor), doAno.Count);
    }

    /// <summary>
    /// A safra de custo mais recente ATÉ o ano da produtividade, no local de referência da cultura.
    ///
    /// <para>Usar uma safra posterior compararia a receita de um ano com o custo de outro, mais novo —
    /// e o número pareceria pior (ou melhor) do que foi.</para>
    /// </summary>
    private static CustoDaSafra? CustoDaReferencia(Cultura cultura, IReadOnlyList<CustoDaSafra> custos, short? ano)
    {
        if (cultura.SerieDeCusto is not { } serie || cultura.LocalDeReferenciaDoCusto is not { } local)
            return null;

        return custos
            .Where(c => c.Cultura.StartsWith(serie, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(c.Local, local, StringComparison.OrdinalIgnoreCase)
                        && (ano is null || c.Safra <= ano))
            .MaxBy(c => c.Safra);
    }
}
