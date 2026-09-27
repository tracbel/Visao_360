using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Integracao.Vortice;

/// <summary>Um processo 31/41/50 do Vórtice, como a leitura do funil o traz.</summary>
/// <param name="Numero">O número (<c>IV_PROCDADO.Processo</c>).</param>
/// <param name="Tipo">O tipo (<c>CodProcesso</c>): 31, 41 ou 50.</param>
/// <param name="NumeroDoDna">O <c>ProcessoDNA</c>; o processo comum aponta para si.</param>
/// <param name="NroEmpresa">A filial do processo no Vórtice — o <c>NN</c> do código <c>0101NN</c> do CRM.</param>
/// <param name="SeqPessoa">A pessoa do processo.</param>
/// <param name="Documento">O documento da pessoa, recomposto e conferido.</param>
/// <param name="SeqCarteira">A carteira da pessoa no departamento MAQ-NOVOS, quando há.</param>
/// <param name="LoginDoResponsavel">O login do responsável (<c>UsuResponsavel</c>) — usado só para achar a conta, nunca gravado.</param>
/// <param name="IncluidoEmUtc">A inclusão (UTC); nula em 3.280 processos (27/09/2026).</param>
/// <param name="PrimeiroAndamentoEmUtc">O primeiro registro do histórico do processo (UTC).</param>
/// <param name="Status">O status do processo na origem.</param>
/// <param name="RealizadoEmUtc">Quando o processo foi encerrado na origem (UTC).</param>
public sealed record ProcessoNoFunilDoVortice(
    long Numero, short Tipo, long NumeroDoDna, int? NroEmpresa, long? SeqPessoa, DocumentoDoVortice Documento, int? SeqCarteira,
    string? LoginDoResponsavel, DateTime? IncluidoEmUtc, DateTime? PrimeiroAndamentoEmUtc, string? Status, DateTime? RealizadoEmUtc)
{
    /// <summary>O processo como a regra do estágio o enxerga.</summary>
    public ProcessoNaRegraDoEstagio NaRegra => new(Numero, Tipo, NumeroDoDna, IncluidoEmUtc, PrimeiroAndamentoEmUtc);
}

/// <summary>Uma linha do histórico que importa ao funil: resultado classificado ou ação geradora de etapa.</summary>
/// <param name="SeqHistorico">O identificador da linha.</param>
/// <param name="Processo">O processo.</param>
/// <param name="Resultado">O código de resultado.</param>
/// <param name="RealizadoEmUtc">Quando (UTC).</param>
/// <param name="AcaoGeradora">A ação que gerou a linha.</param>
public sealed record LinhaDoHistoricoDoFunil(long SeqHistorico, long Processo, int Resultado, DateTime RealizadoEmUtc, int? AcaoGeradora)
{
    /// <summary>A linha como a regra do estágio a enxerga.</summary>
    public ResultadoNaRegraDoEstagio NaRegra => new(SeqHistorico, Processo, Resultado, RealizadoEmUtc, AcaoGeradora);
}

/// <summary>Tudo o que o funil lê do Vórtice, numa sessão só.</summary>
/// <param name="Processos">Todos os processos 31/41/50, sem janela — o pai antigo transmite ao filho novo.</param>
/// <param name="Historico">As linhas do histórico desses processos que a regra usa.</param>
public sealed record LeituraDoFunilDoVortice(IReadOnlyList<ProcessoNoFunilDoVortice> Processos, IReadOnlyList<LinhaDoHistoricoDoFunil> Historico);

/// <summary>
/// A LEITURA DO FUNIL NO VÓRTICE (decisões de 27/09/2026, documento 52).
///
/// <para><b>O que lê, e o que não lê.</b> Os processos de <c>IV_PROCDADO</c> com <c>CodProcesso</c> 31, 41 e 50 — todos,
/// sem janela, porque o pai antigo transmite resultado ao filho novo e porque o processo que some desta leitura é o
/// que deixou de existir no Vórtice (só então as linhas dele saem do CRM) — e, do <c>IV_HISTORICO</c> desses tipos, só
/// as linhas com resultado classificado ou com ação geradora de etapa. Nenhuma coluna de texto livre: o
/// <c>IV_HISTORICO.Contato</c> guarda nomes de pessoas e não aparece em consulta nenhuma; o login do responsável só
/// serve para achar a conta do CRM.</para>
///
/// <para><b>SÓ LEITURA.</b> A conexão declara <c>ApplicationIntent=ReadOnly</c> e toda consulta é <c>SELECT</c> com
/// <c>NOLOCK</c> — o Vórtice é um sistema em produção. Medido em 27/09/2026: o funil em 2–3 s, o histórico 31/41/50 com
/// cerca de 295 mil linhas.</para>
/// </summary>
/// <param name="opcoes">A configuração do Vórtice — a cadeia vem de <c>Vortice__Conexao</c> ou da tela.</param>
public sealed class LeitorDoFunilDoVortice(OpcoesDoVortice opcoes)
{
    /// <summary>Os tipos de processo do funil, como a consulta os escreve.</summary>
    public const string TiposDoFunil = "31, 41, 50";

