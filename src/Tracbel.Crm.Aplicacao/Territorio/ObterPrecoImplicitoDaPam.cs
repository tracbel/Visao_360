using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Territorio;

/// <summary>A série anual do preço implícito de um produto, com as médias plurianuais.</summary>
/// <param name="ProdutoCodigoIbge">O produto na classificação 782 do IBGE.</param>
/// <param name="Produto">O rótulo oficial.</param>
/// <param name="Unidade">A unidade do denominador — "toneladas", "mil frutos", "mil cachos".</param>
/// <param name="Anos">Um por ano, do mais recente para o mais antigo.</param>
/// <param name="MediaDeTresAnos">A média dos três anos mais recentes, ou o motivo de não fechar.</param>
/// <param name="MediaDeCincoAnos">A média dos cinco mais recentes.</param>
/// <param name="MunicipiosComDadoNoUltimoAno">Quantos municípios sustentam o ano mais recente.</param>
public sealed record SerieDoPrecoImplicito(
    int ProdutoCodigoIbge,
    string Produto,
    string Unidade,
    IReadOnlyList<PrecoImplicitoNoAno> Anos,
    MediaPlurianual MediaDeTresAnos,
    MediaPlurianual MediaDeCincoAnos,
    int MunicipiosComDadoNoUltimoAno);

/// <summary>O preço implícito do recorte, por produto.</summary>
/// <param name="Series">Uma por produto, em ordem de nome.</param>
/// <param name="Ressalva">O que quem lê precisa saber para não se enganar.</param>
public sealed record PrecoImplicitoDoRecorte(IReadOnlyList<SerieDoPrecoImplicito> Series, string Ressalva);

/// <summary>
/// O PREÇO RECEBIDO PELO PRODUTOR, DA PAM (issue 198) — anual e municipal, desde 2010.
///
/// <para><b>Ela não substitui a série da CONAB, e não se emenda a ela</b> (documento 49D). São conceitos da
/// mesma família e não intercambiáveis: comparando 2025 em São Paulo, a distância vai de <b>+1,1% na soja a
/// −28,4% no amendoim</b>. Esta rota é separada da de preços justamente para que as duas cheguem à tela
/// como duas coisas, cada uma rotulada.</para>
///
/// <para><b>O que ela destrava:</b> as médias de 3 e 5 anos, que a CONAB não tem — ela é janela de 12 meses
/// —, e o preço <b>por município</b>, que a CONAB também não tem, porque publica por UF.</para>
/// </summary>
public sealed class ObterPrecoImplicitoDaPam(IRepositorioDoPrecoImplicito repositorio, IRelogio relogio)
{
    private const string Ressalva =
        "Preço recebido pelo produtor, derivado da Produção Agrícola Municipal: valor da produção dividido pela " +
        "quantidade produzida, no mesmo ano. É ANUAL e em valor NOMINAL do ano, sem deflator — comparar 2010 com " +
        "2024 compara reais de poder de compra diferente. Não é a série mensal da CONAB e não se soma a ela.";

    /// <summary>Lê a série do recorte.</summary>
    /// <param name="municipioCodigoIbge">O município; vazio soma a ADR inteira.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PrecoImplicitoDoRecorte>>> ExecutarAsync(
        int? municipioCodigoIbge, CancellationToken ct)
    {
        var producao = await repositorio.LerAsync(municipioCodigoIbge, ct);

        var series = producao
            .GroupBy(p => new { p.ProdutoCodigoIbge, p.ProdutoNome })
            .Select(g =>
            {
                // O DOMÍNIO FAZ A CONTA, o repositório só somou. É o que permite provar a regra sem banco.
                var anos = g
                    .Select(p => PrecoImplicitoDaPam.De(
                        p.ProdutoCodigoIbge, p.Ano, p.ValorDaProducaoMilReais, p.QuantidadeProduzida))
                    .OrderByDescending(p => p.Ano)
                    .ToList();

                return new SerieDoPrecoImplicito(
                    g.Key.ProdutoCodigoIbge,
                    g.Key.ProdutoNome,
                    // A unidade é a do ano mais recente: ela pode ter mudado em 2001 nas frutas da nota 2,
                    // e o que a tela rotula é a série como ela está hoje.
                    anos[0].Unidade,
                    anos,
                    PrecoImplicitoDaPam.Media(anos, 3),
                    PrecoImplicitoDaPam.Media(anos, 5),
                    g.OrderByDescending(p => p.Ano).First().MunicipiosComDado);
            })
            .OrderBy(s => s.Produto, StringComparer.Ordinal)
            .ToList();

        return Resultado<ComProcedencia<PrecoImplicitoDoRecorte>>.Ok(
            ComProcedencia<PrecoImplicitoDoRecorte>.DoNossoBanco(
                new PrecoImplicitoDoRecorte(series, Ressalva),
                "organizacao.ProducaoAgricolaNoMunicipio (IBGE/SIDRA — PAM, tabela 5457, variáveis 214 e 215)",
                relogio));
    }
}
