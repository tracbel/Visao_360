using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao;
using Tracbel.Crm.Integracao.Art;

namespace Tracbel.Crm.Carga.Sincronizacao;

/// <summary>
/// A configuração do serviço de sincronização do ART — seção <c>Sincronizacao:Art</c> do
/// <c>appsettings.Production.json</c> do serviço (documento 35, seção 11).
/// </summary>
internal sealed class OpcoesDaSincronizacaoDoArt
{
    /// <summary>A seção da configuração.</summary>
    public const string Secao = "Sincronizacao:Art";

    /// <summary>
    /// Liga a sincronização. Existe só no arquivo do servidor: o serviço rodando com a configuração de
    /// desenvolvimento não grava em banco nenhum.
    /// </summary>
    public bool Habilitada { get; set; }

    /// <summary>Minutos entre o fim de um ciclo e o começo do seguinte.</summary>
    public int IntervaloMinutos { get; set; } = 60;

    /// <summary>Espera depois de o Windows subir o serviço — o SQL Server pode ainda estar subindo.</summary>
    public int EsperaInicialSegundos { get; set; } = 60;

    /// <summary>Tentativas por ciclo quando a leitura ou a gravação falha.</summary>
    public int Tentativas { get; set; } = 3;

    /// <summary>Espera antes da segunda tentativa; dobra a cada nova tentativa, até 15 minutos.</summary>
    public int EsperaEntreTentativasSegundos { get; set; } = 30;

    /// <summary>O que está fora da faixa aceita, ou nulo.</summary>
    public string? Validar() =>
        IntervaloMinutos is < 5 or > 1440 ? "Sincronizacao:Art:IntervaloMinutos vai de 5 a 1440."
        : Tentativas is < 1 or > 10 ? "Sincronizacao:Art:Tentativas vai de 1 a 10."
        : EsperaInicialSegundos is < 0 or > 3600 ? "Sincronizacao:Art:EsperaInicialSegundos vai de 0 a 3600."
        : EsperaEntreTentativasSegundos is < 0 or > 3600 ? "Sincronizacao:Art:EsperaEntreTentativasSegundos vai de 0 a 3600."
        : null;
}

/// <summary>Como terminou um ciclo de sincronização.</summary>
/// <param name="Resultado">Sucesso, Falha ou Ignorada.</param>
/// <param name="Tentativas">Tentativas feitas.</param>
/// <param name="Relatorio">O relatório da carga, quando houve sucesso.</param>
/// <param name="Mensagem">O resumo ou o motivo, sem credencial.</param>
internal sealed record DesfechoDaSincronizacao(
    ResultadoDaExecucao Resultado,
    int Tentativas,
    RelatorioDaCargaDoArt? Relatorio,
    string Mensagem);

