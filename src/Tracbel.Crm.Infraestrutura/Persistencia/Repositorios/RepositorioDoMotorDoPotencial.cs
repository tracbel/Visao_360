using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A ENTRADA DO MOTOR DO POTENCIAL (issues 72 e 161) — as regras vigentes numa data, ligadas ao
/// catálogo de culturas, e a área plantada medida de um município.
///
/// <para><b>Este código morava dentro do repositório do mapa.</b> Saiu de lá quando a calculadora
/// precisou do mesmo catálogo: duas leituras do mesmo conceito divergem no dia em que uma mudar, e é
/// exatamente o defeito que o motor acabou de eliminar do lado do cálculo.</para>
///
/// <para><b>O catálogo é pequeno</b> — seis culturas, seis categorias, algumas regras — e a leitura
/// traz tudo e cruza em memória, em vez de espalhar uma consulta por regra.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDoMotorDoPotencial(CrmDbContext contexto) : IRepositorioDoMotorDoPotencial
{
    /// <summary>O código da categoria de uma regra que não declarou categoria de máquina (D-IM-06 em aberto).</summary>
    public const string SemCategoriaCodigo = "SEM-CATEGORIA";

    /// <summary>O rótulo dela na tela.</summary>
    public const string SemCategoriaNome = "Sem categoria declarada";

    /// <inheritdoc />
    public async Task<CatalogoDoMotor> LerCatalogoAsync(DateOnly data, CancellationToken ct)
    {
        // A VIGÊNCIA É ESCOLHIDA AQUI, e por produto: a regra de hoje é a que o mapa aplica, e a de uma
        // data passada é a que valia lá. Uma vigência futura ainda não vale para nenhuma das duas.
        var regras = (await contexto.RegrasDePotencial.AsNoTracking()
                .Where(r => r.RevogadoEm == null && r.VigenteDesde <= data)
                .ToListAsync(ct))
            .GroupBy(r => r.ProdutoCodigoIbge)
            .Select(g => ParametroComVigencia.VigenteEm(g, data)!)
            .OrderBy(r => r.ProdutoCodigoIbge)
            .ToList();

        if (regras.Count == 0)
            return new CatalogoDoMotor([], new Dictionary<int, ChaveNoMotor>());

        var culturas = await contexto.Culturas.AsNoTracking().ToDictionaryAsync(c => c.Id, ct);
        var vinculos = await contexto.ProdutosDaPamNasCulturas.AsNoTracking().ToListAsync(ct);
        var categorias = await contexto.CategoriasDeMaquina.AsNoTracking().ToDictionaryAsync(c => c.Id, ct);

        var grupos = await contexto.GruposDeCompartilhamento.AsNoTracking().Where(g => g.EstaAtivo).ToListAsync(ct);
        var noGrupo = await contexto.CulturasNosGruposDeCompartilhamento.AsNoTracking().ToListAsync(ct);

        var grupoPorId = grupos.ToDictionary(g => g.Id);

        // A MESMA CULTURA EM DOIS GRUPOS DA MESMA CATEGORIA é configuração ambígua — o índice único da
        // issue 160 é por (grupo, cultura), e não impede isso. Vale o menor código, sempre o mesmo.
        var grupoDaCultura = noGrupo
            .Where(v => grupoPorId.ContainsKey(v.GrupoDeCompartilhamentoId))
            .GroupBy(v => (v.CulturaId, grupoPorId[v.GrupoDeCompartilhamentoId].CategoriaDeMaquinaId))
            .ToDictionary(
                g => g.Key,
                g => g.Select(v => grupoPorId[v.GrupoDeCompartilhamentoId].Codigo).Order(StringComparer.Ordinal).First());

        var culturaDoProduto = vinculos.ToLookup(v => v.ProdutoCodigoIbge);
        var produtosDaCultura = vinculos.Where(v => v.EntraNaSomaDaLavoura).ToLookup(v => v.CulturaId);

        var linhas = new List<(RegraDePotencial Regra, RegraNoMotor Motor)>();

        foreach (var regra in regras)
        {
            // A CULTURA VEM DA REGRA, E QUANDO ELA NÃO A TRAZ, DO CATÁLOGO PELO PRODUTO. A coluna
            // CulturaId nasceu na issue 165 e a rota de cadastro ainda não a preenche: derivar do
            // produto é o que impede uma regra registrada hoje pelo Administrador de ficar invisível.
            var culturaId = regra.CulturaId
                            ?? culturaDoProduto[regra.ProdutoCodigoIbge].Select(v => (int?)v.CulturaId).FirstOrDefault();

            var cultura = culturaId is { } id ? culturas.GetValueOrDefault(id) : null;

            // A ÁREA DA CULTURA É A DOS PRODUTOS QUE ENTRAM NA SOMA: o café tem três linhas na
            // classificação 782 e é uma cultura só. Sem vínculo marcado, fica o produto da própria regra.
            IReadOnlyList<int> produtos = [regra.ProdutoCodigoIbge];
            if (cultura is not null)
            {
                var daCultura = produtosDaCultura[cultura.Id].Select(v => v.ProdutoCodigoIbge).Distinct().Order().ToList();
                if (daCultura.Count > 0) produtos = daCultura;
            }

            var categoria = regra.CategoriaDeMaquinaId is { } cat ? categorias.GetValueOrDefault(cat) : null;

            linhas.Add((regra, new RegraNoMotor(
                cultura?.Codigo ?? $"PAM-{regra.ProdutoCodigoIbge}",
                cultura?.Nome ?? regra.ProdutoNome,
                categoria?.Codigo ?? SemCategoriaCodigo,
                categoria?.Nome ?? SemCategoriaNome,
                regra.HectaresPorMaquina,
                regra.AnosDeRenovacao,
                regra.Situacao == SituacaoDaRegraDePotencial.Confirmada,
                produtos,
                cultura is not null && categoria is not null
                    ? grupoDaCultura.GetValueOrDefault((cultura.Id, categoria.Id))
                    : null)));
        }

        // UMA LINHA POR CULTURA E CATEGORIA: duas regras da mesma cultura na mesma categoria contariam a
        // área dela duas vezes. Vale a vigência mais recente, e o empate fica com o menor produto.
        var doMotor = linhas
            .GroupBy(l => (l.Motor.CulturaCodigo, l.Motor.CategoriaCodigo))
            .Select(g => g.OrderByDescending(l => l.Regra.VigenteDesde).ThenBy(l => l.Regra.ProdutoCodigoIbge).First().Motor)
            .OrderBy(m => m.CategoriaCodigo, StringComparer.Ordinal)
            .ThenBy(m => m.CulturaCodigo, StringComparer.Ordinal)
            .ToList();

        // O DE-PARA VALE PARA TODA REGRA, inclusive a que perdeu o desempate: a ficha do produto continua
        // mostrando o número da cultura em que ele entrou, e não um traço mudo.
        var porProduto = linhas.ToDictionary(
            l => l.Regra.ProdutoCodigoIbge,
            l => new ChaveNoMotor(l.Motor.CategoriaCodigo, l.Motor.CulturaCodigo));

        return new CatalogoDoMotor(doMotor, porProduto);
    }

    /// <inheritdoc />
    public async Task<AreasDoMunicipio?> LerAreasDoMunicipioAsync(
        int municipioCodigoIbge, CatalogoDoMotor catalogo, CancellationToken ct)
    {
        var municipio = await contexto.Municipios.AsNoTracking()
            .Where(m => m.CodigoIbge == municipioCodigoIbge)
            .Select(m => new { m.Id, m.Nome })
            .FirstOrDefaultAsync(ct);

        if (municipio is null) return null;

        var vazio = new AreasDoMunicipio(
            municipioCodigoIbge, municipio.Nome, new Dictionary<string, decimal>(), new Dictionary<string, short>());

        var produtos = catalogo.Regras.SelectMany(r => r.ProdutosDaPam).Distinct().ToList();
        if (produtos.Count == 0) return vazio;

        // CADA CULTURA NO SEU ANO (issue 152): o último em que a área plantada DELA foi divulgada. O
        // maior ano da tabela inteira faria a cultura que ainda não chegou aparecer "sem dado".
        var anoDoProduto = (await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                .Where(p => produtos.Contains(p.ProdutoCodigoIbge) && p.AreaPlantadaHectares != null)
                .GroupBy(p => p.ProdutoCodigoIbge)
                .Select(g => new { Produto = g.Key, Ano = g.Max(p => p.Ano) })
                .ToListAsync(ct))
            .ToDictionary(x => x.Produto, x => x.Ano);

        if (anoDoProduto.Count == 0) return vazio;

        var anos = anoDoProduto.Values.Distinct().ToList();

        var linhas = (await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                .Where(p => p.MunicipioId == municipio.Id && produtos.Contains(p.ProdutoCodigoIbge) && anos.Contains(p.Ano))
                .Select(p => new { p.ProdutoCodigoIbge, p.Ano, p.AreaPlantadaHectares })
                .ToListAsync(ct))
            .Where(l => anoDoProduto.GetValueOrDefault(l.ProdutoCodigoIbge) == l.Ano)
            .ToList();

        var porProduto = linhas.ToDictionary(l => l.ProdutoCodigoIbge, l => l);

        var area = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var ano = new Dictionary<string, short>(StringComparer.Ordinal);

        foreach (var regra in catalogo.Regras)
        {
            decimal? soma = null;
            short? anoDaCultura = null;

            foreach (var produto in regra.ProdutosDaPam)
            {
                if (!porProduto.TryGetValue(produto, out var linha)) continue;

                anoDaCultura ??= linha.Ano;
                if (linha.AreaPlantadaHectares is { } plantada) soma = (soma ?? 0m) + plantada;
            }

            // CULTURA SEM NENHUM PRODUTO DIVULGADO NÃO ENTRA NO DICIONÁRIO: ausência de chave é "não se
            // sabe", e zero seria "não se planta". Quem lê precisa poder separar os dois.
            if (soma is { } total) area[regra.CulturaCodigo] = total;
            if (anoDaCultura is { } doAno) ano[regra.CulturaCodigo] = doAno;
        }

        return new AreasDoMunicipio(municipioCodigoIbge, municipio.Nome, area, ano);
    }
}
