using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Territorio;

/// <summary>
/// O CUSTO DE PRODUÇÃO DAS CULTURAS EM SÃO PAULO — as séries históricas da CONAB (issue 67).
///
/// <para>É a outra metade da rentabilidade do texto-base ("se o cara está rentabilizando X e o custo de
/// produção é tanto"): o preço vem da issue 66. A margem, que junta os dois, é da issue 73 e ainda não
/// está aqui — esta rota mostra o custo, não o interpreta.</para>
///
/// <para>Como os preços, é o mesmo para todas as filiais e não passa pela fronteira de multiempresa.</para>
/// </summary>
public sealed class ObterCustosDeProducao(IRepositorioDeCustosDeProducao repositorio, IRelogio relogio)
{
    /// <summary>Lê todas as séries.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<IReadOnlyList<SerieDeCusto>>>> ExecutarAsync(CancellationToken ct) =>
        Resultado<ComProcedencia<IReadOnlyList<SerieDeCusto>>>.Ok(
            ComProcedencia<IReadOnlyList<SerieDeCusto>>.DoNossoBanco(
                await repositorio.LerAsync(ct), "organizacao.CustoDeProducao", relogio));
}
