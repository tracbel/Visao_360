using System.Globalization;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Art;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// UMA LINHA DA VIEW <c>X_V_BI_SERVICOS_CAPA_E_ITENS_OS</c> — um item (peça ou serviço) de uma OS, com a capa repetida.
/// Só o que o CRM usa: nada de nome, endereço, e-mail ou telefone, que a view também traz. O documento do proprietário
/// vem só para casar com o cadastro do CRM, e é descartado depois.
/// </summary>
/// <param name="Filial">O <c>FILIAL</c> — o código completo da filial no Protheus (<c>0101NN</c>).</param>
/// <param name="NumeroOs">O <c>NUMERO_OS</c>.</param>
/// <param name="StatusDaCapa">O <c>STATUS_CAPA_OS</c> (A, L, D, F, C).</param>
/// <param name="TipoDeAtendimento">O <c>DESC_TIPO_ATEND_CAPA</c>.</param>
/// <param name="AbertaEm">O <c>DATA_ABER</c>.</param>
/// <param name="LiberadaEm">O <c>DATA_LIBE</c>.</param>
/// <param name="CanceladaEm">O <c>DATA_CANC</c>.</param>
/// <param name="FechadaEm">O <c>DATA_FECH</c>.</param>
/// <param name="Chassi">O <c>CHASSI</c>.</param>
/// <param name="Modelo">O <c>MODELO</c>.</param>
/// <param name="Horimetro">O <c>HORIME</c>.</param>
/// <param name="DocumentoDoProprietario">O <c>CPF_CNPJ</c>, só dígitos.</param>
/// <param name="PecaOuServico">O <c>PEC_OU_SRV</c> — <c>PEÇ</c> ou <c>SRV</c>.</param>
/// <param name="TipoDeTempo">O <c>TIPO_TEMPO</c>.</param>
/// <param name="CodigoDoItem">O <c>COD_ITEM</c>.</param>
/// <param name="Requisicao">O <c>NOSS_NUM_REQ</c>.</param>
/// <param name="Quantidade">O <c>QTDADE</c>.</param>
/// <param name="ValorUnitario">O <c>VLR_UNIT</c>.</param>
/// <param name="ValorDoDesconto">O <c>VLR_DESC</c> — o desconto da linha de peça, repetido em cada requisição dela.</param>
/// <param name="Formula">O <c>FORMULA</c>.</param>
/// <param name="Grupo">O <c>GRUPO</c>.</param>
/// <param name="ProdutoRequisitado">O <c>PROD_REQ</c>.</param>
/// <param name="OrigemParalela">O <c>ORIGI_PARALE</c>.</param>
public sealed record ItemDaOrdemNaOrigem(
    string Filial,
    string NumeroOs,
    string? StatusDaCapa,
    string? TipoDeAtendimento,
    DateOnly? AbertaEm,
    DateOnly? LiberadaEm,
    DateOnly? CanceladaEm,
    DateOnly? FechadaEm,
    string? Chassi,
    string? Modelo,
    decimal? Horimetro,
    string? DocumentoDoProprietario,
    string? PecaOuServico,
    string? TipoDeTempo,
    string? CodigoDoItem,
    string? Requisicao,
    decimal? Quantidade,
    decimal? ValorUnitario,
    decimal? ValorDoDesconto,
    string? Formula,
    string? Grupo,
    string? ProdutoRequisitado,
    string? OrigemParalela)
{
    /// <summary>Se a linha é de peça — tudo o que não é <c>SRV</c>, como a <c>CHAVE_PECA</c> do BI.</summary>
    public bool EhPeca => !string.Equals(PecaOuServico?.Trim(), "SRV", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// UMA LINHA DA VIEW <c>X_V_BI_SERVICOS_SRV_EXECUTADO_OS</c> — um serviço executado (VO4) de uma OS, com os tempos e
/// os valores de onde sai o "Valor serviço c/desc" do BI. Os nomes seguem a coluna da origem, para que a régua de
/// <see cref="RegrasDoValorDaOrdemDeServico"/> se leia lado a lado com o script do Qlik.
/// </summary>
#pragma warning disable CS1591 // os parâmetros são as colunas da origem, documentadas no script do BI.
public sealed record ServicoExecutadoNaOrigem(
    string Filial,
    string NumeroOs,
    string? CodProd,
    DateOnly? Abertura,
    string? TpTpo,
    string? DescTpTpo,
    string? TpServico,
    string? GruServico,
    string? CodServico,
    string? DepIntOs,
    string? DepGarOs,
    string? VokIncMob,
    decimal? VokPreKil,
    string? VoiSitTpo,
    string? Vo4TipTem,
    decimal? TemPad,
    decimal? TemTra,
    decimal? TemCob,
    decimal? TemVen,
    decimal? TemPadTotal,
    decimal? TemTraTotal,
    decimal? TemCobTotal,
    decimal? TemVenTotal,
    decimal? Vz1ValDes,
    decimal? VscValDes,
    decimal? Vo4ValDes,
    decimal? Vo4PreKil,
    decimal? Vo4KilRod,
    decimal? VscKilRod,
    decimal? Vo4ValInt,
    decimal? Vo4ValHor,
    decimal? Vo4ValVen,
    decimal? Vz1ValUni,
    decimal? Vz1ValBru,
    decimal? VscValBru);
#pragma warning restore CS1591

/// <summary>O que a leitura das ordens de serviço trouxe.</summary>
/// <param name="Desde">A primeira abertura da janela (a OS ainda aberta entra de qualquer data).</param>
/// <param name="Itens">Os itens das OS, uma linha por item da view.</param>
/// <param name="Servicos">Os serviços executados, uma linha por linha da view.</param>
public sealed record LeituraDasOrdensDeServico(
    DateOnly Desde,
    IReadOnlyList<ItemDaOrdemNaOrigem> Itens,
    IReadOnlyList<ServicoExecutadoNaOrigem> Servicos);

/// <summary>
/// A LEITURA DAS ORDENS DE SERVIÇO DO PROTHEUS — pelas mesmas views que o extrator "Pós Vendas Serviços" do BI lê
/// (pedido do Ricardo em 02/10/2026: "já tem todos os SQL e queries prontos"). A definição das views mora no banco do
/// Protheus e não vem no arquivo do Qlik; as colunas são as que o script do BI seleciona.
///
/// <para><b>A JANELA</b>: as OS abertas a partir de <c>desde</c> (três anos, como o faturamento) e as que ainda estão na
/// oficina, de qualquer data — a OS esquecida aberta há quatro anos é exatamente o que a tela precisa mostrar. A data é
/// convertida no banco pelos dois formatos possíveis (data de verdade, ou texto <c>DD/MM/AAAA</c> ou <c>AAAAMMDD</c>),
/// porque o tipo da coluna da view não se vê daqui.</para>
///
/// <para><b>O QUE NÃO VEM</b>: o histórico da Noroeste de 2022 a 2024, que o BI lê de outro banco
/// (<c>NOROESTE_OS_FECHADA_HIST</c>), e o de-para de filial por consultor que o BI aplica por planilha
/// (<c>De_Para_Filiais_e_Consultores.xlsx</c>) — planilha não é fonte; a filial é a do Protheus.</para>
///
/// <para><b>SÓ LEITURA.</b> A conexão declara <c>ApplicationIntent=ReadOnly</c>, a consulta é um <c>SELECT</c> com
/// <c>NOLOCK</c>, e nada é escrito.</para>
/// </summary>
/// <param name="opcoes">A configuração do banco do Protheus.</param>
public sealed class LeitorDasOrdensDeServicoDoProtheus(OpcoesDoBancoDoProtheus opcoes)
{
    /// <summary>
    /// A DATA DA VIEW, nos dois formatos possíveis — o tipo da coluna não se vê daqui: para uma data, os dois
    /// <c>TRY_CONVERT</c> devolvem a própria data; para texto, um deles lê.
    /// </summary>
    private const string DataDaAbertura = "COALESCE(TRY_CONVERT(date, {0}, 103), TRY_CONVERT(date, {0}, 112))";

    /// <summary>A consulta dos itens. Pública para o teste de contêiner e para quem precisar conferir o que é lido.</summary>
    public static readonly string ConsultaDosItens = $"""
        SELECT RTRIM(CONVERT(varchar(20), FILIAL)), RTRIM(CONVERT(varchar(20), NUMERO_OS)), RTRIM(CONVERT(varchar(5), STATUS_CAPA_OS)),
               RTRIM(CONVERT(nvarchar(120), DESC_TIPO_ATEND_CAPA)),
               DATA_ABER, DATA_LIBE, DATA_CANC, DATA_FECH,
               RTRIM(CONVERT(varchar(60), CHASSI)), RTRIM(CONVERT(nvarchar(120), MODELO)), HORIME, RTRIM(CONVERT(varchar(30), CPF_CNPJ)),
               RTRIM(CONVERT(nvarchar(10), PEC_OU_SRV)), RTRIM(CONVERT(varchar(20), TIPO_TEMPO)), RTRIM(CONVERT(varchar(40), COD_ITEM)),
               RTRIM(CONVERT(varchar(30), NOSS_NUM_REQ)),
               QTDADE, VLR_UNIT, VLR_DESC,
               RTRIM(CONVERT(varchar(40), FORMULA)), RTRIM(CONVERT(varchar(20), GRUPO)), RTRIM(CONVERT(varchar(40), PROD_REQ)),
               RTRIM(CONVERT(varchar(20), ORIGI_PARALE))
        FROM dbo.X_V_BI_SERVICOS_CAPA_E_ITENS_OS WITH (NOLOCK)
        WHERE {string.Format(CultureInfo.InvariantCulture, DataDaAbertura, "DATA_ABER")} >= @desde
           OR UPPER(LTRIM(RTRIM(CONVERT(varchar(5), STATUS_CAPA_OS)))) IN ('A', 'L', 'D')
        """;

    /// <summary>
    /// A consulta dos serviços executados. A janela é a mesma, pela abertura; a OS ainda aberta (sem fechamento nem
    /// cancelamento) entra de qualquer data. O serviço de uma OS que não veio na primeira consulta é descartado na carga.
    /// </summary>
    public static readonly string ConsultaDosServicos = $"""
        SELECT RTRIM(CONVERT(varchar(20), VO4_FILIAL)), RTRIM(CONVERT(varchar(20), VO4_NUMOSV)), RTRIM(CONVERT(varchar(40), COD_PROD)),
               ABERTURA, RTRIM(CONVERT(varchar(20), TpTpo)), RTRIM(CONVERT(nvarchar(120), Desc_TpTpo)), RTRIM(CONVERT(varchar(20), TpServico)),
               RTRIM(CONVERT(varchar(20), GruServico)), RTRIM(CONVERT(varchar(60), CodServico)),
               RTRIM(CONVERT(varchar(20), DEP_INT_OS)), RTRIM(CONVERT(varchar(20), DEP_GAR_OS)),
               RTRIM(CONVERT(varchar(10), VOK_INCMOB)), VOK_PREKIL, RTRIM(CONVERT(varchar(10), VOI_SITTPO)), RTRIM(CONVERT(varchar(20), VO4_TIPTEM)),
               TEMPAD, TEMTRA, TEMCOB, TEMVEN, TEMPAD_TOTAL, TEMTRA_TOTAL, TEMCOB_TOTAL, TEMVEN_TOTAL,
               VZ1_VALDES, VSC_VALDES, VO4_VALDES, VO4_PREKIL, VO4_KILROD, VSC_KILROD, VO4_VALINT, VO4_VALHOR, VO4_VALVEN,
               VZ1_VALUNI, VZ1_VALBRU, VSC_VALBRU
        FROM dbo.X_V_BI_SERVICOS_SRV_EXECUTADO_OS WITH (NOLOCK)
        WHERE {string.Format(CultureInfo.InvariantCulture, DataDaAbertura, "ABERTURA")} >= @desde
           OR (NULLIF(LTRIM(RTRIM(CONVERT(varchar(30), FECHAMENTO, 112))), '') IS NULL
               AND NULLIF(LTRIM(RTRIM(CONVERT(varchar(30), CANCELAMENTO, 112))), '') IS NULL)
        """;

    /// <summary>Lê as duas views.</summary>
    /// <param name="desde">A primeira abertura da janela.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LeituraDasOrdensDeServico>> LerAsync(DateOnly desde, CancellationToken ct)
    {
        if (!opcoes.EstaConfigurada)
            return Resultado<LeituraDasOrdensDeServico>.Indisponivel(
                "A leitura das ordens de serviço do Protheus exige ProtheusBanco__Servidor, __Banco, __Usuario e __Senha.");

        var construtor = new SqlConnectionStringBuilder
        {
            DataSource = opcoes.Servidor,
            InitialCatalog = opcoes.Banco,
            UserID = opcoes.Usuario,
            Password = opcoes.Senha,
            TrustServerCertificate = true,
            ApplicationIntent = ApplicationIntent.ReadOnly,
            ConnectTimeout = 15
        };

        try
        {
            await using var conexao = new SqlConnection(construtor.ConnectionString);
            await conexao.OpenAsync(ct);
            return Resultado<LeituraDasOrdensDeServico>.Ok(await LerAsync(conexao, desde, ct));
        }
        catch (SqlException falha)
        {
            // A MENSAGEM DO CONECTOR PODE CITAR SERVIDOR E USUÁRIO: sai só o código. O 208 é a view que não existe neste
            // banco, e o 229 é a permissão de leitura que o login não tem — os dois pedem ação de quem administra o ERP.
            var causa = falha.Number switch
            {
                208 => " A view do BI não existe neste banco — confira o banco configurado na conexão.",
                229 => " O login da conexão não tem permissão de leitura na view do BI — peça o GRANT SELECT a quem administra o Protheus.",
                _ => string.Empty
            };
            return Resultado<LeituraDasOrdensDeServico>.Indisponivel(
                $"O banco do Protheus não respondeu à leitura das ordens de serviço (erro SQL {falha.Number}).{causa} Nada foi gravado.");
        }
    }

    /// <summary>Lê as duas views numa conexão já aberta — a do teste de contêiner, ou a de <see cref="LerAsync(DateOnly, CancellationToken)"/>.</summary>
    /// <param name="conexao">A conexão aberta.</param>
    /// <param name="desde">A primeira abertura da janela.</param>
    /// <param name="ct">Cancelamento.</param>
    public static async Task<LeituraDasOrdensDeServico> LerAsync(SqlConnection conexao, DateOnly desde, CancellationToken ct)
    {
        var itens = new List<ItemDaOrdemNaOrigem>(200_000);
        await using (var comando = new SqlCommand(ConsultaDosItens, conexao) { CommandTimeout = 900 })
        {
            comando.Parameters.Add(new SqlParameter("@desde", System.Data.SqlDbType.Date) { Value = desde.ToDateTime(TimeOnly.MinValue) });
            await using var leitor = await comando.ExecuteReaderAsync(ct);
            while (await leitor.ReadAsync(ct))
            {
                string? Texto(int i) => leitor.IsDBNull(i) ? null : Convert.ToString(leitor.GetValue(i), CultureInfo.InvariantCulture)?.Trim() is { Length: > 0 } t ? t : null;
                object? Valor(int i) => leitor.IsDBNull(i) ? null : leitor.GetValue(i);

                var documento = LeitorDeClientesDoProtheus.SoDigitos(Texto(11));
                itens.Add(new ItemDaOrdemNaOrigem(
                    Filial: Texto(0) ?? string.Empty,
                    NumeroOs: Texto(1) ?? string.Empty,
                    StatusDaCapa: Texto(2),
                    TipoDeAtendimento: Texto(3),
                    AbertaEm: DataDe(Valor(4)),
                    LiberadaEm: DataDe(Valor(5)),
                    CanceladaEm: DataDe(Valor(6)),
                    FechadaEm: DataDe(Valor(7)),
                    Chassi: Texto(8),
                    Modelo: Texto(9),
                    Horimetro: NumeroDe(Valor(10)),
                    DocumentoDoProprietario: documento.Length is 11 or 14 ? documento : null,
                    PecaOuServico: Texto(12),
                    TipoDeTempo: Texto(13),
                    CodigoDoItem: Texto(14),
                    Requisicao: Texto(15),
                    Quantidade: NumeroDe(Valor(16)),
                    ValorUnitario: NumeroDe(Valor(17)),
                    ValorDoDesconto: NumeroDe(Valor(18)),
                    Formula: Texto(19),
                    Grupo: Texto(20),
                    ProdutoRequisitado: Texto(21),
                    OrigemParalela: Texto(22)));
            }
        }

        var servicos = new List<ServicoExecutadoNaOrigem>(200_000);
        await using (var comando = new SqlCommand(ConsultaDosServicos, conexao) { CommandTimeout = 900 })
        {
            comando.Parameters.Add(new SqlParameter("@desde", System.Data.SqlDbType.Date) { Value = desde.ToDateTime(TimeOnly.MinValue) });
            await using var leitor = await comando.ExecuteReaderAsync(ct);
            while (await leitor.ReadAsync(ct))
            {
                string? Texto(int i) => leitor.IsDBNull(i) ? null : Convert.ToString(leitor.GetValue(i), CultureInfo.InvariantCulture)?.Trim() is { Length: > 0 } t ? t : null;
                decimal? N(int i) => leitor.IsDBNull(i) ? null : NumeroDe(leitor.GetValue(i));

                servicos.Add(new ServicoExecutadoNaOrigem(
                    Filial: Texto(0) ?? string.Empty,
                    NumeroOs: Texto(1) ?? string.Empty,
                    CodProd: Texto(2),
                    Abertura: leitor.IsDBNull(3) ? null : DataDe(leitor.GetValue(3)),
                    TpTpo: Texto(4),
                    DescTpTpo: Texto(5),
                    TpServico: Texto(6),
                    GruServico: Texto(7),
                    CodServico: Texto(8),
                    DepIntOs: Texto(9),
                    DepGarOs: Texto(10),
                    VokIncMob: Texto(11),
                    VokPreKil: N(12),
                    VoiSitTpo: Texto(13),
                    Vo4TipTem: Texto(14),
                    TemPad: N(15),
                    TemTra: N(16),
                    TemCob: N(17),
                    TemVen: N(18),
                    TemPadTotal: N(19),
                    TemTraTotal: N(20),
                    TemCobTotal: N(21),
                    TemVenTotal: N(22),
                    Vz1ValDes: N(23),
                    VscValDes: N(24),
                    Vo4ValDes: N(25),
                    Vo4PreKil: N(26),
                    Vo4KilRod: N(27),
                    VscKilRod: N(28),
                    Vo4ValInt: N(29),
                    Vo4ValHor: N(30),
                    Vo4ValVen: N(31),
                    Vz1ValUni: N(32),
                    Vz1ValBru: N(33),
                    VscValBru: N(34)));
            }
        }

        return new LeituraDasOrdensDeServico(desde, itens, servicos);
    }

    /// <summary>
    /// A DATA DA VIEW, venha como vier: data de verdade, ou texto <c>AAAAMMDD</c>, <c>DD/MM/AAAA</c> ou <c>AAAA-MM-DD</c>.
    /// Vazia, zerada ou fora de 1950–2100 sai nula — nunca uma data substituta.
    /// </summary>
    /// <param name="valor">O valor lido.</param>
    public static DateOnly? DataDe(object? valor) => valor switch
    {
        null or DBNull => null,
        DateTime data => Aceita(DateOnly.FromDateTime(data)),
        DateTimeOffset data => Aceita(DateOnly.FromDateTime(data.DateTime)),
        DateOnly data => Aceita(data),
        _ => DataDeTexto(Convert.ToString(valor, CultureInfo.InvariantCulture))
    };

    /// <summary>
    /// O NÚMERO DA VIEW, venha como vier: numérico do banco, ou texto com ponto ou vírgula decimal. O que não se lê sai
    /// nulo — e o nulo, como no Qlik, não entra na soma.
    /// </summary>
    /// <param name="valor">O valor lido.</param>
    public static decimal? NumeroDe(object? valor)
    {
        switch (valor)
        {
            case null or DBNull: return null;
            case decimal d: return d;
            case double d when double.IsFinite(d) && Math.Abs(d) < 7.9e27: return Convert.ToDecimal(d, CultureInfo.InvariantCulture);
            case float f when float.IsFinite(f) && Math.Abs(f) < 7.9e27f: return Convert.ToDecimal(f, CultureInfo.InvariantCulture);
            case int or long or short or byte: return Convert.ToDecimal(valor, CultureInfo.InvariantCulture);
            case string texto:
                // O ÚLTIMO SEPARADOR É O DECIMAL: "1.234,50" e "12,5" são do Brasil, "1,234.50" e "12.5" não. A cultura
                // invariante sozinha leria "12,5" como 125 — a vírgula como milhar.
                var limpo = texto.Trim();
                if (limpo.Length == 0) return null;
                var cultura = limpo.LastIndexOf(',') > limpo.LastIndexOf('.') ? CultureInfo.GetCultureInfo("pt-BR") : CultureInfo.InvariantCulture;
                return decimal.TryParse(limpo, NumberStyles.Number, cultura, out var lido) ? lido : null;
            default: return null;
        }
    }

    private static DateOnly? DataDeTexto(string? texto)
    {
        var limpo = texto?.Trim() ?? string.Empty;
        if (limpo.Length == 0) return null;
        string[] formatos = ["yyyyMMdd", "dd/MM/yyyy", "yyyy-MM-dd"];
        var data = limpo.Length >= 10 && limpo[4] == '-' ? limpo[..10] : limpo.Length >= 10 && limpo[2] == '/' ? limpo[..10] : limpo;
        return DateOnly.TryParseExact(data, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var lida) ? Aceita(lida) : null;
    }

    private static DateOnly? Aceita(DateOnly data) => data.Year is >= 1950 and <= 2100 ? data : null;
}
