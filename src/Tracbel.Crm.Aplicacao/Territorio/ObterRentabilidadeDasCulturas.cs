using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Territorio;

/// <summary>
/// A RENTABILIDADE POR CULTURA (issue 159) — receita, custo e margem por hectare.
///
/// <para>O pedido: "se o cara está rentabilizando X e o custo de produção é tanto". Preço e custo já
/// estavam na tela, <b>separados</b>: quem quisesse saber se a lavoura paga a conta tinha de fazer a
/// subtração de cabeça, sem saber que a produtividade de um lado e o custo do outro podiam ser de anos
/// diferentes.</para>
///
/// <para><b>Cada número vem com a competência dele</b> — o ano da PAM, os meses de preço e a safra do
/// custo —, que é o que o pedido chama de "local, sistema, safra, fonte e data no tooltip".</para>
///
/// <para>É dado público, o mesmo para todas as filiais: não passa pela fronteira de multiempresa.</para>
/// </summary>
public sealed class ObterRentabilidadeDasCulturas(IRepositorioDeRentabilidade repositorio, IRelogio relogio)
{
    /// <summary>Lê a rentabilidade de cada cultura ativa do catálogo.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<IReadOnlyList<RentabilidadeDaCultura>>>> ExecutarAsync(CancellationToken ct) =>
        Resultado<ComProcedencia<IReadOnlyList<RentabilidadeDaCultura>>>.Ok(
            ComProcedencia<IReadOnlyList<RentabilidadeDaCultura>>.DoNossoBanco(
                await repositorio.LerAsync(ct),
                "organizacao.Cultura · organizacao.ProducaoAgricolaNoEstado · organizacao.CotacaoDeProduto · organizacao.CustoDeProducao",
                relogio));
}
