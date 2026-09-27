using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Integracao.Vortice;

/// <summary>
/// ONDE CADA CAMPO MORA EM CADA FORMULÁRIO. Cada geração do formulário renomeou tudo (<c>REVENDA</c>, <c>REVENDA_VP</c>,
/// <c>VP_REVENDA</c>, <c>REVENDA_CONC_IMPL</c>); o que descrevem é o mesmo. Nulo: o formulário não tem o campo.
/// </summary>
/// <param name="Tabela">A tabela de respostas (<see cref="FormulariosDaVendaPerdida"/>).</param>
/// <param name="Tipo">O tipo de equipamento.</param>
/// <param name="Marca">A marca do concorrente.</param>
/// <param name="Modelo">O modelo do concorrente.</param>
/// <param name="Revenda">A revenda que fechou.</param>
/// <param name="ModeloOfertado">O modelo John Deere oferecido.</param>
/// <param name="Quantidade">A quantidade — número nuns formulários, texto nos <c>VP_*</c>.</param>
/// <param name="OcorridaEm">A data da venda perdida.</param>
/// <param name="PrecoDoConcorrente">O preço do concorrente.</param>
/// <param name="PrecoOfertado">O preço John Deere.</param>
/// <param name="Motivo">O motivo.</param>
/// <param name="Participamos">O "participamos da negociação?" do formulário.</param>
public sealed record MapeamentoDoFormulario(
    string Tabela, string? Tipo, string? Marca, string? Modelo, string? Revenda, string? ModeloOfertado, string? Quantidade,
    string? OcorridaEm, string? PrecoDoConcorrente, string? PrecoOfertado, string? Motivo, string? Participamos)
{
    /// <summary>As colunas que o mapeamento cita — as que precisam existir na tabela.</summary>
    public IEnumerable<string> Colunas =>
        new[] { Tipo, Marca, Modelo, Revenda, ModeloOfertado, Quantidade, OcorridaEm, PrecoDoConcorrente, PrecoOfertado, Motivo, Participamos }
            .Where(c => c is not null).Select(c => c!);
}

/// <summary>Uma resposta de formulário de venda perdida do Vórtice, traduzida.</summary>
/// <param name="Questionario">O <c>SeqQuestionario</c> — a identidade da resposta.</param>
/// <param name="Formulario">O formulário.</param>
/// <param name="RegistradaEmUtc">Quando foi preenchida (UTC); nula quando a origem não tem data.</param>
/// <param name="Processo">O processo que o formulário aponta.</param>
/// <param name="SeqPessoa">A pessoa.</param>
/// <param name="EmpresaDoProcesso">A filial do processo.</param>
/// <param name="EmpresaDoHistorico">A filial da linha do histórico em que o formulário foi preenchido.</param>
/// <param name="Documento">O documento da pessoa.</param>
/// <param name="TipoDoEquipamento">O tipo de equipamento.</param>
/// <param name="Marca">A marca do concorrente.</param>
/// <param name="ModeloDoConcorrente">O modelo do concorrente.</param>
/// <param name="Revenda">A revenda que fechou.</param>
/// <param name="ModeloOfertado">O modelo John Deere.</param>
/// <param name="Quantidade">A quantidade, quando é número.</param>
/// <param name="OcorridaEm">A data da venda perdida, como o CEN declarou (hora de São Paulo).</param>
/// <param name="PrecoDoConcorrente">O preço do concorrente.</param>
/// <param name="PrecoOfertado">O preço John Deere.</param>
/// <param name="Motivo">O motivo.</param>
/// <param name="Participamos">O "participamos?" como veio.</param>
public sealed record RespostaDeVendaPerdidaNoVortice(
    long Questionario, string Formulario, DateTime? RegistradaEmUtc, long? Processo, long? SeqPessoa, int? EmpresaDoProcesso,
    int? EmpresaDoHistorico, DocumentoDoVortice Documento, string? TipoDoEquipamento, string? Marca, string? ModeloDoConcorrente,
    string? Revenda, string? ModeloOfertado, decimal? Quantidade, DateTime? OcorridaEm, decimal? PrecoDoConcorrente,
    decimal? PrecoOfertado, string? Motivo, string? Participamos);

