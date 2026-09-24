using System.Globalization;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Integracao.Art;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// UMA MÁQUINA DO CADASTRO DE VEÍCULOS DO PROTHEUS (<c>VV1010</c>), com o dono atual e a evidência que o sustenta —
/// sem nome de ninguém: o dono chega como documento.
/// </summary>
/// <param name="ChaveInterna">O <c>VV1_CHAINT</c> — a chave interna única, a mesma da venda (<c>VVA</c>) e da ordem
/// de serviço (<c>VO1</c>).</param>
/// <param name="ChassiNaOrigem">O <c>VV1_CHASSI</c> como o Protheus escreve, sem espaço à direita.</param>
/// <param name="Chassi">O chassi normalizado por <see cref="Dominio.Comum.Chassi.Normalizar"/> — sem espaço nenhum,
/// maiúsculo.</param>
/// <param name="CodigoDaMarca">O <c>VV1_CODMAR</c>.</param>
/// <param name="Marca">A descrição da marca (<c>VE1_DESMAR</c>).</param>
/// <param name="CodigoDoModelo">O <c>VV1_MODVEI</c>.</param>
/// <param name="Modelo">A descrição do modelo (<c>VV2_DESMOD</c>).</param>
/// <param name="GrupoDoModelo">O grupo do modelo (<c>VV2_GRUMOD</c>), sem espaço à direita.</param>
/// <param name="DescricaoDoGrupo">A descrição do grupo (<c>VVR_DESCRI</c>).</param>
/// <param name="Nova">Verdadeiro para <c>VV1_ESTVEI = '0'</c> (nova), falso para <c>'1'</c> (usada), nulo sem valor.</param>
/// <param name="Situacao">O <c>VV1_SITVEI</c> aparado: vazio (carga antiga), 0 estoque, 1 vendida, 2 trânsito, 3
/// remessa, 4 consignada, 5 transferida, 6 reservada, 7 progresso, 8 pedido, 9 requisitada em ordem de serviço.</param>
/// <param name="AnoFabricacao">Os quatro primeiros dígitos de <c>VV1_FABMOD</c>, quando são ano.</param>
/// <param name="AnoModelo">Os quatro últimos dígitos de <c>VV1_FABMOD</c>, quando são ano.</param>
/// <param name="DocumentoDoDono">O CPF/CNPJ do dono atual (<c>VV1_PROATU + VV1_LJPATU</c> na SA1), só dígitos,
/// quando tem 11 ou 14.</param>
/// <param name="VendidaEm">A data de venda do próprio cadastro (<c>VV1_DTUVEN</c>, senão <c>VV1_DATVEN</c>).</param>
/// <param name="UltimaVendaEm">A data da nota de venda válida mais recente (<c>VV0_DATMOV</c>).</param>
/// <param name="UltimaVendaEDoDono">Se essa nota é para o dono atual — o mesmo código e loja, ou o mesmo documento.</param>
/// <param name="UltimaOrdemEm">A abertura da ordem de serviço mais recente não cancelada (<c>VO1_DATABE</c>).</param>
/// <param name="UltimaOrdemEDoDono">Se essa ordem é do dono atual — o mesmo código e loja, ou o mesmo documento.</param>
public sealed record MaquinaNoProtheus(
    string ChaveInterna,
    string ChassiNaOrigem,
    string Chassi,
    string? CodigoDaMarca,
    string? Marca,
    string? CodigoDoModelo,
    string? Modelo,
    string? GrupoDoModelo,
    string? DescricaoDoGrupo,
    bool? Nova,
    string Situacao,
    short? AnoFabricacao,
    short? AnoModelo,
    string? DocumentoDoDono,
    DateOnly? VendidaEm,
    DateOnly? UltimaVendaEm,
    bool UltimaVendaEDoDono,
    DateOnly? UltimaOrdemEm,
    bool UltimaOrdemEDoDono)
{
    /// <summary>
    /// Se a situação é de máquina NO CLIENTE: vendida (<c>1</c>) ou da carga antiga, sem situação (vazio). Estoque,
    /// pedido, remessa, consignação, trânsito e o resto são a Tracbel com a máquina, não o cliente.
    /// </summary>
    public bool EstaNoCliente => Situacao is "" or "1";

    /// <summary>
    /// A EVIDÊNCIA DO DONO ATUAL (decisão 3 de 24/09/2026): a nota de venda mais recente é dele; senão, a ordem de
    /// serviço mais recente é dele (a oficina atualiza o dono); senão, só o cadastro antigo o sustenta.
    /// </summary>
    public EvidenciaDoProprietario Evidencia =>
        UltimaVendaEDoDono ? EvidenciaDoProprietario.NotaDeVenda
        : UltimaOrdemEDoDono ? EvidenciaDoProprietario.OrdemDeServico
        : EvidenciaDoProprietario.CadastroAntigo;

    /// <summary>
    /// A data da evidência mais recente que sustenta o dono: a nota e a ordem que são dele, a que vier por último;
    /// no só-cadastro, a data de venda que o cadastro guarda, quando guarda.
    /// </summary>
    public DateOnly? DataDaEvidencia
    {
        get
        {
            var datas = new[] { UltimaVendaEDoDono ? UltimaVendaEm : null, UltimaOrdemEDoDono ? UltimaOrdemEm : null }.OfType<DateOnly>().ToList();
            return datas.Count > 0 ? datas.Max() : VendidaEm;
        }
    }

    /// <summary>
    /// Se há nota ou ordem de serviço DO DONO ATUAL depois da data — a evidência que faz o Protheus prevalecer sobre a
    /// venda do ART (decisão 1 de 24/09/2026).
    /// </summary>
    /// <param name="data">A data da venda no ART.</param>
    public bool TemEvidenciaDoDonoDepoisDe(DateOnly data) =>
        (UltimaVendaEDoDono && UltimaVendaEm > data) || (UltimaOrdemEDoDono && UltimaOrdemEm > data);
}

