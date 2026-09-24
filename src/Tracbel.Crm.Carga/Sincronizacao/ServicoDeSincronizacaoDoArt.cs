using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Multiempresa;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Carga.Sincronizacao;

/// <summary>
/// O SERVIÇO DO WINDOWS <c>TracbelCrmSincronizacaoArt</c> (documento 35, seção 11).
///
/// <para>O mesmo executável da carga, com <c>--servico-art</c>. Sobe com o Windows, sem terminal nem
/// sessão aberta, e roda um ciclo a cada <c>Sincronizacao:Art:IntervaloMinutos</c>. O que acontece fica
/// em <c>integracao.ExecucaoDeSincronizacao</c> (cada ciclo), em <c>integracao.PontoDeSincronismo</c>
/// (o último sucesso) e no Log de Aplicativo do Windows, com a origem <c>TracbelCrmSincronizacaoArt</c>.</para>
/// </summary>
internal static class HospedagemDaSincronizacao
{
    /// <summary>O nome do serviço e da origem no Log de Aplicativo.</summary>
    public const string NomeDoServico = "TracbelCrmSincronizacaoArt";

    /// <summary>Sobe o serviço. Devolve o código de saída.</summary>
    /// <param name="args">A linha de comando.</param>
    public static async Task<int> RodarAsync(string[] args)
    {
        var construtor = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

        construtor.Services.AddWindowsService(o => o.ServiceName = NomeDoServico);
        if (OperatingSystem.IsWindows()) ConfigurarLogDeAplicativo(construtor);

        var opcoes = new OpcoesDaSincronizacaoDoArt();
        construtor.Configuration.GetSection(OpcoesDaSincronizacaoDoArt.Secao).Bind(opcoes);
        construtor.Services.AddSingleton(opcoes);
        construtor.Services.AddHostedService<ServicoDeSincronizacaoDoArt>();

        using var host = construtor.Build();
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Tracbel.Crm.Sincronizacao");

        var problema = ProblemaDeConfiguracao(construtor.Configuration, opcoes);
        if (problema is not null)
        {
            logger.LogError("O serviço {Servico} não vai sincronizar: {Problema}", NomeDoServico, problema);
            return 2;
        }

        await host.RunAsync();
        return 0;
    }

    /// <summary>O Log de Aplicativo com a origem do serviço, a partir do nível Information para o que é nosso.</summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void ConfigurarLogDeAplicativo(HostApplicationBuilder construtor)
    {
        construtor.Services.Configure<EventLogSettings>(ConfigurarOrigemNoLog);
        construtor.Logging.AddFilter<EventLogLoggerProvider>("Tracbel", LogLevel.Information);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void ConfigurarOrigemNoLog(EventLogSettings configuracao)
    {
        configuracao.SourceName = NomeDoServico;
        configuracao.LogName = "Application";
    }

    private static string? ProblemaDeConfiguracao(IConfiguration configuracao, OpcoesDaSincronizacaoDoArt opcoes)
    {
        if (!opcoes.Habilitada)
            return "Sincronizacao:Art:Habilitada não é true. Ela só existe no appsettings.Production.json do servidor — " +
                   "com a configuração de desenvolvimento o serviço não grava em banco nenhum.";

        if (opcoes.Validar() is { } faixa) return faixa;

        if (string.IsNullOrWhiteSpace(configuracao.GetConnectionString("Crm")))
            return "Falta ConnectionStrings:Crm.";

        var art = new OpcoesDoArt();
        configuracao.GetSection(OpcoesDoArt.Secao).Bind(art);
        return art.EstaConfigurada ? null : "Falta a seção Art (Servidor, Banco, Usuario, Senha e Visao).";
    }
}

/// <summary>O laço do serviço: espera inicial, ciclo, intervalo — até o Windows pedir a parada.</summary>
internal sealed class ServicoDeSincronizacaoDoArt(
    IConfiguration configuracao,
    OpcoesDaSincronizacaoDoArt opcoes,
    ILoggerFactory fabricaDeLog) : BackgroundService
{
    private static readonly TimeSpan EsperaQuandoNemComecou = TimeSpan.FromMinutes(5);

    private readonly ILogger _logger = fabricaDeLog.CreateLogger("Tracbel.Crm.Sincronizacao");

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken parada)
    {
        var conexao = configuracao.GetConnectionString("Crm")!;
        var destino = new SqlConnectionStringBuilder(conexao);
        var intervalo = TimeSpan.FromMinutes(opcoes.IntervaloMinutos);

        _logger.LogInformation(
            "Serviço {Servico} iniciado na máquina {Maquina}. Destino: {Servidor} / {Banco}. Intervalo: {Intervalo} min; " +
            "tentativas por ciclo: {Tentativas}. O ART é lido em sessão somente leitura.",
            HospedagemDaSincronizacao.NomeDoServico, Environment.MachineName, destino.DataSource, destino.InitialCatalog,
            opcoes.IntervaloMinutos, opcoes.Tentativas);

        await EsperarAsync(TimeSpan.FromSeconds(opcoes.EsperaInicialSegundos), parada);

        while (!parada.IsCancellationRequested)
        {
            var proxima = intervalo;
            try
            {
                var desfecho = await ExecutarCicloAsync(conexao, parada);
                switch (desfecho.Resultado)
                {
                    case ResultadoDaExecucao.Sucesso:
                        _logger.LogInformation("Sincronização do ART concluída: {Resumo}", desfecho.Mensagem);
                        break;
                    case ResultadoDaExecucao.Ignorada:
                        _logger.LogWarning("Sincronização do ART não rodou: {Motivo}", desfecho.Mensagem);
                        break;
                    default:
                        _logger.LogError("Sincronização do ART falhou: {Motivo}", desfecho.Mensagem);
                        break;
                }
            }
            catch (OperationCanceledException) when (parada.IsCancellationRequested)
            {
                break;
            }
            catch (Exception falha)
            {
                // NEM COMEÇOU: o banco do CRM não respondeu à trava ou à preparação. Não há onde registrar a
                // execução, então o registro é o Log de Aplicativo — e a próxima tentativa vem antes do intervalo.
                var art = new OpcoesDoArt();
                configuracao.GetSection(OpcoesDoArt.Secao).Bind(art);
                var crm = new SqlConnectionStringBuilder(conexao);
                var motivo = Sigilo.Mascarar(
                    $"{falha.GetType().Name}: {falha.GetBaseException().Message}",
                    [art.Usuario, art.Senha, configuracao[$"{OpcoesDoBancoDoProtheus.Secao}:Usuario"], configuracao[$"{OpcoesDoBancoDoProtheus.Secao}:Senha"], crm.UserID, crm.Password]);

                _logger.LogError("O ciclo da sincronização do ART não conseguiu começar: {Motivo}. Nova tentativa em {Minutos} min.",
                    motivo, (int)Math.Min(intervalo.TotalMinutes, EsperaQuandoNemComecou.TotalMinutes));
                proxima = intervalo < EsperaQuandoNemComecou ? intervalo : EsperaQuandoNemComecou;
            }

            await EsperarAsync(proxima, parada);
        }

        _logger.LogInformation("Serviço {Servico} parado.", HospedagemDaSincronizacao.NomeDoServico);
    }

