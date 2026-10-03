using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Cache;

namespace Tracbel.Crm.Api.Comum;

/// <summary>
/// O CACHE DAS LEITURAS (30/09/2026) — a mesma resposta não é calculada duas vezes enquanto o dado por trás dela não mudou.
///
/// <para><b>Por que existe.</b> O Ricardo: "algumas coisas que puxamos da API demoram para carregar na tela, não podemos
/// ter isso". As telas são relatórios sobre dados que mudam quando uma ROTINA grava (ART, Vórtice, Gestão de Negócios,
/// fontes públicas) ou quando alguém grava pela própria tela — e não a cada clique. Recalcular a cada abertura é o banco
/// refazendo a mesma conta.</para>
///
/// <para><b>A regra decidida por ele:</b> a resposta fica guardada por até <see cref="OpcoesDoCacheDasLeituras.Minutos"/>
/// minutos (10) e deixa de valer NA HORA em que uma rotina grava — é a <see cref="VersaoDosDados"/>, que entra na chave.
/// Com a versão nova, a chave é outra, e a leitura seguinte vai ao banco.</para>
///
/// <para><b>Nada atravessa a fronteira de acesso.</b> A chave leva o usuário, a filial escolhida (ou "todas") e o endereço
/// com a consulta inteira: a resposta de um nunca é servida a outro. E o cache fica DEPOIS do meio de campo de permissão:
/// quem perdeu a permissão da rota recebe o 403 de sempre, e não a resposta guardada.</para>
/// </summary>
public sealed class OpcoesDoCacheDasLeituras
{
    /// <summary>A seção do <c>appsettings</c>.</summary>
    public const string Secao = "CacheDasLeituras";

    /// <summary>Por quantos minutos uma resposta vale, no máximo. Zero desliga o cache.</summary>
    public int Minutos { get; set; } = 10;

    /// <summary>De quantos em quantos segundos o vigia confere se uma rotina gravou.</summary>
    public int SegundosEntreConferencias { get; set; } = 15;
}

/// <summary>
/// A VERSÃO DOS DADOS — muda quando alguma coisa gravou, e entra na chave do cache.
///
/// <para><b>Duas origens.</b> A do banco é o fim da última rotina e da última sincronização do ART que trouxe venda nova ou
/// alterada: elas rodam em OUTRO processo (a tarefa agendada e o serviço do ART), e o <see cref="VigiaDaVersaoDosDados"/> é
/// quem as enxerga. A das telas é a gravação feita pela própria API — o cliente cadastrado, a tarefa concluída, o
/// parâmetro alterado —, contada na hora pelo <see cref="MeioDeCampoDeGravacao"/>.</para>
/// </summary>
public sealed class VersaoDosDados
{
    private long _gravacoesPelaApi;
    private volatile string _doBanco = string.Empty;

    /// <summary>A versão de agora — o que vai na chave de cada resposta guardada.</summary>
    public string Atual => $"{_doBanco}|{Interlocked.Read(ref _gravacoesPelaApi)}";

    /// <summary>Uma gravação pela API terminou bem: o que estava guardado deixa de valer.</summary>
    public void RegistrarGravacao() => Interlocked.Increment(ref _gravacoesPelaApi);

    /// <summary>A marca lida do banco; muda a versão só quando ela é diferente da anterior.</summary>
    /// <param name="marca">O fim da última rotina e da última sincronização com mudança.</param>
    /// <returns>Se a versão mudou.</returns>
    public bool AtualizarDoBanco(string marca)
    {
        if (marca == _doBanco) return false;
        _doBanco = marca;
        return true;
    }
}

/// <summary>
/// O VIGIA DAS ROTINAS: de <see cref="OpcoesDoCacheDasLeituras.SegundosEntreConferencias"/> em
/// <see cref="OpcoesDoCacheDasLeituras.SegundosEntreConferencias"/> segundos, pergunta ao banco quando terminou a última
/// rotina e a última sincronização do ART com mudança. São duas contas de máximo em tabelas pequenas.
///
/// <para><b>Se o banco não responde</b>, a versão fica como estava, e o que já está guardado expira no prazo de sempre: o
/// cache nunca serve resposta além dos 10 minutos por causa de uma falha do vigia.</para>
/// </summary>
public sealed class VigiaDaVersaoDosDados(
    IServiceScopeFactory fabrica,
    VersaoDosDados versao,
    IOptions<OpcoesDoCacheDasLeituras> opcoes,
    ILogger<VigiaDaVersaoDosDados> log) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken parar)
    {
        if (opcoes.Value.Minutos <= 0) return;

        using var relogio = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(1, opcoes.Value.SegundosEntreConferencias)));
        var falhando = false;

        do
        {
            try
            {
                using var escopo = fabrica.CreateScope();
                var opcoesDoBanco = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
                await using var db = new CrmDbContext(opcoesDoBanco, ProvedorDeContextoDeSistema.Instancia);

                if (versao.AtualizarDoBanco(await LerMarcaAsync(db, parar)))
                    log.LogInformation("CACHE DAS LEITURAS: uma rotina gravou; as respostas guardadas deixam de valer.");
                falhando = false;
            }
            catch (Exception erro) when (!parar.IsCancellationRequested)
            {
                // UMA VEZ POR FALHA, e não a cada 15 segundos: o banco fora do ar já aparece no resto do log.
                if (!falhando) log.LogWarning(erro, "CACHE DAS LEITURAS: o vigia não leu o banco; o que está guardado expira no prazo.");
                falhando = true;
            }
        }
        while (await relogio.WaitForNextTickAsync(parar));
    }

    /// <summary>A marca do banco: o fim da última rotina e o da última sincronização do ART que mudou alguma venda.</summary>
    /// <param name="db">O banco, sob contexto de sistema.</param>
    /// <param name="ct">Cancelamento.</param>
    public static async Task<string> LerMarcaAsync(CrmDbContext db, CancellationToken ct)
    {
        var rotina = await db.Rotinas.MaxAsync(r => r.UltimaExecucaoTerminadaEm, ct);

        // A RODADA DO ART QUE NÃO MUDOU NADA NÃO LIMPA O CACHE: o serviço roda de minutos em minutos, e quase sempre não
        // traz venda nova.
        var art = await db.ExecucoesDeSincronizacao
            .Where(e => e.Resultado == ResultadoDaExecucao.Sucesso && (e.Incluidos > 0 || e.Atualizados > 0))
            .MaxAsync(e => (DateTime?)e.TerminadaEm, ct);

        return $"{rotina:O}|{art:O}";
    }
}

