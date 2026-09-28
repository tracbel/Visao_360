using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Integracao.Vortice;

/// <summary>ONDE O FINANCIAMENTO MORA EM CADA FORMULÁRIO DA VENDA. Nulo: o formulário não tem o campo.</summary>
/// <param name="Tabela">A tabela de respostas.</param>
/// <param name="Valor">O valor financiado.</param>
/// <param name="Instituicao">A instituição financeira.</param>
/// <param name="Linha">A linha de crédito (no de gestão de crédito, a forma de pagamento).</param>
/// <param name="PedidoEm">A data do pedido de venda.</param>
public sealed record MapeamentoDoFinanciamento(string Tabela, string Valor, string? Instituicao, string? Linha, string? PedidoEm);

/// <summary>Um financiamento como o formulário o traz — sem nome, sem documento, sem texto livre.</summary>
/// <param name="Processo">O processo do Vórtice.</param>
/// <param name="Formulario">O formulário.</param>
/// <param name="Questionario">A resposta (<c>SeqQuestionario</c>).</param>
/// <param name="PreenchidoEm">Quando o formulário foi preenchido (hora de São Paulo).</param>
/// <param name="PedidoEm">A data do pedido, como o CEN a escreveu.</param>
/// <param name="Valor">O valor financiado.</param>
/// <param name="Instituicao">A instituição.</param>
/// <param name="Linha">A linha de crédito.</param>
/// <param name="SeqCidade">A cidade do cadastro do cliente, pelo código do Vórtice.</param>
/// <param name="Cidade">O nome da cidade.</param>
/// <param name="Uf">A UF.</param>
/// <param name="Fase">A fase do processo.</param>
/// <param name="Status">A descrição do status do processo.</param>
public sealed record FinanciamentoNoVortice(
    long Processo, string Formulario, long Questionario, DateTime? PreenchidoEm, DateTime? PedidoEm, decimal? Valor,
    string? Instituicao, string? Linha, long? SeqCidade, string? Cidade, string? Uf, string? Fase, string? Status);

/// <summary>
/// A LEITURA DOS FINANCIAMENTOS DAS VENDAS NO VÓRTICE — os cinco formulários da venda de máquina que trazem o valor
/// financiado, desde 2012 (issue 262, decisão de 28/09/2026).
///
/// <para><b>Só a resposta com valor.</b> O formulário sem valor financiado é venda à vista ou campo em branco; não entra
/// no share e não é lido.</para>
///
/// <para><b>O que não é lido:</b> a pessoa (só a cidade do cadastro dela), quem preencheu e qualquer texto livre.</para>
/// </summary>
/// <param name="opcoes">A configuração do Vórtice.</param>
public sealed class LeitorDeFinanciamentosDoVortice(OpcoesDoVortice opcoes)
{
    private const int TempoLimiteDaConsulta = 300;

    /// <summary>Os cinco formulários e onde cada campo mora — medido no banco em 28/09/2026.</summary>
    public static readonly IReadOnlyList<MapeamentoDoFinanciamento> Formularios =
    [
        new("IV_Q_VENDA_EQUIPAMENTO", "VENDA_VALOR_FINANC", "VENDA_INST_FINANC", "VENDA_LINHA_CREDITO", "VENDA_DATA_PEDIDO"),
        new("IV_Q_VENDA", "VALOR_FINANCIADO", "INSTITUICAO_FINANCEI", "LINHA_DE_CREDITO", "DATA_PEDIDO_DE_VENDA"),
        new("IV_Q_ACOMPANH_VENDA_JDE", "VLR_FINAN1", "INSTITUI_FINANCEIRA1", null, "DATA_PEDIDO_DE_VENDA"),
        new("IV_Q_ACOMPANHAMENTO_VENDA", "VLR_FINANCIADO", "INST_FINANCEIRA", "TIPO_DE_FINANCIAME", "DATA_PEDIDO_DE_VENDA"),
        new("IV_Q_GESTAO_CREDITO", "VALOR_FINANCIADO", "INSTITUICAO_FINANCEI", "FORMA_PAGAMENTO", null)
    ];

    /// <summary>A consulta inteira: a UNION dos cinco, com a resposta, o processo e a cidade do cadastro.</summary>
    public static readonly string Consulta = MontarConsulta();

    /// <summary>Lê os financiamentos, numa sessão de leitura.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<IReadOnlyList<FinanciamentoNoVortice>>> LerAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(opcoes.Conexao))
            return Resultado<IReadOnlyList<FinanciamentoNoVortice>>.Indisponivel(
                "A leitura dos financiamentos exige a credencial do Vórtice. Nada foi gravado.");

        if (LeituraDoVortice.CadeiaDeLeitura(opcoes.Conexao) is not { } cadeia)
            return Resultado<IReadOnlyList<FinanciamentoNoVortice>>.Indisponivel(
                "A cadeia de conexão do Vórtice está malformada. Nada foi gravado.");

