using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Potencial;

/// <summary>
/// AS BANDAS DE PORTE PELOS TERCIS DOS MUNICÍPIOS DA ADR (issue 166). O Ricardo decidiu o critério em 27/09/2026;
/// os números saem da demanda que a tela já mostra, e este caso de uso só os calcula — quem registra a vigência é
/// o administrador, pelo formulário dos parâmetros gerais, que vê os números antes.
///
/// <para><b>A demanda é a mesma da tela</b>: a do potencial de referência (documento 54), sem filtro de região nem de
/// loja. A área de atuação e o potencial são mapa da empresa e não dependem da filial de quem pede — os cortes
/// saem iguais para qualquer administrador.</para>
/// </summary>
public sealed class SugerirBandasDePorte(
    IRepositorioDoTerritorioDeReferencia territorio,
    IRepositorioDoPotencialDeReferencia potencial,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Calcula a sugestão.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<SugestaoDasBandasDePorte>>> ExecutarAsync(CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return LeituraDeParametro.SemPermissao<ComProcedencia<SugestaoDasBandasDePorte>>(Permissoes.ParametroDoPotencialAdministrar);

        var agora = relogio.Agora;
        var hoje = ParametroComVigencia.HojeNoBrasil(agora);

        var doTerritorio = await territorio.LerAsync(ct);
        var doPotencial = await potencial.LerAsync(hoje, ct);

        var daAdr = doTerritorio.Area.Values.Where(m => m.PertenceAAdr).ToList();
        var demandas = doPotencial.Categorias.Count == 0
            ? []
            : daAdr.Select(m => doPotencial.DoMunicipio(m.Codigo).Sobreposto.DemandaAnual).OfType<decimal>().ToList();

        var bandas = ParametroDoPotencial.BandasPelosTercis(demandas);

        var sugestao = bandas is { } b
            ? new SugestaoDasBandasDePorte(
                b.MedioAPartirDe, b.GrandeAPartirDe, daAdr.Count, demandas.Count, doPotencial.AnoDaAreaPlantada,
                Justificativa(b.MedioAPartirDe, b.GrandeAPartirDe, demandas.Count, doPotencial.AnoDaAreaPlantada),
                null)
            : new SugestaoDasBandasDePorte(
                null, null, daAdr.Count, demandas.Count, doPotencial.AnoDaAreaPlantada, null,
                demandas.Count < 3
                    ? $"Só {demandas.Count} município(s) da ADR têm demanda anual — o tercil pede pelo menos três. A demanda sai da área plantada do IBGE e das regras de potencial: confira se as duas estão carregadas."
                    : "Os tercis da demanda não sobem: um terço dos municípios da ADR está sem demanda nenhuma, ou todos têm a mesma. Um corte que não separa ninguém não é banda — informe os valores à mão.");

        return Resultado<ComProcedencia<SugestaoDasBandasDePorte>>.Ok(
            ComProcedencia<SugestaoDasBandasDePorte>.DoNossoBanco(
                sugestao, "organizacao.MunicipioDaAreaDeAtuacao · organizacao.ProducaoAgricolaNoMunicipio · organizacao.RegraDePotencial", relogio));
    }

    /// <summary>
    /// A frase da justificativa — o critério, a base e a data da decisão. Ela cabe nos 400 caracteres da vigência e
    /// escreve tudo por extenso, porque aparece na tela de Configurações e na trilha.
    /// </summary>
    private static string Justificativa(decimal medio, decimal grande, int municipios, short? ano) =>
        $"Tercis da demanda anual de máquinas dos {municipios} municípios da ADR com demanda" +
        (ano is { } a ? $" (área plantada do IBGE de {a})" : "") +
        $": mercado médio a partir de {medio.ToString("N1", PtBr)} e grande a partir de {grande.ToString("N1", PtBr)} " +
        "máquinas por ano, um terço dos municípios em cada porte. Critério decidido pelo Ricardo em 27/09/2026 (issue 166).";
}