/// <summary>
/// O CADASTRO DE VEÍCULOS INDEXADO PELO CHASSI NORMALIZADO — é por ele que o ART, o CRM e o Protheus se encontram.
///
/// <para><b>O mesmo chassi em duas linhas</b> (15 grupos, medido em 24/09/2026): com o mesmo dono, é a mesma máquina
/// cadastrada duas vezes, e vale a linha com a evidência mais recente; com donos diferentes, é AMBÍGUO — nenhum dos
/// dois é escolhido, porque escolher seria inventar quem tem a máquina.</para>
/// </summary>
public sealed class ParqueNoProtheus
{
    private readonly Dictionary<string, MaquinaNoProtheus> _unicas = new(StringComparer.Ordinal);
    private readonly HashSet<string> _ambiguas = new(StringComparer.Ordinal);

    /// <summary>Indexa as máquinas lidas.</summary>
    /// <param name="maquinas">As linhas da VV1, uma por <c>VV1_CHAINT</c>.</param>
    public ParqueNoProtheus(IReadOnlyList<MaquinaNoProtheus> maquinas)
    {
        Maquinas = maquinas;

        foreach (var grupo in maquinas.Where(m => m.Chassi.Length > 0).GroupBy(m => m.Chassi, StringComparer.Ordinal))
        {
            if (grupo.Select(m => m.DocumentoDoDono).Distinct(StringComparer.Ordinal).Count() > 1)
            {
                _ambiguas.Add(grupo.Key);
                continue;
            }

            _unicas[grupo.Key] = grupo
                .OrderByDescending(m => new[] { m.UltimaVendaEm, m.UltimaOrdemEm, m.VendidaEm }.Max())
                .ThenByDescending(m => m.ChaveInterna, StringComparer.Ordinal)
                .First();
        }
    }

    /// <summary>Todas as linhas lidas, inclusive as sem chassi e as repetidas.</summary>
    public IReadOnlyList<MaquinaNoProtheus> Maquinas { get; }

    /// <summary>Os chassis com uma máquina só — ou várias linhas com o mesmo dono.</summary>
    public IReadOnlyDictionary<string, MaquinaNoProtheus> PorChassi => _unicas;

    /// <summary>Os chassis repetidos com donos diferentes.</summary>
    public IReadOnlySet<string> Ambiguos => _ambiguas;