/// <summary>
/// UM CICLO DA SINCRONIZAÇÃO DO ART: trava, registro da execução, carga com novas tentativas e
/// fechamento do registro (documento 35, seção 11).
///
/// <para><b>Reaproveita a carga.</b> Quem lê o ART, saneia, corresponde e grava continua sendo
/// <see cref="CargaDoArt"/> — a mesma que rodou e foi conferida no banco local. O executor só acrescenta
/// o que um serviço sem operador precisa: não rodar duas vezes ao mesmo tempo, tentar de novo quando a
/// rede falha e deixar escrito o que aconteceu.</para>
///
/// <para><b>Trava:</b> <c>sp_getapplock</c> exclusiva, de sessão, sem espera, numa conexão própria. A
/// carga manual (<c>--somente-art</c>) passa pela mesma trava: se o serviço estiver no meio de um
/// ciclo, ela é recusada em vez de gravar em paralelo.</para>
///
/// <para><b>Tentativas:</b> cada uma é uma carga inteira numa transação — a que falha é desfeita, e a
/// seguinte recomeça do zero. Como a carga é repetível, repetir não duplica nada.</para>
///
/// <para><b>Credenciais:</b> nunca vão para mensagem. As leituras do ART e do Protheus já devolvem
/// só o código do erro; as exceções passam por <see cref="Sigilo.Mascarar"/>.</para>
/// </summary>
internal sealed class ExecutorDaSincronizacaoDoArt(
    string conexaoDoCrm,
    Func<CrmDbContext> abrirContexto,
    OpcoesDoArt art,
    OpcoesDoBancoDoProtheus protheus,
    long usuarioId,
    int tentativas,
    TimeSpan esperaBase,
    Action<string> relatar,
    ILogger logger)
{
    /// <summary>O fluxo registrado.</summary>
    public const string Fluxo = "ART.VENDA_DE_MAQUINA";

    private const string RecursoDaTrava = "TracbelCrm.Sincronizacao." + Fluxo;
    private static readonly TimeSpan EsperaMaxima = TimeSpan.FromMinutes(15);

    /// <summary>A espera antes da tentativa <paramref name="tentativa"/> (a primeira não espera).</summary>
    /// <param name="tentativa">O número da tentativa, a partir de 1.</param>
    /// <param name="esperaBase">A espera antes da segunda.</param>
    public static TimeSpan EsperaAntesDa(int tentativa, TimeSpan esperaBase)
    {
        if (tentativa <= 1 || esperaBase <= TimeSpan.Zero) return TimeSpan.Zero;
        var fator = Math.Pow(2, Math.Min(tentativa - 2, 10));
        var espera = TimeSpan.FromTicks((long)Math.Min(esperaBase.Ticks * fator, EsperaMaxima.Ticks));
        return espera;
    }

    /// <summary>Executa um ciclo.</summary>
    /// <param name="simular">Desfaz a carga ao final e não registra execução.</param>
    /// <param name="ct">Cancelamento — a parada do serviço.</param>
    public async Task<DesfechoDaSincronizacao> ExecutarAsync(bool simular, CancellationToken ct)
    {
        var construtor = new SqlConnectionStringBuilder(conexaoDoCrm) { Pooling = false, ApplicationName = "TracbelCrm.Sincronizacao.Trava" };
        await using var trava = new SqlConnection(construtor.ConnectionString);
        await trava.OpenAsync(ct);

        if (!await ObterTravaAsync(trava, ct))
        {
            const string motivo = "Outra sincronização do ART está em andamento (trava sp_getapplock ocupada). Nada foi gravado.";
            if (!simular) await RegistrarIgnoradaAsync(motivo, ct);
            return new DesfechoDaSincronizacao(ResultadoDaExecucao.Ignorada, 0, null, motivo);
        }

        try
        {
            var registroId = simular ? (long?)null : await IniciarRegistroAsync(ct);
            string? ultimaFalha = null;

            for (var tentativa = 1; tentativa <= tentativas; tentativa++)
            {
                var espera = EsperaAntesDa(tentativa, esperaBase);
                if (espera > TimeSpan.Zero)
                {
                    logger.LogInformation("Nova tentativa da sincronização do ART em {Segundos} s ({Tentativa} de {Total}).",
                        (int)espera.TotalSeconds, tentativa, tentativas);
                    await Task.Delay(espera, ct);
                }

                try
                {
                    var carga = new CargaDoArt(
                        abrirContexto,
                        new LeitorDoArt(Options.Create(art)),
                        protheus.EstaConfigurada ? new LeitorDoCadastroDoProtheus(protheus) : null,
                        usuarioId,
                        relatar);

                    var resultado = await carga.ExecutarAsync(simular, ct);
                    if (resultado.EhSucesso)
                    {
                        var relatorio = resultado.Valor;
                        var resumo = Resumir(relatorio, tentativa);
                        if (registroId is { } id)
                            await EncerrarRegistroAsync(id, e => e.Concluir(
                                tentativa, relatorio.Valor(CargaDoArt.RotuloDeLidos), relatorio.Valor(CargaDoArt.RotuloDeVendasIncluidas),
                                relatorio.Valor(CargaDoArt.RotuloDeVendasAtualizadas), relatorio.Valor(CargaDoArt.RotuloDePendentes),
                                resumo, DateTime.UtcNow), relatorio, CancellationToken.None);

                        return new DesfechoDaSincronizacao(ResultadoDaExecucao.Sucesso, tentativa, relatorio, resumo);
                    }

                    ultimaFalha = Mascarar(resultado.Erro);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    if (registroId is { } id)
                        await EncerrarRegistroAsync(id, e => e.Falhar(tentativa,
                            "Interrompida pela parada do serviço; a transação em curso foi desfeita.", DateTime.UtcNow), null, CancellationToken.None);
                    throw;
                }
                catch (Exception falha)
                {
                    ultimaFalha = Descrever(falha);
                }

                logger.LogWarning("Tentativa {Tentativa} de {Total} da sincronização do ART falhou: {Motivo}", tentativa, tentativas, ultimaFalha);
            }

            var mensagem = $"Falhou nas {tentativas} tentativa(s); a última: {ultimaFalha}";
            if (registroId is { } registro)
                await EncerrarRegistroAsync(registro, e => e.Falhar(tentativas, mensagem, DateTime.UtcNow), null, CancellationToken.None);

            return new DesfechoDaSincronizacao(ResultadoDaExecucao.Falha, tentativas, null, mensagem);
        }
        finally
        {
            await LiberarTravaAsync(trava);
        }
    }

    /// <summary>Descreve uma exceção sem credencial.</summary>
    /// <param name="falha">A exceção.</param>
    public string Descrever(Exception falha)
    {
        var causa = falha.GetBaseException();
        var texto = ReferenceEquals(causa, falha)
            ? $"{falha.GetType().Name}: {falha.Message}"
            : $"{falha.GetType().Name}: {falha.Message} | causa {causa.GetType().Name}: {causa.Message}";
        return Mascarar(texto);
    }

    private string Mascarar(string? texto)
    {
        var crm = new SqlConnectionStringBuilder(conexaoDoCrm);
        return Sigilo.Mascarar(texto, [art.Usuario, art.Senha, protheus.Usuario, protheus.Senha, crm.UserID, crm.Password]);
    }

    private static string Resumir(RelatorioDaCargaDoArt relatorio, int tentativa) =>
        FormattableString.Invariant(
            $"{relatorio.Valor(CargaDoArt.RotuloDeLidos)} registros lidos; {relatorio.Valor(CargaDoArt.RotuloDeVendasIncluidas)} vendas incluídas; {relatorio.Valor(CargaDoArt.RotuloDeVendasAtualizadas)} atualizadas pela origem; {relatorio.Valor(CargaDoArt.RotuloDePendentes)} pendentes; {relatorio.Valor(CargaDoArt.RotuloDeMaquinasIncluidas)} máquinas incluídas")
        + (tentativa > 1 ? FormattableString.Invariant($"; sucesso na tentativa {tentativa}.") : ".");

    private static async Task<bool> ObterTravaAsync(SqlConnection conexao, CancellationToken ct)
    {
        await using var comando = conexao.CreateCommand();
        comando.CommandText =
            "DECLARE @r int; EXEC @r = sp_getapplock @Resource = @recurso, @LockMode = 'Exclusive', " +
            "@LockOwner = 'Session', @LockTimeout = 0; SELECT @r;";
        comando.Parameters.AddWithValue("@recurso", RecursoDaTrava);
        return Convert.ToInt32(await comando.ExecuteScalarAsync(ct), CultureInfo.InvariantCulture) >= 0;
    }

    private static async Task LiberarTravaAsync(SqlConnection conexao)
    {
        try
        {
            await using var comando = conexao.CreateCommand();
            comando.CommandText = "EXEC sp_releaseapplock @Resource = @recurso, @LockOwner = 'Session';";
            comando.Parameters.AddWithValue("@recurso", RecursoDaTrava);
            await comando.ExecuteNonQueryAsync(CancellationToken.None);
        }
        catch (SqlException)
        {
            // A conexão sem pool é fechada logo em seguida, e o SQL Server solta a trava de sessão junto.
        }
    }

    private async Task<int> SistemaAsync(CrmDbContext banco, CancellationToken ct) =>
        await CargaDeTerritorio.SistemaAsync(banco, LeitorDoArt.CodigoDoSistema, "ART — vendas de máquina", "MySQL, view somente leitura", ct);

    private async Task<long> IniciarRegistroAsync(CancellationToken ct)
    {
        await using var banco = abrirContexto();
        var execucao = ExecucaoDeSincronizacao.Iniciar(await SistemaAsync(banco, ct), Fluxo, Environment.MachineName, DateTime.UtcNow);
        banco.ExecucoesDeSincronizacao.Add(execucao);
        await banco.SaveChangesAsync(ct);
        return execucao.Id;
    }

    private async Task RegistrarIgnoradaAsync(string motivo, CancellationToken ct)
    {
        await using var banco = abrirContexto();
        var execucao = ExecucaoDeSincronizacao.Iniciar(await SistemaAsync(banco, ct), Fluxo, Environment.MachineName, DateTime.UtcNow);
        execucao.Ignorar(motivo, DateTime.UtcNow);
        banco.ExecucoesDeSincronizacao.Add(execucao);
        await banco.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Fecha o registro e, no sucesso, o PONTO DE SINCRONISMO do fluxo — a "última sincronização bem
    /// sucedida" que o alarme de atraso lê.
    /// </summary>
    private async Task EncerrarRegistroAsync(long id, Action<ExecucaoDeSincronizacao> encerrar, RelatorioDaCargaDoArt? relatorio, CancellationToken ct)
    {
        await using var banco = abrirContexto();
        var execucao = await banco.ExecucoesDeSincronizacao.FirstAsync(e => e.Id == id, ct);
        encerrar(execucao);

        if (execucao.Resultado == ResultadoDaExecucao.Sucesso && relatorio is not null)
        {
            var quando = execucao.TerminadaEm ?? DateTime.UtcNow;
            var marca = quando.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            var ponto = await banco.PontosDeSincronismo.FirstOrDefaultAsync(p => p.SistemaId == execucao.SistemaId && p.Fluxo == Fluxo, ct);
            if (ponto is null)
            {
                ponto = PontoDeSincronismo.Criar(execucao.SistemaId, Fluxo, marca);
                banco.PontosDeSincronismo.Add(ponto);
            }

            ponto.RegistrarRodada(
                marca,
                relatorio.Valor(CargaDoArt.RotuloDeLidos),
                relatorio.Valor(CargaDoArt.RotuloDeVendasIncluidas) + relatorio.Valor(CargaDoArt.RotuloDeVendasAtualizadas),
                relatorio.Valor(CargaDoArt.RotuloDePendentes),
                quando);
        }

        await banco.SaveChangesAsync(ct);
    }
}
