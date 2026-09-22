using Tracbel.Crm.Dominio.Integracao;

namespace Tracbel.Crm.Dominio.Organizacao;

// O CALENDÁRIO DAS FONTES SAIU DAQUI (issue 136, 22/09/2026). Até então ele morava em RotinaDaFonte, repetido
// do scripts/deploy/registrar-rotinas.ps1 e amarrado a ele por um teste. Agora a agenda é da rotina, no banco
// (integracao.Rotina), editável pela tela; aqui fica só QUAL rotina carrega cada fonte.

/// <summary>A situação de uma fonte no painel do administrador.</summary>
public enum SituacaoDaFonte
{
    /// <summary>Rodou depois da última execução agendada.</summary>
    EmDia = 0,

    /// <summary>A última execução agendada passou e a fonte não foi atualizada depois dela.</summary>
    Atrasada = 1,

    /// <summary>A tabela da fonte está vazia no banco.</summary>
    SemDado = 2
}

/// <summary>
/// UMA FONTE PÚBLICA QUE O CRM CARREGA SOZINHO — um fluxo da carga, a tabela que ele grava e a rotina que o
/// roda (issue 77, painel de fontes; a aba "Controle de Fontes" da pasta 360).
/// </summary>
/// <param name="Fluxo">O nome do fluxo, o mesmo do <c>integracao.PontoDeSincronismo</c> e da trava da carga.</param>
/// <param name="Nome">O nome que a tela mostra.</param>
/// <param name="Orgao">Quem publica.</param>
/// <param name="OQueTraz">O dado, em uma frase.</param>
/// <param name="Tabela">A tabela do CRM que o fluxo grava.</param>
/// <param name="Rotina">O código da rotina que o carrega (<see cref="RotinasDoSistema"/>).</param>
/// <param name="EhMunicipal">Se o dado é por município — e então a cobertura da ADR faz sentido.</param>
public sealed record FontePublica(
    string Fluxo, string Nome, string Orgao, string OQueTraz, string Tabela, string Rotina, bool EhMunicipal)
{
    /// <summary>
    /// A MARGEM ENTRE A HORA AGENDADA E A COBRANÇA. A rotina começa às 3h ou às 4h e leva de segundos a uma hora
    /// (a PAM). Sem margem, a fonte apareceria atrasada durante a própria execução.
    /// </summary>
    public static readonly TimeSpan Margem = TimeSpan.FromHours(6);

    /// <summary>
    /// A situação da fonte num instante, com a frase que a explica.
    ///
    /// <para><b>Sem dado vem primeiro:</b> tabela vazia é o caso mais grave e o mais comum logo depois de uma
    /// publicação — foi o dos preços no servidor em 21/09/2026.</para>
    ///
    /// <para><b>Rotina desligada não é "atrasada por falha"</b>: a frase diz que ela está desligada e onde religar.</para>
    /// </summary>
    /// <param name="ultimaAtualizacaoEm">A última rodada bem-sucedida do fluxo; nula quando nunca rodou.</param>
    /// <param name="linhas">Quantas linhas a tabela tem.</param>
    /// <param name="agoraUtc">O instante, em UTC.</param>
    /// <param name="agenda">A agenda da rotina, lida do banco.</param>
    /// <param name="nomeDaRotina">O nome da rotina, para a frase.</param>
    /// <param name="ligada">Se a rotina está ligada.</param>
    public (SituacaoDaFonte Situacao, string Motivo) SituacaoEm(
        DateTime? ultimaAtualizacaoEm, int linhas, DateTime agoraUtc, AgendaDaRotina agenda, string nomeDaRotina, bool ligada)
    {
        if (linhas == 0)
            return (SituacaoDaFonte.SemDado,
                !ligada
                    ? $"A tabela está vazia e a rotina \"{nomeDaRotina}\" está desligada. Religue-a em Configurações › Integrações, ou peça \"Rodar agora\"."
                    : ultimaAtualizacaoEm is null
                        ? $"A tabela está vazia e o fluxo nunca rodou neste banco. A rotina \"{nomeDaRotina}\" carrega na próxima volta do orquestrador — ou peça \"Rodar agora\"."
                        : $"A tabela está vazia, embora o fluxo tenha rodado. Veja as recusas e o histórico da rotina \"{nomeDaRotina}\".");

        var devia = agenda.UltimaPrevistaAte(agoraUtc - Margem);

        if (ultimaAtualizacaoEm is null || ultimaAtualizacaoEm < devia)
            return (SituacaoDaFonte.Atrasada, !ligada
                ? $"A rotina \"{nomeDaRotina}\" está desligada, e o fluxo não foi atualizado desde a execução que a agenda previa para {devia.AddHours(-3):dd/MM/yyyy}."
                : $"A rotina \"{nomeDaRotina}\" devia ter rodado em {devia.AddHours(-3):dd/MM/yyyy} e o fluxo não foi atualizado depois disso. " +
                  "Uma rodada que falha não grava nada: veja o histórico da rotina em Configurações › Integrações.");

        return (SituacaoDaFonte.EmDia, "Atualizada depois da última execução agendada.");
    }
}
/// <summary>
/// AS FONTES PÚBLICAS DO POTENCIAL — o catálogo que o painel do administrador percorre (issue 77).
///
/// <para><b>Cada fluxo aqui existe na carga</b>, com o mesmo nome: <c>FontesPublicasTestes</c> procura cada um no
/// código de <c>Tracbel.Crm.Carga</c>. Fonte nova entra na carga e aqui no mesmo commit.</para>
/// </summary>
public static class FontesPublicas
{
    private const string Anual = RotinasDoSistema.FontesAnuais;
    private const string Mensal = RotinasDoSistema.PrecosMensais;

