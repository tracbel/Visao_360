using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A administração de usuários e concessões (issue 113).
///
/// <para>Lê <c>Usuario</c> e <c>UsuarioPerfil</c>, que estão fora da fronteira de filial por desenho — o
/// alcance chega pronto em <c>filiais</c>, decidido pelo caso de uso a partir da profundidade de
/// <c>Usuario.Ler</c>.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDeAdministracaoDeUsuarios(CrmDbContext contexto)
    : IRepositorioDeUsuariosDaAdministracao, IRepositorioDeConcessoes, IRepositorioDeReferenciasDeAcesso
{
    /// <inheritdoc />
    public async Task<PaginaDe<UsuarioNaAdministracao>> ListarAsync(
        FiltroDeUsuarios filtro, IReadOnlySet<int>? filiais, DateTime agoraUtc, CancellationToken ct)
    {
        var consulta = contexto.Usuarios.AsNoTracking().Where(u => u.ExcluidoEm == null);

        consulta = filtro.Situacao switch
        {
            SituacaoDaConta.AguardandoLiberacao => consulta.Where(u => u.AguardandoLiberacaoDesde != null),
            SituacaoDaConta.Desativada => consulta.Where(u => !u.EstaAtivo),
            _ => consulta.Where(u => u.EstaAtivo && u.AguardandoLiberacaoDesde == null)
        };

        if (filiais is not null)
            consulta = consulta.Where(u => filiais.Contains(u.EmpresaId));

        if (!string.IsNullOrWhiteSpace(filtro.Termo))
        {
            var termo = filtro.Termo.Trim();
            consulta = consulta.Where(u => u.NomeCompleto.Contains(termo) || u.NomePrincipal.Contains(termo));
        }

        var total = await consulta.CountAsync(ct);

        // QUEM ESPERA HÁ MAIS TEMPO APARECE PRIMEIRO na fila de liberação; o resto, pelo nome.
        var ordenada = filtro.Situacao == SituacaoDaConta.AguardandoLiberacao
            ? consulta.OrderBy(u => u.AguardandoLiberacaoDesde).ThenBy(u => u.NomeCompleto)
            : consulta.OrderBy(u => u.NomeCompleto);

        var linhas = await ordenada
            .Skip(filtro.Paginacao.Saltar)
            .Take(filtro.Paginacao.Tamanho)
            .Select(u => new LinhaDeUsuario(u.Id, u.ChavePublica, u.NomeCompleto, u.NomePrincipal, u.Natureza, u.EmpresaId,
                u.EstaAtivo, u.AguardandoLiberacaoDesde, u.UltimoLoginEm))
            .ToListAsync(ct);

        var itens = await ComFilialEPerfisAsync(linhas, agoraUtc, ct);
        return new PaginaDe<UsuarioNaAdministracao>(itens, filtro.Paginacao.Pagina, filtro.Paginacao.Tamanho, total);
    }

    /// <inheritdoc />
    public async Task<UsuarioDetalhadoNaAdministracao?> ObterAsync(Guid chave, IReadOnlySet<int>? filiais, DateTime agoraUtc, CancellationToken ct)
    {
        var linha = await contexto.Usuarios.AsNoTracking()
            .Where(u => u.ChavePublica == chave && u.ExcluidoEm == null)
            .Select(u => new LinhaDeUsuario(u.Id, u.ChavePublica, u.NomeCompleto, u.NomePrincipal, u.Natureza, u.EmpresaId,
                u.EstaAtivo, u.AguardandoLiberacaoDesde, u.UltimoLoginEm))
            .FirstOrDefaultAsync(ct);

        // FORA DO ALCANCE É "NÃO EXISTE" — o mesmo 404 das outras rotas: distinguir contaria a quem não pode ver
        // que a conta existe.
        if (linha is null || (filiais is not null && !filiais.Contains(linha.EmpresaId))) return null;

        var usuario = (await ComFilialEPerfisAsync([linha], agoraUtc, ct))[0];

        var concessoes = await (
                from concessao in contexto.UsuariosPerfis.AsNoTracking()
                join perfil in contexto.Perfis.AsNoTracking() on concessao.PerfilId equals perfil.Id
                join quemConcedeu in contexto.Usuarios.AsNoTracking() on concessao.ConcedidoPorId equals quemConcedeu.Id
                join empresa in contexto.Empresas.AsNoTracking() on concessao.EmpresaId equals (int?)empresa.Id into filiaisDaConcessao
                from filial in filiaisDaConcessao.DefaultIfEmpty()
                join quemRevogou in contexto.Usuarios.AsNoTracking() on concessao.RevogadaPorId equals (long?)quemRevogou.Id into revogadores
                from revogador in revogadores.DefaultIfEmpty()
                where concessao.UsuarioId == linha.Id
                select new
                {
                    concessao.Id, PerfilCodigo = perfil.Codigo, PerfilNome = perfil.Nome, PerfilAtivo = perfil.EstaAtivo,
                    FilialCodigo = filial == null ? null : filial.Codigo, FilialNome = filial == null ? null : filial.Nome,
                    concessao.Justificativa, concessao.ConcedidoEm, ConcedidaPor = quemConcedeu.NomeCompleto,
                    concessao.ExpiraEm, concessao.RevogadaEm, RevogadaPor = revogador == null ? null : revogador.NomeCompleto,
                    concessao.MotivoDaRevogacao
                })
            .ToListAsync(ct);

        var historico = concessoes
            .Select(c => new ConcessaoNaAdministracao(
                c.Id, c.PerfilCodigo, c.PerfilNome, c.FilialCodigo, c.FilialNome, c.Justificativa, c.ConcedidoEm, c.ConcedidaPor,
                c.ExpiraEm, c.RevogadaEm, c.RevogadaPor, c.MotivoDaRevogacao,
                Vigente: c.PerfilAtivo && c.RevogadaEm == null && (c.ExpiraEm == null || c.ExpiraEm > agoraUtc)))
            .OrderByDescending(c => c.Vigente)
            .ThenByDescending(c => c.ConcedidaEm)
            .ToList();

        return new UsuarioDetalhadoNaAdministracao(usuario, historico);
    }

    /// <inheritdoc />
    public Task<Usuario?> ObterContaParaAlterarAsync(Guid chave, CancellationToken ct) =>
        contexto.Usuarios.FirstOrDefaultAsync(u => u.ChavePublica == chave && u.ExcluidoEm == null, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<UsuarioPerfil>> ListarConcessoesParaAlterarAsync(long usuarioId, CancellationToken ct) =>
        await contexto.UsuariosPerfis.Where(c => c.UsuarioId == usuarioId).ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PerfilParaConceder>> ListarPerfisAsync(CancellationToken ct)
    {
        var perfis = await contexto.Perfis.AsNoTracking()
            .Where(p => p.EstaAtivo)
            .OrderBy(p => p.Nome)
            .Select(p => new { p.Id, p.Codigo, p.Nome, p.Descricao, p.EhPadrao })
            .ToListAsync(ct);

        var itens = await contexto.PerfisPermissoes.AsNoTracking()
            .Select(i => new { i.PerfilId, i.CodigoPermissao, i.Profundidade })
            .ToListAsync(ct);

        var porPerfil = itens.ToLookup(i => i.PerfilId);

        return perfis
            .Select(p => new PerfilParaConceder(
                p.Id, p.Codigo, p.Nome, p.Descricao, p.EhPadrao,
                porPerfil[p.Id].Select(i => (i.CodigoPermissao, i.Profundidade)).OrderBy(i => i.CodigoPermissao, StringComparer.Ordinal).ToList()))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<(int Id, string Nome)?> ObterFilialAtivaAsync(string codigo, CancellationToken ct)
    {
        var filial = await contexto.Empresas.AsNoTracking()
            .Where(e => e.EstaAtiva && e.Codigo == codigo)
            .Select(e => new { e.Id, e.Nome })
            .FirstOrDefaultAsync(ct);

        return filial is null ? null : (filial.Id, filial.Nome);
    }

    /// <inheritdoc />
    public async Task AdicionarConcessaoAsync(UsuarioPerfil concessao, CancellationToken ct) =>
        await contexto.UsuariosPerfis.AddAsync(concessao, ct);

    private sealed record LinhaDeUsuario(
        long Id, Guid Chave, string Nome, string NomePrincipal, NaturezaDoUsuario Natureza, int EmpresaId,
        bool EstaAtivo, DateTime? AguardandoLiberacaoDesde, DateTime? UltimoLoginEm);

    /// <summary>Completa as linhas com a filial de casa e os perfis vigentes — duas consultas, não uma por linha.</summary>
    private async Task<List<UsuarioNaAdministracao>> ComFilialEPerfisAsync(IReadOnlyList<LinhaDeUsuario> linhas, DateTime agoraUtc, CancellationToken ct)
    {
        var ids = linhas.Select(l => l.Id).ToList();
        var empresasIds = linhas.Select(l => l.EmpresaId).Distinct().ToList();

        var filiais = await contexto.Empresas.AsNoTracking()
            .Where(e => empresasIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Codigo, e.Nome })
            .ToDictionaryAsync(e => e.Id, ct);

        var perfis = (await (
                    from concessao in contexto.UsuariosPerfis.AsNoTracking()
                    join perfil in contexto.Perfis.AsNoTracking() on concessao.PerfilId equals perfil.Id
                    where ids.Contains(concessao.UsuarioId)
                          && perfil.EstaAtivo
                          && concessao.RevogadaEm == null && (concessao.ExpiraEm == null || concessao.ExpiraEm > agoraUtc)
                    select new { concessao.UsuarioId, perfil.Nome })
                .ToListAsync(ct))
            .ToLookup(p => p.UsuarioId, p => p.Nome);

        return linhas
            .Select(l => new UsuarioNaAdministracao(
                l.Chave, l.Nome, l.NomePrincipal, l.Natureza.ToString(),
                filiais.TryGetValue(l.EmpresaId, out var f) ? f.Codigo : string.Empty,
                filiais.TryGetValue(l.EmpresaId, out var g) ? g.Nome : "(filial não encontrada)",
                l.EstaAtivo, l.AguardandoLiberacaoDesde, l.UltimoLoginEm,
                perfis[l.Id].Distinct().Order(StringComparer.CurrentCulture).ToList()))
            .ToList();
    }
}
