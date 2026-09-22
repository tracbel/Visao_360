using System.Diagnostics;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Integracao.Conexoes;

/// <summary>
/// O BOTÃO "TESTAR" (issue 136) — executado no servidor, SÓ LEITURA, e sem nunca devolver a credencial.
///
/// <list type="bullet">
/// <item><b>Protheus (API REST):</b> pede o token e lê UMA linha da SA1 (<c>A1_FILIAL</c>) pelo <c>genericQuery</c>,
/// que é GET. Prova endereço, credencial e permissão de leitura; nenhum dado volta para a tela.</item>
/// <item><b>Bancos (SQL Server e MySQL):</b> abre a sessão como somente leitura e roda uma consulta que não traz
/// dado — <c>SELECT 1</c>; no ART, <c>SELECT 1 FROM &lt;visão&gt; LIMIT 1</c>, que prova que a visão existe e é
/// legível.</item>
/// <item><b>Fontes públicas e APIs monitoradas:</b> um GET que lê só o cabeçalho da resposta
/// (<see cref="HttpCompletionOption.ResponseHeadersRead"/>) — o ZIP da ANP não é baixado para dizer que ela está no
/// ar. Sem seguir redirecionamento: na monitorada o status é o que foi cadastrado; na fonte pública, 2xx e 3xx
/// contam como no ar.</item>
/// </list>
/// </summary>
/// <param name="fabrica">A fábrica de clientes HTTP.</param>
public sealed class TestadorDeConexoes(IHttpClientFactory fabrica) : ITestadorDeConexoes
{
    /// <summary>O nome do cliente HTTP dos testes: sem descompressão e sem seguir redirecionamento.</summary>
    public const string NomeDoCliente = "TesteDeConexao";

    /// <summary>Quanto o teste espera, no máximo.</summary>
    public static readonly TimeSpan TempoLimite = TimeSpan.FromSeconds(20);

    /// <inheritdoc />
    public async Task<ResultadoDoTesteDeConexao> TestarAsync(ConexaoResolvida conexao, CancellationToken ct)
    {
        var cronometro = Stopwatch.StartNew();
        string[] segredos = [conexao.Usuario ?? string.Empty, conexao.Segredo ?? string.Empty];

        if (conexao.Origem == OrigemDaCredencial.Nenhuma)
            return new ResultadoDoTesteDeConexao(false,
                "Sem credencial: configure pela tela ou pelas variáveis de ambiente do servidor. Nada foi consultado.", 0);

        try
        {
            using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
            limite.CancelAfter(TempoLimite + TimeSpan.FromSeconds(5));

            var (ok, resumo) = conexao.Tipo switch
            {
                TipoDeConexao.ApiRest => await TestarProtheusAsync(conexao, limite.Token),
                TipoDeConexao.SqlServer => await TestarSqlServerAsync(conexao, limite.Token),
                TipoDeConexao.MySql => await TestarMySqlAsync(conexao, limite.Token),
                _ => await TestarHttpAsync(conexao, limite.Token)
            };

            return new ResultadoDoTesteDeConexao(ok, Sigilo.Mascarar(resumo, segredos), (int)cronometro.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return new ResultadoDoTesteDeConexao(false, $"Não respondeu em {TempoLimite.TotalSeconds:0} segundos.", (int)cronometro.ElapsedMilliseconds);
        }
        catch (Exception falha)
        {
            var causa = falha.GetBaseException();
            var texto = ReferenceEquals(causa, falha) ? $"{falha.GetType().Name}: {falha.Message}" : $"{falha.GetType().Name}: {falha.Message} | {causa.Message}";
            return new ResultadoDoTesteDeConexao(false, Sigilo.Mascarar(texto, segredos), (int)cronometro.ElapsedMilliseconds);
        }
    }

    private async Task<(bool, string)> TestarHttpAsync(ConexaoResolvida conexao, CancellationToken ct)
    {
        if (!Uri.TryCreate(conexao.Endereco, UriKind.Absolute, out var endereco))
            return (false, "O endereço não é uma URL completa.");

        using var cliente = fabrica.CreateClient(NomeDoCliente);
        cliente.Timeout = TempoLimite;
        using var pedido = new HttpRequestMessage(HttpMethod.Get, endereco);
        if (conexao.Tipo == TipoDeConexao.Monitorada && !string.IsNullOrWhiteSpace(conexao.NomeDoCabecalho) && !string.IsNullOrEmpty(conexao.Segredo))
            pedido.Headers.TryAddWithoutValidation(conexao.NomeDoCabecalho, conexao.Segredo);

        using var resposta = await cliente.SendAsync(pedido, HttpCompletionOption.ResponseHeadersRead, ct);
        var codigo = (int)resposta.StatusCode;
        var descricao = string.IsNullOrWhiteSpace(resposta.ReasonPhrase) ? string.Empty : $" {resposta.ReasonPhrase}";

        if (conexao.Tipo == TipoDeConexao.Monitorada)
            return codigo == conexao.StatusEsperado
                ? (true, $"HTTP {codigo}{descricao}, o status esperado.")
                : (false, $"HTTP {codigo}{descricao}; o esperado era {conexao.StatusEsperado}.");

        return codigo is >= 200 and < 400
            ? (true, codigo >= 300 ? $"HTTP {codigo}{descricao}: respondeu com redirecionamento — a fonte está no ar." : $"HTTP {codigo}{descricao}: a fonte está no ar.")
            : (false, $"HTTP {codigo}{descricao}: a fonte respondeu com erro.");
    }

    private async Task<(bool, string)> TestarProtheusAsync(ConexaoResolvida conexao, CancellationToken ct)
    {
        var opcoes = new OpcoesDoProtheus
        {
            Base = conexao.Endereco?.TrimEnd('/'),
            Usuario = conexao.Usuario,
            Senha = conexao.Segredo,
            TamanhoDaPagina = 1,
            TempoLimiteSegundos = (int)TempoLimite.TotalSeconds
        };

        var pagina = await new PonteDoProtheus(fabrica, Options.Create(opcoes)).LerPaginaAsync("SA1", "A1_FILIAL", null, 1, null, ct);
        return pagina.EhSucesso
            ? (true, "Autenticou (token) e leu uma linha da SA1 pelo genericQuery — só leitura; nenhum dado foi guardado.")
            : (false, pagina.Erro ?? "O Protheus não respondeu como esperado.");
    }

    private static async Task<(bool, string)> TestarSqlServerAsync(ConexaoResolvida conexao, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(conexao.CadeiaDeConexao)) return (false, "A conexão não tem servidor e banco configurados.");

        var cadeia = new SqlConnectionStringBuilder(conexao.CadeiaDeConexao)
        {
            ApplicationIntent = ApplicationIntent.ReadOnly,
            ConnectTimeout = (int)TempoLimite.TotalSeconds,
            ApplicationName = "TracbelCrm.TesteDeConexao"
        };

        await using var banco = new SqlConnection(cadeia.ConnectionString);
        await banco.OpenAsync(ct);
        await using var comando = banco.CreateCommand();
        comando.CommandText = "SELECT 1";
        comando.CommandTimeout = (int)TempoLimite.TotalSeconds;
        await comando.ExecuteScalarAsync(ct);

        return (true, $"Conectou em {cadeia.DataSource} / {cadeia.InitialCatalog} e respondeu a SELECT 1 — sessão declarada somente leitura.");
    }

