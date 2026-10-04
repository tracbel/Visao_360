using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

namespace Tracbel.Crm.Api.Comum;

/// <summary>Uma tela do aquecimento: o nome, o contexto de acesso do banco dela e o que ela roda.</summary>
/// <param name="Tela">O nome, para o log.</param>
/// <param name="Acesso">O contexto de acesso do banco da tela; nas telas pesadas, o de sistema.</param>
/// <param name="Aquecer">O que a tela roda; devolve nulo quando deu certo, ou o motivo da recusa.</param>
public sealed record TelaParaAquecer(string Tela, IProvedorContextoAcesso Acesso, Func<CrmDbContext, CancellationToken, Task<string?>> Aquecer);

/// <summary>Uma tela aquecida: quanto levou e, quando falhou, por quê.</summary>
/// <param name="Tela">O nome.</param>
/// <param name="Milissegundos">Quanto levou.</param>
/// <param name="Falha">Nulo quando deu certo.</param>
public sealed record TelaAquecida(string Tela, long Milissegundos, string? Falha);

/// <summary>
/// O AQUECIMENTO NA SUBIDA (plano 2 do documento 54). Depois de cada publicação, a primeira tela pagava sozinha as três
/// contas de referência (território, potencial e estrutura) e a primeira compilação de cada consulta do EF — 10,6 s nos
/// Indicadores na captura de produção de 03/10/2026. O aquecimento faz isso logo depois da subida, antes do primeiro
/// usuário: calcula as referências, que ficam no cache, e roda as telas pesadas uma vez, o que deixa as consultas delas
/// compiladas para todos — o EF guarda a consulta compilada pela forma, e não pelo valor.
///
/// <para><b>As telas pesadas</b> (passo 7 do documento 54): os Indicadores e as seis rotas de relatório que a mesma captura
/// mostrou entre 1,7 s e 2,9 s na primeira chamada — Visão 360, funil por estágio, painel do CEN, faturamento, metas e
/// vendas perdidas. Cada uma roda o caso de uso dela, no padrão da tela, sobre um banco em contexto de sistema.</para>
///
/// <para><b>Falhar aqui não derruba nada</b>: a tela que cai vai para o log, as outras rodam, e a primeira chamada dela paga
/// a conta inteira, como antes. <c>Aquecimento:Ligado = false</c> desliga.</para>
/// </summary>
/// <param name="fabrica">De onde sai o banco das telas.</param>
/// <param name="territorio">O território de referência.</param>
/// <param name="potencial">O potencial de referência.</param>
/// <param name="estrutura">A estrutura de referência.</param>
/// <param name="configuracao">Onde mora o <c>Aquecimento:Ligado</c>.</param>
/// <param name="relogio">O relógio.</param>
/// <param name="registro">O log.</param>
public sealed class AquecimentoDaApi(
    IServiceScopeFactory fabrica,
    IRepositorioDoTerritorioDeReferencia territorio,
    IRepositorioDoPotencialDeReferencia potencial,
    IRepositorioDaEstruturaDeReferencia estrutura,
    IConfiguration configuracao,
    IRelogio relogio,
    ILogger<AquecimentoDaApi> registro) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuracao.GetValue("Aquecimento:Ligado", true)) return;

        // A SUBIDA NÃO ESPERA O AQUECIMENTO: o serviço já responde enquanto ele roda.
        await Task.Yield();

        try
        {
            await AquecerAsync(stoppingToken);
        }
        catch (Exception erro) when (erro is not OperationCanceledException)
        {
            registro.LogWarning(erro, "O aquecimento da API falhou: a primeira tela de cada assunto vai pagar a conta inteira.");
        }
    }

    /// <summary>Calcula as três referências e roda as telas pesadas uma vez.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<TelaAquecida>> AquecerAsync(CancellationToken ct)
    {
        var cronometro = Stopwatch.StartNew();
        var agora = relogio.Agora;

        await territorio.LerAsync(ct);
        await potencial.LerAsync(ParametroComVigencia.HojeNoBrasil(agora), ct);
        await estrutura.LerAsync(ct);

        var telas = await AquecerTelasAsync(TelasPesadas(agora), ct);

        registro.LogInformation(
            "API aquecida em {Milissegundos} ms: referências e {Telas}.", cronometro.ElapsedMilliseconds,
            string.Join(", ", telas.Select(t => $"{t.Tela} {t.Milissegundos} ms{(t.Falha is null ? string.Empty : " (falhou)")}")));
        return telas;
    }

    /// <summary>
    /// Roda as telas dadas, cada uma num banco próprio, uma de cada vez. A que falha vai para o log e para o resultado, e as
    /// outras rodam — é o que o teste exercita com uma tela que lança.
    /// </summary>
    /// <param name="telas">As telas.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<TelaAquecida>> AquecerTelasAsync(IReadOnlyList<TelaParaAquecer> telas, CancellationToken ct)
    {
        using var escopo = fabrica.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();

        var aquecidas = new List<TelaAquecida>(telas.Count);
        foreach (var tela in telas)
        {
            var cronometro = Stopwatch.StartNew();
            string? falha;
            try
            {
                await using var db = new CrmDbContext(opcoes, tela.Acesso);
                falha = await tela.Aquecer(db, ct);
                if (falha is not null)
                    registro.LogWarning("O aquecimento de {Tela} foi recusado: {Motivo}.", tela.Tela, falha);
            }
            catch (Exception erro) when (erro is not OperationCanceledException)
            {
                registro.LogWarning(erro, "O aquecimento de {Tela} falhou: a primeira chamada dela vai pagar a conta inteira.", tela.Tela);
                falha = erro.GetBaseException().Message;
            }

            aquecidas.Add(new TelaAquecida(tela.Tela, cronometro.ElapsedMilliseconds, falha));
        }

        return aquecidas;
    }

    /// <summary>As telas pesadas, cada uma no padrão da tela: o ano fiscal até o último mês fechado, sem filtro.</summary>
    /// <param name="agora">O instante do aquecimento.</param>
    private List<TelaParaAquecer> TelasPesadas(DateTime agora)
    {
        var sistema = ProvedorDeContextoDeSistema.Instancia;
        var mesCorrente = AnoFiscal.MesCorrenteEmSaoPaulo(agora);
        var padrao = AnoFiscal.AteOUltimoMesFechado(mesCorrente);

        return
        [
            new("Indicadores", sistema, async (db, ct) =>
            {
                await new RepositorioDeIndicadoresTerritoriais(db, territorio, potencial, estrutura).ApurarAsync(
                    new ConsultaDeIndicadoresTerritoriais(padrao.Inicial, padrao.Final, null, null, MesCorrente: mesCorrente), agora, ct);
                return null;
            }),
            new("Visão 360", sistema, async (db, ct) =>
                (await new ObterIndicadoresExecutivos(new RepositorioDeIndicadoresExecutivos(db), relogio).ExecutarAsync(null, null, ct)).Erro),
            new("Funil por estágio", sistema, async (db, ct) =>
                (await new ObterFunilPorEstagio(new RepositorioDoFunilPorEstagio(db), relogio).ExecutarAsync(null, null, null, null, null, ct)).Erro),
            new("Painel do CEN", sistema, async (db, ct) =>
                (await new ObterPainelDoCen(new RepositorioDoPainelDoCen(db), relogio).ExecutarAsync(null, ct)).Erro),
            new("Faturamento", sistema, async (db, ct) =>
                (await new ObterFaturamento(new RepositorioDeFaturamento(db), relogio).ExecutarAsync(null, ct)).Erro),
            // A META CONFERE Meta.Ler NO CASO DE USO, e o contexto de sistema passa: serviço de sistema alcança toda permissão
            // na organização (ContextoAcesso.ProfundidadeDe).
            new("Metas", sistema, async (db, ct) =>
                (await new ObterMetaERealizado(new RepositorioDeMetas(db), sistema, relogio).ExecutarAsync(null, null, ct)).Erro),
            new("Vendas perdidas", sistema, async (db, ct) =>
                (await new ObterVendasPerdidas(new RepositorioDeVendasPerdidas(db), relogio).ExecutarAsync(null, null, null, null, ct)).Erro)
        ];
    }
}
