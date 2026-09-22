using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Conab;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A CARGA DOS CUSTOS DE PRODUÇÃO DA CONAB — as abas de São Paulo das séries históricas (issue 67).
///
/// <para>Uma transação só para todas as culturas: são ~170 abas, e uma carga pela metade deixaria
/// café de 2025 ao lado de cana de 2024 sem ninguém saber. Se uma planilha não baixar, a carga para e
/// não grava nada — a próxima rodada tenta de novo.</para>
///
/// <para><b>Nada é apagado.</b> Aba que sumiu da planilha (a CONAB não costuma tirar série, mas pode)
/// continua no CRM: é o custo que valeu naquela safra.</para>
///
/// <para><b>Reexecutável.</b> A segunda rodada não grava nada: aba igual não é tocada, nem no carimbo.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="conab">A leitura das séries da CONAB.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDeCustosDeProducao(
    Func<CrmDbContext> abrirContexto,
    LeitorDeCustosDaConab conab,
    long usuarioId,
    Action<string> relatar)
{
    private const string Fluxo = "CONAB.CUSTO_DE_PRODUCAO";
    private const string Etapa = "Custos de produção em SP (CONAB, séries históricas)";

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];

    /// <summary>Executa a carga.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<(string Etapa, string Rotulo, int Valor)>> ExecutarAsync(CancellationToken ct)
    {
        await using var trava = await TravaDeFluxo.TomarAsync(abrirContexto(), Fluxo, ct);

        relatar("Procurando as séries históricas de custo na página da CONAB…");
        var planilhas = await conab.LocalizarPlanilhasAsync(ct);

        var lidas = new List<(PlanilhaDeCustos Planilha, LinhaDeCustoDaConab Linha)>();
        foreach (var planilha in planilhas)
        {
            relatar($"Lendo {planilha.Cultura} ({Path.GetFileName(planilha.Endereco.AbsolutePath)})…");
            var linhas = await conab.LerAsync(planilha, ct);
            lidas.AddRange(linhas.Select(l => (planilha, l)));
            Contar($"abas de SP — {planilha.Cultura}", linhas.Count);
        }

        var agora = DateTime.UtcNow;
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            contexto, "CONAB", "CONAB — preços agropecuários e custos de produção", "Arquivo público, somente leitura", ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        // O LOCAL DA CONAB VIRA MUNICÍPIO QUANDO O NOME CASA SEM AMBIGUIDADE — e o par fica gravado (issue 154),
        // para a rodada seguinte não casar por nome de novo. Quando não casa, o custo entra sem município: ele
        // vale pelo local de referência, não pelo mapa.
        var dePara = await CorrespondenciaDeMunicipios.AbrirAsync(contexto, Fluxo, "SP", usuarioId, ct);

        var existentes = (await contexto.CustosDeProducao.ToListAsync(ct)).ToDictionary(c => (c.Cultura, c.Aba));

        var recusas = new List<(object Conteudo, string Motivo)>();
        var novas = new List<CustoDeProducao>();
        var vistas = new HashSet<(string, string)>();
        int revisadas = 0, mantidas = 0, semMunicipio = 0, semTotal = 0;

        foreach (var (planilha, l) in lidas)
        {
            if (!vistas.Add((l.Cultura, l.Aba)))
            {
                recusas.Add((l, $"A aba \"{l.Aba}\" de {l.Cultura} apareceu duas vezes. Vale a primeira."));
                continue;
            }

            int? municipioId = dePara.Resolver(l.Local, l.Local, agora, out var id) ? id : null;
            if (municipioId is null) semMunicipio++;
            if (l.CustoTotalHa is null) semTotal++;

            var valores = new CustoDeProducao.Valores(
                l.MesDoRelatorio, l.Produtividade, l.UnidadeDaProdutividade,
                l.CustoVariavelHa, l.CustoVariavelUnidade, l.CustoFixoHa, l.CustoFixoUnidade,
                l.CustoOperacionalHa, l.CustoOperacionalUnidade, l.RendaDeFatoresHa, l.RendaDeFatoresUnidade,
                l.CustoTotalHa, l.CustoTotalUnidade);

            try
            {
                if (existentes.TryGetValue((l.Cultura, l.Aba), out var existente))
                {
                    if (existente.Revisar(municipioId, valores, usuarioId, agora)) revisadas++;
                    else mantidas++;
                }
                else
                {
                    novas.Add(CustoDeProducao.Registrar(
                        l.Cultura, l.Aba, l.Local, l.Variante, municipioId, l.Safra,
                        planilha.UnidadeComercial, valores, usuarioId, agora));
                }
            }
            catch (RegraDeNegocioViolada regra)
            {
                recusas.Add((l, regra.Message));
            }
        }

        contexto.CustosDeProducao.AddRange(novas);
        dePara.Gravar(contexto);
        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, Fluxo, recusas, ct);
        await contexto.SaveChangesAsync(ct);

        var safras = lidas.Select(x => x.Linha.Safra).ToList();
        var periodo = safras.Count == 0 ? "sem linhas" : $"safras {safras.Min()} a {safras.Max()}";
        await CargaDeTerritorio.RegistrarRodadaAsync(
            contexto, sistemaId, Fluxo, lidas.Count, novas.Count + revisadas, recusas.Count, ct, periodo);
        await transacao.CommitAsync(ct);

        Contar($"abas lidas ({periodo})", lidas.Count);
        Contar("abas novas", novas.Count);
        Contar("abas revisadas pela CONAB (valor anterior na trilha)", revisadas);
        Contar("abas mantidas sem mudança", mantidas);
        Contar("abas sem custo total (a CONAB parou no operacional)", semTotal);
        Contar("abas cujo local não casou com um município do catálogo", semMunicipio);
        Contar("abas resolvidas pelo de-para já gravado", dePara.CasadosPelaCorrespondencia);
        Contar("correspondências novas gravadas (casadas por nome)", dePara.CasadosPorNome);
        Contar("abas recusadas", recusas.Count);

        return _contagens;
    }

    private void Contar(string rotulo, int valor) => _contagens.Add((Etapa, rotulo, valor));
}
