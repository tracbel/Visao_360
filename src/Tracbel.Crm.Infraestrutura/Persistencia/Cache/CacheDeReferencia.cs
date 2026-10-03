using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Infraestrutura.Identidade;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Cache;

/// <summary>Os assuntos do dado de referência — igual para todo mundo, sem dono de filial (documento 54 §2).</summary>
public enum AssuntoDeReferencia
{
    /// <summary>A área de atuação, os municípios e as lojas.</summary>
    Territorio,

    /// <summary>As regras, o catálogo do motor, a PAM e o estado.</summary>
    Potencial,

    /// <summary>O parque, as propriedades, o rebanho, a área, as usinas e os totais do estado e da região (plano 2).</summary>
    Estrutura
}

/// <summary>As opções do cache de referência — a seção <c>CacheDeReferencia</c>.</summary>
public sealed class OpcoesDoCacheDeReferencia
{
    /// <summary>A seção do <c>appsettings</c>.</summary>
    public const string Secao = "CacheDeReferencia";

    /// <summary>De quantos em quantos segundos a assinatura de um assunto é relida do banco. Zero relê sempre.</summary>
    public int SegundosEntreConferencias { get; set; } = 15;
}

/// <summary>
/// AS GRAVAÇÕES PELA API QUE MUDAM O DADO DE REFERÊNCIA — a regra, a cultura e o catálogo do potencial vêm da tela de
/// Configurações, e quem grava precisa ver o número novo na hora. O catálogo não tem coluna de data; a gravação é o sinal.
/// </summary>
public sealed class ContadorDeGravacoesNaReferencia
{
    /// <summary>Os prefixos de rota cuja gravação muda o Potencial.</summary>
    public static readonly string[] PrefixosDoPotencial = ["/api/v1/admin/parametros-do-potencial"];

    private long _potencial;

