using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Integracao.Vortice;

/// <summary>Um processo 31/41/50 do Vórtice, como a leitura da onda 2 o traz.</summary>
/// <param name="Numero">O número (<c>IV_PROCDADO.Processo</c>).</param>
/// <param name="Tipo">O tipo (<c>CodProcesso</c>): 31, 41 ou 50.</param>
/// <param name="NroEmpresa">A filial do processo no Vórtice — o <c>NN</c> do código <c>0101NN</c> do CRM.</param>
/// <param name="SeqPessoa">A pessoa do processo.</param>
/// <param name="Documento">O documento da pessoa, recomposto e conferido.</param>
/// <param name="SeqCarteira">A carteira da pessoa no departamento MAQ-NOVOS, quando há.</param>
/// <param name="LoginDoResponsavel">O login do responsável (<c>UsuResponsavel</c>) — só para achar a conta, nunca gravado.</param>
/// <param name="IncluidoEmUtc">A inclusão (UTC).</param>
/// <param name="PrimeiroAndamentoEmUtc">O primeiro registro do histórico do processo (UTC).</param>
/// <param name="Status">O status do processo na origem.</param>
/// <param name="RealizadoEmUtc">Quando o processo foi encerrado na origem (UTC).</param>
/// <param name="Fase">A fase do BPM (<c>IV_PROCESSO.Fase</c>), como a origem escreve.</param>
/// <param name="FaseOrdem">A posição da fase no fluxo.</param>
/// <param name="FaseDesdeUtc">Desde quando está na fase (UTC).</param>
/// <param name="StatusDesdeUtc">Desde quando está no status (UTC).</param>
/// <param name="Resumo">O resumo — o ÚNICO texto livre que entra (decisão P1), como título.</param>
/// <param name="Valor">O valor do negócio.</param>
/// <param name="Quantidade">A quantidade negociada.</param>
/// <param name="PrevisaoConclusao">A previsão de conclusão vigente (data de São Paulo).</param>
/// <param name="PrevisaoConclusaoOriginal">A primeira previsão registrada (data de São Paulo).</param>
public sealed record ProcessoDaOportunidadeNoVortice(
    long Numero, short Tipo, int? NroEmpresa, long? SeqPessoa, DocumentoDoVortice Documento, int? SeqCarteira,
    string? LoginDoResponsavel, DateTime? IncluidoEmUtc, DateTime? PrimeiroAndamentoEmUtc, string? Status, DateTime? RealizadoEmUtc,
    string? Fase, int? FaseOrdem, DateTime? FaseDesdeUtc, DateTime? StatusDesdeUtc, string? Resumo, decimal? Valor, decimal? Quantidade,
    DateOnly? PrevisaoConclusao, DateOnly? PrevisaoConclusaoOriginal);

/// <summary>Uma linha da agenda de um processo 31/41/50, com a conclusão procurada nos dois sentidos.</summary>
/// <param name="SeqAgenda">O identificador da agenda.</param>
/// <param name="Processo">O processo.</param>
/// <param name="SeqUsuario">O responsável.</param>
/// <param name="Acao">A ação.</param>
/// <param name="AgendadaParaUtc">Quando está agendada (UTC).</param>
/// <param name="PrazoLimiteUtc">O prazo limite (UTC).</param>
/// <param name="Prioridade">A prioridade, como a origem escreve.</param>
/// <param name="Realizada">A letra de realizada (<c>S</c>/<c>N</c>).</param>
/// <param name="RealizadaEmUtc">Quando foi realizada (UTC) — a da agenda, senão a do histórico que a concluiu.</param>
/// <param name="Resultado">O desfecho — o da agenda, senão o do histórico que a concluiu.</param>
/// <param name="SeqHistoricoDeConclusao">O histórico que a concluiu: o ponteiro de ida, senão o último que aponta de volta.</param>
/// <param name="SeqHistoricoDeOrigem">O histórico que gerou a agenda.</param>
/// <param name="SeqUsuarioQueConcluiu">Quem concluiu — o autor do histórico de conclusão.</param>
/// <param name="TipoAgendamento">A letra do agendamento: <c>A</c> automático, <c>M</c> manual.</param>
/// <param name="GeradaEmUtc">Quando a agenda nasceu (UTC).</param>
public sealed record TarefaDaOportunidadeNoVortice(
    long SeqAgenda, long Processo, long? SeqUsuario, int? Acao, DateTime? AgendadaParaUtc, DateTime? PrazoLimiteUtc, decimal? Prioridade,
    string? Realizada, DateTime? RealizadaEmUtc, int? Resultado, long? SeqHistoricoDeConclusao, long? SeqHistoricoDeOrigem,
    long? SeqUsuarioQueConcluiu, string? TipoAgendamento, DateTime? GeradaEmUtc);

