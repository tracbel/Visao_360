using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.GruposDoRecorte;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.PotencialNaMontagem;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A MONTAGEM DOS INDICADORES (plano 2 do documento 54): o recorte acumulado por cima da referência — o território, o
/// potencial e a estrutura de cada município. Conta em memória: nenhuma leitura de banco passa por aqui.
/// </summary>
internal static class MontagemDosIndicadores
{
    /// <summary>
    /// A MONTAGEM DOS INDICADORES (plano 2 do documento 54) — os municípios do recorte com cobertura, vendas, potencial,
    /// estrutura e ano anterior, e o que fica fora do mapa. Conta em memória: nenhuma leitura de banco passa por aqui.
    /// </summary>
    /// <param name="consulta">O período e os filtros da tela.</param>
    /// <param name="agoraUtc">O instante da leitura.</param>
    /// <param name="territorio">A área de atuação, os municípios e as lojas.</param>
    /// <param name="potencial">As regras, o catálogo do motor, a PAM e a conta de cada município.</param>
    /// <param name="estruturaDeReferencia">O parque, as propriedades, o rebanho, as usinas e os totais.</param>
    /// <param name="acumulado">O recorte somado por grupo, nas duas janelas.</param>
    /// <param name="cobertura">As carteiras comerciais, os vínculos e os responsáveis.</param>
    /// <param name="faturamento">O faturamento do recorte — daqui sai desde quando há carga.</param>
    /// <param name="art">As vendas do ART; nulas quando o ART não trouxe nada ao alcance.</param>
    /// <param name="parqueConectado">O parque conectado de cada município.</param>
    /// <param name="interacaoMaisRecente">A interação mais recente das carteiras ao alcance.</param>
    /// <param name="enderecos">Os endereços ao alcance.</param>
    /// <param name="enderecosComArea">Deles, os que têm área e cultura.</param>
    /// <returns>Os indicadores, sem a lavoura do recorte, e os municípios da ADR que entraram nele.</returns>
    internal static (IndicadoresTerritoriais Indicadores, IReadOnlyList<int> CodigosDaAdrNoRecorte) Montar(
        ConsultaDeIndicadoresTerritoriais consulta,
        DateTime agoraUtc,
        TerritorioDeReferencia territorio,
        PotencialDeReferencia potencial,
        EstruturaDeReferencia estruturaDeReferencia,
        RecorteAcumulado acumulado,
        CoberturaDoRecorte cobertura,
        FaturamentoDoRecorte faturamento,
        VendasDoArtNoRecorte? art,
        IReadOnlyDictionary<int, ParqueConectadoNoMunicipio> parqueConectado,
        DateTime? interacaoMaisRecente,
        int enderecos,
        int enderecosComArea)
    {
        var area = territorio.Area;
        var acumuladores = acumulado.Acumuladores;
        var anteriores = acumulado.Anteriores;
        var janela = consulta.Janela;
        var janelaAnterior = consulta.JanelaAnterior;
        var responsaveisDasCarteiras = cobertura.Responsaveis;

        var oArtTrouxeVenda = art is not null;
        var categoriaDaLinha = art?.CategoriaDaLinha ?? new Dictionary<string, (string Codigo, string Nome, short Ordem)>(StringComparer.Ordinal);
        var vendasSemAData = art?.VendasSemAData ?? 0;
        var vendaMaisRecente = art?.VendaMaisRecente;
        var primeiroMesDoArt = art?.PrimeiroMes;
        var carregadoAte = art?.CarregadoAte;

        // -----------------------------------------------------------------------------------------
        // Potencial: o LEITOR DE REFERÊNCIA (documento 54) — as regras vigentes hoje, o catálogo do motor, a PAM de cada
        // cultura no seu ano e no anterior, a lavoura inteira e o estado, calculados uma vez por versão do assunto.
        // -----------------------------------------------------------------------------------------
        var regras = potencial.Regras;
        var categoriaDaRegra = potencial.CategoriasDasRegras;
        var catalogoDoMotor = potencial.Catalogo;
        var ano = potencial.AnoDaAreaPlantada;
        var anoDaCultura = potencial.AnoDaCultura;
        var daRegra = potencial.Medidas;
        var culturasNoEstado = potencial.CulturasNoEstado;
        var producao = potencial.Producao;

        // A ESTRUTURA É DADO DE REFERÊNCIA (plano 2 do documento 54): guardada pela versão do assunto, e COPIADA aqui — a
        // vocação abaixo é do recorte desta consulta, e a guardada é de todas as telas.
        var estrutura = new Dictionary<int, EstruturaDoMunicipio>(estruturaDeReferencia.PorMunicipio);

        // A VOCAÇÃO AGRÍCOLA (decidida em 28/09/2026): os tercis da fatia de lavoura entre os municípios da ADR — a ADR
        // inteira, com filtro ou sem, como o porte. Fora da ADR não há vocação: o corte é da área de atuação.
        var vocacoes = VocacaoAgricola.PelosTercis(estrutura
            .Where(e => area.TryGetValue(e.Key, out var daArea) && daArea.PertenceAAdr && e.Value.FatiaDeLavouraPercentual is not null)
            .ToDictionary(e => e.Key, e => e.Value.FatiaDeLavouraPercentual!.Value));
        foreach (var (codigo, vocacao) in vocacoes)
            estrutura[codigo] = estrutura[codigo] with { Vocacao = vocacao };
        var totaisDoEstado = estruturaDeReferencia.TotaisDoEstado;

        // -----------------------------------------------------------------------------------------
        // A montagem: os municípios de SP da área de atuação ou com dado, depois dos filtros.
        // -----------------------------------------------------------------------------------------
        var filtrado = consulta.Regiao is not null || consulta.LojaCodigo is not null;

        var nomeOficial = territorio.NomeOficialEmSp;

        List<ResponsavelPelaCarteira> ResponsaveisDe(Acumulador acumulador) =>
        [
            .. acumulador.VinculosPorResponsavel
                .OrderByDescending(r => r.Value).ThenBy(r => r.Key)
                .Select(r => responsaveisDasCarteiras.TryGetValue(r.Key, out var usuario)
                    ? new ResponsavelPelaCarteira(usuario.NomeExibicao, usuario.Natureza.ToString(), r.Value, acumulador.CarteirasPorResponsavel[r.Key].Count)
                    : new ResponsavelPelaCarteira("Responsável fora do alcance desta consulta", "NaoIdentificado", r.Value, acumulador.CarteirasPorResponsavel[r.Key].Count))
        ];

        // -----------------------------------------------------------------------------------------
        // O MOTOR DO POTENCIAL (issue 72). Até aqui o mapa C dividia a área pela regra dentro deste
        // arquivo; agora ele pede o número ao domínio, que já desconta a terra compartilhada entre
        // culturas (issue 160), soma as categorias de máquina sem somar a terra delas, e diz por que um
        // número não saiu em vez de devolver zero.
        // -----------------------------------------------------------------------------------------
        // A CONTA DE CADA MUNICÍPIO É DO POTENCIAL DE REFERÊNCIA (documento 54): feita uma vez por versão e guardada —
        // a área da cultura, as categorias somadas sem somar a terra delas, e a demanda por categoria e cultura.
        var categoriasDoMotor = potencial.Categorias;
        List<ResultadoDaCategoria> MotorDoMunicipio(int municipio, bool doAnoAnterior = false) =>
            [.. potencial.DoMunicipio(municipio, doAnoAnterior).PorCategoria];
        List<DemandaNoMunicipio> DemandaDoMunicipio(int municipio, bool doAnoAnterior = false) =>
            [.. potencial.DoMunicipio(municipio, doAnoAnterior).Demanda];

        var porCategoriaNoRecorte = categoriasDoMotor.ToDictionary(
            c => c.Codigo, _ => new List<PotencialDoRecorte>(), StringComparer.Ordinal);

        var totaisDosMunicipios = new List<PotencialDoRecorte>();
        var codigosNoRecorte = new List<int>();
        var demandasDaAdr = new List<decimal>();

        var itens = new List<IndicadoresDoMunicipio>();

        foreach (var codigo in area.Keys.Concat(acumuladores.Keys.Where(k => k > 0)).Distinct().Order())
        {
            var linhaDaArea = area.GetValueOrDefault(codigo);
            var daAdr = linhaDaArea?.PertenceAAdr == true && categoriasDoMotor.Count > 0;

            if (filtrado
                && (linhaDaArea is null
                    || (consulta.Regiao is not null && linhaDaArea.Regiao != consulta.Regiao)
                    || (consulta.LojaCodigo is not null && linhaDaArea.LojaCodigo != consulta.LojaCodigo)))
            {
                // O PORTE É CORTADO NA ADR INTEIRA (issue 166): o município de fora do recorte não entra na tela, mas
                // entra nos tercis. O motor é conta em memória — nenhuma leitura a mais.
                if (daAdr && potencial.DoMunicipio(codigo).Sobreposto.DemandaAnual is { } fora)
                    demandasDaAdr.Add(fora);
                continue;
            }

            codigosNoRecorte.Add(codigo);

            var doMotor = MotorDoMunicipio(codigo);
            var noMunicipio = potencial.DoMunicipio(codigo).Sobreposto;
            if (daAdr && noMunicipio.DemandaAnual is { } demanda) demandasDaAdr.Add(demanda);
            var parcelaDe = doMotor
                .SelectMany(c => c.Resultado.Parcelas.Select(p => (Chave: new ChaveNoMotor(c.Codigo, p.CulturaCodigo), Parcela: p)))
                .ToDictionary(x => x.Chave, x => x.Parcela);

            foreach (var (categoria, resultado) in doMotor)
                porCategoriaNoRecorte[categoria].Add(resultado);

            if (categoriasDoMotor.Count > 0) totaisDosMunicipios.Add(noMunicipio);

            var acumulador = acumuladores.GetValueOrDefault(codigo) ?? new Acumulador();
            itens.Add(new IndicadoresDoMunicipio(
                codigo,
                linhaDaArea?.Nome ?? nomeOficial.GetValueOrDefault(codigo, codigo.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                linhaDaArea?.PertenceAAdr ?? false,
                linhaDaArea is not null,
                (linhaDaArea?.Regiao ?? RegiaoDaAreaDeAtuacao.NaoInformada).ToString(),
                linhaDaArea?.LojaCodigo,
                linhaDaArea?.LojaNome,
                linhaDaArea?.LojaAtiva,
                acumulador.Cobertura(),
                acumulador.Vendas(),
                [
                    // UMA LINHA POR PRODUTO, COM AS CATEGORIAS DENTRO (issue 240). A PAM é do produto — área,
                    // colheita, valor — e não se repete por categoria; as máquinas, sim: o trator e a
                    // colheitadeira da soja são duas contas sobre a mesma área, e as duas somam.
                    .. regras.GroupBy(r => r.ProdutoCodigoIbge).Select(doProduto =>
                    {
                        var produto = doProduto.Key;
                        var medidas = daRegra.GetValueOrDefault((codigo, produto));
                        var chaves = catalogoDoMotor.PorProdutoDaRegra.GetValueOrDefault(produto) ?? [];

                        var porCategoria = doProduto
                            .Select(regra =>
                            {
                                var categoria = regra.CategoriaDeMaquinaId is { } id && categoriaDaRegra.TryGetValue(id, out var doCatalogo)
                                    ? doCatalogo
                                    : new CategoriaDaRegra(
                                        RepositorioDoMotorDoPotencial.SemCategoriaCodigo,
                                        RepositorioDoMotorDoPotencial.SemCategoriaNome,
                                        short.MaxValue);

                                // O NÚMERO VEM DO MOTOR, e não de uma divisão feita aqui: é a mesma conta do
                                // total da tela e da calculadora. Sem catálogo que ligue o produto — regra de
                                // um produto fora dele —, fica a divisão da própria regra, que é o que havia antes.
                                var chave = chaves.FirstOrDefault(k => k.CategoriaCodigo == categoria.Codigo);
                                var maquinas = chave is not null && parcelaDe.TryGetValue(chave, out var parcela)
                                    ? parcela.Parque
                                    : regra.MaquinasTeoricas(medidas.AreaPlantadaHectares);

                                return (categoria.Ordem, Linha: new MaquinasTeoricasNaCategoria(
                                    categoria.Codigo, categoria.Nome, regra.HectaresPorMaquina, regra.ModeloDeReferencia, maquinas));
                            })
                            .OrderBy(x => x.Ordem)
                            .ThenBy(x => x.Linha.CategoriaCodigo, StringComparer.Ordinal)
                            .Select(x => x.Linha)
                            .ToList();

                        // A SOMA SAI DOS NÚMEROS INTEIROS, e cada linha só é arredondada para mostrar: somar
                        // os arredondados daria um total que não bate com a conta do motor.
                        decimal? total = porCategoria.Any(c => c.Maquinas is not null)
                            ? porCategoria.Sum(c => c.Maquinas ?? 0m)
                            : null;

                        short? anoDela = anoDaCultura.TryGetValue(produto, out var a) ? a : null;
                        var unidade = anoDela is { } doAno ? UnidadesDaPam.DaQuantidade(produto, doAno) : null;
                        return new PotencialTerritorial(
                            produto,
                            medidas.AreaPlantadaHectares,
                            total is { } t ? decimal.Round(t, 1) : null,
                            medidas.AreaColhidaHectares,
                            medidas.ValorDaProducaoMilReais,
                            anoDela,
                            medidas.QuantidadeProduzida,
                            unidade?.Nome,
                            UnidadesDaPam.Produtividade(medidas.QuantidadeProduzida, medidas.AreaColhidaHectares),
                            unidade?.DaProdutividade,
                            [.. porCategoria.Select(c => c with { Maquinas = c.Maquinas is { } m ? decimal.Round(m, 1) : null })]);
                    })
                ],
                ResponsaveisDe(acumulador),
                producao.GetValueOrDefault(codigo),
                estrutura.GetValueOrDefault(codigo) ?? EstruturaVazia,
                categoriasDoMotor.Count == 0
                    ? null
                    : new PotencialEstruturalDoMunicipio(
                        noMunicipio.Parque,
                        noMunicipio.DemandaAnual,
                        noMunicipio.AreaUtilHectares,
                        noMunicipio.Estimativa,
                        noMunicipio.MotivoSemParque,
                        noMunicipio.MotivoSemDemanda),
                oArtTrouxeVenda ? acumulador.MaquinasVendidas : null,
                DemandaPorCategoriaECultura: DemandaDoMunicipio(codigo),
                ParqueConectado: parqueConectado.GetValueOrDefault(codigo),
                DemandaNoAnoAnterior: consulta.ComDemandaDoAnoAnterior && daAdr ? DemandaDoMunicipio(codigo, doAnoAnterior: true) : null));
        }

        var foraDoMapa = GruposForaDoMapa
            .Where(g => acumuladores.ContainsKey(g.Grupo))
            .Select(g => new IndicadoresForaDoMapa(
                g.Codigo, g.Descricao, acumuladores[g.Grupo].Cobertura(), acumuladores[g.Grupo].Vendas(),
                oArtTrouxeVenda ? acumuladores[g.Grupo].MaquinasVendidas : null))
            .ToList();

        // A CATEGORIA SAI NA ORDEM DE EXIBIÇÃO DO CATÁLOGO, a mesma do potencial por categoria: as duas
        // listas ficam lado a lado na tela, e ordens diferentes fariam o leitor comparar linhas trocadas.
        var categoriaPeloCodigo = categoriaDaLinha.Values
            .DistinctBy(c => c.Codigo, StringComparer.Ordinal)
            .ToDictionary(c => c.Codigo, c => (c.Nome, c.Ordem), StringComparer.Ordinal);

        // O TOTAL DE UNIDADES DO RECORTE É A SOMA DOS MUNICÍPIOS QUE ENTRARAM NELE, e não de todos os
        // grupos: a captura divide este número pela demanda DO RECORTE, e o que está fora do mapa não
        // tem demanda do outro lado da razão. Ele sai à parte, para o leitor saber que existe. A MESMA CONTA
        // vale para o ano anterior, que é o numerador da captura de antes (27/09/2026).
        VendasDeMaquinaDoRecorte UnidadesDoRecorte(IReadOnlyDictionary<int, Acumulador> fonte)
        {
            var doRecorte = codigosNoRecorte.Where(fonte.ContainsKey).Select(c => fonte[c]).ToList();

            return new VendasDeMaquinaDoRecorte(
                CriterioDasVendasDoArt.Criterio.ToString(),
                CriterioDasVendasDoArt.Frase,
                doRecorte.Sum(a => a.MaquinasVendidas),
                [
                    .. doRecorte
                        .SelectMany(a => a.MaquinasPorCategoria)
                        .GroupBy(c => c.Key, StringComparer.Ordinal)
                        .Select(c => new UnidadesNaCategoria(
                            c.Key,
                            categoriaPeloCodigo.TryGetValue(c.Key, out var doCatalogo) ? doCatalogo.Nome : c.Key,
                            c.Sum(u => u.Value)))
                        .OrderBy(c => categoriaPeloCodigo.TryGetValue(c.CategoriaCodigo, out var ordem) ? ordem.Ordem : short.MaxValue)
                        .ThenBy(c => c.CategoriaCodigo, StringComparer.Ordinal)
                ],
                GruposForaDoMapa.Where(g => fonte.ContainsKey(g.Grupo)).Sum(g => fonte[g.Grupo].MaquinasVendidas),
                doRecorte.Sum(a => a.MaquinasSemClassificacao),
                doRecorte.Sum(a => a.MaquinasEmLinhaSemCategoria),
                vendasSemAData,
                vendaMaisRecente,
                carregadoAte);
        }

        var maquinasVendidas = !oArtTrouxeVenda ? null : UnidadesDoRecorte(acumuladores);

        // AS UNIDADES DE UM MUNICÍPIO POR CATEGORIA — a mesma leitura do recorte, para a captura da ficha contar
        // só as categorias que têm demanda ali (27/09/2026).
        List<UnidadesNaCategoria> UnidadesDoMunicipioPorCategoria(Acumulador? acumulador) =>
        [
            .. (acumulador?.MaquinasPorCategoria ?? [])
                .Select(c => new UnidadesNaCategoria(
                    c.Key, categoriaPeloCodigo.TryGetValue(c.Key, out var doCatalogo) ? doCatalogo.Nome : c.Key, c.Value))
                .OrderBy(c => categoriaPeloCodigo.TryGetValue(c.CategoriaCodigo, out var ordem) ? ordem.Ordem : short.MaxValue)
                .ThenBy(c => c.CategoriaCodigo, StringComparer.Ordinal)
        ];

        var codigosDaAdrNoRecorte = codigosNoRecorte.Where(c => area.TryGetValue(c, out var daArea) && daArea.PertenceAAdr).ToList();

        var periodoAnterior = MontarPeriodoAnterior(
            janela, janelaAnterior, faturamento.PrimeiraCompetencia, primeiroMesDoArt, oArtTrouxeVenda,
            codigosDaAdrNoRecorte.ToHashSet(),
            acumulado.Mensal,
            UnidadesDoRecorte(anteriores),
            consulta.MesEmCurso);

        // O ANO ANTERIOR DE CADA MUNICÍPIO, em reais e em unidades — só quando a fonte cobre a janela inteira.
        // Este bloco é à parte da montagem de propósito: ele não mexe no potencial, só acrescenta a base da
        // variação a cada linha.
        itens = [
            .. itens.Select(i => i with
            {
                VendasNoPeriodoAnterior = periodoAnterior.VendasCobertas
                    ? (anteriores.GetValueOrDefault(i.CodigoIbge) ?? new Acumulador()).Vendas()
                    : null,
                MaquinasVendidasNoPeriodoAnterior = oArtTrouxeVenda && periodoAnterior.MaquinasCobertas
                    ? anteriores.GetValueOrDefault(i.CodigoIbge)?.MaquinasVendidas ?? 0
                    : null,
                MaquinasPorCategoria = oArtTrouxeVenda
                    ? UnidadesDoMunicipioPorCategoria(acumuladores.GetValueOrDefault(i.CodigoIbge))
                    : null
            })
        ];

        var indicadores = new IndicadoresTerritoriais(
            consulta.CompetenciaInicial,
            consulta.CompetenciaFinal,
            agoraUtc,
            interacaoMaisRecente,
            ano,
            [.. potencial.RegrasAplicadas],
            itens,
            foraDoMapa,
            enderecos,
            enderecosComArea,
            consulta.Visao.ToString(),
            totaisDoEstado,
            culturasNoEstado,
            categoriasDoMotor.Count == 0
                ? null
                : MontarPotencialDoRecorte(
                    [.. categoriasDoMotor.Select(c => (c.Codigo, c.Nome))],
                    porCategoriaNoRecorte,
                    totaisDosMunicipios,
                    RelevanciaDoRecorte(codigosNoRecorte, producao, totaisDoEstado),
                    RelevanciaPorCultura(codigosNoRecorte, daRegra, culturasNoEstado)),
            estruturaDeReferencia.TotaisDaRegiao,
            MontarProcedencias(agoraUtc, maquinasVendidas, estruturaDeReferencia.Anos),
            MaquinasVendidas: maquinasVendidas,
            PeriodoAnterior: periodoAnterior,
            DemandasDosMunicipiosDaAdr: demandasDaAdr);

        return (indicadores, codigosDaAdrNoRecorte);
    }

    /// <summary>
    /// O MESMO TRECHO DO ANO ANTERIOR, montado — a cobertura das duas fontes e o mês a mês da ADR do recorte.
    ///
    /// <para><b>A série é dos municípios DA ADR do recorte</b>, porque é a soma deles que os cartões mostram: os
    /// vizinhos com cliente ficam no mapa, e não no número da área de atuação.</para>
    /// </summary>
    /// <param name="janela">A janela pedida.</param>
    /// <param name="anterior">O mesmo trecho do ano anterior.</param>
    /// <param name="primeiraDoFaturamento">Desde quando há faturamento ao alcance.</param>
    /// <param name="primeiroMesDoArt">Desde quando o ART traz venda.</param>
    /// <param name="oArtTrouxeVenda">Se há venda do ART ao alcance.</param>
    /// <param name="daAdr">Os municípios da ADR que entraram no recorte.</param>
    /// <param name="mensal">As vendas de cada município em cada mês das duas janelas.</param>
    /// <param name="unidadesAnteriores">As unidades do recorte no ano anterior, já com a quebra.</param>
    /// <param name="mesEmCurso">O último mês pedido, quando ele ainda está em curso — aí não há comparação.</param>
    internal static PeriodoAnterior MontarPeriodoAnterior(
        Dominio.Comum.JanelaDeCompetencia janela,
        Dominio.Comum.JanelaDeCompetencia anterior,
        DateOnly? primeiraDoFaturamento,
        DateOnly? primeiroMesDoArt,
        bool oArtTrouxeVenda,
        IReadOnlySet<int> daAdr,
        IReadOnlyDictionary<(int Grupo, DateOnly Mes), VendasDoMes> mensal,
        VendasDeMaquinaDoRecorte unidadesAnteriores,
        DateOnly? mesEmCurso)
    {
        var cobreVendas = mesEmCurso is null && primeiraDoFaturamento is { } primeira && primeira <= anterior.Inicial;
        var cobreMaquinas = mesEmCurso is null && oArtTrouxeVenda && primeiroMesDoArt is { } primeiro && primeiro <= anterior.Inicial;

        List<VendasNoMes> Serie(Dominio.Comum.JanelaDeCompetencia j, bool comUnidades)
        {
            var meses = new List<VendasNoMes>(j.Meses);
            for (var mes = j.Inicial; mes <= j.Final; mes = mes.AddMonths(1))
            {
                var doMes = daAdr.Select(c => mensal.GetValueOrDefault((c, mes))).Where(v => v is not null).ToList();
                meses.Add(new VendasNoMes(
                    mes,
                    decimal.Round(doMes.Sum(v => v!.ValorLiquido), 2),
                    decimal.Round(doMes.Sum(v => v!.Maquina), 2),
                    decimal.Round(doMes.Sum(v => v!.PosVenda), 2),
                    comUnidades ? doMes.Sum(v => v!.MaquinasVendidas) : null));
            }

            return meses;
        }

        return new PeriodoAnterior(
            anterior.Inicial,
            anterior.Final,
            primeiraDoFaturamento,
            oArtTrouxeVenda ? primeiroMesDoArt : null,
            cobreMaquinas ? unidadesAnteriores : null,
            Serie(janela, oArtTrouxeVenda),
            cobreVendas ? Serie(anterior, cobreMaquinas) : [],
            mesEmCurso);
    }

    /// <summary>
    /// DE ONDE VEIO CADA NÚMERO — a procedência por indicador (issue 167).
    ///
    /// <para><b>A competência sai do dado, e não de uma constante:</b> o ano é o que está carregado
    /// nas tabelas municipais neste banco. Um texto fixo no front diria 2017 para sempre, mesmo depois
    /// do Censo de 2028 entrar.</para>
    ///
    /// <para><b>Fonte não carregada devolve nulo</b>, e a tela não mostra carimbo — em vez de carimbar
    /// uma origem que ninguém leu.</para>
    /// </summary>
    /// <param name="agoraUtc">O instante da leitura.</param>
    /// <param name="maquinas">As vendas em unidades do recorte; nulas quando o ART não trouxe nada.</param>
    /// <param name="anos">O ano carregado de cada fonte — da estrutura de referência (plano 2 do documento 54).</param>
    internal static ProcedenciasDoTerritorio MontarProcedencias(
        DateTime agoraUtc, VendasDeMaquinaDoRecorte? maquinas, AnosDasFontes anos)
    {
        var anoDaLavoura = anos.Lavoura;
        var anoDoCenso = anos.Censo;
        var anoDoRebanho = anos.Rebanho;
        var temUsina = anos.TemUsina;

        return new ProcedenciasDoTerritorio(
            AreaPlantada: anoDaLavoura is null
                ? null
                : new ProcedenciaDoIndicador(
                    "IBGE/SIDRA", "PAM — Produção Agrícola Municipal", "5457", "Área plantada",
                    anoDaLavoura.Value.ToString(), agoraUtc,
                    "O município com produção sigilosa não entra na soma — e ausência não é zero."),

            ValorDaProducao: anoDaLavoura is null
                ? null
                : new ProcedenciaDoIndicador(
                    "IBGE/SIDRA", "PAM — Produção Agrícola Municipal", "5457", "Valor da produção",
                    anoDaLavoura.Value.ToString(), agoraUtc,
                    "É o que o município COLHE, em mil reais — não é venda da Tracbel nem preço de máquina. " +
                    "O café entra uma vez só, pelo Total do IBGE."),

            Tratores: anoDoCenso is null
                ? null
                : new ProcedenciaDoIndicador(
                    "IBGE/SIDRA", "Censo Agropecuário", "6778", "Tratores existentes",
                    anoDoCenso.Value.ToString(), agoraUtc,
                    $"O Censo é de {anoDoCenso} e o próximo sai em 2028: o parque tem essa idade. " +
                    "Valor hachurado é sigilo do IBGE, que não é zero — ele oculta o número quando poucos " +
                    "estabelecimentos o compõem. As faixas de potência não se somam ao Total: o Total é uma " +
                    "categoria ao lado delas."),

            Estabelecimentos: anoDoCenso is null
                ? null
                : new ProcedenciaDoIndicador(
                    "IBGE/SIDRA", "Censo Agropecuário", "6779", "Estabelecimentos agropecuários",
                    anoDoCenso.Value.ToString(), agoraUtc,
                    "Sigilo do IBGE oculta o número onde poucos estabelecimentos o compõem; ausência não é zero."),

            Rebanho: anoDoRebanho is null
                ? null
                : new ProcedenciaDoIndicador(
                    "IBGE/SIDRA", "PPM — Pesquisa da Pecuária Municipal", "3939", "Efetivo dos rebanhos — bovino",
                    anoDoRebanho.Value.ToString(), agoraUtc,
                    "A PPM é anual e anda sozinha: o ano dela não acompanha o do Censo."),

            Usinas: !temUsina
                ? null
                : new ProcedenciaDoIndicador(
                    "ANP", "Autorizações de produção de etanol", null, "Capacidade autorizada (m³/dia)",
                    null, agoraUtc,
                    "A ANP só enxerga usina de ETANOL: ausência aqui não prova ausência de usina — " +
                    "a que só faz açúcar não é autorizada por ela e não aparece."),

            // O CARIMBO DA CAPTURA. Ele existe só quando há leitura a carimbar: sem venda no ART, a
            // procedência é nula e a tela não carimba uma origem que não leu.
            MaquinasVendidas: maquinas is null
                ? null
                : new ProcedenciaDoIndicador(
                    "ART — sistema comercial de vendas", "Vendas de máquina, por chassi", null,
                    "Máquinas vendidas (unidades)",
                    maquinas.VendaMaisRecente is { } ate ? $"até {ate:dd/MM/yyyy}" : null,
                    maquinas.CarregadoAte,
                    $"{maquinas.FraseDoCriterio} O ART não traz financiamento. O que se vê é o que a última carga " +
                    "trouxe — a data da carga, ao lado, diz até quando."));
    }
}
