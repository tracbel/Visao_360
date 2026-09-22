using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A leitura de <c>auditoria.AlteracaoDeCampo</c> para a tela de Auditoria (issue 135).
///
/// <para><b>A fronteira de filial é o filtro global</b> que o contexto põe em toda tabela com <c>EmpresaId</c> — a
/// trilha inclusive. Nada aqui a desliga: quem quer ver todas as filiais escolhe "Todas as filiais" no seletor, como
/// em qualquer outra tela.</para>
///
/// <para><b>Paginação por evento, não por linha.</b> Uma gravação que muda cinco campos de um registro grava cinco
/// linhas com o mesmo instante; a página conta eventos, para uma alteração não ficar cortada ao meio entre duas
/// páginas. Duas consultas: a página de eventos (<c>DISTINCT</c> da chave), e os campos só desses eventos.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDaTrilhaDeAuditoria(CrmDbContext contexto) : IRepositorioDaTrilhaDeAuditoria
{
    /// <inheritdoc />
    public async Task<PaginaDe<EventoNaTrilha>> ListarAsync(FiltroDaTrilha filtro, CancellationToken ct)
    {
        var linhas = Filtrar(filtro);

        // O QUE FAZ DE VÁRIAS LINHAS UM EVENTO SÓ: a mesma gravação (o instante é um por SaveChanges), no mesmo
        // registro, pelo mesmo autor. Tipo anônimo, e não registro: é o que o EF traduz em DISTINCT.
        var eventos = linhas
            .Select(a => new { Quando = a.AlteradoEm, a.Entidade, a.RegistroId, a.Operacao, a.Origem, AutorId = a.AlteradoPorId, a.EmpresaId, a.SistemaId, Correlacao = a.CorrelacaoId })
            .Distinct();

        var total = await eventos.CountAsync(ct);

        var pagina = await eventos
            .OrderByDescending(e => e.Quando)
            .ThenBy(e => e.Entidade)
            .ThenByDescending(e => e.RegistroId)
            .Skip(filtro.Paginacao.Saltar)
            .Take(filtro.Paginacao.Tamanho)
            .ToListAsync(ct);

        if (pagina.Count == 0)
            return new PaginaDe<EventoNaTrilha>([], filtro.Paginacao.Pagina, filtro.Paginacao.Tamanho, total);

        // OS CAMPOS SÓ DOS EVENTOS DA PÁGINA: o intervalo de instantes e os registros dela estreitam a consulta, e a
        // chave inteira separa, em memória, o que caiu no intervalo sem ser da página.
        var inicio = pagina.Min(e => e.Quando);
        var fim = pagina.Max(e => e.Quando);
        var registros = pagina.Select(e => e.RegistroId).Distinct().ToList();

        var campos = (await linhas
                .Where(a => a.AlteradoEm >= inicio && a.AlteradoEm <= fim && registros.Contains(a.RegistroId))
                .Select(a => new
                {
                    Chave = new { Quando = a.AlteradoEm, a.Entidade, a.RegistroId, a.Operacao, a.Origem, AutorId = a.AlteradoPorId, a.EmpresaId, a.SistemaId, Correlacao = a.CorrelacaoId },
                    a.Id, a.Campo, a.ValorAnterior, a.ValorNovo
                })
                .ToListAsync(ct))
            .ToLookup(c => c.Chave, c => (c.Id, Campo: new CampoNaTrilha(c.Campo, c.ValorAnterior, c.ValorNovo)));

        var autoresIds = pagina.Select(e => e.AutorId).Distinct().ToList();
        var autores = await contexto.Usuarios.AsNoTracking()
            .Where(u => autoresIds.Contains(u.Id))
            .Select(u => new { u.Id, u.NomeCompleto, u.NomePrincipal })
            .ToDictionaryAsync(u => u.Id, ct);

        var filiaisIds = pagina.Select(e => e.EmpresaId).Distinct().ToList();
        var filiais = await contexto.Empresas.AsNoTracking()
            .Where(e => filiaisIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Codigo, e.Nome })
            .ToDictionaryAsync(e => e.Id, ct);

        var sistemasIds = pagina.Where(e => e.SistemaId != null).Select(e => e.SistemaId!.Value).Distinct().ToList();
        var sistemas = sistemasIds.Count == 0
            ? new Dictionary<int, string>()
            : await contexto.Sistemas.AsNoTracking()
                .Where(s => sistemasIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Nome, ct);

        var itens = pagina
            .Select(e => new EventoNaTrilha(
                e.Quando, e.Entidade, e.RegistroId, e.Operacao, e.Origem,
                e.SistemaId is { } s && sistemas.TryGetValue(s, out var sistema) ? sistema : null,
                e.AutorId,
                autores.TryGetValue(e.AutorId, out var autor) ? autor.NomeCompleto : $"usuário {e.AutorId}",
                autores.TryGetValue(e.AutorId, out var login) ? login.NomePrincipal : string.Empty,
                filiais.TryGetValue(e.EmpresaId, out var filial) ? filial.Codigo : string.Empty,
                filiais.TryGetValue(e.EmpresaId, out var nome) ? nome.Nome : "(filial não encontrada)",
                e.Correlacao,
                campos[e].OrderBy(c => c.Id).Select(c => c.Campo).ToList()))
            .ToList();

        return new PaginaDe<EventoNaTrilha>(itens, filtro.Paginacao.Pagina, filtro.Paginacao.Tamanho, total);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<(string Entidade, long Id), string>> DescreverRegistrosAsync(
        IReadOnlyCollection<(string Entidade, long Id)> registros, CancellationToken ct)
    {
        var resultado = new Dictionary<(string Entidade, long Id), string>();
        var porEntidade = registros.GroupBy(r => r.Entidade).ToDictionary(g => g.Key, g => g.Select(r => r.Id).Distinct().ToList());

        List<long> Ids(string entidade) => porEntidade.GetValueOrDefault(entidade) ?? [];
        List<int> IdsCurtos(string entidade) => Ids(entidade).Where(i => i is > 0 and <= int.MaxValue).Select(i => (int)i).ToList();

        if (Ids(nameof(Usuario)) is { Count: > 0 } usuarios)
            foreach (var u in await contexto.Usuarios.AsNoTracking().Where(u => usuarios.Contains(u.Id)).Select(u => new { u.Id, u.NomeCompleto }).ToListAsync(ct))
                resultado[(nameof(Usuario), u.Id)] = u.NomeCompleto;

        if (IdsCurtos(nameof(Perfil)) is { Count: > 0 } perfis)
            foreach (var p in await contexto.Perfis.AsNoTracking().Where(p => perfis.Contains(p.Id)).Select(p => new { p.Id, p.Nome }).ToListAsync(ct))
                resultado[(nameof(Perfil), p.Id)] = p.Nome;

        if (Ids(nameof(UsuarioPerfil)) is { Count: > 0 } concessoes)
        {
            var linhas = await (
                    from concessao in contexto.UsuariosPerfis.AsNoTracking()
                    join perfil in contexto.Perfis.AsNoTracking() on concessao.PerfilId equals perfil.Id
                    join usuario in contexto.Usuarios.AsNoTracking() on concessao.UsuarioId equals usuario.Id
                    where concessoes.Contains(concessao.Id)
                    select new { concessao.Id, Perfil = perfil.Nome, Pessoa = usuario.NomeCompleto })
                .ToListAsync(ct);
            foreach (var c in linhas) resultado[(nameof(UsuarioPerfil), c.Id)] = $"{c.Perfil} para {c.Pessoa}";
        }

        if (IdsCurtos(nameof(PerfilPermissao)) is { Count: > 0 } itens)
        {
            var linhas = await (
                    from item in contexto.PerfisPermissoes.AsNoTracking()
                    join perfil in contexto.Perfis.AsNoTracking() on item.PerfilId equals perfil.Id
                    where itens.Contains(item.Id)
                    select new { item.Id, item.CodigoPermissao, Perfil = perfil.Nome })
                .ToListAsync(ct);
            foreach (var i in linhas) resultado[(nameof(PerfilPermissao), i.Id)] = $"{i.CodigoPermissao} em {i.Perfil}";
        }

        if (Ids(nameof(Cliente)) is { Count: > 0 } clientes)
            foreach (var c in await contexto.Clientes.AsNoTracking().Where(c => clientes.Contains(c.Id)).Select(c => new { c.Id, c.NomeRazao }).ToListAsync(ct))
                resultado[(nameof(Cliente), c.Id)] = c.NomeRazao;

        if (Ids(nameof(Equipamento)) is { Count: > 0 } equipamentos)
            foreach (var e in await contexto.Equipamentos.AsNoTracking().Where(e => equipamentos.Contains(e.Id)).Select(e => new { e.Id, e.Chassi }).ToListAsync(ct))
                resultado[(nameof(Equipamento), e.Id)] = $"chassi {e.Chassi}";

        if (IdsCurtos(nameof(Municipio)) is { Count: > 0 } municipios)
            foreach (var m in await contexto.Municipios.AsNoTracking().Where(m => municipios.Contains(m.Id)).Select(m => new { m.Id, m.Nome, m.Uf }).ToListAsync(ct))
                resultado[(nameof(Municipio), m.Id)] = $"{m.Nome}/{m.Uf}";

        if (IdsCurtos(nameof(Conexao)) is { Count: > 0 } conexoes)
            foreach (var c in await contexto.Conexoes.AsNoTracking().Where(c => conexoes.Contains(c.Id)).Select(c => new { c.Id, c.Nome }).ToListAsync(ct))
                resultado[(nameof(Conexao), c.Id)] = c.Nome;

        if (IdsCurtos(nameof(Rotina)) is { Count: > 0 } rotinas)
            foreach (var r in await contexto.Rotinas.AsNoTracking().Where(r => rotinas.Contains(r.Id)).Select(r => new { r.Id, r.Nome }).ToListAsync(ct))
                resultado[(nameof(Rotina), r.Id)] = r.Nome;

        return resultado;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<(TipoDeReferencia Tipo, long Id), string>> ResolverReferenciasAsync(
        IReadOnlyCollection<(TipoDeReferencia Tipo, long Id)> referencias, CancellationToken ct)
    {
        var resultado = new Dictionary<(TipoDeReferencia Tipo, long Id), string>();
        var porTipo = referencias.GroupBy(r => r.Tipo).ToDictionary(g => g.Key, g => g.Select(r => r.Id).Distinct().ToList());

        List<long> Ids(TipoDeReferencia tipo) => porTipo.GetValueOrDefault(tipo) ?? [];
        List<int> IdsCurtos(TipoDeReferencia tipo) => Ids(tipo).Where(i => i is > 0 and <= int.MaxValue).Select(i => (int)i).ToList();

        if (Ids(TipoDeReferencia.Usuario) is { Count: > 0 } usuarios)
            foreach (var u in await contexto.Usuarios.AsNoTracking().Where(u => usuarios.Contains(u.Id)).Select(u => new { u.Id, u.NomeCompleto }).ToListAsync(ct))
                resultado[(TipoDeReferencia.Usuario, u.Id)] = u.NomeCompleto;

        if (IdsCurtos(TipoDeReferencia.Empresa) is { Count: > 0 } empresas)
            foreach (var e in await contexto.Empresas.AsNoTracking().Where(e => empresas.Contains(e.Id)).Select(e => new { e.Id, e.Codigo, e.Nome }).ToListAsync(ct))
                resultado[(TipoDeReferencia.Empresa, e.Id)] = $"{e.Codigo} · {e.Nome}";

        if (IdsCurtos(TipoDeReferencia.Perfil) is { Count: > 0 } perfis)
            foreach (var p in await contexto.Perfis.AsNoTracking().Where(p => perfis.Contains(p.Id)).Select(p => new { p.Id, p.Nome }).ToListAsync(ct))
                resultado[(TipoDeReferencia.Perfil, p.Id)] = p.Nome;

        if (IdsCurtos(TipoDeReferencia.Municipio) is { Count: > 0 } municipios)
            foreach (var m in await contexto.Municipios.AsNoTracking().Where(m => municipios.Contains(m.Id)).Select(m => new { m.Id, m.Nome, m.Uf }).ToListAsync(ct))
                resultado[(TipoDeReferencia.Municipio, m.Id)] = $"{m.Nome}/{m.Uf}";

        if (Ids(TipoDeReferencia.Cliente) is { Count: > 0 } clientes)
            foreach (var c in await contexto.Clientes.AsNoTracking().Where(c => clientes.Contains(c.Id)).Select(c => new { c.Id, c.NomeRazao }).ToListAsync(ct))
                resultado[(TipoDeReferencia.Cliente, c.Id)] = c.NomeRazao;

        return resultado;
    }

    private IQueryable<AlteracaoDeCampo> Filtrar(FiltroDaTrilha filtro)
    {
        var consulta = contexto.AlteracoesDeCampo.AsNoTracking()
            .Where(a => a.AlteradoEm >= filtro.DeUtc && a.AlteradoEm < filtro.AteUtc);

        if (filtro.Entidade is { } entidade) consulta = consulta.Where(a => a.Entidade == entidade);
        if (filtro.RegistroId is { } registro) consulta = consulta.Where(a => a.RegistroId == registro);
        if (filtro.Origem is { } origem) consulta = consulta.Where(a => a.Origem == origem);
        if (filtro.Operacao is { } operacao) consulta = consulta.Where(a => a.Operacao == operacao);

        if (!string.IsNullOrWhiteSpace(filtro.Autor))
        {
            var termo = filtro.Autor.Trim();
            var autores = contexto.Usuarios.Where(u => u.NomeCompleto.Contains(termo) || u.NomePrincipal.Contains(termo)).Select(u => u.Id);
            consulta = consulta.Where(a => autores.Contains(a.AlteradoPorId));
        }

        return consulta;
    }
}
