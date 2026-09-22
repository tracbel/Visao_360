using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// As conexões, as rotinas e o histórico dos testes e das execuções (issue 136). Nenhuma dessas tabelas tem filial:
/// a integração é da empresa inteira.
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDeIntegracoes(CrmDbContext contexto)
    : IRepositorioDeConexoes, IRepositorioDeRotinas, IConsultaDoHistoricoDeIntegracoes
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Conexao>> ListarAsync(CancellationToken ct)
    {
        var todas = await contexto.Conexoes.AsNoTracking().ToListAsync(ct);
        var ordem = ConexoesDoSistema.Todas.Select((c, i) => (c.Codigo, i)).ToDictionary(p => p.Codigo, p => p.i, StringComparer.Ordinal);

        return todas
            .OrderBy(c => ordem.GetValueOrDefault(c.Codigo, int.MaxValue))
            .ThenBy(c => c.Nome, StringComparer.CurrentCulture)
            .ToList();
    }

    /// <inheritdoc />
    public Task<Conexao?> ObterParaAlterarAsync(string codigo, CancellationToken ct) =>
        contexto.Conexoes.FirstOrDefaultAsync(c => c.Codigo == codigo, ct);

    /// <inheritdoc />
    public async Task AdicionarAsync(Conexao conexao, CancellationToken ct) => await contexto.Conexoes.AddAsync(conexao, ct);

    /// <inheritdoc />
    public async Task AdicionarVerificacaoAsync(VerificacaoDeConexao verificacao, CancellationToken ct) => await contexto.VerificacoesDeConexao.AddAsync(verificacao, ct);

    /// <inheritdoc />
    async Task<IReadOnlyList<Rotina>> IRepositorioDeRotinas.ListarAsync(CancellationToken ct)
    {
        var todas = await contexto.Rotinas.AsNoTracking().ToListAsync(ct);
        var ordem = RotinasDoSistema.Todas.Select((r, i) => (r.Codigo, i)).ToDictionary(p => p.Codigo, p => p.i, StringComparer.Ordinal);
        return todas.OrderBy(r => ordem.GetValueOrDefault(r.Codigo, int.MaxValue)).ToList();
    }

    /// <inheritdoc />
    Task<Rotina?> IRepositorioDeRotinas.ObterParaAlterarAsync(string codigo, CancellationToken ct) =>
        contexto.Rotinas.FirstOrDefaultAsync(r => r.Codigo == codigo, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<VerificacaoNaTela>> ListarVerificacoesAsync(string codigoDaConexao, int quantos, CancellationToken ct)
    {
        var linhas = await (
                from teste in contexto.VerificacoesDeConexao.AsNoTracking()
                join conexao in contexto.Conexoes.AsNoTracking() on teste.ConexaoId equals conexao.Id
                where conexao.Codigo == codigoDaConexao
                orderby teste.VerificadaEm descending, teste.Id descending
                select new { teste.VerificadaEm, teste.VerificadaPorId, teste.Ok, teste.Resumo, teste.LatenciaMs })
            .Take(quantos)
            .ToListAsync(ct);

        var nomes = await NomesDosUsuariosAsync(linhas.Where(l => l.VerificadaPorId is not null).Select(l => l.VerificadaPorId!.Value).ToList(), ct);

        return linhas
            .Select(l => new VerificacaoNaTela(
                l.VerificadaEm, l.VerificadaPorId is { } id ? nomes.GetValueOrDefault(id, $"usuário {id}") : null, l.Ok, l.Resumo, l.LatenciaMs))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ExecucaoNaTela>> ListarExecucoesAsync(string codigoDaRotina, int quantos, CancellationToken ct)
    {
        var linhas = await (
                from execucao in contexto.ExecucoesDeRotina.AsNoTracking()
                join rotina in contexto.Rotinas.AsNoTracking() on execucao.RotinaId equals rotina.Id
                where rotina.Codigo == codigoDaRotina
                orderby execucao.IniciadaEm descending, execucao.Id descending
                select new
                {
                    execucao.IniciadaEm, execucao.TerminadaEm, execucao.Motivo, execucao.PedidaPorId, execucao.Resultado,
                    execucao.CodigoDeSaida, execucao.Mensagem, execucao.Maquina
                })
            .Take(quantos)
            .ToListAsync(ct);

        var nomes = await NomesDosUsuariosAsync(linhas.Where(l => l.PedidaPorId is not null).Select(l => l.PedidaPorId!.Value).ToList(), ct);

        return linhas
            .Select(l => new ExecucaoNaTela(
                l.IniciadaEm, l.TerminadaEm, l.Motivo.ToString(), l.PedidaPorId is { } id ? nomes.GetValueOrDefault(id, $"usuário {id}") : null,
                l.Resultado.ToString(), l.CodigoDeSaida, l.Mensagem, l.Maquina))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<long, string>> NomesDosUsuariosAsync(IReadOnlyCollection<long> ids, CancellationToken ct)
    {
        if (ids.Count == 0) return new Dictionary<long, string>();
        var distintos = ids.Distinct().ToList();
        return await contexto.Usuarios.AsNoTracking()
            .Where(u => distintos.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.NomeCompleto, ct);
    }
}
