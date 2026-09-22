using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Tracbel.Crm.Infraestrutura.Seguranca;
using Tracbel.Crm.Integracao;
using Tracbel.Crm.Integracao.Conexoes;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Carga;

/// <summary>Roda uma carga (<c>--somente-pam</c>…) e devolve o código de saída e as últimas linhas.</summary>
internal interface IExecutorDeCargas
{
    /// <summary>Roda o modo, gravando a saída no log.</summary>
    Task<(int Codigo, IReadOnlyList<string> UltimasLinhas)> RodarAsync(string modo, string arquivoDeLog, CancellationToken ct);
}

/// <summary>
/// O ORQUESTRADOR (issue 136) — <c>--orquestrar</c>, a tarefa do Windows <c>TracbelCrmOrquestrador</c>, a cada cinco
/// minutos.
///
/// <para><b>O que ele faz, em cada volta:</b></para>
/// <list type="number">
/// <item>pega a trava (<c>sp_getapplock</c>); se outra volta ainda está rodando, sai sem fazer nada;</item>
/// <item>lê as rotinas de <c>integracao.Rotina</c> e roda, na ordem do catálogo, as vencidas — pela agenda, por
/// "Rodar agora" ou pela PRIMEIRA CARGA (nunca rodou e alguma tabela dela está vazia);</item>
/// <item>cada rotina roda as cargas dela em processos filhos deste mesmo executável, um modo por vez, com a saída
/// no log da pasta das rotinas; o resultado vai para <c>integracao.ExecucaoDeRotina</c>;</item>
/// <item>testa as APIs monitoradas cujo intervalo venceu.</item>
/// </list>
///
/// <para><b>Por que processos filhos, e não chamar a carga aqui dentro:</b> cada carga já é um programa inteiro, com
/// a trava do fluxo, a credencial e o tratamento de erro dela, e é assim que ela roda à mão. Uma falha de uma carga
/// não derruba as outras nem a volta, e o log de cada uma fica separado.</para>
///
/// <para><b>A agenda substitui as tarefas do Windows</b> <c>TracbelCrmFontesPublicas</c> e <c>TracbelCrmPrecos</c>,
/// que o <c>registrar-rotinas.ps1</c> passa a apagar.</para>
/// </summary>
internal static class Orquestrador
{
    /// <summary>A opção da linha de comando.</summary>
    public const string Opcao = "--orquestrar";

    private const string RecursoDaTrava = "TracbelCrm.Orquestrador";

    /// <summary>O teto de cada carga. A PAM leva perto de uma hora; três é folga, não expectativa.</summary>
    internal static readonly TimeSpan TempoLimiteDeCadaCarga = TimeSpan.FromHours(3);

    /// <summary>Monta tudo a partir da configuração, pega a trava e roda uma volta.</summary>
    public static async Task<int> RodarAsync(string[] args)
    {
        var configuracao = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conexao = configuracao.GetConnectionString("Crm");
        if (string.IsNullOrWhiteSpace(conexao))
        {
            Console.Error.WriteLine("Não achei a cadeia de conexão do CRM. Defina ConnectionStrings__Crm.");
            return 2;
        }

        var pastaDeLogs = ValorDepois(args, "--pasta-de-logs")
                          ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "TracbelCrm", "logs");
        Directory.CreateDirectory(pastaDeLogs);

        var opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlServer(conexao, sql => sql.CommandTimeout(60)).Options;

        await using var trava = new SqlConnection(new SqlConnectionStringBuilder(conexao) { Pooling = false, ApplicationName = "TracbelCrm.Orquestrador" }.ConnectionString);
        await trava.OpenAsync();
        if (!await ObterTravaAsync(trava))
        {
            Console.WriteLine("Outra volta do orquestrador ainda está rodando; esta não faz nada.");
            return 0;
        }

