using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

namespace Tracbel.Crm.Api.Comum;

/// <summary>
/// O AQUECIMENTO NA SUBIDA (plano 2 do documento 54). Depois de cada publicação, a primeira tela pagava sozinha as três
/// contas de referência (território, potencial e estrutura) e a primeira compilação de cada consulta do EF — 10,6 s nos
/// Indicadores na captura de produção de 03/10/2026. O aquecimento faz isso logo depois da subida, antes do primeiro
/// usuário: calcula as referências, que ficam no cache, e roda a apuração uma vez sob contexto de sistema, o que deixa
/// as consultas do recorte compiladas para todos — o EF guarda a consulta compilada pela forma, e não pelo valor.
///
/// <para><b>Falhar aqui não derruba nada</b>: a falha vai para o log, e a primeira tela paga a conta inteira, como antes.
/// <c>Aquecimento:Ligado = false</c> desliga.</para>
/// </summary>
/// <param name="fabrica">De onde sai o banco da apuração de aquecimento.</param>
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

    /// <summary>Calcula as três referências e roda a apuração uma vez, sob contexto de sistema.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task AquecerAsync(CancellationToken ct)
    {
        var cronometro = Stopwatch.StartNew();
        var agora = relogio.Agora;

        await territorio.LerAsync(ct);
        await potencial.LerAsync(ParametroComVigencia.HojeNoBrasil(agora), ct);
        await estrutura.LerAsync(ct);

        using var escopo = fabrica.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        // O PADRÃO DA TELA: o ano fiscal até o último mês fechado — as mesmas consultas que o primeiro usuário vai pedir.
        var mesCorrente = AnoFiscal.MesCorrenteEmSaoPaulo(agora);
        var padrao = AnoFiscal.AteOUltimoMesFechado(mesCorrente);
        await new RepositorioDeIndicadoresTerritoriais(db, territorio, potencial, estrutura).ApurarAsync(
            new ConsultaDeIndicadoresTerritoriais(padrao.Inicial, padrao.Final, null, null, MesCorrente: mesCorrente), agora, ct);

        registro.LogInformation(
            "API aquecida em {Milissegundos} ms: território, potencial, estrutura e a apuração do recorte.", cronometro.ElapsedMilliseconds);
    }
}
