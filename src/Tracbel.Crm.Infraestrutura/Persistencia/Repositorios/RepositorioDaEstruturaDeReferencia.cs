using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia.Cache;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.CodigosDoIbge;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A ESTRUTURA DE REFERÊNCIA lida do banco — as mesmas leituras que a apuração dos indicadores fazia a cada chamada,
/// movidas para cá sem mudar a regra (issue 65: cada fonte no seu ano; issue 155: o total publicado do estado; issue 163:
/// os totais da Região Tracbel sem os filtros da consulta).
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDaEstruturaDeReferencia(CrmDbContext contexto) : IRepositorioDaEstruturaDeReferencia
{
    /// <inheritdoc />
    public async Task<EstruturaDeReferencia> LerAsync(CancellationToken ct)
    {
        // OS ANOS DE CADA FONTE, lidos uma vez: a estrutura, os totais do estado, os da região e as procedências liam os
        // mesmos máximos, cada um por conta própria.
        var anoDaLavoura = await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking().MaxAsync(a => (short?)a.Ano, ct);
        var anoDoCenso = await contexto.FrotasDeTratoresNosMunicipios.AsNoTracking().MaxAsync(f => (short?)f.Ano, ct);
        var anoDoRebanho = await contexto.RebanhosNosMunicipios.AsNoTracking().MaxAsync(r => (short?)r.Ano, ct);
        var temUsina = await contexto.UsinasDeEtanol.AsNoTracking().AnyAsync(ct);

        // CONGELADO: o mesmo dicionário serve a todas as telas, e a vocação de um recorte não pode entrar nele.
        var porMunicipio = (await LerEstruturaAsync(anoDoCenso, anoDoRebanho, ct)).ToFrozenDictionary();

        return new EstruturaDeReferencia(
            porMunicipio,
            await LerTotaisDoEstadoAsync(anoDaLavoura, anoDoCenso, anoDoRebanho, ct),
            await LerTotaisDaRegiaoTracbelAsync(anoDaLavoura, anoDoCenso, anoDoRebanho, ct),
            new AnosDasFontes(anoDaLavoura, anoDoCenso, anoDoRebanho, temUsina));
    }
    /// <summary>
    /// O QUE JÁ EXISTE EM CADA MUNICÍPIO — parque, propriedades, rebanho, área e usinas (issue 65).
    ///
    /// <para><b>Cada fonte traz o ANO mais recente dela, e os anos não coincidem:</b> o Censo
    /// Agropecuário é de 2017 e a Pesquisa da Pecuária Municipal é anual. Usar um ano só para as duas
    /// esvaziaria a mais recente ou envelheceria a outra — por isso cada bloco descobre o seu.</para>
    ///
    /// <para>Tudo vem indexado pelo <b>código IBGE</b>, que é a chave que o mapa usa; município sem
    /// código não entra, porque não tem polígono para colorir.</para>
    /// </summary>
    private async Task<Dictionary<int, EstruturaDoMunicipio>> LerEstruturaAsync(short? anoDoCenso, short? anoDoRebanho, CancellationToken ct)
    {
        var anoDaArea = await contexto.AreasTerritoriaisDosMunicipios.AsNoTracking().MaxAsync(a => (short?)a.Ano, ct);

        var frota = anoDoCenso is null
            ? []
            : await (from linha in contexto.FrotasDeTratoresNosMunicipios.AsNoTracking()
                     join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                     where linha.Ano == anoDoCenso && municipio.CodigoIbge != null
                     select new
                     {
                         Codigo = municipio.CodigoIbge!.Value,
                         linha.PotenciaCodigoIbge,
                         linha.Tratores,
                         linha.EstabelecimentosComTrator
                     }).ToListAsync(ct);

        var faixas = anoDoCenso is null
            ? []
            : await (from linha in contexto.EstabelecimentosPorAreaNosMunicipios.AsNoTracking()
                     join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                     where linha.Ano == anoDoCenso && municipio.CodigoIbge != null
                     select new { Codigo = municipio.CodigoIbge!.Value, linha.GrupoDeAreaCodigoIbge, linha.Estabelecimentos })
                .ToListAsync(ct);

        var rebanho = anoDoRebanho is null
            ? []
            : await (from linha in contexto.RebanhosNosMunicipios.AsNoTracking()
                     join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                     where linha.Ano == anoDoRebanho && linha.RebanhoCodigoIbge == Bovino && municipio.CodigoIbge != null
                     select new { Codigo = municipio.CodigoIbge!.Value, linha.Cabecas }).ToListAsync(ct);

        // A UTILIZAÇÃO DAS TERRAS (6881, 28/09/2026): o ano é o dela, como o de cada fonte — hoje o mesmo Censo de 2017.
        var anoDaUtilizacao = await contexto.UtilizacoesDasTerrasNosMunicipios.AsNoTracking().MaxAsync(u => (short?)u.Ano, ct);
        var utilizacoes = anoDaUtilizacao is null
            ? []
            : await (from linha in contexto.UtilizacoesDasTerrasNosMunicipios.AsNoTracking()
                     join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                     where linha.Ano == anoDaUtilizacao && municipio.CodigoIbge != null
                     select new
                     {
                         Codigo = municipio.CodigoIbge!.Value,
                         linha.UtilizacaoCodigoIbge,
                         linha.EstabelecimentosComArea,
                         linha.AreaHectares
                     }).ToListAsync(ct);

        var areas = anoDaArea is null
            ? []
            : await (from linha in contexto.AreasTerritoriaisDosMunicipios.AsNoTracking()
                     join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                     where linha.Ano == anoDaArea && municipio.CodigoIbge != null
                     select new { Codigo = municipio.CodigoIbge!.Value, linha.AreaKm2 }).ToListAsync(ct);

        // SÓ AS VIGENTES: a usina que saiu da lista da ANP fica encerrada no banco (issue 153), com a data, mas não é
        // mais usina autorizada — e não entra no mapa nem na contagem.
        var usinas = await (from usina in contexto.UsinasDeEtanol.AsNoTracking()
                            join municipio in contexto.Municipios.AsNoTracking() on usina.MunicipioId equals municipio.Id
                            where municipio.CodigoIbge != null && usina.EncerradaEm == null
                            orderby usina.RazaoSocial
                            select new
                            {
                                Codigo = municipio.CodigoIbge!.Value,
                                usina.RazaoSocial,
                                usina.CapacidadeDeAnidroM3Dia,
                                usina.CapacidadeDeHidratadoM3Dia
                            }).ToListAsync(ct);

        var porCodigo = frota.Select(f => f.Codigo)
            .Concat(faixas.Select(f => f.Codigo))
            .Concat(rebanho.Select(r => r.Codigo))
            .Concat(areas.Select(a => a.Codigo))
            .Concat(usinas.Select(u => u.Codigo))
            .Concat(utilizacoes.Select(u => u.Codigo))
            .Distinct();

        var utilizacoesPorCodigo = utilizacoes.ToLookup(u => u.Codigo);

        var frotaPorCodigo = frota.ToLookup(f => f.Codigo);
        var faixasPorCodigo = faixas.ToLookup(f => f.Codigo);
        var rebanhoPorCodigo = rebanho.ToDictionary(r => r.Codigo, r => r.Cabecas);
        var areaPorCodigo = areas.ToDictionary(a => a.Codigo, a => a.AreaKm2);
        var usinasPorCodigo = usinas.ToLookup(u => u.Codigo);

        var resultado = new Dictionary<int, EstruturaDoMunicipio>();

        foreach (var codigo in porCodigo)
        {
            var linhasDaFrota = frotaPorCodigo[codigo].ToDictionary(f => f.PotenciaCodigoIbge);
            var porGrupo = faixasPorCodigo[codigo].ToDictionary(f => f.GrupoDeAreaCodigoIbge, f => f.Estabelecimentos);
            var linhasDaUtilizacao = utilizacoesPorCodigo[codigo].ToList();
            var total = linhasDaUtilizacao.FirstOrDefault(u => u.UtilizacaoCodigoIbge == UtilizacaoTotal);
            var daUtilizacao = linhasDaUtilizacao.ToDictionary(u => u.UtilizacaoCodigoIbge, u => u.AreaHectares);

            resultado[codigo] = new EstruturaDoMunicipio(
                anoDoCenso,
                linhasDaFrota.GetValueOrDefault(PotenciaTotal)?.Tratores,
                linhasDaFrota.GetValueOrDefault(PotenciaAbaixoDe100Cv)?.Tratores,
                linhasDaFrota.GetValueOrDefault(PotenciaDe100CvEMais)?.Tratores,
                linhasDaFrota.GetValueOrDefault(PotenciaTotal)?.EstabelecimentosComTrator,
                porGrupo.GetValueOrDefault(GrupoDeAreaTotal),
                Reagrupar(porGrupo),
                anoDoRebanho,
                rebanhoPorCodigo.GetValueOrDefault(codigo),
                areaPorCodigo.GetValueOrDefault(codigo),
                [
                    .. usinasPorCodigo[codigo].Select(u => new UsinaDoMunicipio(
                        u.RazaoSocial,
                        u.CapacidadeDeAnidroM3Dia is null && u.CapacidadeDeHidratadoM3Dia is null
                            ? null
                            : (u.CapacidadeDeAnidroM3Dia ?? 0) + (u.CapacidadeDeHidratadoM3Dia ?? 0)))
                ],
                AreaDosEstabelecimentosHectares: total?.AreaHectares,
                EstabelecimentosComArea: total?.EstabelecimentosComArea,
                AreaDeLavouraHectares: AreaDeLavoura(daUtilizacao));
        }

        return resultado;
    }

    /// <summary>
    /// A ÁREA EM LAVOURA — permanente e temporária, e flores quando divulgada. Sem uma das duas primeiras (sigilo), a
    /// soma não sai: ela seria a lavoura de uma parte só com o nome do todo.
    /// </summary>
    private static decimal? AreaDeLavoura(IReadOnlyDictionary<int, decimal?> porUtilizacao)
    {
        if (porUtilizacao.GetValueOrDefault(LavouraPermanente) is not { } permanente
            || porUtilizacao.GetValueOrDefault(LavouraTemporaria) is not { } temporaria)
            return null;

        return permanente + temporaria + (porUtilizacao.GetValueOrDefault(LavouraDeFlores) ?? 0m);
    }

    /// <summary>
    /// As 18 faixas do IBGE nas que o comercial usa.
    ///
    /// <para><b>A conta é feita aqui, e não na carga</b>, porque o reagrupamento é uma escolha de
    /// leitura: o banco guarda as faixas originais, e mudar este mapa não pede recarga nenhuma.</para>
    ///
    /// <para><b>Nulo quando TODAS as faixas de origem vieram sob sigilo</b> — e não zero. Somar
    /// sigiloso como zero diria que não há propriedade daquele tamanho ali.</para>
    /// </summary>
    private static List<FaixaDeArea> Reagrupar(IReadOnlyDictionary<int, int?> porGrupo)
    {
        var faixas = new List<FaixaDeArea>(FaixasDoComercial.Length);

        for (var i = 0; i < FaixasDoComercial.Length; i++)
        {
            var (rotulo, grupos) = FaixasDoComercial[i];
            var presentes = grupos.Select(g => porGrupo.GetValueOrDefault(g)).Where(v => v is not null).ToList();

            faixas.Add(new FaixaDeArea(
                i + 1, rotulo, presentes.Count == 0 ? null : presentes.Sum(v => v!.Value)));
        }

        return faixas;
    }

    /// <summary>
    /// Os totais de São Paulo — o denominador de "que fatia da cultura do estado está na região".
    ///
    /// <para><b>Todos vêm do TOTAL PUBLICADO pelo IBGE</b> (issue 155): a lavoura de
    /// <c>ProducaoAgricolaNoEstado</c>, e o parque, as propriedades e o rebanho de
    /// <c>MedidaDoIbgeNoEstado</c>. O publicado não é a soma dos municípios — o valor municipal
    /// sigiloso entra nele sem aparecer embaixo —, e até a issue 155 três dos quatro eram somados,
    /// o que inflava a fatia da região justamente onde há sigilo.</para>
    ///
    /// <para><b>A soma dos municípios vai junto</b>, para a tela poder dizer quanto o sigilo esconde
    /// em vez de deixar uma diferença sem explicação para quem conferir na mão.</para>
    /// </summary>
    private async Task<TotaisDoEstado?> LerTotaisDoEstadoAsync(short? ano, short? anoDoCenso, short? anoDoRebanho, CancellationToken ct)
    {
        if (ano is null) return null;

        var lavoura = await contexto.ProducoesAgricolasNosEstados.AsNoTracking()
            .Where(p => p.Ano == ano
                        && p.EstadoCodigoIbge == CodigoDeSaoPaulo
                        && !ProdutosQueDuplicamNaSoma.Contains(p.ProdutoCodigoIbge))
            .GroupBy(p => p.EstadoCodigoIbge)
            .Select(g => new
            {
                Plantada = g.Sum(p => p.AreaPlantadaHectares),
                Colhida = g.Sum(p => p.AreaColhidaHectares),
                Valor = g.Sum(p => p.ValorDaProducaoMilReais)
            })
            .FirstOrDefaultAsync(ct);

        if (lavoura is null) return null;


        var tratores = new MedidaDoEstado(
            await PublicadoAsync(TabelaDaFrotaDeTratores, VariavelDeTratores, PotenciaTotal, anoDoCenso, ct),
            anoDoCenso is null
                ? null
                : await contexto.FrotasDeTratoresNosMunicipios.AsNoTracking()
                    .Where(f => f.Ano == anoDoCenso && f.PotenciaCodigoIbge == PotenciaTotal)
                    .SumAsync(f => (int?)f.Tratores, ct));

        var estabelecimentos = new MedidaDoEstado(
            await PublicadoAsync(TabelaDeEstabelecimentos, VariavelDeEstabelecimentos, GrupoDeAreaTotal, anoDoCenso, ct),
            anoDoCenso is null
                ? null
                : await contexto.EstabelecimentosPorAreaNosMunicipios.AsNoTracking()
                    .Where(e => e.Ano == anoDoCenso && e.GrupoDeAreaCodigoIbge == GrupoDeAreaTotal)
                    .SumAsync(e => (int?)e.Estabelecimentos, ct));

        var rebanho = new MedidaDoEstado(
            await PublicadoAsync(TabelaDoRebanho, VariavelDoEfetivoDoRebanho, Bovino, anoDoRebanho, ct),
            anoDoRebanho is null
                ? null
                : await contexto.RebanhosNosMunicipios.AsNoTracking()
                    .Where(r => r.Ano == anoDoRebanho && r.RebanhoCodigoIbge == Bovino)
                    .SumAsync(r => (int?)r.Cabecas, ct));

        return new TotaisDoEstado(
            ano.Value, lavoura.Plantada, lavoura.Valor, tratores, estabelecimentos, anoDoCenso, rebanho, anoDoRebanho,
            lavoura.Colhida);
    }

    /// <summary>
    /// O valor que o IBGE publicou para São Paulo numa célula do SIDRA.
    ///
    /// <para>O ano pedido é o que as tabelas municipais têm carregado: o total publicado de um ano que
    /// não está no mapa não é denominador de nada, e compará-lo com a soma de outro ano diria uma
    /// diferença que não existe.</para>
    /// </summary>
    private async Task<int?> PublicadoAsync(short tabela, short variavel, int categoria, short? ano, CancellationToken ct)
    {
        if (ano is null) return null;

        var valor = await contexto.MedidasDoIbgeNosEstados.AsNoTracking()
            .Where(m => m.EstadoCodigoIbge == CodigoDeSaoPaulo
                        && m.TabelaDoSidra == tabela
                        && m.VariavelDoSidra == variavel
                        && m.CategoriaCodigoIbge == categoria
                        && m.Ano == ano)
            .Select(m => m.Valor)
            .FirstOrDefaultAsync(ct);

        return valor is null ? null : (int)decimal.Round(valor.Value);
    }

    /// <summary>
    /// OS TOTAIS DA REGIÃO TRACBEL — a ADR inteira, sem passar pelos filtros da consulta (issue 163).
    ///
    /// <para><b>Por que uma leitura própria, e não a soma do que a tela já recebeu:</b> este é o
    /// denominador de "que fatia da Região Tracbel este município é?". Se ele fosse a soma do recorte
    /// consultado, escolher a sub-região Norte faria cada município do Norte virar uma fatia maior de
    /// si mesmo — o mesmo município mostraria dois números diferentes conforme o filtro, e a
    /// comparação entre duas telas deixaria de valer.</para>
    ///
    /// <para><b>Região Tracbel não é "região":</b> <c>RegiaoDaAreaDeAtuacao</c> é Norte ou Noroeste,
    /// que são sub-regiões. A hierarquia é São Paulo → Região Tracbel → sub-região → loja → município.</para>
    ///
    /// <para><b>Sigilo não vira zero aqui também:</b> a soma ignora o município oculto em vez de
    /// contá-lo como zero, do mesmo jeito que a soma dos municípios em <see cref="TotaisDoEstado"/>.</para>
    /// </summary>
    private async Task<TotaisDaRegiaoTracbel?> LerTotaisDaRegiaoTracbelAsync(short? anoDaLavoura, short? anoDoCenso, short? anoDoRebanho, CancellationToken ct)
    {
        var daAdr = contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
            .Where(a => a.EncerradoEm == null && a.PertenceAAdr)
            .Select(a => a.MunicipioId);

        var municipios = await daAdr.CountAsync(ct);
        if (municipios == 0) return null;


        var lavoura = anoDaLavoura is null
            ? null
            : await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                .Where(p => p.Ano == anoDaLavoura
                            && daAdr.Contains(p.MunicipioId)
                            && !ProdutosQueDuplicamNaSoma.Contains(p.ProdutoCodigoIbge))
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Plantada = g.Sum(p => p.AreaPlantadaHectares),
                    Colhida = g.Sum(p => p.AreaColhidaHectares),
                    Valor = g.Sum(p => p.ValorDaProducaoMilReais)
                })
                .FirstOrDefaultAsync(ct);

        var tratores = anoDoCenso is null
            ? null
            : await contexto.FrotasDeTratoresNosMunicipios.AsNoTracking()
                .Where(f => f.Ano == anoDoCenso && f.PotenciaCodigoIbge == PotenciaTotal && daAdr.Contains(f.MunicipioId))
                .SumAsync(f => (int?)f.Tratores, ct);

        var estabelecimentos = anoDoCenso is null
            ? null
            : await contexto.EstabelecimentosPorAreaNosMunicipios.AsNoTracking()
                .Where(e => e.Ano == anoDoCenso && e.GrupoDeAreaCodigoIbge == GrupoDeAreaTotal && daAdr.Contains(e.MunicipioId))
                .SumAsync(e => (int?)e.Estabelecimentos, ct);

        var bovinos = anoDoRebanho is null
            ? null
            : await contexto.RebanhosNosMunicipios.AsNoTracking()
                .Where(r => r.Ano == anoDoRebanho && r.RebanhoCodigoIbge == Bovino && daAdr.Contains(r.MunicipioId))
                .SumAsync(r => (int?)r.Cabecas, ct);

        return new TotaisDaRegiaoTracbel(
            anoDaLavoura,
            lavoura?.Plantada,
            lavoura?.Colhida,
            lavoura?.Valor,
            anoDoCenso,
            tratores,
            estabelecimentos,
            anoDoRebanho,
            bovinos,
            municipios);
    }
}

/// <summary>A ESTRUTURA DE REFERÊNCIA GUARDADA (plano 2 do documento 54) — uma conta por versão do assunto.</summary>
/// <param name="cache">O cache de referência.</param>
/// <param name="fabrica">De onde sai o banco próprio da conta.</param>
public sealed class EstruturaDeReferenciaEmCache(CacheDeReferencia cache, IServiceScopeFactory fabrica) : IRepositorioDaEstruturaDeReferencia
{
    /// <inheritdoc />
    public Task<EstruturaDeReferencia> LerAsync(CancellationToken ct) =>
        cache.ObterAsync(AssuntoDeReferencia.Estrutura, "estrutura", CalcularAsync, ct);

    private async Task<EstruturaDeReferencia> CalcularAsync()
    {
        using var escopo = fabrica.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        return await new RepositorioDaEstruturaDeReferencia(db).LerAsync(CancellationToken.None);
    }
}