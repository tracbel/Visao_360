using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Diagnostico;

/// <summary>
/// QUANTAS VEZES UMA REQUISIÇÃO FOI AO BANCO (documento 54 §3.5) — o número que explica o tempo: 38 idas em sequência
/// custam mais que uma consulta pesada.
///
/// <para><b>Por fluxo assíncrono, e não por requisição HTTP.</b> O meio de campo de desempenho abre a medição antes do
/// pipeline; tudo o que roda dentro dele — inclusive o cálculo do cache de referência que essa requisição disparou —
/// conta para ela. Consulta fora de uma medição (o vigia do cache, as rotinas) não conta para ninguém.</para>
/// </summary>
public static class ContadorDeConsultas
{
    private static readonly AsyncLocal<Medicao?> Atual = new();

    /// <summary>Abre a medição do fluxo atual.</summary>
    public static Medicao Iniciar()
    {
        var medicao = new Medicao();
        Atual.Value = medicao;
        return medicao;
    }

    /// <summary>Uma ida ao banco no fluxo atual.</summary>
    internal static void Registrar()
    {
        if (Atual.Value is { } medicao) Interlocked.Increment(ref medicao.ContadorInterno);
    }

    /// <summary>A medição aberta: quantas consultas o fluxo fez até agora.</summary>
    public sealed class Medicao : IDisposable
    {
        internal int ContadorInterno;

        /// <summary>As consultas até agora.</summary>
        public int Consultas => Volatile.Read(ref ContadorInterno);

        /// <inheritdoc />
        public void Dispose()
        {
            if (ReferenceEquals(Atual.Value, this)) Atual.Value = null;
        }
    }
}

/// <summary>Conta cada comando que o EF Core manda ao banco — leitura, escalar ou gravação.</summary>
public sealed class InterceptadorDeConsultas : DbCommandInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        ContadorDeConsultas.Registrar();
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        ContadorDeConsultas.Registrar();
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        ContadorDeConsultas.Registrar();
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        ContadorDeConsultas.Registrar();
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        ContadorDeConsultas.Registrar();
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ContadorDeConsultas.Registrar();
        return ValueTask.FromResult(result);
    }
}