        try
        {
            var protetor = new ProtetorDeSegredos();
            var resolvedor = new ResolvedorDeConexoes(configuracao, protetor);
            var servicos = new ServiceCollection();
            servicos.AddHttpClient(TestadorDeConexoes.NomeDoCliente)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, AutomaticDecompression = System.Net.DecompressionMethods.None });
            servicos.AddHttpClient(PonteDoProtheus.NomeDoCliente);
            await using var provedor = servicos.BuildServiceProvider();
            var testador = new TestadorDeConexoes(provedor.GetRequiredService<IHttpClientFactory>());

            // O QUE A SAÍDA DE UMA CARGA NÃO PODE REPETIR: a credencial da tela e a do ambiente, se alguma carga um dia
            // imprimir o que não devia. As cargas já mascaram; isto é a segunda camada.
            var (credenciais, _) = await CredenciaisDaTela.LerAsync(conexao, configuracao, CancellationToken.None);
            var segredos = credenciais.Values
                .Concat(ConexoesDoSistema.Todas.SelectMany(c => ResolvedorDeConexoes.ChavesDoAmbiente(c.Codigo)).Select(chave => configuracao[chave]))
                .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return await ExecutarVoltaAsync(
                () => new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia),
                new ExecutorDeCargaPorProcesso(Environment.ProcessPath!, TempoLimiteDeCadaCarga),
                resolvedor, testador, pastaDeLogs, segredos, () => DateTime.UtcNow, Console.WriteLine, CancellationToken.None);
        }
        finally
        {
            await LiberarTravaAsync(trava);
        }
    }

    /// <summary>
    /// UMA VOLTA: as rotinas vencidas, depois as APIs monitoradas. Devolve 0, ou 3 quando alguma rotina falhou — o
    /// Agendador do Windows mostra o código da última volta.
    /// </summary>
    internal static async Task<int> ExecutarVoltaAsync(
        Func<CrmDbContext> abrir, IExecutorDeCargas executor, IResolvedorDeConexoes resolvedor, ITestadorDeConexoes testador,
        string pastaDeLogs, IReadOnlyList<string?> segredos, Func<DateTime> relogio, Action<string> relatar, CancellationToken ct)
    {
        var pior = 0;

        List<(string Codigo, MotivoDaExecucao Motivo)> aRodar;
        await using (var banco = abrir())
        {
            var rotinas = await banco.Rotinas.AsNoTracking().ToListAsync(ct);
            var primeiras = await RotinasComPrimeiraCargaAsync(banco, rotinas, ct);
            var agora = relogio();

            aRodar = [];
            foreach (var item in RotinasDoSistema.Todas)
            {
                if (rotinas.FirstOrDefault(r => r.Codigo == item.Codigo) is not { } rotina) continue;
                if (rotina.EstaVencida(agora)) aRodar.Add((rotina.Codigo, rotina.MotivoAgora()));
                else if (primeiras.Contains(rotina.Codigo)) aRodar.Add((rotina.Codigo, MotivoDaExecucao.PrimeiraCarga));
            }
        }

        foreach (var (codigo, motivo) in aRodar)
        {
            var resultado = await RodarRotinaAsync(abrir, executor, codigo, motivo, pastaDeLogs, segredos, relogio, relatar, ct);
            if (resultado != ResultadoDaExecucao.Sucesso) pior = 3;
        }

        await TestarMonitoradasAsync(abrir, resolvedor, testador, relogio, relatar, ct);

        if (aRodar.Count == 0) relatar($"{relogio():yyyy-MM-dd HH:mm:ss}Z — nenhuma rotina vencida.");
        return pior;
    }

    private static async Task<ResultadoDaExecucao> RodarRotinaAsync(
        Func<CrmDbContext> abrir, IExecutorDeCargas executor, string codigo, MotivoDaExecucao motivo, string pastaDeLogs,
        IReadOnlyList<string?> segredos, Func<DateTime> relogio, Action<string> relatar, CancellationToken ct)
    {
        var catalogo = RotinasDoSistema.Obter(codigo)!;
        long execucaoId;

        await using (var banco = abrir())
        {
            var rotina = await banco.Rotinas.FirstAsync(r => r.Codigo == codigo, ct);
            var pedidoPor = rotina.IniciarExecucao(relogio());
            var execucao = ExecucaoDeRotina.Iniciar(rotina.Id, motivo, pedidoPor, Environment.MachineName, relogio());
            banco.ExecucoesDeRotina.Add(execucao);
            await banco.SaveChangesAsync(ct);
            execucaoId = execucao.Id;
        }

        relatar($"{relogio():yyyy-MM-dd HH:mm:ss}Z — {catalogo.Nome} ({motivo}): {string.Join(' ', catalogo.Modos)}");

        var pior = 0;
        var partes = new List<string>();
        foreach (var modo in catalogo.Modos)
        {
            var arquivo = Path.Combine(pastaDeLogs, string.Create(CultureInfo.InvariantCulture,
                $"{codigo.ToLowerInvariant().Replace('_', '-')}-{relogio():yyyyMMdd-HHmm}.log"));

            int codigoDeSaida;
            IReadOnlyList<string> ultimas;
            try
            {
                (codigoDeSaida, ultimas) = await executor.RodarAsync(modo, arquivo, ct);
            }
            catch (Exception falha) when (falha is InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
            {
                (codigoDeSaida, ultimas) = (4, [$"a carga não começou: {falha.Message}"]);
            }

            pior = Math.Max(pior, codigoDeSaida);
            var motivoDaFalha = codigoDeSaida == 0 ? null : ultimas.LastOrDefault(l => !string.IsNullOrWhiteSpace(l));
            partes.Add(motivoDaFalha is null ? $"{modo}: ok" : $"{modo}: código {codigoDeSaida} — {motivoDaFalha}");
            relatar($"  {modo}: código {codigoDeSaida}");
        }

        var resultado = pior == 0 ? ResultadoDaExecucao.Sucesso : ResultadoDaExecucao.Falha;
        var mensagem = Sigilo.Mascarar(string.Join("; ", partes), segredos);

        await using (var banco = abrir())
        {
            var rotina = await banco.Rotinas.FirstAsync(r => r.Codigo == codigo, ct);
            rotina.EncerrarExecucao(resultado, mensagem, relogio());
            var execucao = await banco.ExecucoesDeRotina.FirstAsync(e => e.Id == execucaoId, ct);
            execucao.Encerrar(resultado, pior, mensagem, relogio());
            await banco.SaveChangesAsync(ct);
        }

        return resultado;
    }

    /// <summary>
    /// A PRIMEIRA CARGA NÃO ESPERA O CALENDÁRIO — a regra que o <c>registrar-rotinas.ps1</c> aplicava e que mudou para
    /// cá: rotina ligada que nunca rodou por aqui, com alguma tabela vazia, roda na primeira volta.
    /// </summary>
    private static async Task<HashSet<string>> RotinasComPrimeiraCargaAsync(CrmDbContext banco, IReadOnlyList<Rotina> rotinas, CancellationToken ct)
    {
        var candidatas = rotinas.Where(r => r.EstaLigada && r.UltimaExecucaoIniciadaEm is null).Select(r => r.Codigo).ToHashSet(StringComparer.Ordinal);
        var fontes = FontesPublicas.Todas.Where(f => candidatas.Contains(f.Rotina)).ToList();
        if (fontes.Count == 0) return [];

        var estados = await new RepositorioDeFontesPublicas(banco).LerAsync(fontes, ct);
        var vazias = estados.Where(e => e.Linhas == 0).Select(e => e.Fluxo).ToHashSet(StringComparer.Ordinal);
        return fontes.Where(f => vazias.Contains(f.Fluxo)).Select(f => f.Rotina).ToHashSet(StringComparer.Ordinal);
    }

    private static async Task TestarMonitoradasAsync(
        Func<CrmDbContext> abrir, IResolvedorDeConexoes resolvedor, ITestadorDeConexoes testador, Func<DateTime> relogio, Action<string> relatar, CancellationToken ct)
    {
        await using var banco = abrir();
        var agora = relogio();
        foreach (var conexao in (await banco.Conexoes.ToListAsync(ct)).Where(c => c.VerificacaoVencida(agora)))
        {
            ResultadoDoTesteDeConexao resultado;
            try
            {
                resultado = await testador.TestarAsync(resolvedor.Resolver(conexao), ct);
            }
            catch (InvalidOperationException falha)
            {
                resultado = new ResultadoDoTesteDeConexao(false, falha.Message, 0);
            }

            conexao.RegistrarVerificacao(resultado.Ok, resultado.Resumo, relogio());
            banco.VerificacoesDeConexao.Add(VerificacaoDeConexao.Registrar(conexao.Id, relogio(), null, resultado.Ok, resultado.Resumo, resultado.LatenciaMs));
            relatar($"  monitoramento {conexao.Codigo}: {(resultado.Ok ? "no ar" : "FALHOU")}");
        }

        await banco.SaveChangesAsync(ct);
    }

    private static async Task<bool> ObterTravaAsync(SqlConnection conexao)
    {
        await using var comando = conexao.CreateCommand();
        comando.CommandText =
            "DECLARE @r int; EXEC @r = sp_getapplock @Resource = @recurso, @LockMode = 'Exclusive', " +
            "@LockOwner = 'Session', @LockTimeout = 0; SELECT @r;";
        comando.Parameters.AddWithValue("@recurso", RecursoDaTrava);
        return Convert.ToInt32(await comando.ExecuteScalarAsync(), CultureInfo.InvariantCulture) >= 0;
    }

    private static async Task LiberarTravaAsync(SqlConnection conexao)
    {
        try
        {
            await using var comando = conexao.CreateCommand();
            comando.CommandText = "EXEC sp_releaseapplock @Resource = @recurso, @LockOwner = 'Session';";
            comando.Parameters.AddWithValue("@recurso", RecursoDaTrava);
            await comando.ExecuteNonQueryAsync();
        }
        catch (SqlException)
        {
            // A conexão sem pool fecha em seguida, e o SQL Server solta a trava de sessão junto.
        }
    }

    private static string? ValorDepois(string[] args, string opcao)
    {
        var i = Array.IndexOf(args, opcao);
        return i >= 0 && i + 1 < args.Length && !string.IsNullOrWhiteSpace(args[i + 1]) ? args[i + 1] : null;
    }
}