/// <summary>Uma linha do histórico de um processo 31/41/50 — sem texto livre nenhum.</summary>
/// <param name="SeqHistorico">O identificador da linha.</param>
/// <param name="Processo">O processo.</param>
/// <param name="SeqUsuario">O autor.</param>
/// <param name="AcaoGeradora">A ação que gerou a linha.</param>
/// <param name="Resultado">O código de resultado.</param>
/// <param name="SeqAgendaDeOrigem">A agenda que esta linha concluiu (<c>AgendaOrigem</c>).</param>
/// <param name="Natureza">A letra de natureza (<c>A</c> ativa, <c>R</c> receptiva).</param>
/// <param name="DeSistema">Se quem incluiu é um dos autores de sistema — o nome de quem incluiu nunca sai da consulta.</param>
/// <param name="RealizadaEmUtc">Quando (UTC).</param>
/// <param name="Duracao">A duração, em minutos — zero em quase todo o histórico.</param>
/// <param name="Latitude">A latitude do atendimento.</param>
/// <param name="Longitude">A longitude do atendimento.</param>
public sealed record InteracaoDaOportunidadeNoVortice(
    long SeqHistorico, long Processo, long? SeqUsuario, int? AcaoGeradora, int? Resultado, long? SeqAgendaDeOrigem, string? Natureza,
    bool DeSistema, DateTime? RealizadaEmUtc, decimal? Duracao, decimal? Latitude, decimal? Longitude);

/// <summary>Um tipo de processo do Vórtice (<c>IV_CodProcesso</c>).</summary>
/// <param name="Codigo">O <c>CodProcesso</c>.</param>
/// <param name="Descricao">O nome.</param>
/// <param name="DescricaoReduzida">O nome curto.</param>
/// <param name="EmUso">Se está em uso na origem.</param>
public sealed record TipoDeProcessoNoVortice(int Codigo, string? Descricao, string? DescricaoReduzida, bool EmUso);

/// <summary>Uma ação do Vórtice (<c>IV_Acao</c>) — o tipo de tarefa.</summary>
/// <param name="Codigo">A ação.</param>
/// <param name="Descricao">O nome.</param>
/// <param name="DescricaoReduzida">O nome curto.</param>
/// <param name="EmUso">Se está em uso na origem.</param>
/// <param name="PrazoEmDias">O prazo de realização, quando a origem o declara.</param>
public sealed record AcaoNoVortice(int Codigo, string? Descricao, string? DescricaoReduzida, bool EmUso, decimal? PrazoEmDias);

/// <summary>Um resultado do Vórtice (<c>IV_Resultado</c>) — o desfecho.</summary>
/// <param name="Codigo">O resultado.</param>
/// <param name="Acao">A ação dona do resultado.</param>
/// <param name="Descricao">O nome.</param>
/// <param name="DescricaoReduzida">O nome curto.</param>
public sealed record ResultadoNoVortice(int Codigo, int? Acao, string? Descricao, string? DescricaoReduzida);

/// <summary>O que o resultado faz com o processo 31/41/50, pelo <c>IV_ProcResultado</c>.</summary>
/// <param name="Resultado">O resultado.</param>
/// <param name="MoveFase">Se algum mapeamento dele aponta fase.</param>
/// <param name="StatusDeDestino">O status que o mapeamento grava, quando há.</param>
public sealed record EfeitoDoResultadoNoVortice(int Resultado, bool MoveFase, string? StatusDeDestino);

/// <summary>O login de um usuário do Vórtice — só para achar a conta do CRM, nunca gravado.</summary>
/// <param name="SeqUsuario">O usuário.</param>
/// <param name="Login">O <c>CodUsuario</c>.</param>
public sealed record LoginNoVortice(long SeqUsuario, string? Login);