    private async Task<DesfechoDaSincronizacao> ExecutarCicloAsync(string conexao, CancellationToken parada)
    {
        var opcoesDoBanco = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlServer(conexao, sql => sql.CommandTimeout(180))
            .Options;
        var diario = new DiarioDeAlcanceEntreEmpresasEmLog(fabricaDeLog.CreateLogger<DiarioDeAlcanceEntreEmpresasEmLog>());

        var (dePara, usuarioId, empresaId, erro) = await PreparacaoDaCarga.PrepararAsync(opcoesDoBanco, diario, parada);
        if (erro is not null) return new DesfechoDaSincronizacao(ResultadoDaExecucao.Falha, 0, null, erro);

        var contexto = new ContextoDeCargaDeSistema(usuarioId, empresaId, dePara.Values.ToHashSet());

        // O ORQUESTRADOR MANDA NO ART QUANDO A ROTINA DELE ESTÁ LIGADA (issue 136): duas sincronizações do mesmo fluxo
        // em horários diferentes seriam dois donos da mesma agenda. A trava já impede as duas ao mesmo tempo; isto
        // impede as duas no mesmo dia.
        await using (var banco = new CrmDbContext(opcoesDoBanco, contexto, diario))
        {
            if (await banco.Rotinas.AsNoTracking().AnyAsync(r => r.Codigo == Dominio.Integracao.RotinasDoSistema.ArtVendas && r.EstaLigada, parada))
                return new DesfechoDaSincronizacao(ResultadoDaExecucao.Ignorada, 0, null,
                    "A rotina ART_VENDAS está ligada no orquestrador (Configurações › Integrações): é ele que sincroniza o ART. O serviço não roda em paralelo.");
        }

        // A credencial da tela vai por cima da do arquivo, a cada ciclo: trocar a senha na tela vale no ciclo seguinte.
        var (credenciais, _) = await CredenciaisDaTela.LerAsync(conexao, configuracao, parada);
        var efetiva = credenciais.Count > 0 ? CredenciaisDaTela.Sobrepor(configuracao, credenciais) : configuracao;

        var art = new OpcoesDoArt();
        efetiva.GetSection(OpcoesDoArt.Secao).Bind(art);
        var protheus = new OpcoesDoBancoDoProtheus();
        efetiva.GetSection(OpcoesDoBancoDoProtheus.Secao).Bind(protheus);

        var executor = new ExecutorDaSincronizacaoDoArt(
            conexao,
            () => new CrmDbContext(opcoesDoBanco, contexto, diario),
            art,
            protheus,
            ParceirosPorRaizDeCnpj.RaizesDoGrupoConfiguradas(efetiva[ParceirosPorRaizDeCnpj.ChaveDaConfiguracao]),
            usuarioId,
            opcoes.Tentativas,
            TimeSpan.FromSeconds(opcoes.EsperaEntreTentativasSegundos),
            andamento => _logger.LogDebug("{Andamento}", andamento),
            _logger);

        return await executor.ExecutarAsync(simular: false, parada);
    }

    private static async Task EsperarAsync(TimeSpan quanto, CancellationToken parada)
    {
        if (quanto <= TimeSpan.Zero) return;
        try { await Task.Delay(quanto, parada); }
        catch (OperationCanceledException) { }
    }
}