/// <summary>
/// A LEITURA DAS VENDAS PERDIDAS NO VÓRTICE — os doze formulários, com todo o histórico desde 2012 (decisão de
/// 27/09/2026, documento 52 §4).
///
/// <para><b>O motivo da perda é resposta de formulário</b>, não coluna do processo: <c>IV_Questionario</c> (a resposta,
/// com a data, o processo e a pessoa) e uma tabela tipada por formulário. Os nomes de coluna mudam a cada geração; a
/// UNION os normaliza uma vez, aqui, a partir de <see cref="Formularios"/>.</para>
///
/// <para><b>A filial</b> vem do processo e, sem processo, da linha do histórico em que o formulário foi preenchido — o
/// antigo, o <c>_JDE</c>, o <c>_PROD</c> e o <c>_IMPLEM</c> quase nunca apontam processo (medido em 27/09/2026).</para>
///
/// <para><b>O que não é lido:</b> quem preencheu (é login de pessoa, e a pergunta da perda não precisa dele) e toda
/// coluna de texto livre do histórico — <c>IV_HISTORICO.Contato</c> guarda nomes e não aparece aqui. O <c>_MANITO</c>
/// (Colorado) fica de fora por decisão.</para>
/// </summary>
/// <param name="opcoes">A configuração do Vórtice.</param>
public sealed class LeitorDeVendasPerdidasDoVortice(OpcoesDoVortice opcoes)
{
    private const int TempoLimiteDaConsulta = 300;
    private const int HorasDeDiferencaParaUtc = 3;

    /// <summary>Os doze formulários e onde cada campo mora em cada um — conferido contra o DDL da extração.</summary>
    public static readonly IReadOnlyList<MapeamentoDoFormulario> Formularios =
    [
        new(FormulariosDaVendaPerdida.Antigo, "TIPO_DE_EQUIPAMENTO", "MARCA_VP", "MODELO_VP", "REVENDA", "MODELO_JOHN_DEER",
            "QUANTIDADE", "DATA_DA_VENDA", "PRECO_CONCORRENTE", "PRECO_JOHN_DEERE", "MOTIVO", "PARTICIPAMOS_DA_NEGO"),
        new(FormulariosDaVendaPerdida.Fy25, "VP_TIPO_EQUIP_CONCOR", "VP_MARCA_EQUIP_CONCO", "VP_MODELO_EQUIP_CONC", "VP_REVENDA", "VP_MODELO_JD",
            "VP_QUANTIDADE", "VP_DATA_VP", "VP_PRECO_CONCORRENTE", "VP_PRECO_JD", "VP_MOTIVO_VP", null),
        new(FormulariosDaVendaPerdida.MaqImp, "TIPO_EQUIP_VP", "MARCA_VP", "MODELO_VP", "REVENDA_VP", "MODELO_JOHN_DEERE_VP",
            "QUANTIDADE_VP", "DATA_VENDA_VP", "PRECO_CONCORRENTE_VP", "PRECO_JOHN_DEERE_VP", "MOTIVO_VP", null),
        new(FormulariosDaVendaPerdida.SemParticipacao, "TIPO_CONCORRENTE", "MARCA_CONCORRENTE", "MODELO_CONCORRENTE", "REVENDA_VP", "MODELO_JD_VP",
            "QUANTIDADE_VP", "DATA_VP", "PRECO_VP", "PRECO_JD_VP", "MOTIVO_VP", null),
        new(FormulariosDaVendaPerdida.Jde, "TIPO_EQUIPAMENTO", "MARCA_VP", "MODELO_VP", "REVENDA", "MODELO_JOHN_DEER",
            "QUANTIDADE", "DATA_DA_VENDA", "PRECO_CONCORRENTE", "PRECO_JOHN_DEERE", "MOTIVO", null),
        new(FormulariosDaVendaPerdida.Prod, "TIPO_EQUIPAMENTO", "MARCA", "MODELO", "REVENDA", "MODELO_JOHN_DEERE",
            "QUANTIDADE", "DATA_DA_VENDA", "PRECO_CONCORRENTE", "PRECO_JOHN_DEERE", "MOTIVA", "PARTICIPAMOS_DA_NEGO"),
        new(FormulariosDaVendaPerdida.Implem, "TIPO_DO_IMPLEMENTO", "MARCA_VC_IMPL", null, "REVENDA_CONC_IMPL", "MODELO_JD_IMPL",
            "QUANTIDADE_IMPLEMENT", "DTA_DA_VENDA", "PRECO_CONC", "PRECO_JD", "MOTIVO_VD_IMP", "PART_NEGOCIACAO"),
        new(FormulariosDaVendaPerdida.Impl, null, "MARCA_VP", "MODELO_VP", "REVENDA", "MODELO_JOHN_DEER",
            "QUANTIDADE", "DATA_DA_VENDA", "PRECO_CONCORRENTE", "PRECO_JOHN_DEERE", "MOTIVO", null),
        new(FormulariosDaVendaPerdida.VpTrator, "VP_TIPO_EQUIPAMENTO", "VP_MARCA_CONCORRENT", "VP_MODELO_CONCORRENT", null, "VP_MODELO_JD",
            "VP_QUANTIDADE_EQUIP", null, "VP_PRECO_CONCORRENTE", "VP_PRECO_JD", null, null),
        new(FormulariosDaVendaPerdida.VpColheitadeira, "VP_CA_TIPO", "VP_CA_MARCA_CONC", "VP_CA_MODELO_CONC", null, "VP_CA_MODELO_JD",
            "VP_CA_QTDE_CONC", null, "VP_CA_PRECO_CONC", "VP_CA_PRECO_JD", null, null),
        new(FormulariosDaVendaPerdida.VpPlantadeira, "VP_PL_TIPO_EQUIP_CON", "VP_PL_MARCA_CONC", "VP_PL_MODELO_CONC", null, "VP_PL_MODELO_JD",
            null, null, "VP_PL_PRECO_CONC", "VP_PL_PRECO_JD", null, null),
        new(FormulariosDaVendaPerdida.VpColhedora, "VP_CH_TIPO", "VP_CH_MARCA_CONC", "VP_CH_MODELO_CONC", null, "VP_CH_MODELO_JD",
            "VP_CH_QTDE", null, "VP_CH_PRECO_CONC", "VP_CH_PRECO_JD", null, null)
    ];