/// <summary>Tudo o que a onda 2 lê do Vórtice, numa sessão só.</summary>
/// <param name="Processos">Os processos 31/41/50 da janela — todos, casados ou não: quem casa é decidido no CRM.</param>
/// <param name="Tarefas">A agenda desses tipos na janela.</param>
/// <param name="Interacoes">O histórico desses tipos na janela.</param>
/// <param name="TiposDeProcesso">Os tipos 31, 41 e 50.</param>
/// <param name="Acoes">As ações usadas pela agenda, pelo histórico e pelos resultados.</param>
/// <param name="Resultados">Os resultados usados pela agenda e pelo histórico.</param>
/// <param name="Efeitos">O efeito de cada resultado nos processos 31/41/50.</param>
/// <param name="Logins">Os logins dos usuários da agenda e do histórico.</param>
public sealed record LeituraDasOportunidadesDoVortice(
    IReadOnlyList<ProcessoDaOportunidadeNoVortice> Processos,
    IReadOnlyList<TarefaDaOportunidadeNoVortice> Tarefas,
    IReadOnlyList<InteracaoDaOportunidadeNoVortice> Interacoes,
    IReadOnlyList<TipoDeProcessoNoVortice> TiposDeProcesso,
    IReadOnlyList<AcaoNoVortice> Acoes,
    IReadOnlyList<ResultadoNoVortice> Resultados,
    IReadOnlyList<EfeitoDoResultadoNoVortice> Efeitos,
    IReadOnlyList<LoginNoVortice> Logins);

/// <summary>
/// A LEITURA DA ONDA 2 NO VÓRTICE — processo, agenda e histórico dos processos 31/41/50 desde 01/11/2023 (decisões de
/// 27/09/2026, documento 52 §12). O molde é o <see cref="LeitorDoFunilDoVortice"/>; a carga legada
/// (<c>LeitorDeCargaDoVortice</c>) fica intocada, e as regras que vêm dela estão citadas onde entram.
///
/// <para><b>SÓ O RESUMO ENTRA (decisão P1).</b> Nenhuma consulta pronuncia a <c>Descricao</c> do processo, o
/// <c>Assunto</c>, o <c>AssuntoCmpl</c> e o <c>Detalhe</c> da agenda, o <c>Detalhe</c> e o <c>ResultadoCmpl</c> do
/// histórico, nem as colunas de nome de pessoa (<c>Contato</c>, <c>Vendedor</c>). O <c>USUINCLUSAO</c> aparece só dentro
/// de um <c>CASE</c> que responde "foi o sistema?" — o login de quem incluiu nunca sai. O <c>CodUsuario</c> é lido por
/// lista, só para achar a conta do CRM, e não é gravado.</para>
///
/// <para><b>SÓ LEITURA.</b> A conexão declara <c>ApplicationIntent=ReadOnly</c> e toda consulta é <c>SELECT</c> com
/// <c>NOLOCK</c>. Os catálogos vêm por lista de inteiros — só os códigos usados, em fatias de 1.000. O filtro "casado"
/// é do CRM, em memória: a leitura traz os processos da janela, casados ou não.</para>
/// </summary>
/// <param name="opcoes">A configuração do Vórtice — a cadeia vem de <c>Vortice__Conexao</c> ou da tela.</param>
public sealed class LeitorDasOportunidadesDoVortice(OpcoesDoVortice opcoes)
{
    /// <summary>
    /// OS NOMES QUE A ORIGEM USA PARA DIZER "ISTO NÃO FOI GENTE QUE ESCREVEU" — os mesmos da carga antiga
    /// (<c>LeitorDeCargaDoVortice.AutoresDeSistema</c>). Vão como parâmetro, e não escritos na consulta.
    /// </summary>
    public static readonly IReadOnlyList<string> AutoresDeSistema = ["VRTCSERVER", "RD", "* Indefinido *", "IMPORT", "PTF v4.01"];

    /// <summary>Quantos códigos vão numa consulta por lista.</summary>
    private const int CodigosPorConsulta = 1_000;

    /// <summary>O teto de cada consulta: a agenda e o histórico da janela são as maiores leituras do Vórtice que o CRM faz.</summary>
    private const int TempoLimiteDaConsulta = 600;

    /// <summary>O Vórtice grava hora de São Paulo, sem fuso; o Brasil não tem horário de verão desde 2019.</summary>
    private const int HorasDeDiferencaParaUtc = 3;

    /// <summary>O começo da janela como o Vórtice escreve: 01/11/2023, 00:00 de São Paulo (<see cref="RegraDoEstagio.InicioDaJanela"/>).</summary>
    public static DateTime JanelaNaOrigem => DateTime.SpecifyKind(RegraDoEstagio.InicioDaJanela.AddHours(-HorasDeDiferencaParaUtc), DateTimeKind.Unspecified);

