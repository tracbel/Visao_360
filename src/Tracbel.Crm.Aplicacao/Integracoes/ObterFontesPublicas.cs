using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Integracoes;

/// <summary>Uma fonte pública no painel do administrador.</summary>
/// <param name="Fluxo">O fluxo da carga.</param>
/// <param name="Nome">O nome da fonte.</param>
/// <param name="Orgao">Quem publica.</param>
/// <param name="OQueTraz">O dado, em uma frase.</param>
/// <param name="Tabela">A tabela do CRM.</param>
/// <param name="Rotina">A tarefa agendada que a roda.</param>
/// <param name="Cadencia">Anual ou Mensal.</param>
/// <param name="Situacao">EmDia, Atrasada ou SemDado.</param>
/// <param name="Motivo">A frase que explica a situação.</param>
/// <param name="UltimaAtualizacaoEm">A última rodada bem-sucedida (UTC).</param>
/// <param name="UltimaExecucaoPrevistaEm">Quando a rotina devia ter rodado pela última vez (UTC).</param>
/// <param name="ProximaExecucaoEm">A próxima execução agendada (UTC).</param>
/// <param name="Linhas">Linhas da fonte no banco.</param>
/// <param name="PeriodoInicial">O primeiro período com dado.</param>
/// <param name="PeriodoFinal">O último período com dado.</param>
/// <param name="MunicipiosCobertos">Municípios da ADR com dado; nulo quando a fonte não é por município.</param>
/// <param name="RegistrosLidos">Linhas lidas na última rodada.</param>
/// <param name="RegistrosGravados">Linhas gravadas ou alteradas na última rodada.</param>
/// <param name="Recusados">Linhas recusadas na última rodada.</param>
/// <param name="RecusasPendentes">Recusas guardadas para revisão.</param>
/// <param name="ExemplosDeRecusa">Até três motivos de recusa.</param>
public sealed record FontePublicaResumo(
    string Fluxo,
    string Nome,
    string Orgao,
    string OQueTraz,
    string Tabela,
    string Rotina,
    string Cadencia,
    string Situacao,
    string Motivo,
    DateTime? UltimaAtualizacaoEm,
    DateTime UltimaExecucaoPrevistaEm,
    DateTime ProximaExecucaoEm,
    int Linhas,
    string? PeriodoInicial,
    string? PeriodoFinal,
    int? MunicipiosCobertos,
    int RegistrosLidos,
    int RegistrosGravados,
    int Recusados,
    int RecusasPendentes,
    IReadOnlyList<string> ExemplosDeRecusa);

/// <summary>O painel de fontes públicas.</summary>
/// <param name="MunicipiosDaAdr">O denominador da cobertura: os municípios vigentes da ADR.</param>
/// <param name="Atrasadas">Quantas fontes estão atrasadas.</param>
/// <param name="SemDado">Quantas estão sem dado.</param>
/// <param name="Fontes">As fontes, na ordem do catálogo.</param>
public sealed record PainelDeFontesPublicas(int MunicipiosDaAdr, int Atrasadas, int SemDado, IReadOnlyList<FontePublicaResumo> Fontes);

/// <summary>
/// O PAINEL DE FONTES PÚBLICAS (issue 77): para cada fonte, quando rodou pela última vez, que período e que
/// municípios o banco cobre, o que foi recusado, quando roda de novo — e se está atrasada.
///
/// <para>Espelha a aba "Controle de Fontes" da pasta 360, com uma diferença: lá a data era digitada; aqui é a
/// rodada que a própria carga gravou no banco.</para>
/// </summary>
public sealed class ObterFontesPublicas(
    IRepositorioDeFontesPublicas repositorio, IRepositorioDeOpcoesDosParametros opcoes, IRelogio relogio)
{
    /// <summary>Lê o painel.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PainelDeFontesPublicas>>> ExecutarAsync(CancellationToken ct)
    {
        var agora = relogio.Agora;
        var catalogo = FontesPublicas.Todas;
        var estados = (await repositorio.LerAsync(catalogo, ct)).ToDictionary(e => e.Fluxo, StringComparer.Ordinal);

        var fontes = catalogo.Select(fonte =>
        {
            var estado = estados[fonte.Fluxo];
            var (situacao, motivo) = fonte.SituacaoEm(estado.UltimaAtualizacaoEm, estado.Linhas, agora);

            return new FontePublicaResumo(
                fonte.Fluxo, fonte.Nome, fonte.Orgao, fonte.OQueTraz, fonte.Tabela, fonte.Rotina.Tarefa,
                fonte.Rotina.Cadencia.ToString(), situacao.ToString(), motivo,
                estado.UltimaAtualizacaoEm,
                fonte.Rotina.UltimaPrevistaAte(agora),
                fonte.Rotina.ProximaDepoisDe(agora),
                estado.Linhas, estado.PeriodoInicial, estado.PeriodoFinal, estado.MunicipiosCobertos,
                estado.RegistrosLidos, estado.RegistrosGravados, estado.Recusados, estado.RecusasPendentes,
                estado.ExemplosDeRecusa);
        }).ToList();

        var painel = new PainelDeFontesPublicas(
            await opcoes.ContarMunicipiosDaAdrAsync(ct),
            fontes.Count(f => f.Situacao == nameof(SituacaoDaFonte.Atrasada)),
            fontes.Count(f => f.Situacao == nameof(SituacaoDaFonte.SemDado)),
            fontes);

        return Resultado<ComProcedencia<PainelDeFontesPublicas>>.Ok(
            ComProcedencia<PainelDeFontesPublicas>.DoNossoBanco(
                painel, "integracao.PontoDeSincronismo · integracao.MensagemDescartada · as tabelas de cada fonte", relogio));
    }
}
