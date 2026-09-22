using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.Art;

/// <summary>
/// A configuração da leitura DIRETA do banco do Protheus — só <c>SELECT</c>, com intenção de leitura.
///
/// <para>As credenciais chegam por <c>ProtheusBanco__Servidor</c>, <c>ProtheusBanco__Banco</c>,
/// <c>ProtheusBanco__Usuario</c> e <c>ProtheusBanco__Senha</c>; nenhum valor mora no repositório.</para>
/// </summary>
public sealed class OpcoesDoBancoDoProtheus
{
    /// <summary>O nome da seção na configuração (<c>ProtheusBanco__Servidor</c>…).</summary>
    public const string Secao = "ProtheusBanco";

    /// <summary>O servidor, com a porta depois da vírgula.</summary>
    public string? Servidor { get; set; }

    /// <summary>O banco.</summary>
    public string? Banco { get; set; }

    /// <summary>O usuário de leitura.</summary>
    public string? Usuario { get; set; }

    /// <summary>A senha.</summary>
    public string? Senha { get; set; }

    /// <summary>Se a configuração tem o mínimo para tentar uma conexão.</summary>
    public bool EstaConfigurada =>
        !string.IsNullOrWhiteSpace(Servidor) && !string.IsNullOrWhiteSpace(Banco)
        && !string.IsNullOrWhiteSpace(Usuario) && !string.IsNullOrWhiteSpace(Senha);
}

/// <summary>O dono de um chassi segundo o cadastro de veículos do Protheus (VV1).</summary>
/// <param name="Chassi">O chassi, como o Protheus escreve.</param>
/// <param name="Documento">O CPF ou CNPJ do dono, quando o código do cliente resolve para UM documento só.</param>
/// <param name="Ambiguo">Verdadeiro quando o código do cliente tem lojas com documentos diferentes.</param>
public sealed record DonoNoProtheus(string Chassi, string? Documento, bool Ambiguo);

/// <summary>O que o cadastro de clientes do Protheus (SA1) diz de um documento — sem nome nem endereço.</summary>
/// <param name="Documento">O CPF ou CNPJ.</param>
/// <param name="Bloqueado">Se todas as lojas com o documento estão bloqueadas.</param>
/// <param name="TemEndereco">Se alguma loja tem endereço.</param>
/// <param name="TemMunicipio">Se alguma loja tem código de município.</param>
/// <param name="TemInscricaoEstadual">Se alguma loja tem inscrição estadual.</param>
/// <param name="NomeNormalizado">O nome sem acento, caixa e pontuação — só para comparar.</param>
public sealed record CadastroNoProtheus(
    string Documento, bool Bloqueado, bool TemEndereco, bool TemMunicipio, bool TemInscricaoEstadual, string NomeNormalizado);

