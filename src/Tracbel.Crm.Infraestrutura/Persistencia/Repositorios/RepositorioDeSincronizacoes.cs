using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O registro das sincronizações para a administração: por fluxo, a última execução, o último sucesso e
/// as execuções recentes. A tabela não tem filial — a execução é do sistema, não de uma filial.
/// </summary>
public sealed class RepositorioDeSincronizacoes(CrmDbContext contexto) : IRepositorioSincronizacoes
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SituacaoDaSincronizacao>> ListarAsync(int execucoesPorFluxo, CancellationToken ct)
    {
        var sistemas = await contexto.Sistemas.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Codigo, ct);

        var fluxos = await contexto.ExecucoesDeSincronizacao.AsNoTracking()
            .Select(e => new { e.SistemaId, e.Fluxo })
            .Distinct()
            .ToListAsync(ct);

        var situacoes = new List<SituacaoDaSincronizacao>();
        foreach (var fluxo in fluxos.OrderBy(f => f.Fluxo, StringComparer.Ordinal))
        {
            var doFluxo = contexto.ExecucoesDeSincronizacao.AsNoTracking()
                .Where(e => e.SistemaId == fluxo.SistemaId && e.Fluxo == fluxo.Fluxo);

            var recentes = await doFluxo
                .OrderByDescending(e => e.IniciadaEm)
                .Take(execucoesPorFluxo)
                .ToListAsync(ct);

            var ultimoSucesso = await doFluxo
                .Where(e => e.Resultado == ResultadoDaExecucao.Sucesso)
                .MaxAsync(e => e.TerminadaEm, ct);

            situacoes.Add(new SituacaoDaSincronizacao(
                sistemas.GetValueOrDefault(fluxo.SistemaId, "?"),
                fluxo.Fluxo,
                recentes.FirstOrDefault()?.IniciadaEm,
                recentes.FirstOrDefault()?.Resultado.ToString(),
                ultimoSucesso,
                [.. recentes.Select(e => new ExecucaoParaConsulta(
                    e.Maquina, e.IniciadaEm, e.TerminadaEm, e.Resultado.ToString(), e.Tentativas,
                    e.RegistrosLidos, e.Incluidos, e.Atualizados, e.Pendentes, e.Mensagem))]));
        }

        return situacoes;
    }
}
