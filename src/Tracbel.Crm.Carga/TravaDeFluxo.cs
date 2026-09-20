using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A TRAVA DE UM FLUXO DE CARGA, no próprio banco, pelo tempo de uma conexão.
///
/// <para><b>Por que no banco e não num arquivo.</b> A rotina anual roda no servidor e a carga manual
/// roda na estação — duas máquinas diferentes, um banco só. Um arquivo de trava numa máquina não
/// enxerga a outra. O <c>sp_getapplock</c> é do SQL Server, morre junto com a conexão (inclusive se o
/// processo cair) e não deixa trava órfã para alguém limpar depois.</para>
///
/// <para><b>Ela não faz fila: recusa.</b> Com espera zero, a segunda rodada para na hora e diz o que
/// está acontecendo, em vez de ficar pendurada e depois refazer o que a primeira acabou de fazer.
/// Medido em 20/09/2026: com a trava tomada por outra sessão, a carga parou em 4 segundos.</para>
///
/// <para><b>Dona da conexão.</b> A trava é de sessão, não de transação: ela precisa sobreviver às
/// transações de escrita, que abrem e fecham dentro dela. Por isso esta classe fica com o contexto e
/// o descarta no fim — e é por isso que ela é um <c>IAsyncDisposable</c> e não um método.</para>
///
/// <para><b>Uma trava por FLUXO, e não uma por carga.</b> A produção agrícola e a estrutura
/// agropecuária escrevem em tabelas diferentes e podem rodar ao mesmo tempo sem se atrapalhar; o que
/// não pode é duas rodadas do MESMO fluxo.</para>
/// </summary>
/// <param name="contexto">O contexto que segura a trava.</param>
internal sealed class TravaDeFluxo(CrmDbContext contexto) : IAsyncDisposable
{
    /// <summary>Toma a trava do fluxo, ou recusa dizendo o que está acontecendo.</summary>
    /// <param name="contexto">Um contexto SÓ para a trava; ela passa a ser dona dele.</param>
    /// <param name="fluxo">O nome do fluxo, que é o nome da trava.</param>
    /// <param name="ct">Cancelamento.</param>
    /// <exception cref="InvalidOperationException">Quando outra rodada do mesmo fluxo está em curso.</exception>
    public static async Task<TravaDeFluxo> TomarAsync(CrmDbContext contexto, string fluxo, CancellationToken ct)
    {
        var trava = new TravaDeFluxo(contexto);

        try
        {
            // A CONEXÃO FICA ABERTA DE PROPÓSITO: é ela que segura a trava. Fechada, o SQL Server a
            // libera na hora.
            await contexto.Database.OpenConnectionAsync(ct);

            var resultado = new SqlParameter("@resultado", SqlDbType.Int) { Direction = ParameterDirection.Output };

            await contexto.Database.ExecuteSqlRawAsync(
                "EXEC @resultado = sp_getapplock @Resource = {0}, @LockMode = 'Exclusive', " +
                "@LockOwner = 'Session', @LockTimeout = 0",
                [$"carga:{fluxo}", resultado],
                ct);

            // O sp_getapplock devolve 0 (travou na hora) ou 1 (travou depois de esperar). Negativo é
            // recusa: -1 esgotou o tempo, -3 vítima de impasse, -999 erro de parâmetro.
            if (resultado.Value is int codigo && codigo < 0)
                throw new InvalidOperationException(
                    $"Outra carga do fluxo {fluxo} já está rodando neste banco (sp_getapplock devolveu " +
                    $"{codigo}). Esta parou sem ler a fonte e sem gravar nada — espere a primeira " +
                    "terminar e rode de novo.");

            return trava;
        }
        catch
        {
            await trava.DisposeAsync();
            throw;
        }
    }

    /// <summary>Solta a trava fechando a conexão que a segura.</summary>
    public async ValueTask DisposeAsync() => await contexto.DisposeAsync();
}