/// <summary>
/// A LEITURA DO CADASTRO DO PROTHEUS para a integração do ART: o dono do chassi (VV1010) e a
/// completude do cadastro do comprador (SA1010).
///
/// <para><b>SÓ LEITURA.</b> A conexão declara <c>ApplicationIntent=ReadOnly</c>, as consultas são
/// <c>SELECT</c> com <c>NOLOCK</c> — para não segurar trava num ERP em produção — e nada é escrito.</para>
///
/// <para><b>A VV1010 guarda o código do cliente, sem a loja</b> (medido em 14/09/2026: não existe
/// <c>VV1_LJULV</c>). Um código com lojas de CNPJs diferentes não diz de qual documento é a
/// máquina; nesse caso o dono fica <see cref="DonoNoProtheus.Ambiguo"/>, e nenhuma divergência é
/// afirmada a partir dele.</para>
/// </summary>
/// <param name="opcoes">A configuração.</param>
public sealed class LeitorDoCadastroDoProtheus(OpcoesDoBancoDoProtheus opcoes)
{
    /// <summary>Lê os donos dos chassis e o cadastro dos documentos pedidos.</summary>
    /// <param name="documentos">Os documentos cujo cadastro interessa.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<(IReadOnlyDictionary<string, DonoNoProtheus> Donos, IReadOnlyDictionary<string, CadastroNoProtheus> Cadastros)>>
        LerAsync(IReadOnlySet<string> documentos, CancellationToken ct)
    {
        if (!opcoes.EstaConfigurada)
            return Resultado<(IReadOnlyDictionary<string, DonoNoProtheus>, IReadOnlyDictionary<string, CadastroNoProtheus>)>.Indisponivel(
                "A conferência no Protheus exige ProtheusBanco__Servidor, __Banco, __Usuario e __Senha.");

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

            // Código → documentos distintos das lojas ativas no cadastro.
            var documentosPorCodigo = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            var porDocumento = new Dictionary<string, List<(bool Bloqueado, bool Endereco, bool Municipio, bool Inscricao, string Nome)>>(StringComparer.Ordinal);

            await using (var comando = new SqlCommand(
                "SELECT RTRIM(A1_COD), RTRIM(A1_CGC), RTRIM(A1_MSBLQL), RTRIM(A1_END), RTRIM(A1_COD_MUN), RTRIM(A1_INSCR), RTRIM(A1_NOME) " +
                "FROM dbo.SA1010 WITH (NOLOCK) WHERE D_E_L_E_T_ = ' '", conexao) { CommandTimeout = 300 })
            await using (var leitor = await comando.ExecuteReaderAsync(ct))
            {
                while (await leitor.ReadAsync(ct))
                {
                    var codigo = leitor.IsDBNull(0) ? string.Empty : leitor.GetString(0);
                    var documento = SoDigitos(leitor.IsDBNull(1) ? null : leitor.GetString(1));
                    if (documento.Length is not (11 or 14)) continue;

                    if (!documentosPorCodigo.TryGetValue(codigo, out var docs))
                        documentosPorCodigo[codigo] = docs = new HashSet<string>(StringComparer.Ordinal);
                    docs.Add(documento);

                    if (!documentos.Contains(documento)) continue;

                    if (!porDocumento.TryGetValue(documento, out var lojas))
                        porDocumento[documento] = lojas = [];

                    lojas.Add((
                        (leitor.IsDBNull(2) ? string.Empty : leitor.GetString(2)) == "1",
                        !leitor.IsDBNull(3) && !string.IsNullOrWhiteSpace(leitor.GetString(3)),
                        !leitor.IsDBNull(4) && !string.IsNullOrWhiteSpace(leitor.GetString(4)),
                        !leitor.IsDBNull(5) && !string.IsNullOrWhiteSpace(leitor.GetString(5)),
                        ClassificacaoDoArt.NormalizarParaComparar(leitor.IsDBNull(6) ? string.Empty : leitor.GetString(6))));
                }
            }

            var donos = new Dictionary<string, DonoNoProtheus>(StringComparer.Ordinal);
            await using (var comando = new SqlCommand(
                "SELECT UPPER(RTRIM(VV1_CHASSI)), RTRIM(VV1_CLIULV) FROM dbo.VV1010 WITH (NOLOCK) " +
                "WHERE D_E_L_E_T_ = ' ' AND RTRIM(VV1_CHASSI) <> ''", conexao) { CommandTimeout = 300 })
            await using (var leitor = await comando.ExecuteReaderAsync(ct))
            {
                while (await leitor.ReadAsync(ct))
                {
                    var chassi = leitor.GetString(0);
                    var codigo = leitor.IsDBNull(1) ? string.Empty : leitor.GetString(1);

                    DonoNoProtheus dono = documentosPorCodigo.TryGetValue(codigo, out var docs)
                        ? docs.Count == 1 ? new(chassi, docs.First(), false) : new(chassi, null, true)
                        : new(chassi, null, false);

                    // O MESMO CHASSI EM DUAS LINHAS com donos diferentes também é ambíguo.
                    if (donos.TryGetValue(chassi, out var anterior) && anterior.Documento != dono.Documento)
                        dono = new(chassi, null, true);

                    donos[chassi] = dono;
                }
            }

            var cadastros = porDocumento.ToDictionary(
                p => p.Key,
                p => new CadastroNoProtheus(
                    p.Key,
                    p.Value.All(l => l.Bloqueado),
                    p.Value.Any(l => l.Endereco),
                    p.Value.Any(l => l.Municipio),
                    p.Value.Any(l => l.Inscricao),
                    p.Value[0].Nome),
                StringComparer.Ordinal);

            return Resultado<(IReadOnlyDictionary<string, DonoNoProtheus>, IReadOnlyDictionary<string, CadastroNoProtheus>)>.Ok(
                (donos, cadastros));
        }
        catch (SqlException falha)
        {
            return Resultado<(IReadOnlyDictionary<string, DonoNoProtheus>, IReadOnlyDictionary<string, CadastroNoProtheus>)>.Indisponivel(
                $"O banco do Protheus não respondeu à leitura (erro SQL {falha.Number}).");
        }
    }

    private static string SoDigitos(string? texto) =>
        texto is null ? string.Empty : new string([.. texto.Where(char.IsAsciiDigit)]);
}