    /// <summary>
    /// A CONSULTA INTEIRA: a UNION dos doze formulários, normalizada, com a resposta, a filial do processo e a do
    /// histórico, e o documento da pessoa. A quantidade sai como texto em todos — nos <c>VP_*</c> ela é
    /// <c>varchar</c> — e vira número em C#. O formulário sem resposta com o <c>SEM_PARTICIPACAO</c> afirma "não".
    /// </summary>
    public static readonly string Consulta = MontarConsulta();

    /// <summary>Lê as respostas dos doze formulários, numa sessão de leitura.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<IReadOnlyList<RespostaDeVendaPerdidaNoVortice>>> LerAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(opcoes.Conexao))
            return Resultado<IReadOnlyList<RespostaDeVendaPerdidaNoVortice>>.Indisponivel(
                "A leitura das vendas perdidas exige a credencial do Vórtice. Nada foi gravado.");

        if (LeituraDoVortice.CadeiaDeLeitura(opcoes.Conexao) is not { } cadeia)
            return Resultado<IReadOnlyList<RespostaDeVendaPerdidaNoVortice>>.Indisponivel(
                "A cadeia de conexão do Vórtice está malformada. Nada foi gravado.");

        try
        {
            await using var conexao = new SqlConnection(cadeia);
            await conexao.OpenAsync(ct);

            var respostas = new List<RespostaDeVendaPerdidaNoVortice>(4_000);
            await using var comando = new SqlCommand(Consulta, conexao) { CommandTimeout = TempoLimiteDaConsulta };
            await using var leitor = await comando.ExecuteReaderAsync(ct);
            while (await leitor.ReadAsync(ct)) respostas.Add(Mapear(leitor));

            return Resultado<IReadOnlyList<RespostaDeVendaPerdidaNoVortice>>.Ok(respostas);
        }
        catch (SqlException falha)
        {
            return Resultado<IReadOnlyList<RespostaDeVendaPerdidaNoVortice>>.Indisponivel(
                $"O Vórtice não respondeu à leitura das vendas perdidas (erro SQL {falha.Number}). Nada foi gravado.");
        }
    }

    /// <summary>Uma linha da consulta, traduzida.</summary>
    /// <param name="linha">A linha.</param>
    public static RespostaDeVendaPerdidaNoVortice Mapear(IDataRecord linha) => new(
        LeituraDoVortice.Longo(linha, "Questionario")!.Value,
        LeituraDoVortice.Texto(linha, "Formulario")!,
        LeituraDoVortice.DataUtc(linha, "DtaRealizacao", HorasDeDiferencaParaUtc),
        LeituraDoVortice.Longo(linha, "Processo") is > 0 and var processo ? processo : null,
        LeituraDoVortice.Longo(linha, "SeqPessoa"),
        (int?)LeituraDoVortice.Longo(linha, "EmpresaDoProcesso"),
        (int?)LeituraDoVortice.Longo(linha, "EmpresaDoHistorico"),
        DocumentoDoVortice.Recompor(
            LeituraDoVortice.Numero(linha, "NroCGCCPF"), LeituraDoVortice.Numero(linha, "DigCGCCPF"),
            LeituraDoVortice.Texto(linha, "FisicaJuridica")),
        LeituraDoVortice.Texto(linha, "TipoDoEquipamento"),
        LeituraDoVortice.Texto(linha, "Marca"),
        LeituraDoVortice.Texto(linha, "ModeloDoConcorrente"),
        LeituraDoVortice.Texto(linha, "Revenda"),
        LeituraDoVortice.Texto(linha, "ModeloOfertado"),
        Quantidade(LeituraDoVortice.Texto(linha, "Quantidade")),
        LeituraDoVortice.DataLocal(linha, "OcorridaEm"),
        LeituraDoVortice.Numero(linha, "PrecoDoConcorrente"),
        LeituraDoVortice.Numero(linha, "PrecoOfertado"),
        LeituraDoVortice.Texto(linha, "Motivo"),
        LeituraDoVortice.Texto(linha, "Participamos"));

    /// <summary>A quantidade escrita, quando é número: "2", "02", "2,0". O resto é "não declarada".</summary>
    /// <param name="texto">O que veio.</param>
    public static decimal? Quantidade(string? texto) =>
        decimal.TryParse(texto?.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var valor) ? valor : null;

    private static string MontarConsulta()
    {
        static string Texto(string? coluna, int tamanho) =>
            coluna is null ? $"CAST(NULL AS varchar({tamanho}))" : $"CAST(v.{coluna} AS varchar({tamanho}))";

        static string Ou(string? coluna, string tipoNulo) => coluna is null ? $"CAST(NULL AS {tipoNulo})" : $"v.{coluna}";

        var respostas = new StringBuilder();
        foreach (var f in Formularios)
        {
            if (respostas.Length > 0) respostas.Append("\n    UNION ALL\n");

            // O SEM_PARTICIPACAO NÃO PERGUNTA — afirma: o nome do formulário é a resposta.
            var participamos = f.Tabela == FormulariosDaVendaPerdida.SemParticipacao ? "CAST('NAO' AS varchar(3))" : Texto(f.Participamos, 3);

            respostas.Append(CultureInfo.InvariantCulture,
                $"""
                    SELECT v.SEQQUESTIONARIO AS Questionario, CAST('{f.Tabela}' AS varchar(40)) AS Formulario,
                           {Texto(f.Tipo, 60)} AS TipoDoEquipamento, {Texto(f.Marca, 60)} AS Marca,
                           {Texto(f.Modelo, 120)} AS ModeloDoConcorrente, {Texto(f.Revenda, 100)} AS Revenda,
                           {Texto(f.ModeloOfertado, 250)} AS ModeloOfertado, {Texto(f.Quantidade, 20)} AS Quantidade,
                           {Ou(f.OcorridaEm, "datetime")} AS OcorridaEm, {Ou(f.PrecoDoConcorrente, "decimal(14,2)")} AS PrecoDoConcorrente,
                           {Ou(f.PrecoOfertado, "decimal(14,2)")} AS PrecoOfertado, {Texto(f.Motivo, 40)} AS Motivo,
                           {participamos} AS Participamos
                    FROM {f.Tabela} v WITH (NOLOCK)
                """);
        }

        return $"""
            WITH Respostas AS (
            {respostas}
            )
            SELECT r.Questionario, r.Formulario, r.TipoDoEquipamento, r.Marca, r.ModeloDoConcorrente, r.Revenda, r.ModeloOfertado,
                   r.Quantidade, r.OcorridaEm, r.PrecoDoConcorrente, r.PrecoOfertado, r.Motivo, r.Participamos,
                   q.DtaRealizacao, q.Processo, q.SeqPessoa,
                   d.NroEmpresa AS EmpresaDoProcesso, h.NroEmpresa AS EmpresaDoHistorico,
                   g.FisicaJuridica, g.NroCGCCPF, g.DigCGCCPF
            FROM Respostas r
            JOIN IV_Questionario q WITH (NOLOCK) ON q.SeqQuestionario = r.Questionario
            LEFT JOIN IV_PROCDADO d WITH (NOLOCK) ON d.Processo = q.Processo
            LEFT JOIN IV_HISTORICO h WITH (NOLOCK) ON h.SeqHistorico = CAST(q.SeqHistorico AS numeric(18,0))
            LEFT JOIN GE_Pessoa g WITH (NOLOCK) ON g.SeqPessoa = q.SeqPessoa
            """;
    }
}