    private static async Task<(bool, string)> TestarMySqlAsync(ConexaoResolvida conexao, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(conexao.Objeto)) return (false, "Falta a visão lida.");

        var cadeia = new MySqlConnectionStringBuilder(conexao.CadeiaDeConexao ?? string.Empty)
        {
            ConnectionTimeout = (uint)TempoLimite.TotalSeconds,
            DefaultCommandTimeout = (uint)TempoLimite.TotalSeconds
        };
        if (string.IsNullOrWhiteSpace(cadeia.Server))
        {
            cadeia.Server = conexao.Endereco;
            cadeia.Port = (uint)(conexao.Porta ?? 3306);
            cadeia.Database = conexao.Banco;
            cadeia.UserID = conexao.Usuario;
            cadeia.Password = conexao.Segredo;
        }

        // A VISÃO ENTRA NA CONSULTA, e por isso só com o formato conferido (letras, números, sublinhado e um ponto)
        // e entre crases — a mesma conferência que o domínio faz antes de gravar.
        var visao = string.Join('.', conexao.Objeto.Split('.').Select(parte => $"`{parte.Replace("`", string.Empty, StringComparison.Ordinal)}`"));

        await using var banco = new MySqlConnection(cadeia.ConnectionString);
        await banco.OpenAsync(ct);
        await using (var somenteLeitura = banco.CreateCommand())
        {
            somenteLeitura.CommandText = "SET SESSION TRANSACTION READ ONLY";
            await somenteLeitura.ExecuteNonQueryAsync(ct);
        }

        await using var comando = banco.CreateCommand();
        comando.CommandText = $"SELECT 1 FROM {visao} LIMIT 1";
        await comando.ExecuteScalarAsync(ct);

        return (true, string.Create(CultureInfo.InvariantCulture,
            $"Conectou em {cadeia.Server}:{cadeia.Port} / {cadeia.Database} e leu a visão {conexao.Objeto} — sessão somente leitura."));
    }
}