    /// <summary>A máquina do chassi já normalizado, quando ele é única no cadastro.</summary>
    /// <param name="chassi">O chassi normalizado.</param>
    /// <param name="maquina">A máquina.</param>
    public bool TentarAchar(string chassi, out MaquinaNoProtheus maquina) => _unicas.TryGetValue(chassi, out maquina!);
}

/// <summary>
/// A LEITURA DO CADASTRO DE VEÍCULOS DO PROTHEUS — o parque de máquinas e o dono atual de cada uma. É a ÚNICA leitura
/// da <c>VV1010</c> no CRM: a sincronia do parque e a conferência do ART usam esta mesma (decisão de 24/09/2026).
///
/// <para><b>O DONO É O PROPRIETÁRIO ATUAL, <c>VV1_PROATU + VV1_LJPATU</c></b>, casado com a SA1 por código E loja.
/// Até 24/09/2026 a conferência do ART lia <c>VV1_CLIULV</c> — o cliente da última venda —, que está VAZIO em 28.979
/// das 32.595 máquinas com chassi e não traz a loja: um código com lojas de CNPJs diferentes não dizia de quem era a
/// máquina. Medido nessa data: pelo proprietário atual, 30.765 das 32.595 máquinas com chassi chegam a UM documento, e
/// nenhuma fica ambígua. O proprietário atual é o que a oficina mantém — das 962 máquinas em que ele difere do cliente
/// da última nota, 812 têm ordem de serviço depois da nota.</para>
///
/// <para><b>A EVIDÊNCIA VEM JUNTO, agregada no banco:</b> a nota de venda válida mais recente (<c>VV0 × VVA</c>, com
/// <c>VV0_OPEMOV = '0'</c> e <c>VV0_SITNFI = '1'</c>, pela chave interna) e a ordem de serviço mais recente não
/// cancelada (<c>VO1</c>, <c>VO1_STATUS &lt;&gt; 'C'</c>), cada uma com a marca de ser ou não do dono atual. O
/// banco devolve uma linha por máquina, em menos de dois segundos (medido em 24/09/2026, 38.622 linhas).</para>
///
/// <para><b>SÓ LEITURA.</b> A conexão declara <c>ApplicationIntent=ReadOnly</c>, a consulta é um <c>SELECT</c> com
/// <c>NOLOCK</c> — para não segurar trava num ERP em produção — e nada é escrito.</para>
///
/// <para><b>O chassi é normalizado aqui, em C#</b>, pela MESMA função do domínio: aparar só as pontas, como a leitura
/// antiga fazia, perdia 98 chassis com espaço no meio.</para>
/// </summary>
/// <param name="opcoes">A configuração do banco do Protheus.</param>
public sealed class LeitorDoParqueDoProtheus(OpcoesDoBancoDoProtheus opcoes)
{
    /// <summary>
    /// A CONSULTA. Pública para o teste de contêiner e para quem precisar conferir o que é lido: nenhum nome, nenhum
    /// valor — o dono sai como documento, a evidência como data e marca.
    /// </summary>
    public const string Consulta = """
        WITH venda AS (
            SELECT a.VVA_CHAINT AS chaint, c.VV0_DATMOV AS data, c.VV0_CODCLI AS codigo, c.VV0_LOJA AS loja,
                   ROW_NUMBER() OVER (PARTITION BY a.VVA_CHAINT ORDER BY c.VV0_DATMOV DESC, c.VV0_NUMTRA DESC) AS ordem
            FROM dbo.VVA010 a WITH (NOLOCK)
            JOIN dbo.VV0010 c WITH (NOLOCK)
              ON c.VV0_FILIAL = a.VVA_FILIAL AND c.VV0_NUMTRA = a.VVA_NUMTRA AND c.D_E_L_E_T_ = ' '
            WHERE a.D_E_L_E_T_ = ' ' AND a.VVA_CHAINT <> ' ' AND c.VV0_OPEMOV = '0' AND c.VV0_SITNFI = '1'),
        oficina AS (
            SELECT o.VO1_CHAINT AS chaint, o.VO1_DATABE AS data, o.VO1_PROVEI AS codigo, o.VO1_LOJPRO AS loja,
                   ROW_NUMBER() OVER (PARTITION BY o.VO1_CHAINT ORDER BY o.VO1_DATABE DESC, o.VO1_NUMOSV DESC) AS ordem
            FROM dbo.VO1010 o WITH (NOLOCK)
            WHERE o.D_E_L_E_T_ = ' ' AND o.VO1_CHAINT <> ' ' AND o.VO1_STATUS <> 'C')
        SELECT RTRIM(v.VV1_CHAINT), RTRIM(v.VV1_CHASSI), RTRIM(v.VV1_CODMAR), RTRIM(e.VE1_DESMAR),
               RTRIM(v.VV1_MODVEI), RTRIM(m.VV2_DESMOD), RTRIM(m.VV2_GRUMOD), RTRIM(g.VVR_DESCRI),
               RTRIM(v.VV1_ESTVEI), RTRIM(v.VV1_SITVEI), RTRIM(v.VV1_FABMOD), RTRIM(d.A1_CGC),
               RTRIM(v.VV1_DTUVEN), RTRIM(v.VV1_DATVEN),
               ve.data,
               CASE WHEN ve.chaint IS NOT NULL AND v.VV1_PROATU <> ' '
                         AND ((ve.codigo = v.VV1_PROATU AND ve.loja = v.VV1_LJPATU)
                              OR (RTRIM(dv.A1_CGC) <> '' AND RTRIM(dv.A1_CGC) = RTRIM(d.A1_CGC)))
                    THEN 1 ELSE 0 END,
               os.data,
               CASE WHEN os.chaint IS NOT NULL AND v.VV1_PROATU <> ' '
                         AND ((os.codigo = v.VV1_PROATU AND os.loja = v.VV1_LJPATU)
                              OR (RTRIM(dos.A1_CGC) <> '' AND RTRIM(dos.A1_CGC) = RTRIM(d.A1_CGC)))
                    THEN 1 ELSE 0 END
        FROM dbo.VV1010 v WITH (NOLOCK)
        LEFT JOIN dbo.SA1010 d WITH (NOLOCK)
          ON d.A1_COD = v.VV1_PROATU AND d.A1_LOJA = v.VV1_LJPATU AND d.D_E_L_E_T_ = ' ' AND v.VV1_PROATU <> ' '
        OUTER APPLY (SELECT TOP (1) x.VV2_DESMOD, x.VV2_GRUMOD FROM dbo.VV2010 x WITH (NOLOCK)
                     WHERE x.VV2_FILIAL = v.VV1_FILIAL AND x.VV2_CODMAR = v.VV1_CODMAR AND x.VV2_MODVEI = v.VV1_MODVEI
                       AND x.D_E_L_E_T_ = ' '
                     ORDER BY CASE WHEN x.VV2_SEGMOD = v.VV1_SEGMOD THEN 0 ELSE 1 END, x.R_E_C_N_O_) m
        LEFT JOIN dbo.VVR010 g WITH (NOLOCK)
          ON g.VVR_FILIAL = v.VV1_FILIAL AND g.VVR_CODMAR = v.VV1_CODMAR AND g.VVR_GRUMOD = m.VV2_GRUMOD AND g.D_E_L_E_T_ = ' '
        LEFT JOIN dbo.VE1010 e WITH (NOLOCK)
          ON e.VE1_FILIAL = v.VV1_FILIAL AND e.VE1_CODMAR = v.VV1_CODMAR AND e.D_E_L_E_T_ = ' '
        LEFT JOIN venda ve ON ve.chaint = v.VV1_CHAINT AND ve.ordem = 1
        LEFT JOIN dbo.SA1010 dv WITH (NOLOCK) ON dv.A1_COD = ve.codigo AND dv.A1_LOJA = ve.loja AND dv.D_E_L_E_T_ = ' '
        LEFT JOIN oficina os ON os.chaint = v.VV1_CHAINT AND os.ordem = 1
        LEFT JOIN dbo.SA1010 dos WITH (NOLOCK) ON dos.A1_COD = os.codigo AND dos.A1_LOJA = os.loja AND dos.D_E_L_E_T_ = ' '
        WHERE v.D_E_L_E_T_ = ' '
        """;

