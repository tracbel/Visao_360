using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O funil por estágio, contado no banco sobre <c>processo.EstagioDoProcesso</c>.
///
/// <para><b>Um <c>GROUP BY</c> por estágio, e nada mais.</b> A tabela tem uma linha por processo por estágio (índice
/// único), e o estágio já é cumulativo na gravação: contar as linhas de cada estágio É contar os processos que o
/// alcançaram. Os dois recortes caem nos dois índices da tabela — (filial, estágio, abertura) para a coorte e (filial,
/// estágio, alcance) para o fluxo.</para>
///
/// <para>Como em todo repositório, <b>não há <c>Where</c> de empresa aqui</b>: a filial entra pelo filtro global.</para>
/// </summary>
public sealed class RepositorioDoFunilPorEstagio(CrmDbContext contexto) : IRepositorioFunilPorEstagio
{
    /// <inheritdoc />
    public async Task<FunilPorEstagioApurado?> ApurarAsync(ConsultaDoFunilPorEstagio consulta, CancellationToken ct)
    {
        long? carteiraId = null;
        if (consulta.Carteira is { } chaveDaCarteira)
        {
            carteiraId = await contexto.Carteiras.AsNoTracking()
                .Where(c => c.ChavePublica == chaveDaCarteira)
                .Select(c => (long?)c.Id)
                .FirstOrDefaultAsync(ct);
            if (carteiraId is null) return null;
        }

        long? responsavelId = null;
        if (consulta.Responsavel is { } chaveDoResponsavel)
        {
            responsavelId = await contexto.Usuarios.AsNoTracking()
                .Where(u => u.ChavePublica == chaveDoResponsavel)
                .Select(u => (long?)u.Id)
                .FirstOrDefaultAsync(ct);
            if (responsavelId is null) return null;
        }

        var linhas = contexto.EstagiosDoProcesso.AsNoTracking();
        var recorte = linhas;
        if (carteiraId is { } carteira) recorte = recorte.Where(e => e.CarteiraId == carteira);
        if (responsavelId is { } responsavel) recorte = recorte.Where(e => e.ResponsavelId == responsavel);

        var noPeriodo = consulta.Base == BaseDoFunil.Abertura
            ? recorte.Where(e => e.AbertoEm >= consulta.DeUtc && e.AbertoEm < consulta.AteUtc)
            : recorte.Where(e => e.AlcancadoEm >= consulta.DeUtc && e.AlcancadoEm < consulta.AteUtc);

        var estagios = await noPeriodo
            .GroupBy(e => e.Estagio)
            .Select(g => new EstagioContado(
                g.Key,
                g.Count(),
                g.Count(e => e.PelaEntradaDigital),
                g.Count(e => e.Desfecho == SituacaoDoProcesso.Ganho),
                g.Count(e => e.Desfecho == SituacaoDoProcesso.Perdido),
                g.Count(e => e.Desfecho == SituacaoDoProcesso.Aberto),
                g.Count(e => e.Desfecho == SituacaoDoProcesso.Cancelado || e.Desfecho == SituacaoDoProcesso.Suspenso)))
            .ToListAsync(ct);

        // PARADO É O ESTÁGIO MAIS AVANÇADO, AINDA ABERTO, ALCANÇADO HÁ MAIS DO QUE O CORTE. O estágio é gravado como
        // texto, e "Pedido" > "Negociacao" em ordem alfabética não é a ordem do funil: a conta do "mais avançado" diz os
        // estágios acima de cada um pelo nome, em vez de comparar a coluna.
        var corte = consulta.ParadoDesdeAntesDeUtc;
        var parados = await recorte
            .Where(e => e.Desfecho == SituacaoDoProcesso.Aberto && e.AlcancadoEm < corte
                        && ((e.Estagio == EstagioDoFunil.Pedido
                             && !linhas.Any(o => o.NumeroDoProcessoNaOrigem == e.NumeroDoProcessoNaOrigem
                                                 && o.Estagio == EstagioDoFunil.Faturamento))
                            || (e.Estagio == EstagioDoFunil.Negociacao
                                && !linhas.Any(o => o.NumeroDoProcessoNaOrigem == e.NumeroDoProcessoNaOrigem
                                                    && (o.Estagio == EstagioDoFunil.Pedido || o.Estagio == EstagioDoFunil.Faturamento)))))
            .CountAsync(ct);

        var naFilial = await linhas.CountAsync(ct);

        var rotina = await contexto.Rotinas.AsNoTracking()
            .Where(r => r.Codigo == RotinasDoSistema.ProcessosVortice)
            .Select(r => new { r.Nome, r.EstaLigada, r.UltimaExecucaoIniciadaEm, r.UltimaExecucaoTerminadaEm, r.UltimoResultado })
            .FirstOrDefaultAsync(ct);

        return new FunilPorEstagioApurado(
            estagios,
            parados,
            naFilial,
            rotina is null
                ? null
                : new ExecucaoDaRotinaDoFunil(
                    rotina.Nome, rotina.EstaLigada, rotina.UltimaExecucaoIniciadaEm, rotina.UltimaExecucaoTerminadaEm,
                    rotina.UltimoResultado?.ToString()));
    }
}