    /// <summary>As fontes, na ordem em que o painel as mostra.</summary>
    public static readonly IReadOnlyList<FontePublica> Todas =
    [
        new("IBGE.PRODUCAO_AGRICOLA", "Produção Agrícola Municipal", "IBGE (SIDRA 5457)",
            "Área plantada e colhida, quantidade e valor da produção por cultura e município, em três anos.",
            "organizacao.ProducaoAgricolaNoMunicipio", Anual, EhMunicipal: true),
        new("IBGE.FROTA_DE_TRATORES", "Frota de tratores", "IBGE — Censo Agropecuário (SIDRA 6871)",
            "Tratores e estabelecimentos com trator, por faixa de potência.",
            "organizacao.FrotaDeTratoresNoMunicipio", Anual, EhMunicipal: true),
        new("IBGE.ESTABELECIMENTOS_POR_AREA", "Estabelecimentos por área", "IBGE — Censo Agropecuário (SIDRA 6780)",
            "Propriedades por grupo de área total.",
            "organizacao.EstabelecimentosPorAreaNoMunicipio", Anual, EhMunicipal: true),
        new("IBGE.REBANHO", "Rebanho bovino", "IBGE — Pesquisa da Pecuária Municipal (SIDRA 3939)",
            "Efetivo do rebanho, anual.",
            "organizacao.RebanhoNoMunicipio", Anual, EhMunicipal: true),
        new("IBGE.AREA_TERRITORIAL", "Área territorial", "IBGE (SIDRA 4714)",
            "Área do município em km².",
            "organizacao.AreaTerritorialDoMunicipio", Anual, EhMunicipal: true),
        new("ANP.USINA_DE_ETANOL", "Usinas de etanol", "ANP — dados abertos",
            "Usinas autorizadas, com CNPJ, município e capacidade de produção.",
            "organizacao.UsinaDeEtanol", Anual, EhMunicipal: false),
        new("CONAB.PRECO_RECEBIDO", "Preço recebido pelo produtor", "CONAB",
            "Preço mensal das culturas em SP. A CONAB publica só os últimos 12 meses; o CRM acumula a série.",
            "organizacao.CotacaoDeProduto", Mensal, EhMunicipal: false),
        new("SOCICANA.PRECO_DO_ATR", "Preço do kg de ATR", "Socicana",
            "Preço da cana (kg de ATR), mensal e acumulado da safra.",
            "organizacao.CotacaoDeProduto", Mensal, EhMunicipal: false),
        new("BCB.PTAX_MENSAL", "Dólar PTAX", "Banco Central (SGS 3698)",
            "Média mensal do dólar de venda, para converter os preços.",
            "organizacao.CotacaoDoDolar", Mensal, EhMunicipal: false),
        new("CONAB.CUSTO_DE_PRODUCAO", "Custo de produção", "CONAB — séries históricas",
            "Custo variável, fixo, operacional e total por hectare e por unidade, por cultura, local e safra.",
            "organizacao.CustoDeProducao", Mensal, EhMunicipal: false),
        new("BCB.SICOR_INVESTIMENTO", "Crédito rural de investimento", "Banco Central — SICOR",
            "Valor financiado por município, mês e produto, desde 2013.",
            "organizacao.CreditoRuralDeInvestimento", Mensal, EhMunicipal: true),
        new("BCB.SICOR_TABELAS", "Tabelas auxiliares do SICOR", "Banco Central — SICOR",
            "Programa, subprograma, fonte e produto, para ler o crédito.",
            "organizacao.ItemDoSicor", Mensal, EhMunicipal: false)
    ];
}
