using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia.Cache;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.CodigosDoIbge;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O POTENCIAL DE REFERÊNCIA lido do banco — as mesmas leituras que a apuração dos indicadores fazia, movidas para cá sem
/// mudar a regra (issue 71: a regra vigente hoje; issue 152: cada cultura no seu ano; issue 240: por produto E categoria).
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
/// <param name="motor">A entrada do motor do potencial — o catálogo das regras ligadas às culturas (issue 161).</param>
public sealed class RepositorioDoPotencialDeReferencia(CrmDbContext contexto, IRepositorioDoMotorDoPotencial motor)
    : IRepositorioDoPotencialDeReferencia
{
    /// <inheritdoc />
    public async Task<PotencialDeReferencia> LerAsync(DateOnly hoje, CancellationToken ct)
    {
        // POR PRODUTO E CATEGORIA (issue 240): a soja tem regra de trator e de colheitadeira que começam no
        // mesmo dia, e agrupar só pelo produto deixava uma delas de fora sem aviso.
        var regras = (await contexto.RegrasDePotencial.AsNoTracking()
                .Where(r => r.RevogadoEm == null && r.VigenteDesde <= hoje)
                .ToListAsync(ct))
            .GroupBy(r => (r.ProdutoCodigoIbge, r.CategoriaDeMaquinaId))
            .Select(g => ParametroComVigencia.VigenteEm(g, hoje)!)
            .OrderBy(r => r.ProdutoCodigoIbge)
            .ThenBy(r => r.CategoriaDeMaquinaId)
            .ToList();

        // A CATEGORIA DE CADA REGRA, pelo nome e na ordem do catálogo — a ficha lista as máquinas do produto
        // categoria por categoria, e a tela cita a regra com a máquina dela.
        var categoriaDaRegra = await contexto.CategoriasDeMaquina.AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => new CategoriaDaRegra(c.Codigo, c.Nome, c.Ordem), ct);

        // O MOTOR (issue 72) lê a regra pela CULTURA do catálogo, e a área de uma cultura é a dos produtos
        // que entram na soma dela — por isso a leitura da PAM abre para eles, e não só para o produto da regra.
        var catalogo = await motor.LerCatalogoAsync(hoje, ct);

        var produtos = regras.Select(r => r.ProdutoCodigoIbge)
            .Concat(catalogo.Regras.SelectMany(r => r.ProdutosDaPam))
            .Distinct().ToList();

        var ano = await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking().MaxAsync(a => (short?)a.Ano, ct);

        // CADA CULTURA NO SEU ANO (issue 152): o último em que a área plantada DELA foi divulgada. O maior ano da
        // tabela inteira misturaria anos em silêncio no dia em que a PAM nova entrasse incompleta: a cultura que
        // ainda não chegou apareceria "sem dado", e não com o ano anterior dela.
        var anoDaCultura = produtos.Count == 0
            ? new Dictionary<int, short>()
            : (await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                    .Where(p => produtos.Contains(p.ProdutoCodigoIbge) && p.AreaPlantadaHectares != null)
                    .GroupBy(p => p.ProdutoCodigoIbge)
                    .Select(g => new { Produto = g.Key, Ano = g.Max(p => p.Ano) })
                    .ToListAsync(ct))
                .ToDictionary(c => c.Produto, c => c.Ano);

        var medidas = new Dictionary<(int Codigo, int Produto), MedidasDaCulturaNoMunicipio>();
        var medidasDoAnoAnterior = new Dictionary<(int Codigo, int Produto), MedidasDaCulturaNoMunicipio>();

        if (anoDaCultura.Count > 0)
        {
            var anosDasCulturas = anoDaCultura.Values.Distinct().ToList();
            var linhas = await (
                    from linha in contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                    join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                    where anosDasCulturas.Contains(linha.Ano) && produtos.Contains(linha.ProdutoCodigoIbge) && municipio.CodigoIbge != null
                    select new
                    {
                        Codigo = municipio.CodigoIbge!.Value,
                        linha.ProdutoCodigoIbge,
                        linha.Ano,
                        linha.AreaPlantadaHectares,
                        linha.AreaColhidaHectares,
                        linha.QuantidadeProduzida,
                        linha.ValorDaProducaoMilReais
                    })
                .ToListAsync(ct);

            foreach (var linha in linhas.Where(l => anoDaCultura.GetValueOrDefault(l.ProdutoCodigoIbge) == l.Ano))
                medidas[(linha.Codigo, linha.ProdutoCodigoIbge)] = new MedidasDaCulturaNoMunicipio(
                    linha.AreaPlantadaHectares, linha.AreaColhidaHectares, linha.QuantidadeProduzida, linha.ValorDaProducaoMilReais);

            // O ANO ANTERIOR DA PAM (a Demanda e Previsão, 02/10/2026): cada cultura no ano antes do dela — a mesma regra
            // do "cada cultura no seu ano", um ano para trás. Sem a linha do ano anterior, a área fica nula, e a demanda
            // daquele ano também: sigilo do IBGE não é lavoura zero. Vem sempre junto: uma leitura a mais por versão.
            var anosAnteriores = anoDaCultura.Values.Select(a => (short)(a - 1)).Distinct().ToList();
            var linhasDoAnoAnterior = await (
                    from linha in contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                    join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                    where anosAnteriores.Contains(linha.Ano) && produtos.Contains(linha.ProdutoCodigoIbge) && municipio.CodigoIbge != null
                    select new { Codigo = municipio.CodigoIbge!.Value, linha.ProdutoCodigoIbge, linha.Ano, linha.AreaPlantadaHectares })
                .ToListAsync(ct);

            foreach (var linha in linhasDoAnoAnterior.Where(l => anoDaCultura.GetValueOrDefault(l.ProdutoCodigoIbge) - 1 == l.Ano))
                medidasDoAnoAnterior[(linha.Codigo, linha.ProdutoCodigoIbge)] =
                    new MedidasDaCulturaNoMunicipio(linha.AreaPlantadaHectares, null, null, null);
        }

        // -----------------------------------------------------------------------------------------
        // A lavoura inteira: a soma de TODAS as culturas do município, no mesmo ano.
        //
        // O CAFÉ ENTRA UMA VEZ SÓ. A classificação 782 traz "Café (em grão) Total" ao lado de Arábica
        // e Canephora, e os três na mesma resposta; somar os três contaria o café duas vezes. Ficam
        // de fora os dois detalhados, e fica o total — o mesmo critério da planilha do comercial.
        //
        // A QUANTIDADE PRODUZIDA NÃO É SOMADA, de propósito: o IBGE publica cada produto na unidade
        // dele (tonelada, e mil frutos no abacaxi e no coco — UnidadesDaPam), e um total disso não teria unidade nenhuma.
        // -----------------------------------------------------------------------------------------
        var producao = new Dictionary<int, ProducaoAgricolaDoMunicipio>();

        if (ano is not null)
        {
            var somas = await (
                    from linha in contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                    join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                    where linha.Ano == ano
                          && municipio.CodigoIbge != null
                          && !ProdutosQueDuplicamNaSoma.Contains(linha.ProdutoCodigoIbge)
                    group linha by municipio.CodigoIbge!.Value into porMunicipio
                    select new
                    {
                        Codigo = porMunicipio.Key,
                        Plantada = porMunicipio.Sum(l => l.AreaPlantadaHectares),
                        Colhida = porMunicipio.Sum(l => l.AreaColhidaHectares),
                        Valor = porMunicipio.Sum(l => l.ValorDaProducaoMilReais),
                        Culturas = porMunicipio.Count(l => l.AreaPlantadaHectares > 0)
                    })
                .ToListAsync(ct);

            foreach (var soma in somas)
                producao[soma.Codigo] = new ProducaoAgricolaDoMunicipio(
                    ano.Value, soma.Plantada, soma.Colhida, soma.Valor, soma.Culturas);
        }

        return new PotencialDeReferencia(
            ano, regras, categoriaDaRegra, catalogo, anoDaCultura, medidas, medidasDoAnoAnterior, producao,
            await LerCulturasNoEstadoAsync(anoDaCultura, ct));
    }

    /// <summary>
    /// AS CULTURAS DAS REGRAS NO TOTAL DE SÃO PAULO, cada uma no ano dela — o termo de comparação da ficha do
    /// município ("produtividade aqui × em SP"). A linha é a publicada pelo IBGE, não a soma dos municípios.
    /// </summary>
    private async Task<List<CulturaNoEstado>> LerCulturasNoEstadoAsync(IReadOnlyDictionary<int, short> anoDaCultura, CancellationToken ct)
    {
        if (anoDaCultura.Count == 0) return [];

        var produtos = anoDaCultura.Keys.ToList();
        var anos = anoDaCultura.Values.Distinct().ToList();

        var linhas = await contexto.ProducoesAgricolasNosEstados.AsNoTracking()
            .Where(p => p.EstadoCodigoIbge == CodigoDeSaoPaulo && produtos.Contains(p.ProdutoCodigoIbge) && anos.Contains(p.Ano))
            .ToListAsync(ct);

        return
        [
            .. linhas
                .Where(l => anoDaCultura[l.ProdutoCodigoIbge] == l.Ano)
                .OrderBy(l => l.ProdutoCodigoIbge)
                .Select(l =>
                {
                    var unidade = UnidadesDaPam.DaQuantidade(l.ProdutoCodigoIbge, l.Ano);
                    return new CulturaNoEstado(
                        l.ProdutoCodigoIbge, l.ProdutoNome, l.Ano, l.AreaPlantadaHectares, l.AreaColhidaHectares,
                        l.QuantidadeProduzida, unidade.Nome, l.ValorDaProducaoMilReais,
                        UnidadesDaPam.Produtividade(l.QuantidadeProduzida, l.AreaColhidaHectares), unidade.DaProdutividade);
                })
        ];
    }
}

/// <summary>O POTENCIAL DE REFERÊNCIA GUARDADO (documento 54 §3.2) — uma conta por versão do assunto e por data das vigências.</summary>
/// <param name="cache">O cache de referência.</param>
/// <param name="fabrica">De onde sai o banco próprio da conta.</param>
public sealed class PotencialDeReferenciaEmCache(CacheDeReferencia cache, IServiceScopeFactory fabrica) : IRepositorioDoPotencialDeReferencia
{
    /// <inheritdoc />
    public Task<PotencialDeReferencia> LerAsync(DateOnly hoje, CancellationToken ct) =>
        cache.ObterAsync(
            AssuntoDeReferencia.Potencial,
            hoje.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            () => CalcularAsync(hoje),
            ct);

    private async Task<PotencialDeReferencia> CalcularAsync(DateOnly hoje)
    {
        using var escopo = fabrica.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        return await new RepositorioDoPotencialDeReferencia(db, new RepositorioDoMotorDoPotencial(db)).LerAsync(hoje, CancellationToken.None);
    }
}
