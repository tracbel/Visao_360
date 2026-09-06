using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso ao TERRITÓRIO — o catálogo de municípios e a cobertura por filial e por carteira.
///
/// <para><b>O agrupamento que este repositório entrega é o que existe no dado</b>, e não o do
/// protótipo: filial e carteira. A "regional" das telas atuais não tem equivalente no sistema de
/// origem — <c>IVS_Regional</c> existe lá e está vazia —, e por isso não existe consulta nenhuma
/// aqui que a produza.</para>
///
/// <para><b>A fronteira de multiempresa chega pelas carteiras.</b> As duas consultas de cobertura
/// partem de <c>organizacao.Carteira</c>, que carrega <c>EmpresaId</c> e o filtro global do
/// <see cref="CrmDbContext"/> — nenhuma delas escreve um <c>WHERE</c> de empresa à mão, que é a
/// regra 11.3 do documento 14. O catálogo de municípios, ao contrário, é deliberadamente NÃO
/// filtrado: município é dado nacional, e o CEN precisa poder cadastrar cliente em cidade de
/// outra filial.</para>
/// </summary>
public sealed class RepositorioDeTerritorio(CrmDbContext contexto) : IRepositorioTerritorio
{
    /// <inheritdoc />
    public async Task<PaginaDe<MunicipioParaSelecao>> ListarMunicipiosAsync(
        ConsultaDeMunicipios consulta, CancellationToken ct)
    {
        var municipios = contexto.Municipios.AsNoTracking().Where(m => m.EstaAtivo);

        if (!string.IsNullOrWhiteSpace(consulta.Uf))
        {
            var uf = consulta.Uf.Trim().ToUpperInvariant();
            municipios = municipios.Where(m => m.Uf == uf);
        }

        // BUSCA POR PREFIXO, e a decisão está declarada em ConsultaDeMunicipios: StartsWith usa
        // o índice IX_Municipio_Nome; Contains varreria as dez mil linhas a cada tecla. Sob a
        // colação Latin1_General_CI_AI, "sao" já encontra "São" sem nenhuma coluna derivada.
        if (!string.IsNullOrWhiteSpace(consulta.Termo))
        {
            var termo = consulta.Termo.Trim();
            municipios = municipios.Where(m => m.Nome.StartsWith(termo));
        }

        var total = await municipios.CountAsync(ct);

        var itens = await municipios
            .OrderBy(m => m.Nome).ThenBy(m => m.Uf)
            .Skip(consulta.Paginacao.Saltar)
            .Take(consulta.Paginacao.Tamanho)
            .Select(m => new MunicipioParaSelecao(m.Id, m.Nome, m.Uf, m.CodigoIbge))
            .ToListAsync(ct);

        return new PaginaDe<MunicipioParaSelecao>(
            itens, consulta.Paginacao.Pagina, consulta.Paginacao.Tamanho, total);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CoberturaDeFilial>> ResumirCoberturaPorFilialAsync(
        CancellationToken ct)
    {
        // UMA LEITURA, NÃO UMA POR FILIAL. São treze filiais e algumas centenas de carteiras: o
        // par carteira × município vem inteiro e o agrupamento acontece aqui, porque a pergunta
        // "quantos municípios DISTINTOS esta filial atende" é uma contagem sobre conjunto, e
        // conjunto é o que a memória faz melhor que o SQL sem uma segunda varredura.
        var pares = await (
                from vinculo in contexto.CarteiraMunicipios.AsNoTracking()
                    .Where(v => v.DesvinculadoEm == null)
                join carteira in contexto.Carteiras.Where(c => c.ExcluidoEm == null)
                    on vinculo.CarteiraId equals carteira.Id
                join municipio in contexto.Municipios on vinculo.MunicipioId equals municipio.Id
                select new { carteira.EmpresaId, carteira.Id, municipio.Uf, MunicipioId = municipio.Id })
            .ToListAsync(ct);

        var carteirasPorEmpresa = await contexto.Carteiras.AsNoTracking()
            .Where(c => c.ExcluidoEm == null)
            .GroupBy(c => c.EmpresaId)
            .Select(g => new { EmpresaId = g.Key, Carteiras = g.Count() })
            .ToListAsync(ct);

        var empresas = await contexto.Empresas.AsNoTracking()
            .Select(e => new { e.Id, e.ChavePublica, e.Codigo, e.Nome })
            .ToDictionaryAsync(e => e.Id, ct);

        var porEmpresa = pares.GroupBy(p => p.EmpresaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return
        [
            .. carteirasPorEmpresa
                .Where(c => empresas.ContainsKey(c.EmpresaId))
                .Select(c =>
                {
                    var doTerritorio = porEmpresa.TryGetValue(c.EmpresaId, out var lista)
                        ? lista
                        : [];

                    var empresa = empresas[c.EmpresaId];

                    return new CoberturaDeFilial(
                        empresa.ChavePublica,
                        empresa.Codigo,
                        empresa.Nome,
                        c.Carteiras,
                        doTerritorio.Select(p => p.Id).Distinct().Count(),
                        doTerritorio.Select(p => p.MunicipioId).Distinct().Count(),
                        [.. doTerritorio.Select(p => p.Uf).Distinct().Order(StringComparer.Ordinal)]);
                })
                .OrderByDescending(f => f.Municipios)
                .ThenBy(f => f.EmpresaCodigo, StringComparer.Ordinal)
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TerritorioDeCarteira>> ListarTerritorioPorCarteiraAsync(
        string? empresaCodigo, CancellationToken ct)
    {
        var carteiras = contexto.Carteiras.AsNoTracking().Where(c => c.ExcluidoEm == null);

        if (!string.IsNullOrWhiteSpace(empresaCodigo))
        {
            var codigo = empresaCodigo.Trim();

            carteiras = carteiras.Where(c => contexto.Empresas
                .Any(e => e.Id == c.EmpresaId && e.Codigo == codigo));
        }

        var cabecalhos = await carteiras
            .Select(c => new
            {
                c.Id,
                c.ChavePublica,
                c.Codigo,
                c.Nome,
                LinhaDeNegocio = contexto.LinhasDeNegocio
                    .Where(l => l.Id == c.LinhaDeNegocioId).Select(l => l.Nome).FirstOrDefault(),
                Responsavel = contexto.Usuarios
                    .Where(u => u.Id == c.ResponsavelId).Select(u => u.NomeExibicao).FirstOrDefault(),
                EmpresaCodigo = contexto.Empresas
                    .Where(e => e.Id == c.EmpresaId).Select(e => e.Codigo).FirstOrDefault(),
                EmpresaNome = contexto.Empresas
                    .Where(e => e.Id == c.EmpresaId).Select(e => e.Nome).FirstOrDefault()
            })
            .ToListAsync(ct);

        var aoAlcance = cabecalhos.Select(c => c.Id).ToHashSet();

        var cidades = await (
                from vinculo in contexto.CarteiraMunicipios.AsNoTracking()
                    .Where(v => v.DesvinculadoEm == null)
                join municipio in contexto.Municipios on vinculo.MunicipioId equals municipio.Id
                select new
                {
                    vinculo.CarteiraId,
                    municipio.Id,
                    municipio.Nome,
                    municipio.Uf,
                    municipio.CodigoIbge
                })
            .ToListAsync(ct);

        var porCarteira = cidades
            .Where(c => aoAlcance.Contains(c.CarteiraId))
            .GroupBy(c => c.CarteiraId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<MunicipioParaSelecao>)
                [
                    .. g.Select(c => new MunicipioParaSelecao(c.Id, c.Nome, c.Uf, c.CodigoIbge))
                        .OrderBy(m => m.Uf, StringComparer.Ordinal)
                        .ThenBy(m => m.Nome, StringComparer.Ordinal)
                ]);

        return
        [
            .. cabecalhos
                .Select(c => new TerritorioDeCarteira(
                    c.ChavePublica,
                    c.Codigo,
                    c.Nome,
                    c.LinhaDeNegocio ?? string.Empty,
                    c.Responsavel ?? string.Empty,
                    c.EmpresaCodigo ?? string.Empty,
                    c.EmpresaNome ?? string.Empty,
                    porCarteira.TryGetValue(c.Id, out var suas) ? suas : []))
                .OrderByDescending(t => t.Municipios.Count)
                .ThenBy(t => t.CarteiraCodigo, StringComparer.Ordinal)
        ];
    }
}