    /// <summary>Um minuto basta com folga; o teto largo evita que um Vórtice lento num dia ruim derrube a rotina.</summary>
    private const int TempoLimiteDaConsulta = 300;

    /// <summary>O Vórtice grava hora de São Paulo, sem fuso; o Brasil não tem horário de verão desde 2019.</summary>
    private const int HorasDeDiferencaParaUtc = 3;

    /// <summary>
    /// OS PROCESSOS DO FUNIL: o processo, a pessoa e o documento dela, a carteira MAQ-NOVOS dela, o responsável, a
    /// inclusão, o primeiro andamento e o desfecho. A carteira vem de <c>IVS_Pes</c> no departamento <c>MAQ-NOVOS</c> —
    /// o mesmo caminho da sincronia das carteiras —, e com várias, a de menor identificador, para a leitura dar a
    /// mesma resposta a cada rodada.
    /// </summary>
    public const string ConsultaDosProcessos = $"""
        SELECT d.Processo, d.CodProcesso, d.ProcessoDNA, d.NroEmpresa, d.SeqPessoa,
               g.FisicaJuridica, g.NroCGCCPF, g.DigCGCCPF,
               c.SeqCarteira,
               p.UsuResponsavel, p.DtaInclusao, p.Status, p.DtaRealizacao,
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
            WHERE h.CodProcesso IN ({TiposDoFunil}) AND h.Processo IS NOT NULL
            GROUP BY h.Processo) a ON a.Processo = d.Processo
        WHERE d.CodProcesso IN ({TiposDoFunil})
        """;

    /// <summary>
    /// AS LINHAS DO HISTÓRICO que a regra usa: resultado classificado (o estágio) ou ação geradora de etapa (o
    /// <c>DTA_ETAPA</c> do BI). As listas entram escritas — números inteiros, formatados aqui —, e a de resultados vem
    /// da tabela de classificação do CRM, não de uma constante.
    /// </summary>
    /// <param name="resultados">Os códigos de resultado classificados.</param>
    public static string ConsultaDoHistorico(IReadOnlyCollection<int> resultados)
    {
        if (resultados.Count == 0)
            throw new ArgumentException("A consulta do histórico precisa de ao menos um código de resultado.", nameof(resultados));

        return $"""
            SELECT h.SeqHistorico, h.Processo, h.Resultado, h.DtaRealizacao, h.AcaoGeradora
            FROM IV_HISTORICO h WITH (NOLOCK)
            WHERE h.CodProcesso IN ({TiposDoFunil}) AND h.Processo IS NOT NULL
              AND (h.Resultado IN ({Lista(resultados)}) OR h.AcaoGeradora IN ({Lista(RegraDoEstagio.TodasAsAcoesDaEtapa)}))
            """;
    }

    /// <summary>Lê os processos e o histórico, numa sessão de leitura.</summary>
    /// <param name="resultados">Os códigos de resultado classificados (a tabela de classificação do CRM).</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LeituraDoFunilDoVortice>> LerAsync(IReadOnlyCollection<int> resultados, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(opcoes.Conexao))
            return Resultado<LeituraDoFunilDoVortice>.Indisponivel(
                "A leitura do funil exige a credencial do Vórtice: grave-a em Configurações › Integrações ou defina " +
                "Vortice__Conexao no servidor. Nada foi gravado.");

        if (resultados.Count == 0)
            return Resultado<LeituraDoFunilDoVortice>.Indisponivel(
                "A classificação dos resultados do Vórtice está vazia no CRM: sem ela, nenhum estágio existe. Nada foi gravado.");

