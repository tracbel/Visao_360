using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Territorio;

/// <summary>
/// OS PREÇOS DE MERCADO DE SÃO PAULO — a base que "não varia, só vamos acrescentando" (issue 66).
///
/// <para>Preço recebido pelo produtor (CONAB), preço do kg de ATR da cana (Socicana) e o dólar PTAX
/// do mês (Banco Central), todos de fonte aberta e carregados pelo servidor. É a matéria-prima dos
/// indicadores da issue 73 — momento de preço, rentabilidade e termo de troca —, que ainda não estão
/// aqui: esta rota mostra o preço, não o interpreta.</para>
///
/// <para><b>Não é dado de cliente nem de empresa</b>: é o mesmo para todas as filiais, e por isso não
/// passa pela fronteira de multiempresa.</para>
/// </summary>
public sealed class ObterPrecosDeMercado(IRepositorioDePrecosDeMercado repositorio, IRelogio relogio)
{
    /// <summary>Lê todas as séries.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PrecosDeMercado>>> ExecutarAsync(CancellationToken ct) =>
        Resultado<ComProcedencia<PrecosDeMercado>>.Ok(
            ComProcedencia<PrecosDeMercado>.DoNossoBanco(
                await repositorio.LerAsync(ct),
                "organizacao.CotacaoDeProduto + organizacao.CotacaoDoDolar",
                relogio));
}