/// <summary>
/// CONTA AS GRAVAÇÕES FEITAS PELA API: toda chamada que não é leitura e terminou bem muda a
/// <see cref="VersaoDosDados"/>. Quem cadastra um cliente vê o cliente na lista seguinte, e não daqui a 10 minutos.
/// </summary>
public sealed class MeioDeCampoDeGravacao(RequestDelegate proximo, VersaoDosDados versao, ContadorDeGravacoesNaReferencia referencia)
{
    /// <summary>Executa o meio de campo.</summary>
    /// <param name="http">A requisição em curso.</param>
    public async Task InvokeAsync(HttpContext http)
    {
        await proximo(http);

        var metodo = http.Request.Method;
        var leitura = HttpMethods.IsGet(metodo) || HttpMethods.IsHead(metodo) || HttpMethods.IsOptions(metodo);
        if (!leitura && http.Response.StatusCode < 400 && http.Request.Path.StartsWithSegments("/api"))
        {
            versao.RegistrarGravacao();
            referencia.RegistrarGravacao(http.Request.Path.Value ?? string.Empty);
        }
    }
}

/// <summary>
/// A POLÍTICA DO CACHE: o que é guardado, com que chave e por quanto tempo.
///
/// <para><b>Só as leituras de relatório e de cadastro.</b> Ficam fora, sempre ao vivo: o acesso e o escopo (quem a pessoa
/// é), a administração (usuários, perfis, integrações), a auditoria, o estado das rotinas e o desempenho da API — são
/// telas de conferir o que está acontecendo agora.</para>
/// </summary>
public sealed class PoliticaDeCacheDasLeituras(VersaoDosDados versao, IOptions<OpcoesDoCacheDasLeituras> opcoes) : IOutputCachePolicy
{
    /// <summary>Os grupos de rota cujas leituras são guardadas.</summary>
    public static readonly string[] Prefixos =
    [
        "/api/v1/relatorios",
        "/api/v1/territorio",
        "/api/v1/cobertura",
        "/api/v1/municipios",
        "/api/v1/mercado",
        "/api/v1/catalogos",
        "/api/v1/clientes",
        "/api/v1/equipamentos",
        "/api/v1/processos",
        "/api/v1/tarefas",
        "/api/v1/interacoes",
        // A CONFERÊNCIA COM A GN é relatório, embora more no grupo das integrações.
        "/api/v1/integracoes/conferencia-gn",
    ];

    /// <summary>Se a leitura entra no cache: GET, num dos <see cref="Prefixos"/>.</summary>
    /// <param name="requisicao">A requisição.</param>
    public static bool EhCacheavel(HttpRequest requisicao) =>
        HttpMethods.IsGet(requisicao.Method) && Prefixos.Any(p => requisicao.Path.StartsWithSegments(p));

    /// <inheritdoc />
    ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext contexto, CancellationToken ct)
    {
        var http = contexto.HttpContext;
        var ligado = opcoes.Value.Minutos > 0 && EhCacheavel(http.Request);

        contexto.EnableOutputCaching = ligado;
        contexto.AllowCacheLookup = ligado;
        contexto.AllowCacheStorage = ligado;
        if (!ligado) return ValueTask.CompletedTask;

        // A MESMA LEITURA PEDIDA AO MESMO TEMPO É CALCULADA UMA VEZ: a segunda espera a primeira, em vez de ir ao banco
        // junto com ela.
        contexto.AllowLocking = true;
        contexto.ResponseExpirationTimeSpan = TimeSpan.FromMinutes(opcoes.Value.Minutos);

        var acesso = http.RequestServices.GetRequiredService<ContextoAcessoDaRequisicao>().Atual;
        contexto.CacheVaryByRules.QueryKeys = "*";
        contexto.CacheVaryByRules.VaryByValues["usuario"] = acesso.UsuarioId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        contexto.CacheVaryByRules.VaryByValues["filial"] = acesso.VeTodasAsFiliais
            ? "TODAS"
            : acesso.EmpresaId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        contexto.CacheVaryByRules.VaryByValues["versao"] = versao.Atual;
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext contexto, CancellationToken ct) => ValueTask.CompletedTask;

    /// <inheritdoc />
    ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext contexto, CancellationToken ct)
    {
        // SÓ O SUCESSO É GUARDADO. O erro precisa ser refeito na próxima tentativa; e a resposta que renova o cookie da
        // sessão é daquela pessoa naquele instante, e não se repete.
        var resposta = contexto.HttpContext.Response;
        if (resposta.StatusCode != StatusCodes.Status200OK || resposta.Headers.ContainsKey(HeaderNames.SetCookie))
            contexto.AllowCacheStorage = false;
        return ValueTask.CompletedTask;
    }
}