        var etapa = "a conexão";
        try
        {
            var cadeia = new SqlConnectionStringBuilder(opcoes.Conexao) { ApplicationIntent = ApplicationIntent.ReadOnly };
            await using var conexao = new SqlConnection(cadeia.ConnectionString);
            await conexao.OpenAsync(ct);

            etapa = "os processos";
            var processos = new List<ProcessoNoFunilDoVortice>(110_000);
            await using (var comando = new SqlCommand(ConsultaDosProcessos, conexao) { CommandTimeout = TempoLimiteDaConsulta })
            {
                comando.Parameters.AddWithValue("@depto", LeitorDeCarteirasDoVortice.DepartamentoDeMaquinasNovas);
                await using var leitor = await comando.ExecuteReaderAsync(ct);
                while (await leitor.ReadAsync(ct)) processos.Add(MapearProcesso(leitor));
            }

            etapa = "o histórico";
            var historico = new List<LinhaDoHistoricoDoFunil>(300_000);
            await using (var comando = new SqlCommand(ConsultaDoHistorico(resultados), conexao) { CommandTimeout = TempoLimiteDaConsulta })
            await using (var leitor = await comando.ExecuteReaderAsync(ct))
            {
                while (await leitor.ReadAsync(ct)) historico.Add(MapearHistorico(leitor));
            }

            return Resultado<LeituraDoFunilDoVortice>.Ok(new LeituraDoFunilDoVortice(processos, historico));
        }
        catch (SqlException falha)
        {
            // A MENSAGEM DO CONECTOR PODE CITAR SERVIDOR E USUÁRIO: sai só o código e a etapa.
            return Resultado<LeituraDoFunilDoVortice>.Indisponivel(
                $"O Vórtice não respondeu à leitura de {etapa} (erro SQL {falha.Number}). Quase sempre é a rede ou a " +
                "credencial; a rotina é idempotente e pode rodar de novo. Nada foi gravado.");
        }
        catch (ArgumentException)
        {
            return Resultado<LeituraDoFunilDoVortice>.Indisponivel(
                "A cadeia de conexão do Vórtice está malformada. Confira a credencial em Configurações › Integrações. Nada foi gravado.");
        }
    }

    /// <summary>Uma linha da consulta dos processos, traduzida.</summary>
    /// <param name="linha">A linha.</param>
    public static ProcessoNoFunilDoVortice MapearProcesso(IDataRecord linha)
    {
        var numero = LeituraDoVortice.Longo(linha, "Processo")!.Value;
        return new ProcessoNoFunilDoVortice(
            numero,
            (short)(LeituraDoVortice.Numero(linha, "CodProcesso") ?? 0),
            LeituraDoVortice.Longo(linha, "ProcessoDNA") ?? numero,
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
            LeituraDoVortice.DataUtc(linha, "DtaRealizacao", HorasDeDiferencaParaUtc));
    }

    /// <summary>Uma linha da consulta do histórico, traduzida.</summary>
    /// <param name="linha">A linha.</param>
    public static LinhaDoHistoricoDoFunil MapearHistorico(IDataRecord linha) => new(
        LeituraDoVortice.Longo(linha, "SeqHistorico")!.Value,
        LeituraDoVortice.Longo(linha, "Processo")!.Value,
        (int)LeituraDoVortice.Longo(linha, "Resultado")!.Value,
        LeituraDoVortice.DataUtc(linha, "DtaRealizacao", HorasDeDiferencaParaUtc)!.Value,
        (int?)LeituraDoVortice.Longo(linha, "AcaoGeradora"));

    private static string Lista(IEnumerable<int> codigos) =>
        string.Join(", ", codigos.Distinct().Order().Select(c => c.ToString(CultureInfo.InvariantCulture)));
}

/// <summary>A tradução das colunas do Vórtice, comum aos leitores do funil e da venda perdida.</summary>
internal static class LeituraDoVortice
{
    private static object? Valor(IDataRecord linha, string coluna)
    {
        var i = linha.GetOrdinal(coluna);
        return linha.IsDBNull(i) ? null : linha.GetValue(i);
    }

    /// <summary>O texto aparado; vazio vira nulo — nunca texto vazio.</summary>
    public static string? Texto(IDataRecord linha, string coluna)
    {
        var valor = Convert.ToString(Valor(linha, coluna), CultureInfo.InvariantCulture)?.Trim();
        return string.IsNullOrEmpty(valor) ? null : valor;
    }

    /// <summary>O número, de qualquer tipo numérico da origem.</summary>
    public static decimal? Numero(IDataRecord linha, string coluna) =>
        Valor(linha, coluna) is { } valor ? Convert.ToDecimal(valor, CultureInfo.InvariantCulture) : null;

    /// <summary>O número inteiro.</summary>
    public static long? Longo(IDataRecord linha, string coluna) => Numero(linha, coluna) is { } valor ? (long)valor : null;

    /// <summary>A data da origem convertida para UTC.</summary>
    public static DateTime? DataUtc(IDataRecord linha, string coluna, int horasParaUtc) =>
        Valor(linha, coluna) is DateTime data ? DateTime.SpecifyKind(data.AddHours(horasParaUtc), DateTimeKind.Utc) : null;

    /// <summary>A data da origem como ela está (hora de São Paulo).</summary>
    public static DateTime? DataLocal(IDataRecord linha, string coluna) => Valor(linha, coluna) is DateTime data ? data : null;
}
