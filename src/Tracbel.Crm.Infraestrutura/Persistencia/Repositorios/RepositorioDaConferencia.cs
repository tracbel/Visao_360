using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A CONFERÊNCIA COM A GESTÃO DE NEGÓCIOS (28/09/2026): a última apuração da rotina 13 e as divergências abertas do
/// realizado, chassi a chassi. As duas passam pelo filtro global — as filiais ao alcance de quem lê.
/// </summary>
public sealed class RepositorioDaConferencia(CrmDbContext contexto) : IRepositorioDaConferencia
{
    private static readonly TipoDeDivergencia[] Tipos =
    [
        TipoDeDivergencia.RealizadoSoNaGestao, TipoDeDivergencia.RealizadoSoNoCrm, TipoDeDivergencia.RealizadoEmOutraFilial,
        TipoDeDivergencia.RealizadoEmOutroMes, TipoDeDivergencia.RealizadoNaoEntregueNoCrm, TipoDeDivergencia.RealizadoPendenteNoArt
    ];

    /// <inheritdoc />
    public async Task<ConferenciaLida> LerAsync(CancellationToken ct)
    {
        var linhas = await contexto.ConferenciasDaGestaoDeNegocios.AsNoTracking()
            .Join(contexto.Empresas.AsNoTracking(), c => c.EmpresaId, e => e.Id,
                (c, e) => new LinhaDaConferencia(e.Nome, c.Indicador, c.Competencia, c.NaGestao, c.NoCrm))
            .ToListAsync(ct);

        var tipos = Tipos.ToList();
        var divergencias = await contexto.DivergenciasDeIntegracao.AsNoTracking()
            .Where(d => d.Situacao == SituacaoDaDivergencia.Aberta && tipos.Contains(d.Tipo))
            .Join(contexto.Empresas.AsNoTracking(), d => d.EmpresaId, e => e.Id,
                (d, e) => new DivergenciaDaConferencia(d.Tipo, d.ChaveOrigem, e.Nome, d.Descricao, d.ValorNoCrm, d.ValorNaOrigem, d.DetectadaEm))
            .ToListAsync(ct);

        var ponto = await contexto.PontosDeSincronismo.AsNoTracking()
            .Where(p => p.Fluxo == ConferenciaDaGestaoDeNegocios.FluxoDaCarga)
            .Select(p => new { p.ProcessadoEm, p.UltimoValor })
            .FirstOrDefaultAsync(ct);

        DateTime? gerada = ponto is not null
                           && DateTime.TryParse(ponto.UltimoValor, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var instante)
            ? DateTime.SpecifyKind(instante, DateTimeKind.Utc)
            : null;

        return new ConferenciaLida(
            linhas,
            [.. divergencias.OrderBy(d => d.Tipo).ThenBy(d => d.Filial, StringComparer.Ordinal).ThenBy(d => d.Chassi, StringComparer.Ordinal)],
            ponto is null ? null : DateTime.SpecifyKind(ponto.ProcessadoEm, DateTimeKind.Utc),
            gerada);
    }
}
