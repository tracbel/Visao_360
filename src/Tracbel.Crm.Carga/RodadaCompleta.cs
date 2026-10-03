using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A ÚLTIMA LEITURA COMPLETA DE UMA CARGA QUE REGISTRA A RODADA NUM PONTO DE SINCRONISMO (documento 54, passo 6) — as ordens
/// de serviço e as peças. A completa tem um ponto próprio (o fluxo da carga com <c>_COMPLETO</c>), gravado só na transação que
/// deu certo, e o "até onde leu" dele guarda o instante da leitura pelo relógio da carga — é dele que sai a próxima decisão.
/// </summary>
internal static class RodadaCompleta
{
    /// <summary>O instante da última leitura completa; nulo quando nunca houve.</summary>
    /// <param name="contexto">O banco.</param>
    /// <param name="fluxo">O fluxo da completa.</param>
    /// <param name="ct">Cancelamento.</param>
    internal static async Task<DateTime?> LerAsync(CrmDbContext contexto, string fluxo, CancellationToken ct)
    {
        var ultimoValor = await contexto.PontosDeSincronismo.AsNoTracking()
            .Where(p => p.Fluxo == fluxo)
            .Select(p => p.UltimoValor)
            .FirstOrDefaultAsync(ct);

        return DateTime.TryParse(ultimoValor, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var instante)
            ? instante
            : null;
    }

    /// <summary>Registra a leitura completa, com o instante dela no "até onde leu".</summary>
    /// <param name="contexto">O banco — dentro da transação da carga.</param>
    /// <param name="sistemaId">O sistema de origem.</param>
    /// <param name="fluxo">O fluxo da completa.</param>
    /// <param name="lidaEm">O instante da leitura, pelo relógio da carga.</param>
    /// <param name="lidos">O que a leitura trouxe.</param>
    /// <param name="gravados">O que a rodada gravou.</param>
    /// <param name="ct">Cancelamento.</param>
    internal static Task RegistrarAsync(
        CrmDbContext contexto, int sistemaId, string fluxo, DateTime lidaEm, int lidos, int gravados, CancellationToken ct) =>
        CargaDeTerritorio.RegistrarRodadaAsync(
            contexto, sistemaId, fluxo, lidos, gravados, 0, ct, lidaEm.ToString("O", CultureInfo.InvariantCulture));
}
