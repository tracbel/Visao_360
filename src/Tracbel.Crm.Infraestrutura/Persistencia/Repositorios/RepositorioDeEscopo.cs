using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// As filiais que o usuário pode escolher — a mesma regra do <c>EscopoDeAcesso</c> (P-20): a de casa, as
/// que têm perfil concedido e vigente nelas e, para quem tem a visão entre filiais em profundidade
/// Organização, todas as ativas.
///
/// <para>Lê <c>Usuario</c> e <c>UsuarioPerfil</c>, que estão fora da fronteira de filial por desenho —
/// são o que define o escopo, e não o que ele protege.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDeEscopo(CrmDbContext contexto) : IRepositorioDeEscopo
{
    /// <inheritdoc />
    public async Task<(FilialDoEscopo Atual, IReadOnlyList<FilialDoEscopo> Permitidas)> FiliaisAsync(ContextoAcesso acesso, CancellationToken ct)
    {
        var agora = DateTime.UtcNow;

        var casa = await contexto.Usuarios.AsNoTracking()
            .Where(u => u.Id == acesso.UsuarioId)
            .Select(u => u.EmpresaId)
            .FirstOrDefaultAsync(ct);

        var ativas = contexto.Empresas.AsNoTracking().Where(e => e.EstaAtiva);

        if (!acesso.PodeAlcancarTodasAsEmpresas)
        {
            var concedidas = await (
                    from concessao in contexto.UsuariosPerfis.AsNoTracking()
                    join perfil in contexto.Perfis.AsNoTracking() on concessao.PerfilId equals perfil.Id
                    where concessao.UsuarioId == acesso.UsuarioId
                          && concessao.EmpresaId != null
                          && perfil.EstaAtivo
                          && concessao.RevogadaEm == null && (concessao.ExpiraEm == null || concessao.ExpiraEm > agora)
                    select concessao.EmpresaId!.Value)
                .Distinct()
                .ToListAsync(ct);

            concedidas.Add(casa);
            ativas = ativas.Where(e => concedidas.Contains(e.Id));
        }

        var permitidas = (await ativas.OrderBy(e => e.Codigo).Select(e => new { e.Id, e.Codigo, e.Nome }).ToListAsync(ct))
            .Select(e => new FilialDoEscopo(e.Codigo, e.Nome, e.Id == casa))
            .ToList();

        var atual = await contexto.Empresas.AsNoTracking()
            .Where(e => e.Id == acesso.EmpresaId)
            .Select(e => new FilialDoEscopo(e.Codigo, e.Nome, e.Id == casa))
            .FirstOrDefaultAsync(ct)
            ?? new FilialDoEscopo(string.Empty, "(filial do contexto não encontrada)", false);

        return (atual, permitidas);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PerfilDoEscopo>> PerfisAsync(ContextoAcesso acesso, CancellationToken ct)
    {
        var agora = DateTime.UtcNow;

        var padrao = await contexto.Perfis.AsNoTracking()
            .Where(p => p.EhPadrao && p.EstaAtivo)
            .Select(p => new PerfilDoEscopo(p.Codigo, p.Nome, true, null, null, null))
            .ToListAsync(ct);

        // AS MESMAS CONCESSÕES QUE O ESCOPO HONRA: perfil ativo, concessão vigente. Uma que expirou não
        // aparece, porque não dá mais nada.
        var concedidos = await (
                from concessao in contexto.UsuariosPerfis.AsNoTracking()
                join perfil in contexto.Perfis.AsNoTracking() on concessao.PerfilId equals perfil.Id
                join empresa in contexto.Empresas.AsNoTracking() on concessao.EmpresaId equals (int?)empresa.Id into filiais
                from filial in filiais.DefaultIfEmpty()
                where concessao.UsuarioId == acesso.UsuarioId
                      && perfil.EstaAtivo
                      && !perfil.EhPadrao
                      && concessao.RevogadaEm == null && (concessao.ExpiraEm == null || concessao.ExpiraEm > agora)
                select new PerfilDoEscopo(
                    perfil.Codigo, perfil.Nome, false,
                    filial == null ? null : filial.Codigo,
                    filial == null ? null : filial.Nome,
                    concessao.ExpiraEm))
            .ToListAsync(ct);

        return [.. padrao, .. concedidos.OrderBy(p => p.Nome, StringComparer.CurrentCulture).ThenBy(p => p.FilialCodigo, StringComparer.Ordinal)];
    }
}
