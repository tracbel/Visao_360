using System.Globalization;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Integracao.Art;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// OS ITENS DE MÁQUINA DAS NOTAS DE VENDA — a <c>SD2</c> do Protheus, grupo <c>VEIC</c>, item a item (issue 70,
/// D-P12, decidida pelo Ricardo em 27/09/2026).
///
/// <para><b>É a mesma venda do faturamento</b> (<see cref="LeitorDeFaturamentoDoProtheus"/>): nota normal e CFOP
/// da mesma lista de inclusão — a remessa para demonstração, o retorno de conserto e a transferência entre filiais
/// não são preço de venda. E o mesmo <c>D2_GRUPO = 'VEIC'</c> que o faturamento chama de máquina. O filtro é
/// montado das constantes daquele leitor, e não reescrito: não há segunda cópia da regra que possa discordar.</para>
///
/// <para><b>Item a item, e não agregado</b>, ao contrário do faturamento: o casamento com a venda do ART precisa
/// saber quantos itens de máquina a nota tem — duas máquinas na mesma nota ficam de fora, porque não se sabe qual
/// valor é de qual. A leitura é pequena (alguns milhares de itens por ano).</para>
///
/// <para><b>O valor é <c>D2_TOTAL</c></b>, sem IPI e sem ICMS-ST e líquido de desconto — o mesmo número do
/// faturamento, e não o preço de tabela (<c>D2_PRUNIT</c>). É convertido para <c>decimal</c> na própria consulta:
/// na origem ele é <c>float</c>.</para>
///
/// <para><b>SÓ LEITURA</b>, com <c>ApplicationIntent=ReadOnly</c> e <c>NOLOCK</c>, como toda leitura do ERP.</para>
/// </summary>
/// <param name="opcoes">A configuração do banco do Protheus.</param>
public sealed class LeitorDeItensDeMaquinaDoProtheus(OpcoesDoBancoDoProtheus opcoes)
{
    /// <summary>O grupo de produto que é máquina — o mesmo de <see cref="LeitorDeFaturamentoDoProtheus.Classificar"/>.</summary>
    public const string GrupoDeMaquina = "VEIC";

    /// <summary>A consulta dos itens de máquina vendidos desde uma data.</summary>
    public static readonly string Consulta = $"""
        SELECT RTRIM(d.D2_FILIAL), RTRIM(d.D2_SERIE), RTRIM(d.D2_DOC), RTRIM(d.D2_ITEM), d.D2_EMISSAO,
               CAST(d.D2_QUANT AS decimal(18, 4)), CAST(d.D2_TOTAL AS decimal(18, 2))
        FROM dbo.SD2010 d WITH (NOLOCK)
        WHERE d.D_E_L_E_T_ = ' '
          AND d.D2_EMISSAO >= @desde
          AND d.D2_GRUPO = '{GrupoDeMaquina}'
          AND d.D2_TIPO = '{LeitorDeFaturamentoDoProtheus.TipoDeVenda}'
          AND d.D2_CF IN ({string.Join(", ", LeitorDeFaturamentoDoProtheus.CfopsDeVenda.Order(StringComparer.Ordinal).Select(c => $"'{c}'"))})
        """;

    /// <summary>Lê os itens de máquina das notas de venda emitidas a partir de uma data.</summary>
    /// <param name="desde">A primeira emissão a considerar.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<IReadOnlyList<ItemDeMaquinaNaNota>>> LerAsync(DateOnly desde, CancellationToken ct)
    {
        if (!opcoes.EstaConfigurada)
            return Resultado<IReadOnlyList<ItemDeMaquinaNaNota>>.Indisponivel(
                "A leitura do preço da máquina exige ProtheusBanco__Servidor, __Banco, __Usuario e __Senha " +
                "(ou a conexão \"Protheus — banco (leitura)\" configurada em Configurações > Integrações).");

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

            // O MESMO TIPO DA COLUNA: `D2_EMISSAO` é varchar(8) 'yyyyMMdd' — um nvarchar faria o SQL Server
            // converter a coluna inteira, e o índice por emissão deixaria de servir.
            comando.Parameters.Add(new SqlParameter("@desde", System.Data.SqlDbType.VarChar, 8)
            {
                Value = desde.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
            });

            await using var leitor = await comando.ExecuteReaderAsync(ct);

            var itens = new List<ItemDeMaquinaNaNota>(10_000);
            while (await leitor.ReadAsync(ct))
            {
                string Texto(int i) => leitor.IsDBNull(i) ? string.Empty : leitor.GetString(i).Trim();

                // SEM DATA VÁLIDA NÃO HÁ COMO CONFERIR CONTRA A VENDA: o item fica fora, e não com uma data substituta.
                if (LeitorDoParqueDoProtheus.Data(Texto(4)) is not { } emissao) continue;

                itens.Add(new ItemDeMaquinaNaNota(
                    Texto(0), Texto(1), Texto(2), Texto(3), emissao,
                    leitor.IsDBNull(5) ? 0m : leitor.GetDecimal(5),
                    leitor.IsDBNull(6) ? 0m : leitor.GetDecimal(6)));
            }

            return Resultado<IReadOnlyList<ItemDeMaquinaNaNota>>.Ok(itens);
        }
        catch (SqlException falha)
        {
            // A MENSAGEM DO CONECTOR PODE CITAR SERVIDOR E USUÁRIO: sai só o código.
            return Resultado<IReadOnlyList<ItemDeMaquinaNaNota>>.Indisponivel(
                $"O banco do Protheus não respondeu à leitura dos itens de máquina das notas (erro SQL {falha.Number}). Nada foi gravado.");
        }
    }
}