        try
        {
            await using var conexao = new SqlConnection(cadeia);
            await conexao.OpenAsync(ct);

            var financiamentos = new List<FinanciamentoNoVortice>(12_000);
            await using var comando = new SqlCommand(Consulta, conexao) { CommandTimeout = TempoLimiteDaConsulta };
            await using var leitor = await comando.ExecuteReaderAsync(ct);
            while (await leitor.ReadAsync(ct)) financiamentos.Add(Mapear(leitor));

            return Resultado<IReadOnlyList<FinanciamentoNoVortice>>.Ok(financiamentos);
        }
        catch (SqlException falha)
        {
            return Resultado<IReadOnlyList<FinanciamentoNoVortice>>.Indisponivel(
                $"O Vórtice não respondeu à leitura dos financiamentos (erro SQL {falha.Number}). Nada foi gravado.");
        }
    }

    /// <summary>Uma linha da consulta, traduzida.</summary>
    /// <param name="linha">A linha.</param>
    public static FinanciamentoNoVortice Mapear(IDataRecord linha) => new(
        LeituraDoVortice.Longo(linha, "Processo") ?? 0,
        LeituraDoVortice.Texto(linha, "Formulario")!,
        LeituraDoVortice.Longo(linha, "Questionario") ?? 0,
        LeituraDoVortice.DataLocal(linha, "DtaRealizacao"),
        LeituraDoVortice.DataLocal(linha, "PedidoEm"),
        LeituraDoVortice.Numero(linha, "Valor"),
        LeituraDoVortice.Texto(linha, "Instituicao"),
        LeituraDoVortice.Texto(linha, "Linha"),
        LeituraDoVortice.Longo(linha, "SeqCidade"),
        LeituraDoVortice.Texto(linha, "Cidade"),
        LeituraDoVortice.Texto(linha, "Uf"),
        LeituraDoVortice.Texto(linha, "Fase"),
        LeituraDoVortice.Texto(linha, "StatusDesc"));

    /// <summary>
    /// UM FINANCIAMENTO POR PROCESSO. O mesmo processo aparece em mais de um formulário (o de acompanhamento é cópia do
    /// JDE) e, às vezes, duas vezes no mesmo (200 processos no de gestão de crédito): vale o formulário mais novo, na
    /// ordem de <see cref="FinanciamentoDaVenda.PrioridadeDoFormulario"/>, e nele a resposta preenchida por último.
    /// </summary>
    /// <param name="lidos">O que a consulta trouxe.</param>
    public static IReadOnlyList<FinanciamentoNoVortice> UmPorProcesso(IEnumerable<FinanciamentoNoVortice> lidos)
    {
        static int Prioridade(string formulario)
        {
            for (var i = 0; i < FinanciamentoDaVenda.PrioridadeDoFormulario.Count; i++)
                if (string.Equals(FinanciamentoDaVenda.PrioridadeDoFormulario[i], formulario, StringComparison.OrdinalIgnoreCase))
                    return i;
            return int.MaxValue;
        }

        return lidos
            .Where(f => f.Processo > 0)
            .GroupBy(f => f.Processo)
            .Select(g => g
                .OrderBy(f => Prioridade(f.Formulario))
                .ThenByDescending(f => f.PreenchidoEm ?? DateTime.MinValue)
                .ThenByDescending(f => f.Questionario)
                .First())
            .ToList();
    }

    private static string MontarConsulta()
    {
        static string Texto(string? coluna) => coluna is null ? "CAST(NULL AS varchar(80))" : $"CAST(v.{coluna} AS varchar(80))";
        static string Data(string? coluna) => coluna is null ? "CAST(NULL AS datetime)" : $"v.{coluna}";

        var respostas = new StringBuilder();
        foreach (var f in Formularios)
        {
            if (respostas.Length > 0) respostas.Append("\n    UNION ALL\n");
            respostas.Append(CultureInfo.InvariantCulture,
                $"""
                    SELECT v.SEQQUESTIONARIO AS Questionario, CAST('{f.Tabela}' AS varchar(40)) AS Formulario,
                           CAST(v.{f.Valor} AS decimal(18,2)) AS Valor, {Texto(f.Instituicao)} AS Instituicao,
                           {Texto(f.Linha)} AS Linha, {Data(f.PedidoEm)} AS PedidoEm
                    FROM {f.Tabela} v WITH (NOLOCK)
                    WHERE v.{f.Valor} > 0
                """);
        }

        return $"""
            WITH Respostas AS (
            {respostas}
            )
            SELECT r.Questionario, r.Formulario, r.Valor, r.Instituicao, r.Linha, r.PedidoEm,
                   q.DtaRealizacao, q.Processo,
                   CAST(p.Fase AS varchar(60)) AS Fase, CAST(p.StatusDesc AS varchar(60)) AS StatusDesc,
                   c.SeqCidade, CAST(c.Cidade AS varchar(80)) AS Cidade, CAST(c.Uf AS varchar(2)) AS Uf
            FROM Respostas r
            JOIN IV_Questionario q WITH (NOLOCK) ON q.SeqQuestionario = r.Questionario
            JOIN IV_Processo p WITH (NOLOCK) ON p.Processo = q.Processo
            LEFT JOIN GE_Pessoa g WITH (NOLOCK) ON g.SeqPessoa = q.SeqPessoa
            LEFT JOIN GE_Cidade c WITH (NOLOCK) ON c.SeqCidade = g.SeqCidade
            """;
    }
}
