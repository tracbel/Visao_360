using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Protheus;
using Tracbel.Crm.Integracao.Vortice;

namespace Tracbel.Crm.Integracao.Conexoes;

/// <summary>
/// QUE CREDENCIAL CADA CONEXÃO USA (issue 136): a gravada pela tela, se estiver completa; senão a das variáveis de
/// ambiente do servidor — a forma de antes da tela, que continua valendo como reserva.
///
/// <para><b>As chaves de ambiente são as mesmas que a carga e a API sempre leram</b> (<c>Protheus__Senha</c>,
/// <c>Art__Servidor</c>, <c>Vortice__Conexao</c>…), tiradas das próprias classes de opções. Nenhum nome novo para
/// alguém configurar.</para>
/// </summary>
/// <param name="configuracao">A configuração do processo — onde as variáveis de ambiente chegam.</param>
/// <param name="protetor">Quem abre a credencial da tela.</param>
public sealed class ResolvedorDeConexoes(IConfiguration configuracao, IProtetorDeSegredos protetor) : IResolvedorDeConexoes
{
    /// <inheritdoc />
    public ConexaoResolvida Resolver(Conexao conexao)
    {
        var origem = OrigemDe(conexao);

        return origem switch
        {
            OrigemDaCredencial.Tela => DaTela(conexao),
            OrigemDaCredencial.Ambiente => DoAmbiente(conexao),
            _ => new ConexaoResolvida(
                conexao.Codigo, conexao.Tipo, origem, conexao.Endereco, conexao.Porta, conexao.Banco, conexao.Objeto, conexao.Usuario,
                conexao.Tipo == TipoDeConexao.Monitorada && conexao.TemSegredo ? protetor.Revelar(conexao.SegredoProtegido!) : null,
                conexao.NomeDoCabecalho, conexao.StatusEsperado, null)
        };
    }

    /// <inheritdoc />
    public OrigemDaCredencial OrigemDe(Conexao conexao)
    {
        if (conexao.Tipo is TipoDeConexao.FontePublica or TipoDeConexao.Monitorada) return OrigemDaCredencial.NaoSeAplica;
        if (conexao.EstaConfiguradaPelaTela) return OrigemDaCredencial.Tela;
        return ChavesDoAmbiente(conexao.Codigo) is { Count: > 0 } chaves && chaves.All(c => !string.IsNullOrWhiteSpace(configuracao[c]))
            ? OrigemDaCredencial.Ambiente
            : OrigemDaCredencial.Nenhuma;
    }

    /// <summary>As chaves que o ambiente precisa ter para a conexão valer sem a tela.</summary>
    public static IReadOnlyList<string> ChavesDoAmbiente(string codigo) => codigo switch
    {
        ConexoesDoSistema.Protheus => [$"{OpcoesDoProtheus.Secao}:Base", $"{OpcoesDoProtheus.Secao}:Usuario", $"{OpcoesDoProtheus.Secao}:Senha"],
        ConexoesDoSistema.ProtheusBanco =>
            [$"{OpcoesDoBancoDoProtheus.Secao}:Servidor", $"{OpcoesDoBancoDoProtheus.Secao}:Banco", $"{OpcoesDoBancoDoProtheus.Secao}:Usuario", $"{OpcoesDoBancoDoProtheus.Secao}:Senha"],
        ConexoesDoSistema.Art =>
            [$"{OpcoesDoArt.Secao}:Servidor", $"{OpcoesDoArt.Secao}:Banco", $"{OpcoesDoArt.Secao}:Usuario", $"{OpcoesDoArt.Secao}:Senha", $"{OpcoesDoArt.Secao}:Visao"],
        ConexoesDoSistema.Vortice => [$"{OpcoesDoVortice.Secao}:Conexao"],
        _ => []
    };

    /// <summary>
    /// A CREDENCIAL DA TELA NO FORMATO DA CONFIGURAÇÃO — o que a carga e a API sobrepõem às variáveis de ambiente.
    /// Só as conexões do sistema que a carga e as pontes leem; as outras não têm seção.
    /// </summary>
    public static IReadOnlyDictionary<string, string?> ParaConfiguracao(ConexaoResolvida conexao)
    {
        if (conexao.Origem != OrigemDaCredencial.Tela) return new Dictionary<string, string?>();

        return conexao.Codigo switch
        {
            ConexoesDoSistema.Protheus => new Dictionary<string, string?>
            {
                [$"{OpcoesDoProtheus.Secao}:Base"] = conexao.Endereco,
                [$"{OpcoesDoProtheus.Secao}:Usuario"] = conexao.Usuario,
                [$"{OpcoesDoProtheus.Secao}:Senha"] = conexao.Segredo
            },
            ConexoesDoSistema.ProtheusBanco => new Dictionary<string, string?>
            {
                [$"{OpcoesDoBancoDoProtheus.Secao}:Servidor"] = conexao.Endereco,
                [$"{OpcoesDoBancoDoProtheus.Secao}:Banco"] = conexao.Banco,
                [$"{OpcoesDoBancoDoProtheus.Secao}:Usuario"] = conexao.Usuario,
                [$"{OpcoesDoBancoDoProtheus.Secao}:Senha"] = conexao.Segredo
            },
            ConexoesDoSistema.Art => new Dictionary<string, string?>
            {
                [$"{OpcoesDoArt.Secao}:Servidor"] = conexao.Endereco,
                [$"{OpcoesDoArt.Secao}:Porta"] = (conexao.Porta ?? 3306).ToString(System.Globalization.CultureInfo.InvariantCulture),
                [$"{OpcoesDoArt.Secao}:Banco"] = conexao.Banco,
                [$"{OpcoesDoArt.Secao}:Visao"] = conexao.Objeto,
                [$"{OpcoesDoArt.Secao}:Usuario"] = conexao.Usuario,
                [$"{OpcoesDoArt.Secao}:Senha"] = conexao.Segredo
            },
            ConexoesDoSistema.Vortice => new Dictionary<string, string?>
            {
                [$"{OpcoesDoVortice.Secao}:Conexao"] = conexao.CadeiaDeConexao
            },
            _ => new Dictionary<string, string?>()
        };
    }

