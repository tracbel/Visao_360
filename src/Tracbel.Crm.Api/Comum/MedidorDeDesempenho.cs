using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Routing;

namespace Tracbel.Crm.Api.Comum;

/// <summary>
/// O TEMPO DE RESPOSTA DA API, MEDIDO PELO PRÓPRIO SERVIDOR (issue 51) — as últimas chamadas de cada rota, com o p50 e
/// o p95.
///
/// <para><b>Por que existe.</b> A fase 2 pôs a trilha de auditoria na mesma transação de cada gravação, e o ensaio no
/// Docker Desktop mediu ~5 ms a mais em p50 na criação de cliente (documento 45 §6.1). A decisão do critério ficou para
/// "medir no servidor", onde o SQL Server é nativo — e a medida nunca saiu, porque o roteiro dependia de uma sessão
/// autenticada na estação. Aqui a medida é feita pela API, com o tráfego de verdade, sem roteiro e sem ninguém rodar
/// nada: é só abrir Configurações › Integrações.</para>
///
/// <para><b>Em memória, por janela.</b> Cada rota guarda as últimas <see cref="AmostrasPorRota"/> durações num anel; a
/// medição recomeça quando o serviço sobe (toda publicação), e a resposta diz desde quando está medindo. Não grava nada
/// no banco — medir o tempo de gravação gravando seria medir a si mesmo.</para>
///
/// <para><b>A rota é o modelo, e não o endereço</b> (<c>/api/v1/clientes/{chave}</c>): cada cliente não vira uma rota
/// com uma chamada só.</para>
/// </summary>
public sealed class MedidorDeDesempenho
{
    /// <summary>Quantas chamadas recentes cada rota guarda.</summary>
    public const int AmostrasPorRota = 1000;

    private readonly ConcurrentDictionary<(string Metodo, string Rota), JanelaDaRota> _rotas = new();

    /// <summary>Desde quando está medindo (UTC) — a subida do serviço.</summary>
    public DateTime DesdeUtc { get; } = DateTime.UtcNow;

    /// <summary>Registra uma chamada.</summary>
    /// <param name="metodo">GET, POST…</param>
    /// <param name="rota">O modelo da rota.</param>
    /// <param name="milissegundos">Quanto durou, do começo ao fim do pipeline.</param>
    /// <param name="status">O código HTTP devolvido.</param>
    public void Registrar(string metodo, string rota, double milissegundos, int status) =>
        _rotas.GetOrAdd((metodo, rota), _ => new JanelaDaRota()).Registrar(milissegundos, status >= 500);

    /// <summary>O resumo de cada rota medida, da que tem o p95 mais alto para a mais baixa.</summary>
    public IReadOnlyList<DesempenhoDaRota> Resumo() =>
    [
        .. _rotas
            .Select(par => par.Value.Resumir(par.Key.Metodo, par.Key.Rota))
            .OrderByDescending(r => r.P95)
            .ThenBy(r => r.Rota, StringComparer.Ordinal)
    ];

    /// <summary>
    /// O PERCENTIL POR POSTO MAIS PRÓXIMO — o valor que deixa <paramref name="percentil"/>% das amostras em ou abaixo
    /// dele. Sem interpolação: o p95 é uma chamada que aconteceu, e não um tempo inventado entre duas.
    /// </summary>
    /// <param name="ordenadas">As durações, em ordem crescente.</param>
    /// <param name="percentil">De 0 a 100.</param>
    public static double Percentil(IReadOnlyList<double> ordenadas, double percentil)
    {
        if (ordenadas.Count == 0) return 0;
        var posto = (int)Math.Ceiling(percentil / 100 * ordenadas.Count);
        return ordenadas[Math.Clamp(posto, 1, ordenadas.Count) - 1];
    }

    private sealed class JanelaDaRota
    {
        private readonly double[] _anel = new double[AmostrasPorRota];
        private readonly Lock _trava = new();
        private int _preenchidas;
        private int _proxima;
        private long _chamadas;
        private long _erros;