    /// <summary>Uma gravação bem-sucedida pela API.</summary>
    /// <param name="caminho">O caminho da requisição.</param>
    public void RegistrarGravacao(string caminho)
    {
        if (PrefixosDoPotencial.Any(p => caminho.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            Interlocked.Increment(ref _potencial);
    }

    /// <summary>Quantas gravações o assunto recebeu desde a subida.</summary>
    /// <param name="assunto">O assunto.</param>
    public long Gravacoes(AssuntoDeReferencia assunto) =>
        assunto == AssuntoDeReferencia.Potencial ? Interlocked.Read(ref _potencial) : 0;
}

/// <summary>A versão de um assunto: muda quando o dado dele muda.</summary>
public interface IAssinaturaDosAssuntos
{
    /// <summary>A assinatura atual do assunto.</summary>
    /// <param name="assunto">O assunto.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<string> LerAsync(AssuntoDeReferencia assunto, CancellationToken ct);
}

/// <summary>
/// A ASSINATURA DO PRÓPRIO DADO (documento 54 §3.2) — máximos de data e contagens nas tabelas do assunto. A rotina que
/// grava em outro processo muda a assinatura sem precisar avisar ninguém; a rodada que não grava nada não muda nada.
///
/// <para><b>Relida no máximo a cada <see cref="OpcoesDoCacheDeReferencia.SegundosEntreConferencias"/> segundos</b>, sob
/// contexto de sistema: é uma consulta só, com as contas de máximo e de contagem como subconsultas. As gravações pela API
/// entram na hora, sem esperar a janela.</para>
/// </summary>
public sealed class AssinaturaDosAssuntos(
    IServiceScopeFactory fabrica,
    ContadorDeGravacoesNaReferencia gravacoes,
    IOptions<OpcoesDoCacheDeReferencia> opcoes,
    IRelogio relogio) : IAssinaturaDosAssuntos
{
    private readonly ConcurrentDictionary<AssuntoDeReferencia, (DateTime LidaEm, string Valor)> _lidas = new();

    /// <inheritdoc />
    public async Task<string> LerAsync(AssuntoDeReferencia assunto, CancellationToken ct)
    {
        var agora = relogio.Agora;
        var janela = TimeSpan.FromSeconds(Math.Max(0, opcoes.Value.SegundosEntreConferencias));

        if (janela == TimeSpan.Zero || !_lidas.TryGetValue(assunto, out var lida) || agora - lida.LidaEm >= janela)
        {
            lida = (agora, await LerDoBancoAsync(assunto, ct));
            _lidas[assunto] = lida;
        }

        return $"{lida.Valor}|{gravacoes.Gravacoes(assunto)}";
    }

    private async Task<string> LerDoBancoAsync(AssuntoDeReferencia assunto, CancellationToken ct)
    {
        using var escopo = fabrica.CreateScope();
        var opcoesDoBanco = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoesDoBanco, ProvedorDeContextoDeSistema.Instancia);
        return await LerDoBancoAsync(db, assunto, ct);
    }

    /// <summary>
    /// A ASSINATURA NUMA CONSULTA SÓ (plano 2 do documento 54): cada conta de máximo e de contagem é uma subconsulta escalar
    /// do mesmo SELECT. Cinco idas ao banco por assunto, a cada janela de 15 s, viravam o custo de quem abria a tela depois
    /// de um minuto parado.
    ///
    /// <para><b>Ancorado numa linha do catálogo de categorias de máquina</b>, que a semente cria em todo banco do CRM (o de
    /// produção, o de ensaio e o dos testes). Sem categoria não há potencial nem território a calcular, e a assinatura
    /// sai vazia.</para>
    /// </summary>
    /// <param name="db">O banco, sob contexto de sistema.</param>
    /// <param name="assunto">O assunto.</param>
    /// <param name="ct">Cancelamento.</param>
    public static async Task<string> LerDoBancoAsync(CrmDbContext db, AssuntoDeReferencia assunto, CancellationToken ct)
    {
        var ancora = db.CategoriasDeMaquina.AsNoTracking().OrderBy(c => c.Id).Take(1);

        // O `Count(_ => true)` É DE PROPÓSITO: o `Count()` sem predicado, numa tabela que não depende da linha da âncora, o EF
        // calcula ANTES, numa ida própria ao banco, e manda como parâmetro. Com o predicado ele vira subconsulta do mesmo
        // SELECT — `COUNT(*)`, o mesmo número.

        if (assunto == AssuntoDeReferencia.Territorio)
        {
            // O CATÁLOGO DE MUNICÍPIOS MUDA SEM MUDAR DE TAMANHO: a carga reconhece o município no IBGE e ele ganha o código e
            // o nome oficial na mesma linha (Municipio.ReconhecerNoIbge) — por isso a soma dos códigos e o tamanho dos nomes.
            var t = await ancora.Select(_ => new
            {
                Area = db.MunicipiosDaAreaDeAtuacao.Max(a => (DateTime?)a.ImportadoEm),
                Encerrada = db.MunicipiosDaAreaDeAtuacao.Max(a => a.EncerradoEm),
                Municipios = db.Municipios.Count(_ => true),
                ComCodigo = db.Municipios.Count(m => m.CodigoIbge != null),
                Codigos = db.Municipios.Sum(m => (long?)m.CodigoIbge),
                Letras = db.Municipios.Sum(m => (long?)m.Nome.Length),
                Lojas = db.Empresas.Max(e => (DateTime?)(e.AlteradoEm ?? e.CriadoEm)),
                QuantasLojas = db.Empresas.Count(_ => true)
            }).FirstOrDefaultAsync(ct);

            return t is null
                ? string.Empty
                : $"{t.Area:O}|{t.Encerrada:O}|{t.Municipios}:{t.ComCodigo}:{t.Codigos}:{t.Letras}|{t.Lojas:O}|{t.QuantasLojas}";
        }

        if (assunto == AssuntoDeReferencia.Estrutura)
        {
            // A ESTRUTURA DEPENDE DE ONZE TABELAS: as cinco da estrutura, as usinas, o total publicado do estado, a PAM (o ano
            // e a lavoura da região), a PAM do estado, a área de atuação (os totais da região) e o catálogo de municípios.
            var e = await ancora.Select(_ => new
            {
                Area = db.MunicipiosDaAreaDeAtuacao.Max(a => (DateTime?)a.ImportadoEm),
                AreaEncerrada = db.MunicipiosDaAreaDeAtuacao.Max(a => a.EncerradoEm),
                LinhasDaArea = db.MunicipiosDaAreaDeAtuacao.Count(_ => true),
                Pam = db.ProducoesAgricolasNosMunicipios.Max(x => (DateTime?)x.ImportadoEm),
                LinhasDaPam = db.ProducoesAgricolasNosMunicipios.Count(_ => true),
                Estado = db.ProducoesAgricolasNosEstados.Max(x => (DateTime?)x.ImportadoEm),
                LinhasDoEstado = db.ProducoesAgricolasNosEstados.Count(_ => true),
                Frota = db.FrotasDeTratoresNosMunicipios.Max(x => (DateTime?)x.ImportadoEm),
                LinhasDaFrota = db.FrotasDeTratoresNosMunicipios.Count(_ => true),
                Faixas = db.EstabelecimentosPorAreaNosMunicipios.Max(x => (DateTime?)x.ImportadoEm),
                LinhasDasFaixas = db.EstabelecimentosPorAreaNosMunicipios.Count(_ => true),
                Utilizacao = db.UtilizacoesDasTerrasNosMunicipios.Max(x => (DateTime?)x.ImportadoEm),
                LinhasDaUtilizacao = db.UtilizacoesDasTerrasNosMunicipios.Count(_ => true),
                Rebanho = db.RebanhosNosMunicipios.Max(x => (DateTime?)x.ImportadoEm),
                LinhasDoRebanho = db.RebanhosNosMunicipios.Count(_ => true),
                AreaTerritorial = db.AreasTerritoriaisDosMunicipios.Max(x => (DateTime?)x.ImportadoEm),
                LinhasDaAreaTerritorial = db.AreasTerritoriaisDosMunicipios.Count(_ => true),
                Publicado = db.MedidasDoIbgeNosEstados.Max(x => (DateTime?)x.ImportadoEm),
                LinhasDoPublicado = db.MedidasDoIbgeNosEstados.Count(_ => true),
                Usina = db.UsinasDeEtanol.Max(x => (DateTime?)x.ImportadoEm),
                UsinaEncerrada = db.UsinasDeEtanol.Max(x => x.EncerradaEm),
                Usinas = db.UsinasDeEtanol.Count(_ => true),
                Municipios = db.Municipios.Count(_ => true),
                Codigos = db.Municipios.Sum(m => (long?)m.CodigoIbge)
            }).FirstOrDefaultAsync(ct);

            return e is null
                ? string.Empty
                : string.Join('|',
                    $"{e.Area:O}", $"{e.AreaEncerrada:O}", e.LinhasDaArea, $"{e.Pam:O}", e.LinhasDaPam, $"{e.Estado:O}", e.LinhasDoEstado,
                    $"{e.Frota:O}", e.LinhasDaFrota, $"{e.Faixas:O}", e.LinhasDasFaixas, $"{e.Utilizacao:O}", e.LinhasDaUtilizacao,
                    $"{e.Rebanho:O}", e.LinhasDoRebanho, $"{e.AreaTerritorial:O}", e.LinhasDaAreaTerritorial,
                    $"{e.Publicado:O}", e.LinhasDoPublicado, $"{e.Usina:O}", $"{e.UsinaEncerrada:O}", e.Usinas,
                    e.Municipios, e.Codigos);
        }

        var p = await ancora.Select(_ => new
        {
            Pam = db.ProducoesAgricolasNosMunicipios.Max(x => (DateTime?)x.ImportadoEm),
            LinhasDaPam = db.ProducoesAgricolasNosMunicipios.Count(_ => true),
            RegraInformada = db.RegrasDePotencial.Max(r => (DateTime?)r.InformadoEm),
            RegraRevogada = db.RegrasDePotencial.Max(r => r.RevogadoEm),
            Estado = db.ProducoesAgricolasNosEstados.Max(x => (DateTime?)x.ImportadoEm)
        }).FirstOrDefaultAsync(ct);

        return p is null ? string.Empty : $"{p.Pam:O}|{p.LinhasDaPam}|{p.RegraInformada:O}|{p.RegraRevogada:O}|{p.Estado:O}";
    }
}

/// <summary>
/// O CACHE DE REFERÊNCIA (documento 54 §3.2) — o resultado de um leitor guardado na memória da API pela versão do assunto.
///
/// <para><b>Uma conta por versão, mesmo com dez telas pedindo ao mesmo tempo:</b> o que fica guardado é a tarefa da conta,
/// e quem chega depois espera a mesma. A conta que falha não fica guardada — a leitura seguinte recalcula, mesmo quando
/// quem a pediu já tinha desistido antes de ela falhar.</para>
///
/// <para><b>A versão velha some sozinha</b>: a chave nova é outra, e a entrada antiga expira sem uso em 6 horas.</para>
/// </summary>
public sealed class CacheDeReferencia(IMemoryCache memoria, IAssinaturaDosAssuntos assinatura)
{
    /// <summary>O resultado guardado, ou a conta feita agora.</summary>
    /// <typeparam name="T">O resultado do leitor.</typeparam>
    /// <param name="assunto">O assunto do dado.</param>
    /// <param name="chave">O que distingue uma conta de outra no mesmo assunto (a data das vigências, por exemplo).</param>
    /// <param name="calcular">A conta, sem cancelamento: ela é de todos os que esperam por ela.</param>
    /// <param name="ct">Cancelamento de quem espera — não cancela a conta.</param>
    public async Task<T> ObterAsync<T>(AssuntoDeReferencia assunto, string chave, Func<Task<T>> calcular, CancellationToken ct)
        where T : class
    {
        var completa = $"referencia|{assunto}|{await assinatura.LerAsync(assunto, ct)}|{chave}";
        var conta = Guardada(completa, calcular);

        // A CONTA GUARDADA JÁ FALHOU, e ninguém a tirou: quem a pediu tinha desistido (a tela foi fechada) antes de o banco
        // cair. Ela sai, e esta leitura recalcula. Só a conta que já estava guardada — a que esta leitura começa agora, se
        // falhar, falha para ela.
        if (conta.IsValueCreated && (conta.Value.IsFaulted || conta.Value.IsCanceled))
        {
            TirarSeForEsta(completa, conta);
            conta = Guardada(completa, calcular);
        }

        try
        {
            return await conta.Value.WaitAsync(ct);
        }
        catch when (!ct.IsCancellationRequested)
        {
            TirarSeForEsta(completa, conta);
            throw;
        }
    }

    private Lazy<Task<T>> Guardada<T>(string chave, Func<Task<T>> calcular) =>
        memoria.GetOrCreate(chave, entrada =>
        {
            entrada.SlidingExpiration = TimeSpan.FromHours(6);
            return new Lazy<Task<T>>(() => Iniciar(calcular), LazyThreadSafetyMode.ExecutionAndPublication);
        })!;

    // A FALHA SÍNCRONA VIRA TAREFA FALHA: o Lazy guardaria a exceção para sempre, e a conferência acima não a veria.
    private static Task<T> Iniciar<T>(Func<Task<T>> calcular)
    {
        try
        {
            return calcular();
        }
        catch (Exception erro)
        {
            return Task.FromException<T>(erro);
        }
    }

    // SÓ SAI A MESMA CONTA: outra leitura pode já ter guardado uma nova no lugar, e essa não é para tirar.
    private void TirarSeForEsta(string chave, object conta)
    {
        if (memoria.TryGetValue(chave, out var atual) && ReferenceEquals(atual, conta))
            memoria.Remove(chave);
    }
}