    /// <summary>
    /// OS PROCESSOS DA JANELA: os do funil (<see cref="LeitorDoFunilDoVortice.ConsultaDosProcessos"/>) mais a fase, o
    /// resumo, o valor, a quantidade e as previsões. A janela é a abertura — a inclusão ou, sem ela, o primeiro andamento
    /// — a partir de 01/11/2023; entra também quem tem a inclusão antiga (ou absurda) e o andamento na janela, e a regra
    /// da abertura crível decide no CRM, como no funil.
    /// </summary>
    public const string ConsultaDosProcessos = $"""
        SELECT d.Processo, d.CodProcesso, d.NroEmpresa, d.SeqPessoa,
               g.FisicaJuridica, g.NroCGCCPF, g.DigCGCCPF,
               c.SeqCarteira,
               p.UsuResponsavel, p.DtaInclusao, p.Status, p.DtaRealizacao, p.Fase, p.FaseOrdem, p.DtaFase, p.DtaStatus,
               p.Resumo, p.Valor, p.Qtde, p.DtaPrevConclusao, p.DtaPrevConcOrig,
               a.PrimeiroAndamento
        FROM IV_PROCDADO d WITH (NOLOCK)
        JOIN IV_PROCESSO p WITH (NOLOCK) ON p.Processo = d.Processo
        LEFT JOIN GE_Pessoa g WITH (NOLOCK) ON g.SeqPessoa = d.SeqPessoa
        OUTER APPLY (
            SELECT TOP 1 s.SeqCarteira
            FROM IVS_Pes s WITH (NOLOCK)
            JOIN IVS_Depto t WITH (NOLOCK) ON t.SeqDepto = s.SeqDepto
            WHERE s.SeqPessoa = d.SeqPessoa AND t.Depto = @depto AND s.SeqCarteira IS NOT NULL
            ORDER BY s.SeqCarteira) c
        LEFT JOIN (
            SELECT h.Processo, MIN(h.DtaRealizacao) AS PrimeiroAndamento
            FROM IV_HISTORICO h WITH (NOLOCK)
            WHERE h.CodProcesso IN ({LeitorDoFunilDoVortice.TiposDoFunil}) AND h.Processo IS NOT NULL
            GROUP BY h.Processo) a ON a.Processo = d.Processo
        WHERE d.CodProcesso IN ({LeitorDoFunilDoVortice.TiposDoFunil})
          AND (p.DtaInclusao >= @janela OR a.PrimeiroAndamento >= @janela)
        """;

    /// <summary>
    /// A AGENDA DOS PROCESSOS 31/41/50 NA JANELA — pelo processo: tarefa avulsa não entra (decisão P6). A conclusão é
    /// procurada nos dois sentidos, como na carga antiga (<c>LeitorDeCargaDoVortice.LerTarefasAsync</c>): o ponteiro de ida
    /// (<c>UltHistorico</c>) quase nunca é gravado, o de volta (<c>AgendaOrigem</c>) quase sempre, e usar só o de ida
    /// perderia 87% das conclusões.
    /// </summary>
    public const string ConsultaDaAgenda = $"""
        WITH Conclusao AS (
            SELECT h.AgendaOrigem AS SeqAgenda, MAX(h.SeqHistorico) AS SeqHistorico
            FROM IV_HISTORICO h WITH (NOLOCK)
            WHERE h.CodProcesso IN ({LeitorDoFunilDoVortice.TiposDoFunil}) AND h.AgendaOrigem > 0 AND h.DtaRealizacao >= @janela
            GROUP BY h.AgendaOrigem
        )
        SELECT a.SeqAgenda, a.Processo, a.SeqUsuario, a.Acao, a.DtaAgenda, a.DtaLimiteExecucao, a.Prioridade,
               a.Realizada, a.HistoricoOrigem, a.TipoAgendamento, a.DtaGeracao,
               COALESCE(NULLIF(a.UltResultado, 0), h.Resultado) AS UltResultado,
               COALESCE(NULLIF(a.UltHistorico, 0), c.SeqHistorico) AS UltHistorico,
               COALESCE(a.DtaRealizacao, h.DtaRealizacao) AS DtaRealizacao,
               h.SeqUsuario AS UsuarioQueConcluiu
        FROM IV_AGENDA a WITH (NOLOCK)
        JOIN IV_PROCDADO d WITH (NOLOCK) ON d.Processo = a.Processo
        LEFT JOIN Conclusao c ON c.SeqAgenda = a.SeqAgenda
        LEFT JOIN IV_HISTORICO h WITH (NOLOCK) ON h.SeqHistorico = COALESCE(NULLIF(a.UltHistorico, 0), c.SeqHistorico)
        WHERE d.CodProcesso IN ({LeitorDoFunilDoVortice.TiposDoFunil}) AND a.DtaAgenda >= @janela
        """;