        public void Registrar(double milissegundos, bool erro)
        {
            lock (_trava)
            {
                _anel[_proxima] = milissegundos;
                _proxima = (_proxima + 1) % AmostrasPorRota;
                if (_preenchidas < AmostrasPorRota) _preenchidas++;
                _chamadas++;
                if (erro) _erros++;
            }
        }

        public DesempenhoDaRota Resumir(string metodo, string rota)
        {
            double[] copia;
            long chamadas, erros;
            lock (_trava)
            {
                copia = _anel[.._preenchidas];
                chamadas = _chamadas;
                erros = _erros;
            }

            Array.Sort(copia);
            return new DesempenhoDaRota(
                metodo,
                rota,
                chamadas,
                copia.Length,
                Math.Round(Percentil(copia, 50), 1),
                Math.Round(Percentil(copia, 95), 1),
                copia.Length == 0 ? 0 : Math.Round(copia[^1], 1),
                erros,
                // GRAVAÇÃO PASSA PELA TRILHA de auditoria na mesma transação (fase 2) — é a leitura que o doc 45 pede.
                !HttpMethods.IsGet(metodo) && !HttpMethods.IsHead(metodo));
        }
    }
}

/// <summary>O tempo de resposta de uma rota, nas chamadas recentes.</summary>
/// <param name="Metodo">GET, POST…</param>
/// <param name="Rota">O modelo da rota.</param>
/// <param name="Chamadas">Quantas chamadas desde a subida.</param>
/// <param name="Amostras">Quantas entraram no cálculo — as mais recentes, até o tamanho da janela.</param>
/// <param name="P50">A mediana, em milissegundos.</param>
/// <param name="P95">O percentil 95, em milissegundos.</param>
/// <param name="Maximo">A mais lenta da janela, em milissegundos.</param>
/// <param name="Erros">Quantas responderam 5xx desde a subida.</param>
/// <param name="Grava">Se é gravação — e, portanto, passa pela trilha de auditoria.</param>
public sealed record DesempenhoDaRota(
    string Metodo, string Rota, long Chamadas, int Amostras, double P50, double P95, double Maximo, long Erros, bool Grava);

/// <summary>A medição inteira.</summary>
/// <param name="DesdeUtc">Desde quando o serviço está medindo — a última subida.</param>
/// <param name="AmostrasPorRota">O tamanho da janela de cada rota.</param>
/// <param name="Rotas">Cada rota chamada, do p95 mais alto para o mais baixo.</param>
public sealed record DesempenhoDaApi(DateTime DesdeUtc, int AmostrasPorRota, IReadOnlyList<DesempenhoDaRota> Rotas);

/// <summary>
/// O CRONÔMETRO DE CADA CHAMADA À API — do começo ao fim do pipeline, como o "Request finished" do servidor que o doc 45
/// usou no ensaio. Fica ANTES do contexto de acesso: montar o contexto também é tempo que o usuário espera.
/// </summary>
/// <param name="proximo">O resto do pipeline.</param>
/// <param name="medidor">Onde a medida fica.</param>
public sealed class MeioDeCampoDeDesempenho(RequestDelegate proximo, MedidorDeDesempenho medidor)
{
    /// <summary>Mede a chamada, se for da API.</summary>
    /// <param name="http">A chamada.</param>
    public async Task InvokeAsync(HttpContext http)
    {
        if (!http.Request.Path.StartsWithSegments("/api"))
        {
            await proximo(http);
            return;
        }

        var inicio = Stopwatch.GetTimestamp();
        try
        {
            await proximo(http);
        }
        finally
        {
            // A ROTA DE GRUPO VEM COM A BARRA DO FIM ("/api/v1/clientes/"): sem ela, a tela mostra o endereço que se digita.
            var modelo = (http.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
            var rota = modelo is null ? "(rota não encontrada)" : modelo.Length > 1 ? modelo.TrimEnd('/') : modelo;
            medidor.Registrar(http.Request.Method, rota, Stopwatch.GetElapsedTime(inicio).TotalMilliseconds, http.Response.StatusCode);
        }
    }
}
