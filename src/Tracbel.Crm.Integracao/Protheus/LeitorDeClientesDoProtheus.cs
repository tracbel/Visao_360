using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Art;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// Uma LOJA do cadastro de clientes do Protheus (<c>SA1010</c>), como ela veio.
///
/// <para><b>Loja, e não cliente.</b> A SA1 tem uma linha por <i>código + loja</i>, e o mesmo CNPJ
/// aparece em várias — matriz e filiais de entrega do mesmo comprador. Quem junta as lojas num
/// cliente é a carga, pelo documento; o leitor entrega o que a origem tem, sem decidir nada.</para>
///
/// <para><b>O que não está aqui, de propósito:</b> limite de crédito, saldo, risco, condição de
/// pagamento, tabela de preço, vendedor e transportadora. A SA1 tem mais de 200 colunas; nenhuma
/// financeira entra nesta integração, e a consulta nem as seleciona.</para>
/// </summary>
/// <param name="Documento">O CPF ou CNPJ, só dígitos.</param>
/// <param name="Codigo">O código do cliente no Protheus (<c>A1_COD</c>).</param>
/// <param name="Loja">A loja (<c>A1_LOJA</c>).</param>
/// <param name="NomeRazao">A razão social (<c>A1_NOME</c>).</param>
/// <param name="NomeFantasia">O nome reduzido (<c>A1_NREDUZ</c>).</param>
/// <param name="PessoaFisica">Verdadeiro para <c>A1_PESSOA = 'F'</c>.</param>
/// <param name="Uf">A unidade federativa (<c>A1_EST</c>).</param>
/// <param name="CodigoDoMunicipio">O município SEM o prefixo da UF (<c>A1_COD_MUN</c>, 5 dígitos).</param>
/// <param name="MunicipioNaOrigem">O nome do município como o Protheus o escreve (<c>A1_MUN</c>).</param>
/// <param name="Logradouro">O endereço (<c>A1_END</c>).</param>
/// <param name="Bairro">O bairro (<c>A1_BAIRRO</c>).</param>
/// <param name="Cep">O CEP (<c>A1_CEP</c>).</param>
/// <param name="InscricaoEstadual">A inscrição estadual (<c>A1_INSCR</c>).</param>
/// <param name="Bloqueado">Se a loja está bloqueada (<c>A1_MSBLQL = '1'</c>).</param>
public sealed record LojaDeClienteNoProtheus(
    string Documento,
    string Codigo,
    string Loja,
    string NomeRazao,
    string? NomeFantasia,
    bool PessoaFisica,
    string Uf,
    string CodigoDoMunicipio,
    string? MunicipioNaOrigem,
    string? Logradouro,
    string? Bairro,
    string? Cep,
    string? InscricaoEstadual,
    bool Bloqueado);

/// <summary>
/// A LEITURA DO CADASTRO DE CLIENTES DO PROTHEUS — a <c>SA1010</c> inteira, em sessão de leitura.
///
/// <para><b>Por que a SA1 e não o Vórtice</b> (decisão de 24/09/2026). A carga de cadastro do
/// Vórtice foi aposentada atrás de uma bandeira; a SA1 tem 38.752 clientes ativos e é o cadastro
/// que a operação de fato usa. A fila de compradores do ART já a consultava para dizer se um
/// comprador existia — esta leitura é a mesma fonte, agora para criar o cliente e não só para
/// conferi-lo.</para>
///
/// <para><b>SÓ LEITURA.</b> A conexão declara <c>ApplicationIntent=ReadOnly</c> e a consulta é um
/// <c>SELECT</c> com <c>NOLOCK</c> — para não segurar trava num ERP em produção. Nada é escrito.</para>
///
/// <para><b>A FILIAL DONA NÃO VEM DAQUI.</b> Medido em 24/09/2026: <c>A1_FILIAL</c> vem <b>vazio</b>
/// nas 38.752 linhas — no Protheus o cadastro de cliente é compartilhado entre as filiais. Quem diz
/// a filial responsável é a área de atuação, pelo município do cliente. Procurar a filial aqui é
/// procurar o que a origem não tem.</para>
/// </summary>
/// <param name="opcoes">A configuração do banco do Protheus.</param>
public sealed class LeitorDeClientesDoProtheus(OpcoesDoBancoDoProtheus opcoes)
{
    /// <summary>O código do sistema em <c>integracao.Sistema</c>.</summary>
    public const string CodigoDoSistema = "PROTHEUS";