    /// <summary>
    /// O HISTÓRICO DOS PROCESSOS 31/41/50 NA JANELA — sem texto livre. Se a linha é do sistema sai de um <c>CASE</c>
    /// sobre quem incluiu, com os nomes em parâmetro: o nome em si nunca sai.
    /// </summary>
    public static readonly string ConsultaDoHistorico = $"""
        SELECT h.SeqHistorico, h.Processo, h.SeqUsuario, h.AcaoGeradora, h.Resultado, h.AgendaOrigem, h.Natureza,
               h.DtaRealizacao, h.Duracao, h.Latitude, h.Longitude,
               CASE WHEN LTRIM(RTRIM(h.USUINCLUSAO)) IN ({string.Join(", ", AutoresDeSistema.Select((_, i) => $"@autor{i}"))}) THEN 1 ELSE 0 END AS DeSistema
        FROM IV_HISTORICO h WITH (NOLOCK)
        WHERE h.CodProcesso IN ({LeitorDoFunilDoVortice.TiposDoFunil}) AND h.Processo IS NOT NULL AND h.DtaRealizacao >= @janela
        """;

    /// <summary>Os tipos de processo 31, 41 e 50.</summary>
    public const string ConsultaDosTiposDeProcesso = $"""
        SELECT cp.CodProcesso, cp.Descricao, cp.DescrRed, cp.EmUso
        FROM IV_CodProcesso cp WITH (NOLOCK)
        WHERE cp.CodProcesso IN ({LeitorDoFunilDoVortice.TiposDoFunil})
        """;

    /// <summary>As ações da lista — nome, em uso e prazo.</summary>
    /// <param name="acoes">Os códigos.</param>
    public static string ConsultaDasAcoes(IEnumerable<int> acoes) => $"""
        SELECT ac.Acao, ac.Descricao, ac.DescReduzida, ac.EmUso, ac.PrazoRealizacao
        FROM IV_Acao ac WITH (NOLOCK)
        WHERE ac.Acao IN ({Lista(acoes)})
        """;

    /// <summary>Os resultados da lista — nome e ação dona.</summary>
    /// <param name="resultados">Os códigos.</param>
    public static string ConsultaDosResultados(IEnumerable<int> resultados) => $"""
        SELECT r.Resultado, r.Acao, r.Descricao, r.DescReduzida
        FROM IV_Resultado r WITH (NOLOCK)
        WHERE r.Resultado IN ({Lista(resultados)})
        """;

    /// <summary>
    /// O EFEITO DE CADA RESULTADO NOS PROCESSOS 31/41/50 — a mesma dedução da carga antiga
    /// (<c>LeitorDeCargaDoVortice.LerResultadosAsync</c>), restrita aos tipos da onda 2.
    /// </summary>
    /// <param name="resultados">Os códigos.</param>
    public static string ConsultaDosEfeitos(IEnumerable<int> resultados) => $"""
        SELECT pr.Resultado,
               MAX(CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(pr.FaseSeguinte, ''))), '') IS NOT NULL
                          OR NULLIF(LTRIM(RTRIM(ISNULL(pr.Fase, ''))), '') IS NOT NULL
                        THEN 1 ELSE 0 END) AS MoveFase,
               MAX(LTRIM(RTRIM(ISNULL(pr.Status, '')))) AS StatusDestino
        FROM IV_ProcResultado pr WITH (NOLOCK)
        WHERE pr.CodProcesso IN ({LeitorDoFunilDoVortice.TiposDoFunil}) AND pr.Resultado IN ({Lista(resultados)})
        GROUP BY pr.Resultado
        """;

    /// <summary>Os logins da lista de usuários — só para achar a conta do CRM.</summary>
    /// <param name="usuarios">Os <c>SeqUsuario</c>.</param>
    public static string ConsultaDosLogins(IEnumerable<long> usuarios) => $"""
        SELECT u.SeqUsuario, u.CodUsuario
        FROM GE_Usuario u WITH (NOLOCK)
        WHERE u.SeqUsuario IN ({string.Join(", ", usuarios.Distinct().Order().Select(c => c.ToString(CultureInfo.InvariantCulture)))})
        """;