/// <summary>
/// A CARGA COMO PROCESSO FILHO — este mesmo executável, com o modo na linha de comando. A saída inteira vai para o
/// log da rotina (UTF-8); as últimas linhas voltam para o resumo da execução.
/// </summary>
/// <param name="executavel">O caminho deste executável.</param>
/// <param name="tempoLimite">Quanto esperar antes de encerrar a carga.</param>
internal sealed class ExecutorDeCargaPorProcesso(string executavel, TimeSpan tempoLimite) : IExecutorDeCargas
{
    private const int LinhasGuardadas = 20;

    /// <inheritdoc />
    public async Task<(int Codigo, IReadOnlyList<string> UltimasLinhas)> RodarAsync(string modo, string arquivoDeLog, CancellationToken ct)
    {
        var ultimas = new Queue<string>();
        var trava = new object();
        await using var log = new StreamWriter(arquivoDeLog, append: true, new UTF8Encoding(false));
        await log.WriteLineAsync($"=== {modo} em {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        void Anotar(string? linha)
        {
            if (linha is null) return;
            lock (trava)
            {
                log.WriteLine(linha);
                ultimas.Enqueue(linha);
                while (ultimas.Count > LinhasGuardadas) ultimas.Dequeue();
            }
        }

        var inicio = new ProcessStartInfo(executavel)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            WorkingDirectory = Path.GetDirectoryName(executavel)!
        };
        inicio.ArgumentList.Add(modo);

        using var processo = new Process { StartInfo = inicio };
        processo.OutputDataReceived += (_, e) => Anotar(e.Data);
        processo.ErrorDataReceived += (_, e) => Anotar(e.Data);
        processo.Start();
        processo.BeginOutputReadLine();
        processo.BeginErrorReadLine();

        using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limite.CancelAfter(tempoLimite);
        int codigo;
        try
        {
            await processo.WaitForExitAsync(limite.Token);
            codigo = processo.ExitCode;
        }
        catch (OperationCanceledException)
        {
            processo.Kill(entireProcessTree: true);
            Anotar($"encerrada: passou de {tempoLimite.TotalHours:0.#} horas");
            codigo = 124;
        }

        lock (trava) log.WriteLine($"codigo de saida de {modo}: {codigo}");
        return (codigo, ultimas.ToList());
    }
}