    /// <summary>
    /// As colunas lidas. Nenhuma financeira: ver <see cref="LojaDeClienteNoProtheus"/>.
    /// </summary>
    private const string Colunas =
        "RTRIM(A1_CGC), RTRIM(A1_COD), RTRIM(A1_LOJA), RTRIM(A1_NOME), RTRIM(A1_NREDUZ), RTRIM(A1_PESSOA), " +
        "RTRIM(A1_EST), RTRIM(A1_COD_MUN), RTRIM(A1_MUN), RTRIM(A1_END), RTRIM(A1_BAIRRO), RTRIM(A1_CEP), " +
        "RTRIM(A1_INSCR), RTRIM(A1_MSBLQL)";

    /// <summary>Lê o cadastro inteiro, uma linha por loja.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<IReadOnlyList<LojaDeClienteNoProtheus>>> LerAsync(CancellationToken ct)
    {
        if (!opcoes.EstaConfigurada)
            return Resultado<IReadOnlyList<LojaDeClienteNoProtheus>>.Indisponivel(
                "A leitura do cadastro do Protheus exige ProtheusBanco__Servidor, __Banco, __Usuario e __Senha.");

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

            await using var comando = new SqlCommand(
                $"SELECT {Colunas} FROM dbo.SA1010 WITH (NOLOCK) WHERE D_E_L_E_T_ = ' '", conexao)
            { CommandTimeout = 600 };

            await using var leitor = await comando.ExecuteReaderAsync(ct);

            var lojas = new List<LojaDeClienteNoProtheus>(40_000);
            while (await leitor.ReadAsync(ct))
            {
                string Texto(int i) => leitor.IsDBNull(i) ? string.Empty : leitor.GetString(i);
                string? Opcional(int i)
                {
                    var v = Texto(i);
                    return string.IsNullOrWhiteSpace(v) ? null : v;
                }

                var documento = SoDigitos(Texto(0));

                // SEM DOCUMENTO NÃO HÁ CLIENTE. O CRM identifica cliente por CPF/CNPJ — é a chave
                // que casa com o ART, com a nota do Protheus e com o Vórtice. Uma linha sem
                // documento válido não tem como ser reencontrada na próxima carga, e criar cliente
                // que a carga seguinte duplicaria é pior do que não criar.
                if (documento.Length is not (11 or 14)) continue;

                lojas.Add(new LojaDeClienteNoProtheus(
                    documento,
                    Texto(1),
                    Texto(2),
                    Texto(3),
                    Opcional(4),
                    Texto(5) == "F",
                    Texto(6),
                    Texto(7),
                    Opcional(8),
                    Opcional(9),
                    Opcional(10),
                    Opcional(11),
                    Opcional(12),
                    Texto(13) == "1"));
            }

            return Resultado<IReadOnlyList<LojaDeClienteNoProtheus>>.Ok(lojas);
        }
        catch (SqlException falha)
        {
            // A MENSAGEM DO CONECTOR PODE CITAR SERVIDOR E USUÁRIO: sai só o código.
            return Resultado<IReadOnlyList<LojaDeClienteNoProtheus>>.Indisponivel(
                $"O banco do Protheus não respondeu à leitura do cadastro (erro SQL {falha.Number}). Nada foi gravado.");
        }
    }

    /// <summary>O documento sem pontuação — é assim que ele é comparado em toda parte.</summary>
    /// <param name="texto">O documento como a origem o escreveu.</param>
    public static string SoDigitos(string? texto) =>
        texto is null ? string.Empty : new string([.. texto.Where(char.IsAsciiDigit)]);

    /// <summary>
    /// O CÓDIGO IBGE INTEIRO, a partir do que a SA1 guarda.
    ///
    /// <para>Medido em 24/09/2026: <c>A1_COD_MUN</c> tem <b>cinco dígitos</b> e é o código do IBGE
    /// <b>sem o prefixo da UF</b>. O código inteiro é <c>prefixo(UF) * 100000 + A1_COD_MUN</c> —
    /// conferido contra o catálogo de municípios do CRM, bate em 99,99% (1 falha em 35.030).</para>
    ///
    /// <para>O prefixo NÃO é uma tabela escrita aqui: ele sai do próprio catálogo de municípios,
    /// porque uma segunda lista de UFs no código é uma lista que um dia discorda da primeira.</para>
    /// </summary>
    /// <param name="prefixoDaUf">O prefixo IBGE da UF, do catálogo de municípios.</param>
    /// <param name="codigoDoMunicipio">O <c>A1_COD_MUN</c>.</param>
    public static int? CodigoIbge(int prefixoDaUf, string codigoDoMunicipio) =>
        int.TryParse(codigoDoMunicipio, out var numero) && numero > 0
            ? prefixoDaUf * 100_000 + numero
            : null;

    /// <summary>O nome normalizado, para comparar cadastro com origem sem tropeçar em acento.</summary>
    /// <param name="nome">O nome como veio.</param>
    public static string NormalizarNome(string? nome) => ClassificacaoDoArt.NormalizarParaComparar(nome ?? string.Empty);
}
