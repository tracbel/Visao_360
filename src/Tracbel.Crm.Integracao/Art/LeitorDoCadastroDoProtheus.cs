using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Protheus;

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
/// A LEITURA DO CADASTRO DO PROTHEUS para a integração do ART: o parque de máquinas com o dono atual (VV1010) e a
/// completude do cadastro do comprador (SA1010).
///
/// <para><b>SÓ LEITURA.</b> A conexão declara <c>ApplicationIntent=ReadOnly</c>, as consultas são
/// <c>SELECT</c> com <c>NOLOCK</c> — para não segurar trava num ERP em produção — e nada é escrito.</para>
///
/// <para><b>O DONO DO CHASSI VEM DE <see cref="LeitorDoParqueDoProtheus"/></b>, a única leitura da VV1 no CRM
/// (decisão de 24/09/2026). Até essa data esta classe lia <c>VV1_CLIULV</c>, o cliente da última venda, dizendo que a
/// VV1 não tinha a loja — e a loja existe: <c>VV1_CLJULV</c> ao lado do cliente da última venda, e
/// <c>VV1_LJPATU</c> ao lado do proprietário atual (<c>VV1_PROATU</c>), que é o dono que a oficina mantém. O cliente
/// da última venda estava vazio em 28.979 das 32.595 máquinas com chassi; pelo proprietário atual, 30.765 chegam a um
/// documento só.</para>
/// </summary>
/// <param name="opcoes">A configuração.</param>
public sealed class LeitorDoCadastroDoProtheus(OpcoesDoBancoDoProtheus opcoes)
{
    /// <summary>Lê o parque de máquinas e o cadastro dos documentos pedidos.</summary>
    /// <param name="documentos">Os documentos cujo cadastro interessa.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<(ParqueNoProtheus Parque, IReadOnlyDictionary<string, CadastroNoProtheus> Cadastros)>>
        LerAsync(IReadOnlySet<string> documentos, CancellationToken ct)
    {
        if (!opcoes.EstaConfigurada)
            return Resultado<(ParqueNoProtheus, IReadOnlyDictionary<string, CadastroNoProtheus>)>.Indisponivel(
                "A conferência no Protheus exige ProtheusBanco__Servidor, __Banco, __Usuario e __Senha.");

        var parque = await new LeitorDoParqueDoProtheus(opcoes).LerAsync(ct);
        if (!parque.EhSucesso)
            return Resultado<(ParqueNoProtheus, IReadOnlyDictionary<string, CadastroNoProtheus>)>.Indisponivel(parque.Erro!);

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

            var porDocumento = new Dictionary<string, List<(bool Bloqueado, bool Endereco, bool Municipio, bool Inscricao, string Nome)>>(StringComparer.Ordinal);

            await using (var comando = new SqlCommand(
                "SELECT RTRIM(A1_CGC), RTRIM(A1_MSBLQL), RTRIM(A1_END), RTRIM(A1_COD_MUN), RTRIM(A1_INSCR), RTRIM(A1_NOME) " +
                "FROM dbo.SA1010 WITH (NOLOCK) WHERE D_E_L_E_T_ = ' '", conexao) { CommandTimeout = 300 })
            await using (var leitor = await comando.ExecuteReaderAsync(ct))
            {
                while (await leitor.ReadAsync(ct))
                {
                    var documento = LeitorDeClientesDoProtheus.SoDigitos(leitor.IsDBNull(0) ? null : leitor.GetString(0));
                    if (documento.Length is not (11 or 14) || !documentos.Contains(documento)) continue;

                    if (!porDocumento.TryGetValue(documento, out var lojas))
                        porDocumento[documento] = lojas = [];

                    lojas.Add((
                        (leitor.IsDBNull(1) ? string.Empty : leitor.GetString(1)) == "1",
                        !leitor.IsDBNull(2) && !string.IsNullOrWhiteSpace(leitor.GetString(2)),
                        !leitor.IsDBNull(3) && !string.IsNullOrWhiteSpace(leitor.GetString(3)),
                        !leitor.IsDBNull(4) && !string.IsNullOrWhiteSpace(leitor.GetString(4)),
                        ClassificacaoDoArt.NormalizarParaComparar(leitor.IsDBNull(5) ? string.Empty : leitor.GetString(5))));
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

            return Resultado<(ParqueNoProtheus, IReadOnlyDictionary<string, CadastroNoProtheus>)>.Ok((parque.Valor, cadastros));
        }
        catch (SqlException falha)
        {
            return Resultado<(ParqueNoProtheus, IReadOnlyDictionary<string, CadastroNoProtheus>)>.Indisponivel(
                $"O banco do Protheus não respondeu à leitura (erro SQL {falha.Number}).");
        }
    }
}