    /// <summary>Lê o cadastro de veículos inteiro, uma linha por <c>VV1_CHAINT</c>.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ParqueNoProtheus>> LerAsync(CancellationToken ct)
    {
        if (!opcoes.EstaConfigurada)
            return Resultado<ParqueNoProtheus>.Indisponivel(
                "A leitura do parque do Protheus exige ProtheusBanco__Servidor, __Banco, __Usuario e __Senha.");

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

            await using var comando = new SqlCommand(Consulta, conexao) { CommandTimeout = 600 };
            await using var leitor = await comando.ExecuteReaderAsync(ct);

            var maquinas = new List<MaquinaNoProtheus>(40_000);
            while (await leitor.ReadAsync(ct))
            {
                string Texto(int i) => leitor.IsDBNull(i) ? string.Empty : leitor.GetString(i).Trim();
                string? Opcional(int i) => Texto(i) is { Length: > 0 } t ? t : null;
                bool Marca(int i) => !leitor.IsDBNull(i) && Convert.ToInt32(leitor.GetValue(i), CultureInfo.InvariantCulture) == 1;

                var (fabricacao, modelo) = AnosDe(Texto(10));
                var documento = LeitorDeClientesDoProtheus.SoDigitos(Texto(11));

                maquinas.Add(new MaquinaNoProtheus(
                    ChaveInterna: Texto(0),
                    ChassiNaOrigem: Texto(1),
                    Chassi: Chassi.Normalizar(Texto(1)),
                    CodigoDaMarca: Opcional(2),
                    Marca: Opcional(3),
                    CodigoDoModelo: Opcional(4),
                    Modelo: Opcional(5),
                    GrupoDoModelo: Opcional(6),
                    DescricaoDoGrupo: Opcional(7),
                    Nova: Texto(8) switch { "0" => true, "1" => false, _ => null },
                    Situacao: Texto(9),
                    AnoFabricacao: fabricacao,
                    AnoModelo: modelo,
                    DocumentoDoDono: documento.Length is 11 or 14 ? documento : null,
                    VendidaEm: Data(Texto(12)) ?? Data(Texto(13)),
                    UltimaVendaEm: Data(Texto(14)),
                    UltimaVendaEDoDono: Marca(15),
                    UltimaOrdemEm: Data(Texto(16)),
                    UltimaOrdemEDoDono: Marca(17)));
            }

            return Resultado<ParqueNoProtheus>.Ok(new ParqueNoProtheus(maquinas));
        }
        catch (SqlException falha)
        {
            // A MENSAGEM DO CONECTOR PODE CITAR SERVIDOR E USUÁRIO: sai só o código.
            return Resultado<ParqueNoProtheus>.Indisponivel(
                $"O banco do Protheus não respondeu à leitura do cadastro de veículos (erro SQL {falha.Number}). Nada foi gravado.");
        }
    }

    /// <summary>
    /// A data do Protheus (<c>AAAAMMDD</c>, texto). Vazia, zerada ou fora de 1950–2100 sai nula — nunca uma data
    /// substituta.
    /// </summary>
    /// <param name="texto">A data como o Protheus grava.</param>
    public static DateOnly? Data(string? texto) =>
        texto is { Length: 8 }
        && DateOnly.TryParseExact(texto, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
        && data.Year is >= 1950 and <= 2100
            ? data
            : null;

    /// <summary>
    /// Os anos de <c>VV1_FABMOD</c> — oito dígitos, fabricação e modelo. O ano que não passa em
    /// <see cref="Equipamento.AnoAceito"/> sai nulo: <c>00000000</c> existe na origem.
    /// </summary>
    /// <param name="fabmod">O <c>VV1_FABMOD</c>.</param>
    public static (short? Fabricacao, short? Modelo) AnosDe(string? fabmod)
    {
        if (fabmod is not { Length: 8 } || !fabmod.All(char.IsAsciiDigit)) return (null, null);

        static short? Ano(string texto) => short.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var ano) && Equipamento.AnoAceito(ano) ? ano : null;
        return (Ano(fabmod[..4]), Ano(fabmod[4..]));
    }
}