    /// <summary>Lê processos, agenda, histórico e os catálogos que eles usam, numa sessão de leitura.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LeituraDasOportunidadesDoVortice>> LerAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(opcoes.Conexao))
            return Resultado<LeituraDasOportunidadesDoVortice>.Indisponivel(
                "A leitura das oportunidades exige a credencial do Vórtice: grave-a em Configurações › Integrações ou defina " +
                "Vortice__Conexao no servidor. Nada foi gravado.");

        if (LeituraDoVortice.CadeiaDeLeitura(opcoes.Conexao) is not { } cadeia)
            return Resultado<LeituraDasOportunidadesDoVortice>.Indisponivel(
                "A cadeia de conexão do Vórtice está malformada. Confira a credencial em Configurações › Integrações. Nada foi gravado.");

        var etapa = "a conexão";
        try
        {
            await using var conexao = new SqlConnection(cadeia);
            await conexao.OpenAsync(ct);

            etapa = "os processos";
            var processos = await LerAsync(conexao, ConsultaDosProcessos, MapearProcesso, ct,
                ("@janela", JanelaNaOrigem), ("@depto", LeitorDeCarteirasDoVortice.DepartamentoDeMaquinasNovas));

            etapa = "a agenda";
            var tarefas = await LerAsync(conexao, ConsultaDaAgenda, MapearTarefa, ct, ("@janela", JanelaNaOrigem));

            etapa = "o histórico";
            var parametrosDoHistorico = AutoresDeSistema.Select((autor, i) => ($"@autor{i}", (object)autor))
                .Prepend(("@janela", JanelaNaOrigem)).ToArray();
            var interacoes = await LerAsync(conexao, ConsultaDoHistorico, MapearInteracao, ct, parametrosDoHistorico);

            etapa = "os tipos de processo";
            var tipos = await LerAsync(conexao, ConsultaDosTiposDeProcesso, MapearTipoDeProcesso, ct);

            etapa = "os resultados";
            var codigosDeResultado = tarefas.Select(t => t.Resultado).Concat(interacoes.Select(i => i.Resultado))
                .OfType<int>().Where(c => c > 0).Distinct().ToList();
            var resultados = await PorFatiasAsync(conexao, codigosDeResultado, ConsultaDosResultados, MapearResultado, ct);
            var efeitos = await PorFatiasAsync(conexao, codigosDeResultado, ConsultaDosEfeitos, MapearEfeito, ct);

            etapa = "as ações";
            var codigosDeAcao = tarefas.Select(t => t.Acao).Concat(interacoes.Select(i => i.AcaoGeradora)).Concat(resultados.Select(r => r.Acao))
                .OfType<int>().Where(c => c > 0).Distinct().ToList();
            var acoes = await PorFatiasAsync(conexao, codigosDeAcao, ConsultaDasAcoes, MapearAcao, ct);

            etapa = "os logins";
            var usuarios = tarefas.SelectMany(t => new[] { t.SeqUsuario, t.SeqUsuarioQueConcluiu }).Concat(interacoes.Select(i => i.SeqUsuario))
                .OfType<long>().Where(u => u > 0).Distinct().ToList();
            var logins = new List<LoginNoVortice>(usuarios.Count);
            foreach (var fatia in usuarios.Chunk(CodigosPorConsulta))
                logins.AddRange(await LerAsync(conexao, ConsultaDosLogins(fatia), MapearLogin, ct));

            return Resultado<LeituraDasOportunidadesDoVortice>.Ok(
                new LeituraDasOportunidadesDoVortice(processos, tarefas, interacoes, tipos, acoes, resultados, efeitos, logins));
        }
        catch (SqlException falha)
        {
            // A MENSAGEM DO CONECTOR PODE CITAR SERVIDOR E USUÁRIO: sai só o código e a etapa.
            return Resultado<LeituraDasOportunidadesDoVortice>.Indisponivel(
                $"O Vórtice não respondeu à leitura de {etapa} (erro SQL {falha.Number}). Quase sempre é a rede ou a " +
                "credencial; a rotina é idempotente e pode rodar de novo. Nada foi gravado.");
        }
    }

    /// <summary>Uma linha da consulta dos processos, traduzida.</summary>
    /// <param name="linha">A linha.</param>
    public static ProcessoDaOportunidadeNoVortice MapearProcesso(IDataRecord linha) => new(
        LeituraDoVortice.Longo(linha, "Processo")!.Value,
        (short)(LeituraDoVortice.Numero(linha, "CodProcesso") ?? 0),
        (int?)LeituraDoVortice.Longo(linha, "NroEmpresa"),
        LeituraDoVortice.Longo(linha, "SeqPessoa"),
        DocumentoDoVortice.Recompor(
            LeituraDoVortice.Numero(linha, "NroCGCCPF"), LeituraDoVortice.Numero(linha, "DigCGCCPF"),
            LeituraDoVortice.Texto(linha, "FisicaJuridica")),
        (int?)LeituraDoVortice.Longo(linha, "SeqCarteira"),
        LeituraDoVortice.Texto(linha, "UsuResponsavel"),
        LeituraDoVortice.DataUtc(linha, "DtaInclusao", HorasDeDiferencaParaUtc),
        LeituraDoVortice.DataUtc(linha, "PrimeiroAndamento", HorasDeDiferencaParaUtc),
        LeituraDoVortice.Texto(linha, "Status"),
        LeituraDoVortice.DataUtc(linha, "DtaRealizacao", HorasDeDiferencaParaUtc),
        LeituraDoVortice.Texto(linha, "Fase"),
        (int?)LeituraDoVortice.Longo(linha, "FaseOrdem"),
        LeituraDoVortice.DataUtc(linha, "DtaFase", HorasDeDiferencaParaUtc),
        LeituraDoVortice.DataUtc(linha, "DtaStatus", HorasDeDiferencaParaUtc),
        LeituraDoVortice.Texto(linha, "Resumo"),
        LeituraDoVortice.Numero(linha, "Valor"),
        LeituraDoVortice.Numero(linha, "Qtde"),
        SomenteData(LeituraDoVortice.DataLocal(linha, "DtaPrevConclusao")),
        SomenteData(LeituraDoVortice.DataLocal(linha, "DtaPrevConcOrig")));

    /// <summary>Uma linha da consulta da agenda, traduzida. Código zero é "não há", como na carga antiga.</summary>
    /// <param name="linha">A linha.</param>
    public static TarefaDaOportunidadeNoVortice MapearTarefa(IDataRecord linha) => new(
        LeituraDoVortice.Longo(linha, "SeqAgenda")!.Value,
        LeituraDoVortice.Longo(linha, "Processo")!.Value,
        Positivo(LeituraDoVortice.Longo(linha, "SeqUsuario")),
        (int?)Positivo(LeituraDoVortice.Longo(linha, "Acao")),
        LeituraDoVortice.DataUtc(linha, "DtaAgenda", HorasDeDiferencaParaUtc),
        LeituraDoVortice.DataUtc(linha, "DtaLimiteExecucao", HorasDeDiferencaParaUtc),
        LeituraDoVortice.Numero(linha, "Prioridade"),
        LeituraDoVortice.Texto(linha, "Realizada"),
        LeituraDoVortice.DataUtc(linha, "DtaRealizacao", HorasDeDiferencaParaUtc),
        (int?)Positivo(LeituraDoVortice.Longo(linha, "UltResultado")),
        Positivo(LeituraDoVortice.Longo(linha, "UltHistorico")),
        Positivo(LeituraDoVortice.Longo(linha, "HistoricoOrigem")),
        Positivo(LeituraDoVortice.Longo(linha, "UsuarioQueConcluiu")),
        LeituraDoVortice.Texto(linha, "TipoAgendamento"),
        LeituraDoVortice.DataUtc(linha, "DtaGeracao", HorasDeDiferencaParaUtc));

    /// <summary>Uma linha da consulta do histórico, traduzida.</summary>
    /// <param name="linha">A linha.</param>
    public static InteracaoDaOportunidadeNoVortice MapearInteracao(IDataRecord linha) => new(
        LeituraDoVortice.Longo(linha, "SeqHistorico")!.Value,
        LeituraDoVortice.Longo(linha, "Processo")!.Value,
        Positivo(LeituraDoVortice.Longo(linha, "SeqUsuario")),
        (int?)Positivo(LeituraDoVortice.Longo(linha, "AcaoGeradora")),
        (int?)Positivo(LeituraDoVortice.Longo(linha, "Resultado")),
        Positivo(LeituraDoVortice.Longo(linha, "AgendaOrigem")),
        LeituraDoVortice.Texto(linha, "Natureza"),
        LeituraDoVortice.Longo(linha, "DeSistema") is > 0,
        LeituraDoVortice.DataUtc(linha, "DtaRealizacao", HorasDeDiferencaParaUtc),
        LeituraDoVortice.Numero(linha, "Duracao"),
        LeituraDoVortice.Numero(linha, "Latitude"),
        LeituraDoVortice.Numero(linha, "Longitude"));

    /// <summary>Uma linha dos tipos de processo.</summary>
    /// <param name="linha">A linha.</param>
    public static TipoDeProcessoNoVortice MapearTipoDeProcesso(IDataRecord linha) => new(
        (int)LeituraDoVortice.Longo(linha, "CodProcesso")!.Value,
        LeituraDoVortice.Texto(linha, "Descricao"),
        LeituraDoVortice.Texto(linha, "DescrRed"),
        LeituraDoVortice.Numero(linha, "EmUso") is > 0);

    /// <summary>Uma linha das ações.</summary>
    /// <param name="linha">A linha.</param>
    public static AcaoNoVortice MapearAcao(IDataRecord linha) => new(
        (int)LeituraDoVortice.Longo(linha, "Acao")!.Value,
        LeituraDoVortice.Texto(linha, "Descricao"),
        LeituraDoVortice.Texto(linha, "DescReduzida"),
        string.Equals(LeituraDoVortice.Texto(linha, "EmUso"), "S", StringComparison.OrdinalIgnoreCase),
        LeituraDoVortice.Numero(linha, "PrazoRealizacao"));

    /// <summary>Uma linha dos resultados.</summary>
    /// <param name="linha">A linha.</param>
    public static ResultadoNoVortice MapearResultado(IDataRecord linha) => new(
        (int)LeituraDoVortice.Longo(linha, "Resultado")!.Value,
        (int?)Positivo(LeituraDoVortice.Longo(linha, "Acao")),
        LeituraDoVortice.Texto(linha, "Descricao"),
        LeituraDoVortice.Texto(linha, "DescReduzida"));

    /// <summary>Uma linha dos efeitos.</summary>
    /// <param name="linha">A linha.</param>
    public static EfeitoDoResultadoNoVortice MapearEfeito(IDataRecord linha) => new(
        (int)LeituraDoVortice.Longo(linha, "Resultado")!.Value,
        LeituraDoVortice.Numero(linha, "MoveFase") is > 0,
        LeituraDoVortice.Texto(linha, "StatusDestino"));

    /// <summary>Uma linha dos logins.</summary>
    /// <param name="linha">A linha.</param>
    public static LoginNoVortice MapearLogin(IDataRecord linha) => new(
        LeituraDoVortice.Longo(linha, "SeqUsuario")!.Value,
        LeituraDoVortice.Texto(linha, "CodUsuario"));

    private static async Task<List<T>> LerAsync<T>(
        SqlConnection conexao, string consulta, Func<IDataRecord, T> mapear, CancellationToken ct, params (string Nome, object Valor)[] parametros)
    {
        var linhas = new List<T>();
        await using var comando = new SqlCommand(consulta, conexao) { CommandTimeout = TempoLimiteDaConsulta };
        foreach (var (nome, valor) in parametros) comando.Parameters.AddWithValue(nome, valor);
        await using var leitor = await comando.ExecuteReaderAsync(ct);
        while (await leitor.ReadAsync(ct)) linhas.Add(mapear(leitor));
        return linhas;
    }

    private static async Task<List<T>> PorFatiasAsync<T>(
        SqlConnection conexao, IReadOnlyList<int> codigos, Func<IEnumerable<int>, string> consulta, Func<IDataRecord, T> mapear, CancellationToken ct)
    {
        var linhas = new List<T>();
        foreach (var fatia in codigos.Chunk(CodigosPorConsulta))
            linhas.AddRange(await LerAsync(conexao, consulta(fatia), mapear, ct));
        return linhas;
    }

    private static long? Positivo(long? valor) => valor is > 0 ? valor : null;

    /// <summary>A data de São Paulo como data — a previsão não tem hora que importe.</summary>
    private static DateOnly? SomenteData(DateTime? local) => local is { } valor ? DateOnly.FromDateTime(valor) : null;

    private static string Lista(IEnumerable<int> codigos)
    {
        var lista = codigos.Distinct().Order().ToList();
        if (lista.Count == 0) throw new ArgumentException("A consulta por lista precisa de ao menos um código.", nameof(codigos));
        return string.Join(", ", lista.Select(c => c.ToString(CultureInfo.InvariantCulture)));
    }
}
