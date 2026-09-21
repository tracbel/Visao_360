namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>Com que frequência a rotina de uma fonte roda no servidor.</summary>
public enum CadenciaDaRotina
{
    /// <summary>Uma vez por ano, num mês e dia fixos.</summary>
    Anual = 0,

    /// <summary>Todo mês, num dia fixo.</summary>
    Mensal = 1
}

/// <summary>
/// UMA ROTINA AGENDADA DO SERVIDOR — a tarefa do Windows que roda a carga das fontes públicas (issue 77).
///
/// <para><b>O calendário mora em dois lugares, e um teste os mantém iguais.</b> Quem agenda de verdade é o
/// <c>scripts/deploy/registrar-rotinas.ps1</c>; aqui ele é repetido para o painel dizer quando cada fonte
/// devia ter rodado e quando roda de novo. <c>RotinasDasFontesTestes</c> lê o script e compara — mudar um sem
/// o outro quebra o build, e não o painel em silêncio.</para>
///
/// <para><b>O horário é o de São Paulo</b>, que é o do servidor: UTC−3 o ano inteiro, sem horário de verão
/// desde 2019 (a mesma conta de <see cref="ParametroComVigencia.HojeNoBrasil"/>).</para>
/// </summary>
/// <param name="Tarefa">O nome da tarefa agendada.</param>
/// <param name="Cadencia">Anual ou mensal.</param>
/// <param name="Mes">O mês, na anual; nulo na mensal.</param>
/// <param name="Dia">O dia do mês.</param>
/// <param name="Hora">A hora, no horário de São Paulo.</param>
public sealed record RotinaDaFonte(string Tarefa, CadenciaDaRotina Cadencia, int? Mes, int Dia, TimeOnly Hora)
{
    private static readonly TimeSpan FusoDeSaoPaulo = TimeSpan.FromHours(-3);

    /// <summary>
    /// A MARGEM ENTRE A HORA AGENDADA E A COBRANÇA. A rotina começa às 3h ou às 4h e leva de segundos a uma hora
    /// (a PAM). Sem margem, a fonte apareceria atrasada durante a própria execução.
    /// </summary>
    public static readonly TimeSpan Margem = TimeSpan.FromHours(6);

    /// <summary>A execução agendada mais recente até o instante (UTC).</summary>
    /// <param name="agoraUtc">O instante, em UTC.</param>
    public DateTime UltimaPrevistaAte(DateTime agoraUtc)
    {
        var local = agoraUtc + FusoDeSaoPaulo;
        var candidata = Agendada(local.Year, local.Month);
        if (candidata > local) candidata = Cadencia == CadenciaDaRotina.Mensal
            ? Agendada(local.AddMonths(-1).Year, local.AddMonths(-1).Month)
            : Agendada(local.Year - 1, local.Month);
        return DateTime.SpecifyKind(candidata - FusoDeSaoPaulo, DateTimeKind.Utc);
    }

    /// <summary>A próxima execução agendada depois do instante (UTC).</summary>
    /// <param name="agoraUtc">O instante, em UTC.</param>
    public DateTime ProximaDepoisDe(DateTime agoraUtc)
    {
        var local = agoraUtc + FusoDeSaoPaulo;
        var candidata = Agendada(local.Year, local.Month);
        if (candidata <= local) candidata = Cadencia == CadenciaDaRotina.Mensal
            ? Agendada(local.AddMonths(1).Year, local.AddMonths(1).Month)
            : Agendada(local.Year + 1, local.Month);
        return DateTime.SpecifyKind(candidata - FusoDeSaoPaulo, DateTimeKind.Utc);
    }

    // Na anual o mês de referência é ignorado: vale o mês da rotina.
    private DateTime Agendada(int ano, int mesDeReferencia) =>
        new DateOnly(ano, Mes ?? mesDeReferencia, Dia).ToDateTime(Hora);
}

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
/// <param name="Rotina">A rotina agendada que o roda.</param>
/// <param name="EhMunicipal">Se o dado é por município — e então a cobertura da ADR faz sentido.</param>
public sealed record FontePublica(
    string Fluxo, string Nome, string Orgao, string OQueTraz, string Tabela, RotinaDaFonte Rotina, bool EhMunicipal)
{
    /// <summary>
    /// A situação da fonte num instante, com a frase que a explica.
    ///
    /// <para><b>Sem dado vem primeiro:</b> tabela vazia é o caso mais grave e o mais comum logo depois de uma
    /// publicação — foi o dos preços no servidor em 21/09/2026.</para>
    /// </summary>
    /// <param name="ultimaAtualizacaoEm">A última rodada bem-sucedida do fluxo; nula quando nunca rodou.</param>
    /// <param name="linhas">Quantas linhas a tabela tem.</param>
    /// <param name="agoraUtc">O instante, em UTC.</param>
    public (SituacaoDaFonte Situacao, string Motivo) SituacaoEm(DateTime? ultimaAtualizacaoEm, int linhas, DateTime agoraUtc)
    {
        if (linhas == 0)
            return (SituacaoDaFonte.SemDado,
                ultimaAtualizacaoEm is null
                    ? $"A tabela está vazia e o fluxo nunca rodou neste banco. A rotina {Rotina.Tarefa} carrega na próxima execução — ou dispare-a agora."
                    : $"A tabela está vazia, embora o fluxo tenha rodado. Veja as recusas e o log da rotina {Rotina.Tarefa}.");

        var devia = Rotina.UltimaPrevistaAte(agoraUtc - RotinaDaFonte.Margem);

        if (ultimaAtualizacaoEm is null || ultimaAtualizacaoEm < devia)
            return (SituacaoDaFonte.Atrasada,
                $"A rotina {Rotina.Tarefa} devia ter rodado em {devia.AddHours(-3):dd/MM/yyyy} e o fluxo não foi atualizado depois disso. " +
                "Uma rodada que falha não grava nada: veja o log da rotina no servidor.");

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
    /// <summary>A rotina anual: a PAM e a estrutura agropecuária, em 1 de outubro às 3h.</summary>
    public static readonly RotinaDaFonte Anual = new("TracbelCrmFontesPublicas", CadenciaDaRotina.Anual, 10, 1, new TimeOnly(3, 0));

    /// <summary>A rotina mensal: preços, dólar, custos e crédito, no dia 20 às 4h.</summary>
    public static readonly RotinaDaFonte Mensal = new("TracbelCrmPrecos", CadenciaDaRotina.Mensal, null, 20, new TimeOnly(4, 0));

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