    private ConexaoResolvida DaTela(Conexao conexao)
    {
        var segredo = protetor.Revelar(conexao.SegredoProtegido!);
        var opcoes = new OpcoesDeBanco(conexao.Endereco, conexao.Porta, conexao.Banco, conexao.Usuario, segredo);
        var cadeia = conexao.Tipo switch
        {
            TipoDeConexao.SqlServer => CadeiaDoSqlServer(opcoes),
            TipoDeConexao.MySql => CadeiaDoMySql(opcoes),
            _ => null
        };

        return new ConexaoResolvida(
            conexao.Codigo, conexao.Tipo, OrigemDaCredencial.Tela, conexao.Endereco, conexao.Porta, conexao.Banco, conexao.Objeto,
            conexao.Usuario, segredo, conexao.NomeDoCabecalho, conexao.StatusEsperado, cadeia);
    }

    private ConexaoResolvida DoAmbiente(Conexao conexao)
    {
        switch (conexao.Codigo)
        {
            case ConexoesDoSistema.Protheus:
                return new ConexaoResolvida(
                    conexao.Codigo, conexao.Tipo, OrigemDaCredencial.Ambiente, configuracao[$"{OpcoesDoProtheus.Secao}:Base"], null, null, null,
                    configuracao[$"{OpcoesDoProtheus.Secao}:Usuario"], configuracao[$"{OpcoesDoProtheus.Secao}:Senha"], null, 200, null);

            case ConexoesDoSistema.ProtheusBanco:
            {
                var opcoes = new OpcoesDeBanco(
                    configuracao[$"{OpcoesDoBancoDoProtheus.Secao}:Servidor"], null, configuracao[$"{OpcoesDoBancoDoProtheus.Secao}:Banco"],
                    configuracao[$"{OpcoesDoBancoDoProtheus.Secao}:Usuario"], configuracao[$"{OpcoesDoBancoDoProtheus.Secao}:Senha"]);
                return new ConexaoResolvida(conexao.Codigo, conexao.Tipo, OrigemDaCredencial.Ambiente, opcoes.Servidor, null, opcoes.Banco, null,
                    opcoes.Usuario, opcoes.Senha, null, 200, CadeiaDoSqlServer(opcoes));
            }

            case ConexoesDoSistema.Art:
            {
                var opcoes = new OpcoesDeBanco(
                    configuracao[$"{OpcoesDoArt.Secao}:Servidor"],
                    int.TryParse(configuracao[$"{OpcoesDoArt.Secao}:Porta"], out var porta) ? porta : 3306,
                    configuracao[$"{OpcoesDoArt.Secao}:Banco"], configuracao[$"{OpcoesDoArt.Secao}:Usuario"], configuracao[$"{OpcoesDoArt.Secao}:Senha"]);
                return new ConexaoResolvida(conexao.Codigo, conexao.Tipo, OrigemDaCredencial.Ambiente, opcoes.Servidor, opcoes.Porta, opcoes.Banco,
                    configuracao[$"{OpcoesDoArt.Secao}:Visao"], opcoes.Usuario, opcoes.Senha, null, 200, CadeiaDoMySql(opcoes));
            }

            case ConexoesDoSistema.Vortice:
                return new ConexaoResolvida(conexao.Codigo, conexao.Tipo, OrigemDaCredencial.Ambiente, null, null, null, null, null, null, null, 200,
                    configuracao[$"{OpcoesDoVortice.Secao}:Conexao"]);

            default:
                return new ConexaoResolvida(conexao.Codigo, conexao.Tipo, OrigemDaCredencial.Nenhuma, conexao.Endereco, null, null, null, null, null, null, 200, null);
        }
    }

    // O CONSTRUTOR DA CADEIA ESCAPA CADA VALOR: uma senha com ponto e vírgula não vira um parâmetro a mais. A senha passa
    // por um objeto de opções, como em toda leitura do CRM — é o que a varredura de segredos reconhece como referência.
    private static string CadeiaDoSqlServer(OpcoesDeBanco opcoes) => new SqlConnectionStringBuilder
    {
        DataSource = opcoes.Servidor,
        InitialCatalog = opcoes.Banco,
        UserID = opcoes.Usuario,
        Password = opcoes.Senha,
        TrustServerCertificate = true,
        ApplicationIntent = ApplicationIntent.ReadOnly,
        ConnectTimeout = 15
    }.ConnectionString;

    private static string CadeiaDoMySql(OpcoesDeBanco opcoes) => new MySqlConnectionStringBuilder
    {
        Server = opcoes.Servidor,
        Port = (uint)(opcoes.Porta ?? 3306),
        Database = opcoes.Banco,
        UserID = opcoes.Usuario,
        Password = opcoes.Senha,
        ConnectionTimeout = 15
    }.ConnectionString;

    /// <summary>O que uma cadeia de conexão de banco precisa. Sem <c>ToString</c> com a senha: é registro privado e só vive aqui.</summary>
    private sealed record OpcoesDeBanco(string? Servidor, int? Porta, string? Banco, string? Usuario, string? Senha)
    {
        public override string ToString() => $"OpcoesDeBanco {{ Servidor = {Servidor}, Banco = {Banco} }}";
    }
}