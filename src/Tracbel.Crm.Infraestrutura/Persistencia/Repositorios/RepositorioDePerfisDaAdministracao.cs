using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>Os perfis na administração (issue 113, parte 2b). Perfil não tem filial: fica fora da fronteira.</summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDePerfisDaAdministracao(CrmDbContext contexto) : IRepositorioDePerfisDaAdministracao
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<PerfilNaAdministracao>> ListarTodosAsync(DateTime agoraUtc, CancellationToken ct)
    {
        var perfis = await contexto.Perfis.AsNoTracking()
            .Select(p => new { p.Id, p.Codigo, p.Nome, p.Descricao, p.EhPadrao, p.EstaAtivo })
            .ToListAsync(ct);

        var itens = (await contexto.PerfisPermissoes.AsNoTracking()
                .Select(i => new { i.PerfilId, i.CodigoPermissao, i.Profundidade })
                .ToListAsync(ct))
            .ToLookup(i => i.PerfilId);

        // QUANTAS PESSOAS: concessões vigentes, de contas ativas e liberadas — as que o perfil afeta hoje.
        var pessoas = (await (
                    from concessao in contexto.UsuariosPerfis.AsNoTracking()
                    join usuario in contexto.Usuarios.AsNoTracking() on concessao.UsuarioId equals usuario.Id
                    where concessao.RevogadaEm == null && (concessao.ExpiraEm == null || concessao.ExpiraEm > agoraUtc)
                          && usuario.EstaAtivo && usuario.ExcluidoEm == null && usuario.AguardandoLiberacaoDesde == null
                    select new { concessao.PerfilId, concessao.UsuarioId })
                .Distinct()
                .ToListAsync(ct))
            .GroupBy(c => c.PerfilId)
            .ToDictionary(g => g.Key, g => g.Count());

        return perfis
            .Select(p => new PerfilNaAdministracao(
                p.Codigo, p.Nome, p.Descricao, p.EhPadrao, PerfisDeSistema.EhDoSistema(p.Codigo), p.EstaAtivo,
                pessoas.GetValueOrDefault(p.Id),
                itens[p.Id]
                    .Select(i => new PermissaoDoEscopo(
                        i.CodigoPermissao,
                        Permissoes.Catalogo.TryGetValue(i.CodigoPermissao, out var descricao) ? descricao : i.CodigoPermissao,
                        i.Profundidade.ToString()))
                    .OrderBy(i => i.Codigo, StringComparer.Ordinal)
                    .ToList()))
            // Os do sistema primeiro, na ordem da semente; os próprios depois, pelo nome.
            .OrderByDescending(p => p.EhDoSistema)
            .ThenBy(p => p.EhDoSistema ? PerfisDeSistema.Todos.ToList().FindIndex(s => s.Codigo == p.Codigo) : 0)
            .ThenBy(p => p.Nome, StringComparer.CurrentCulture)
            .ToList();
    }

    /// <inheritdoc />
    public Task<Perfil?> ObterParaAlterarAsync(string codigo, CancellationToken ct) =>
        contexto.Perfis.Include(p => p.Permissoes).FirstOrDefaultAsync(p => p.Codigo == codigo, ct);

    /// <inheritdoc />
    public Task<bool> CodigoEmUsoAsync(string codigo, CancellationToken ct) =>
        contexto.Perfis.AnyAsync(p => p.Codigo.ToUpper() == codigo.ToUpper(), ct);

    /// <inheritdoc />
    public async Task AdicionarAsync(Perfil perfil, CancellationToken ct) => await contexto.Perfis.AddAsync(perfil, ct);
}
