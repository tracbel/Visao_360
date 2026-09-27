using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Territorio;

/// <summary>
/// A LISTA DE MUNICÍPIOS que o campo de endereço oferece — o que torna a regra exequível.
///
/// <para>O documento 16, seção 3, diz que campo com catálogo não aceita digitação livre. A tela
/// só cumpre a parte dela se tiver de onde puxar a lista, e é isto aqui: o mesmo papel que
/// <c>ListarCatalogos</c> cumpre para origem de lead e motivo de descarte, o município tem numa
/// rota própria porque a lista tem milhares de linhas e não cabe no formato "catálogo inteiro
/// numa resposta só".</para>
///
/// <para><b>Não existe caminho para criar município por aqui</b>, e é decisão: o catálogo é a
/// lista oficial de municípios do Brasil, não um campo que cresce com o que o usuário digitou.
/// Quando o município não aparece na busca, o que falta é carga de catálogo — um chamado, não um
/// item novo criado pela tela. É a diferença entre este catálogo e os de
/// <c>metadado.Catalogo</c>, que trazem <c>permiteItemNovo = true</c>.</para>
/// </summary>
public sealed class ListarMunicipios(IRepositorioTerritorio repositorio, IRelogio relogio)
{
    /// <summary>Executa a busca.</summary>
    /// <param name="pagina">Página pedida.</param>
    /// <param name="tamanho">Linhas por página.</param>
    /// <param name="termo">O começo do nome do município.</param>
    /// <param name="uf">Filtro por estado, duas letras.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PaginaDe<MunicipioParaSelecao>>>> ExecutarAsync(
        int? pagina, int? tamanho, string? termo, string? uf, CancellationToken ct)
    {
        var paginacao = Paginacao.Criar(pagina, tamanho);
        if (!paginacao.EhSucesso)
            return Resultado<ComProcedencia<PaginaDe<MunicipioParaSelecao>>>.FalhaDeValidacao(
                paginacao.Erro!, paginacao.Erros);

        var erros = new ColetorDeErros();

        // A UF É DOMÍNIO FECHADO NO BANCO (CK_Municipio_Uf), e recusar aqui o que o banco
        // recusaria evita a resposta vazia que parece "não existe cidade" quando na verdade o
        // filtro é que era inválido.
        if (!string.IsNullOrWhiteSpace(uf) && !UnidadesFederativas.Contains(uf.Trim().ToUpperInvariant()))
            erros.Registrar(
                "uf", "Não é uma das 27 unidades federativas do Brasil.", uf);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PaginaDe<MunicipioParaSelecao>>>(
                "A consulta tem parâmetros que não valem.");

        var pagi = await repositorio.ListarMunicipiosAsync(
            new ConsultaDeMunicipios(paginacao.Valor, termo, uf), ct);

        return Resultado<ComProcedencia<PaginaDe<MunicipioParaSelecao>>>.Ok(
            ComProcedencia<PaginaDe<MunicipioParaSelecao>>.DoNossoBanco(
                pagi, "organizacao.Municipio", relogio));
    }

    /// <summary>As 27 unidades federativas — a mesma lista que <c>CK_Municipio_Uf</c> enumera.</summary>
    private static readonly HashSet<string> UnidadesFederativas = new(StringComparer.Ordinal)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
        "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };
}

/// <summary>
/// A COBERTURA AGRUPADA POR FILIAL — o primeiro nível do agrupamento que existe de verdade.
///
/// <para><b>Não há regional aqui, e é o ponto.</b> A tela de Cobertura hoje mostra sete regionais
/// (MT Norte, GO, BA Oeste) que vieram do protótipo e não têm lastro: a tabela de regional do
/// sistema de origem existe e está vazia. O agrupamento real, preenchido, é filial e carteira —
/// e é o que esta rota entrega.</para>
///
/// <para>A métrica ausente é declarada, como manda o padrão desta camada: quando nenhuma carteira
/// da filial declara cidade, a filial aparece com zero municípios e a resposta diz por quê, em
/// vez de sugerir que a filial não atende ninguém.</para>
/// </summary>
public sealed class ObterCoberturaPorFilial(IRepositorioTerritorio repositorio, IRelogio relogio)
{
    /// <summary>Executa o agregado.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<Agregado<CoberturaDeFilial>>>> ExecutarAsync(
        CancellationToken ct)
    {
        var filiais = await repositorio.ResumirCoberturaPorFilialAsync(ct);

        var ausentes = new List<MetricaSemDado>();

        var carteiras = filiais.Sum(f => f.Carteiras);
        var comMunicipio = filiais.Sum(f => f.CarteirasComMunicipio);

        if (filiais.Count == 0)
            ausentes.Add(new MetricaSemDado(
                "coberturaPorFilial",
                "Não há filial com carteira ao alcance deste contexto de acesso."));
        else if (comMunicipio == 0)
            ausentes.Add(new MetricaSemDado(
                "municipiosPorFilial",
                $"Nenhuma das {carteiras} carteiras ao seu alcance declara município. O vínculo " +
                "carteira × município vem da carga do sistema de origem — sem ela, a cobertura " +
                "territorial não tem o que mostrar."));
        else if (comMunicipio < carteiras)
            ausentes.Add(new MetricaSemDado(
                "municipiosPorFilial",
                $"{carteiras - comMunicipio} das {carteiras} carteiras ao seu alcance não " +
                "declaram nenhum município no sistema de origem. A cobertura territorial abaixo " +
                "é a das que declaram, e não a da operação inteira."));

        return Resultado<ComProcedencia<Agregado<CoberturaDeFilial>>>.Ok(
            ComProcedencia<Agregado<CoberturaDeFilial>>.DoNossoBanco(
                new Agregado<CoberturaDeFilial>(filiais, ausentes),
                "organizacao.CarteiraMunicipio", relogio));
    }
}

/// <summary>
/// COMO LER UM INDICADOR — o que é medido, o que é regra comercial provisória e o que é estimativa.
/// A tela mostra o <paramref name="Selo"/> junto do indicador, e o motivo ao lado.
/// </summary>
/// <param name="Indicador">vendas, coberturaDeVisita, posVenda, potencial ou cenDoMunicipio.</param>
/// <param name="Situacao">Medido, RegraComercialProvisoria, Estimativa ou FonteAConfirmar.</param>
/// <param name="Selo">O texto curto do selo.</param>
/// <param name="Motivo">Por quê, com a decisão que falta.</param>
public sealed record ClassificacaoDeIndicador(string Indicador, string Situacao, string Selo, string Motivo);

/// <summary>O painel geográfico, junto da lista do que ele não consegue responder.</summary>
/// <param name="Indicadores">Os indicadores por município e os totais fora do mapa.</param>
/// <param name="MetricasSemDado">O que a tela pede e o dado não sustenta, com o número que o prova.</param>
/// <param name="PodeVerEmpresaInteira">
/// Se o perfil de quem pediu tem a permissão de alcance entre filiais em profundidade Organização —
/// é o que a tela usa para ligar ou desligar a visão da empresa inteira.
/// </param>
/// <param name="Classificacoes">Como ler cada indicador — nenhum de regra pendente aparece como validado.</param>
/// <param name="NumerosDeDecisao">Os quatro números do topo, com o motivo de cada ausência.</param>
/// <param name="ComparacaoComOAnoAnterior">
/// Os quatro números no mesmo trecho do ano anterior — a base do "vs. ano anterior" dos cartões do topo
/// (27/09/2026). Só a captura e a oportunidade têm número: a demanda e o mercado anual são estruturais.
/// </param>
public sealed record PainelTerritorial(
    IndicadoresTerritoriais Indicadores,
    IReadOnlyList<MetricaSemDado> MetricasSemDado,
    bool PodeVerEmpresaInteira,
    IReadOnlyList<ClassificacaoDeIndicador> Classificacoes,
    NumerosDeDecisao NumerosDeDecisao,
    ComparacaoComOAnoAnterior? ComparacaoComOAnoAnterior = null);

/// <summary>
/// OS FILTROS DE ALCANCE JÁ LIDOS — visão, filiais, tipo de produto e CEN —, os mesmos para o painel e para o
/// histórico do município.
///
/// <para><b>Uma leitura só para as duas rotas</b>: a ficha do município mostra o histórico ao lado dos números
/// do painel, e um filtro validado de um jeito numa e de outro jeito na outra faria os dois discordarem sobre o
/// mesmo recorte.</para>
/// </summary>
/// <param name="Visao">Filial ou empresa inteira.</param>
/// <param name="FilialDaVendaId">A filial que emitiu a nota.</param>
/// <param name="FilialDoClienteId">A filial de cadastro do cliente.</param>
/// <param name="CategoriaDeMaquina">O código da categoria do filtro "Tipo de produto".</param>
/// <param name="ResponsavelId">O responsável do filtro "CEN / gestor".</param>
/// <param name="Categorias">As categorias que o filtro oferece.</param>
/// <param name="Responsaveis">Os responsáveis que o filtro oferece.</param>
public sealed record FiltrosDeAlcance(
    VisaoTerritorial Visao,
    int? FilialDaVendaId,
    int? FilialDoClienteId,
    string? CategoriaDeMaquina,
    long? ResponsavelId,
    IReadOnlyList<CategoriaParaFiltro> Categorias,
    IReadOnlyList<ResponsavelDeCarteira> Responsaveis)
{
    /// <summary>
    /// Lê e confere os filtros. Parâmetro que não vale volta em <c>Erros</c> (422); pedido fora do alcance, em
    /// <c>SemPermissao</c> (403) — e, como antes, os erros de forma vêm antes da permissão.
    /// </summary>
    public static async Task<(FiltrosDeAlcance? Filtros, ColetorDeErros Erros, string? SemPermissao)> LerAsync(
        IRepositorioIndicadoresTerritoriais repositorio,
        ContextoAcesso contextoDeAcesso,
        string? visao,
        string? filialDaVenda,
        string? filialDoCliente,
        string? categoriaDeMaquina,
        string? responsavel,
        CancellationToken ct)
    {
        var erros = new ColetorDeErros();

        var visaoEscolhida = VisaoTerritorial.Filial;
        if (!string.IsNullOrWhiteSpace(visao)
            && !(Enum.TryParse(visao.Trim(), ignoreCase: true, out visaoEscolhida) && Enum.IsDefined(visaoEscolhida)))
            erros.Registrar("visao", "A visão é Filial ou Empresa.", visao);

        int? filialDaVendaId = null, filialDoClienteId = null;
        if (!string.IsNullOrWhiteSpace(filialDaVenda) || !string.IsNullOrWhiteSpace(filialDoCliente))
        {
            var filiais = await repositorio.ListarFiliaisAsync(ct);
            filialDaVendaId = ResolverFilial(filialDaVenda, "filialDaVenda", filiais, erros);
            filialDoClienteId = ResolverFilial(filialDoCliente, "filialDoCliente", filiais, erros);
        }

        // O TIPO DE PRODUTO É A CATEGORIA DO CATÁLOGO, pelo de-para da linha de produto: a lista vem do banco, e
        // o código que não está nela é recusado em vez de filtrar para uma categoria que ninguém ligou a nada.
        var categorias = await repositorio.ListarCategoriasDeMaquinaAsync(ct);
        string? categoria = null;
        if (!string.IsNullOrWhiteSpace(categoriaDeMaquina))
        {
            categoria = categorias.FirstOrDefault(c => string.Equals(c.Codigo, categoriaDeMaquina.Trim(), StringComparison.OrdinalIgnoreCase))?.Codigo;
            if (categoria is null)
                erros.Registrar("categoriaDeMaquina",
                    "Não há categoria de máquina com este código ligada a uma linha de produto.", categoriaDeMaquina);
        }

        // O CEN É O RESPONSÁVEL DE UMA CARTEIRA COMERCIAL AO ALCANCE — e só esse: pedir o de uma carteira que o
        // contexto não enxerga seria pedir para ver o que não é seu. NA VISÃO DA EMPRESA o alcance é o da
        // empresa inteira, e a lista também (revisão de 27/09/2026) — só para quem tem a permissão; sem ela, a
        // lista fica na filial do cabeçalho, e a recusa (403) vem logo abaixo.
        var responsaveis = await repositorio.ListarResponsaveisDasCarteirasAsync(
            visaoEscolhida == VisaoTerritorial.Empresa && contextoDeAcesso.PodeAlcancarTodasAsEmpresas, ct);
        long? responsavelId = null;
        if (!string.IsNullOrWhiteSpace(responsavel))
        {
            if (long.TryParse(responsavel.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var lido)
                && responsaveis.Any(r => r.Id == lido))
                responsavelId = lido;
            else
                erros.Registrar("responsavel",
                    "Não há carteira comercial deste responsável ao alcance do seu contexto de acesso.", responsavel);
        }

        if (erros.TemErro) return (null, erros, null);

        // A PERMISSÃO REAL DECIDE, e não a tela. A empresa inteira exige a permissão de alcance
        // entre filiais em profundidade Organização; na visão da filial, pedir uma filial que o
        // contexto não alcança seria pedir para ver o que não é seu.
        if (visaoEscolhida == VisaoTerritorial.Empresa && !contextoDeAcesso.PodeAlcancarTodasAsEmpresas)
            return (null, erros,
                $"A visão da empresa inteira exige a permissão {ContextoAcesso.PermissaoDeAlcanceEntreEmpresas} " +
                "em profundidade Organização, e o seu perfil não a tem (documento 32, P-10).");

        if (visaoEscolhida == VisaoTerritorial.Filial
            && new[] { (Id: filialDaVendaId, Codigo: filialDaVenda), (Id: filialDoClienteId, Codigo: filialDoCliente) }
                .FirstOrDefault(f => f.Id is { } id && !contextoDeAcesso.EmpresasVisiveis.Contains(id)) is { Id: not null } foraDoAlcance)
            return (null, erros,
                $"A filial {foraDoAlcance.Codigo!.Trim()} não está ao alcance do seu contexto de acesso. " +
                "Troque a filial no cabeçalho, ou use a visão da empresa se o seu perfil permitir.");

        return (new FiltrosDeAlcance(
            visaoEscolhida, filialDaVendaId, filialDoClienteId, categoria, responsavelId, categorias, responsaveis), erros, null);
    }

    private static int? ResolverFilial(
        string? codigo, string campo, IReadOnlyDictionary<string, int> filiais, ColetorDeErros erros)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        if (filiais.TryGetValue(codigo.Trim(), out var id)) return id;

        erros.Registrar(campo, "Não há filial com este código. Consulte /api/v1/catalogos/EMPRESA.", codigo);
        return null;
    }
}

/// <summary>
/// OS TRÊS MAPAS DA ADR — cobertura de visita, vendas e potencial por área (documento 32, seção 8).
///
/// <para><b>O período padrão é o ANO FISCAL ATÉ O ÚLTIMO MÊS FECHADO</b> (FYTD) — decisão do Ricardo de
/// 27/09/2026, que encerrou a dúvida entre "FYTD" e "últimos 12 meses" que a maquete deixava aberta. O ano
/// fiscal vai de novembro a outubro (<see cref="AnoFiscal"/>). O mês em curso continua de fora: no dia 13 ele
/// tem 13 dias de nota, e um mapa com o mês parcial faria toda cidade parecer ter vendido menos. Os doze
/// meses fechados e o ano civil seguem no filtro de período, como escolha.</para>
///
/// <para><b>Os filtros que não têm dado não são aceitos em silêncio.</b> Tipo de cliente e modelo não
/// chegam aqui como parâmetro: a resposta os lista em <c>metricasSemDado</c>, com o motivo, e a tela os
/// mostra desligados. O TIPO DE PRODUTO chega (27/09/2026), e só para o que tem o dado — as unidades do ART,
/// pela categoria do de-para da linha de produto; os reais continuam sem o item da nota. O CEN chega como o
/// responsável da carteira comercial.</para>
///
/// <para><b>Cada resposta traz o mesmo trecho do ano anterior</b> (<see cref="PeriodoAnterior"/>), e os
/// quatro números de decisão trazem o número de antes, ou por que ele não existe.</para>
/// </summary>
public sealed class ObterIndicadoresTerritoriais(
    IRepositorioIndicadoresTerritoriais repositorio,
    IRelogio relogio,
    IProvedorContextoAcesso acesso,
    IRepositorioDeIndicadoresDeMercado indicadoresDeMercado,
    IRepositorioDeParametrosDoPotencial parametros)
{
    /// <summary>
    /// O MOMENTO DO MERCADO DO RECORTE — o fator POR CULTURA e o agregado (fases T3 e T3.1).
    ///
    /// <para><b>O fator é de cada cultura</b> (issue 74): a cana pode estar retraída enquanto o café
    /// está aquecido, porque preço e rentabilidade são de cada uma. Crédito e percepção são do
    /// recorte e entram iguais em todas.</para>
    ///
    /// <para><b>O agregado é a razão entre o que o motor já calcula</b> — demanda ajustada total
    /// sobre demanda estrutural total. Não é um "índice médio de commodity": nenhuma fórmula nova
    /// entrou, e cada cultura pesa exatamente pela demanda que representa.</para>
    ///
    /// <para><b>A versão anterior usava o índice da cultura de maior área</b>, e foi recusada
    /// (23/09/2026): era solução técnica sem decisão de negócio, e conflitava com a issue 74. A
    /// predominante continua, como <b>contexto</b>, com o critério dito.</para>
    ///
    /// <para><b>Sem município escolhido não há crédito nem percepção</b>, e o fator sai só com o
    /// preço: indicador ausente vale desvio zero, e não fator indeterminado.</para>
    /// </summary>
    /// <summary>
    /// OS QUATRO NÚMEROS DE DECISÃO (documento 50, §4.1), montados aqui e não no front.
    ///
    /// <para><b>Nenhum número muda com isto.</b> Os três que ainda não têm dado saem exatamente como
    /// saíam — vazios —, mas o MOTIVO passa a vir da API. Ele era constante de TypeScript repetida em
    /// dezesseis lugares de três arquivos, e no dia em que a issue 69 trouxer as unidades alguém teria de
    /// achar os dezesseis.</para>
    ///
    /// <para><b>A composição por categoria nasce vazia de propósito</b>: o preço de referência é a issue
    /// 70 e não existe no CRM. Passar uma lista vazia é o que faz o mercado anual sair com
    /// <c>SemPrecoDeMaquina</c> em vez de com um total inventado — e é a mesma lista que, quando a 70
    /// chegar, passa a trazer demanda e preço de cada categoria.</para>
    /// </summary>
    private static (NumerosDeDecisao Numeros, ComparacaoComOAnoAnterior Comparacao) NumerosDeDecisaoDo(
        IndicadoresTerritoriais indicadores, string? categoriaFiltrada, int mesesDoPeriodo)
    {
        // A DEMANDA DO PERÍODO (decisão do Ricardo de 27/09/2026): a captura e a oportunidade dividem as vendas do
        // período pela demanda anual × meses/12. Com o ano fiscal inteiro, o número é o da planilha.
        // A CAPTURA COMPARA MÁQUINA COM MÁQUINA DA MESMA CATEGORIA (D-P01, 27/09/2026). A demanda é das
        // categorias que têm regra de potencial — hoje, só trator —, e o numerador fica nelas: a
        // colheitadeira e a colhedora de cana que o ART também traz não têm demanda do outro lado da conta.
        //
        // AS VENDAS EM UNIDADES VÊM DO ART (D-P08, decidida em 24/09/2026), e a base continua NULA quando
        // ele não trouxe venda nenhuma. Zero aqui afirmaria que a Tracbel não vendeu máquina no recorte;
        // nulo diz que ninguém contou ainda.
        //
        // COM O FILTRO "TIPO DE PRODUTO", OS DOIS LADOS FICAM NA CATEGORIA ESCOLHIDA: as unidades já vêm só
        // dela, e a demanda passa a ser a dela — dividir os implementos vendidos pela demanda de trator daria
        // um número que não é captura de nada.
        var comDemanda = indicadores.PotencialDoRecorte?.PorCategoria
            .Where(c => c.DemandaAnualDeMaquinas is not null
                        && (categoriaFiltrada is null || string.Equals(c.CategoriaCodigo, categoriaFiltrada, StringComparison.Ordinal)))
            .Select(c => (c.CategoriaCodigo, c.CategoriaNome))
            .ToList() ?? [];

        BaseDaCaptura? Base(VendasDeMaquinaDoRecorte? unidades) => BaseDaCaptura.Montar(
            unidades?.Unidades,
            unidades?.PorCategoria.Select(c => (c.CategoriaCodigo, c.Unidades)) ?? [],
            comDemanda,
            mesesDoPeriodo);

        var (demanda, ajustada) = categoriaFiltrada is null
            ? (indicadores.PotencialDoRecorte?.DemandaAnualDeMaquinas, indicadores.Momento?.DemandaAjustadaTotal)
            : DemandaDaCategoria(indicadores, categoriaFiltrada);

        var baseDaCaptura = Base(indicadores.MaquinasVendidas);

        var numeros = DecisaoDoMercado.Calcular(
            demanda,
            ajustada,
            vendasEmUnidades: baseDaCaptura?.Unidades,
            porCategoria: [],
            mesesDoPeriodo) with { BaseDaCaptura = baseDaCaptura };

        // O MESMO TRECHO DO ANO ANTERIOR (27/09/2026): a MESMA base — as mesmas categorias com demanda, contra a
        // mesma demanda. O que muda entre os dois lados é só a venda.
        var anterior = indicadores.PeriodoAnterior;
        var comparacao = DecisaoDoMercado.CompararComOAnoAnterior(
            numeros,
            demanda,
            ajustada,
            anterior?.MaquinasVendidas is { } unidadesAnteriores ? Base(unidadesAnteriores) : null,
            anterior?.MotivoSemMaquinas,
            mesesDoPeriodo);

        return (numeros, comparacao);
    }

    /// <summary>
    /// A DEMANDA DE UMA CATEGORIA SÓ — a estrutural e a ajustada pelo momento — para o filtro "Tipo de produto".
    ///
    /// <para><b>Nenhuma conta nova</b>: a estrutural é a da categoria no potencial do recorte, e a ajustada
    /// aplica a cada cultura dela o fator que o momento já calculou para essa cultura, agregando pela mesma
    /// regra do momento (só entram as culturas com os dois números).</para>
    /// </summary>
    private static (decimal? Estrutural, decimal? Ajustada) DemandaDaCategoria(
        IndicadoresTerritoriais indicadores, string categoria)
    {
        var daCategoria = indicadores.PotencialDoRecorte?.PorCategoria
            .FirstOrDefault(c => string.Equals(c.CategoriaCodigo, categoria, StringComparison.Ordinal));
        if (daCategoria is null) return (null, null);

        var fatores = indicadores.Momento?.PorCultura
                          .GroupBy(c => c.CulturaCodigo, StringComparer.Ordinal)
                          .ToDictionary(g => g.Key, g => g.First().Fator, StringComparer.Ordinal)
                      ?? new Dictionary<string, FatorDoCiclo>(StringComparer.Ordinal);

        var (_, _, ajustada, _) = MomentoAgregado.Agregar(
        [
            .. daCategoria.PorCultura.Select(p =>
            {
                var fator = fatores.GetValueOrDefault(p.CulturaCodigo);
                return new MomentoDaCultura(
                    p.CulturaCodigo, p.Cultura, p.DemandaAnual, p.AreaUtilHectares, null,
                    fator ?? new FatorDoCiclo(null, null, null, null, null, false, 0, false, "SemFator"),
                    p.DemandaAnual is { } d && fator?.Fator is { } f ? d * f : null);
            })
        ]);

        return (daCategoria.DemandaAnualDeMaquinas, ajustada);
    }

    private async Task<MomentoDoRecorte?> MomentoDoMercadoAsync(
        IndicadoresTerritoriais indicadores, DateTime agoraUtc, CancellationToken ct)
    {
        if (indicadores.PotencialDoRecorte is not { } recorte) return null;

        var data = ParametroComVigencia.HojeNoBrasil(agoraUtc);
        var vigente = ParametroComVigencia.VigenteEm(await parametros.ListarGeraisAsync(ct), data);
        var doRecorte = await indicadoresDeMercado.LerAsync(data, null, ct);

        // UM FATOR POR CULTURA, cada um com o preço dela. O crédito e a percepção são do recorte, e
        // por isso entram iguais nas três contas — é a estrutura do modelo, não uma simplificação.
        var porCultura = recorte.PorCultura
            .Select(p =>
            {
                var indice = doRecorte.PrecoPorCultura.TryGetValue(p.CulturaCodigo, out var m) ? m.Indice : null;

                var ajustado = FatorDeCiclo.Ajustar(
                    p.DemandaAnual, indice, doRecorte.Credito?.Indice, doRecorte.PercepcaoDoGestor, vigente,
                    recorte.Estimativa);

                return new MomentoDaCultura(
                    p.CulturaCodigo, p.Cultura, p.DemandaAnual, p.AreaUtilHectares, indice,
                    ajustado.Fator, ajustado.DemandaAjustada);
            })
            .ToList();

        var (fatorAgregado, estrutural, ajustada, motivo) = MomentoAgregado.Agregar(porCultura);

        var porte = vigente?.PorteDe(recorte.DemandaAnualDeMaquinas);
        var faixa = LeituraDoMercado.FaixaDoFator(fatorAgregado, vigente);

        return new MomentoDoRecorte(
            fatorAgregado,
            motivo,
            estrutural,
            ajustada,
            porCultura,
            MomentoAgregado.Predominante(porCultura),
            doRecorte.Credito?.Indice,
            doRecorte.PercepcaoDoGestor,
            porte,
            faixa,
            LeituraDoMercado.Frase(porte, faixa),
            new ProcedenciaDoIndicador(
                "CRM Tracbel",
                "Fator de ciclo de mercado (issue 74), agregado pela demanda",
                null,
                "Demanda ajustada total ÷ demanda estrutural total",
                doRecorte.UltimoMesDePreco is { } mes ? $"preço até {mes:MM/yyyy}" : null,
                agoraUtc,
                "O fator é de CADA CULTURA: preço e rentabilidade são dela, crédito e percepção são do " +
                "recorte. O número do topo é a razão entre a demanda ajustada somada e a estrutural somada — " +
                "não é um índice médio de commodity, e cada cultura pesa pela demanda que representa. Com todas " +
                "neutras, o agregado é 1,00. Indicador ausente vale desvio ZERO. O custo entra dentro da " +
                "parcela de preço e rentabilidade; o termo de troca ficou de fora (D-P05) e precisa da issue 70."));
    }

    /// <summary>O período mais longo aceito: três anos, a janela da curva ABC (documento 27).</summary>
    private const int MesesNoMaximo = 36;

    /// <summary>Executa a apuração.</summary>
    /// <param name="competenciaInicial">O primeiro mês, <c>aaaa-mm</c>. Nulo é o começo do ano fiscal do último mês.</param>
    /// <param name="competenciaFinal">O último mês, <c>aaaa-mm</c>, inclusive. Nulo é o último mês fechado.</param>
    /// <param name="regiao">Norte ou Noroeste. Nulo é a área inteira.</param>
    /// <param name="lojaCodigo">O código da filial responsável. Nulo é todas.</param>
    /// <param name="visao"><c>Filial</c> (padrão) ou <c>Empresa</c>.</param>
    /// <param name="filialDaVenda">Código da filial que emitiu a nota. Nulo é todas ao alcance.</param>
    /// <param name="filialDoCliente">Código da filial de cadastro do cliente. Nulo é todas ao alcance.</param>
    /// <param name="categoriaDeMaquina">O "Tipo de produto": o código da categoria de máquina. Nulo é todas.</param>
    /// <param name="responsavel">O "CEN / gestor": o Id do responsável de carteira comercial. Nulo é todos.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PainelTerritorial>>> ExecutarAsync(
        string? competenciaInicial,
        string? competenciaFinal,
        string? regiao,
        string? lojaCodigo,
        string? visao,
        string? filialDaVenda,
        string? filialDoCliente,
        string? categoriaDeMaquina,
        string? responsavel,
        CancellationToken ct)
    {
        var contextoDeAcesso = acesso.Atual;

        var (filtros, errosDosFiltros, semPermissao) = await FiltrosDeAlcance.LerAsync(
            repositorio, contextoDeAcesso, visao, filialDaVenda, filialDoCliente, categoriaDeMaquina, responsavel, ct);

        if (errosDosFiltros.TemErro)
            return errosDosFiltros.Recusar<ComProcedencia<PainelTerritorial>>("A consulta tem parâmetros que não valem.");
        if (semPermissao is not null)
            return Resultado<ComProcedencia<PainelTerritorial>>.SemPermissao(semPermissao);

        var agora = relogio.Agora;
        // O MÊS DE SÃO PAULO, e não o do UTC: às 22h de 31/10 o UTC já está em novembro, e o ano fiscal viraria
        // três horas antes da hora.
        var mesCorrente = AnoFiscal.MesCorrenteEmSaoPaulo(agora);
        var erros = new ColetorDeErros();

        // SEM PERÍODO NO PEDIDO, O ANO FISCAL ATÉ O ÚLTIMO MÊS FECHADO (decisão de 27/09/2026) — a conta do
        // domínio, e não uma refeita aqui. Com só o último mês, o começo é o do ano fiscal que o contém — a
        // mesma regra, e não doze meses para trás.
        var padrao = AnoFiscal.AteOUltimoMesFechado(mesCorrente);
        var final = LerCompetencia(competenciaFinal, "competenciaFinal", padrao.Final, erros);
        var inicial = LerCompetencia(competenciaInicial, "competenciaInicial", AnoFiscal.InicioDe(final), erros);

        if (final > mesCorrente)
            erros.Registrar("competenciaFinal", "O período não pode terminar depois do mês corrente.", competenciaFinal);

        if (inicial > final)
            erros.Registrar("competenciaInicial", "O primeiro mês vem antes do último.", competenciaInicial);
        else if (((final.Year - inicial.Year) * 12) + final.Month - inicial.Month + 1 > MesesNoMaximo)
            erros.Registrar("competenciaInicial", $"O período vai até {MesesNoMaximo} meses.", competenciaInicial);

        RegiaoDaAreaDeAtuacao? regiaoEscolhida = null;
        if (!string.IsNullOrWhiteSpace(regiao))
        {
            if (Enum.TryParse<RegiaoDaAreaDeAtuacao>(regiao.Trim(), ignoreCase: true, out var lida)
                && lida != RegiaoDaAreaDeAtuacao.NaoInformada)
                regiaoEscolhida = lida;
            else
                erros.Registrar("regiao", "A ADR tem duas regiões: Norte e Noroeste.", regiao);
        }

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PainelTerritorial>>("A consulta tem parâmetros que não valem.");

        var indicadores = await repositorio.ApurarAsync(
            new ConsultaDeIndicadoresTerritoriais(
                inicial,
                final,
                regiaoEscolhida,
                string.IsNullOrWhiteSpace(lojaCodigo) ? null : lojaCodigo.Trim(),
                filtros!.Visao,
                filtros.FilialDaVendaId,
                filtros.FilialDoClienteId,
                filtros.CategoriaDeMaquina,
                filtros.ResponsavelId,
                mesCorrente),
            agora,
            ct);

        // O MOMENTO DO MERCADO DO RECORTE (fase T3): o fator de ciclo e as parcelas que o explicam.
        // Ele é montado aqui, e não no repositório, pelo mesmo motivo da calculadora — o fator é conta
        // de DOMÍNIO sobre índices e vigências, e não uma leitura de banco.
        indicadores = indicadores with
        {
            Momento = await MomentoDoMercadoAsync(indicadores, agora, ct),
            CategoriasDeMaquina = filtros.Categorias,
            ResponsaveisDasCarteiras = filtros.Responsaveis
        };

        var (numeros, comparacao) = NumerosDeDecisaoDo(
            indicadores, filtros.CategoriaDeMaquina, new JanelaDeCompetencia(inicial, final).Meses);

        return Resultado<ComProcedencia<PainelTerritorial>>.Ok(
            ComProcedencia<PainelTerritorial>.DoNossoBanco(
                new PainelTerritorial(
                    indicadores,
                    Lacunas(indicadores, contextoDeAcesso.PodeAlcancarTodasAsEmpresas, final >= mesCorrente, filtros),
                    contextoDeAcesso.PodeAlcancarTodasAsEmpresas,
                    Classificar(indicadores),
                    numeros,
                    comparacao),
                "organizacao.MunicipioDaAreaDeAtuacao · comercial.ClienteCarteira · comercial.FaturamentoDoCliente · organizacao.ProducaoAgricolaNoMunicipio",
                relogio));
    }

    /// <summary>
    /// O que os mapas pedem e o dado não sustenta — cada frase com a medida que a prova.
    /// </summary>
    private static List<MetricaSemDado> Lacunas(
        IndicadoresTerritoriais indicadores, bool podeVerEmpresaInteira, bool periodoParcial, FiltrosDeAlcance filtros)
    {
        var lacunas = new List<MetricaSemDado>();

        // O ANO ANTERIOR QUE A CARGA NÃO COBRE (27/09/2026): a variação fica vazia, e a frase diz desde quando
        // cada fonte foi carregada — R$ e unidades separados, porque começaram em meses diferentes.
        if (indicadores.PeriodoAnterior is { } anterior)
        {
            if (anterior.MotivoSemVendas is { } semVendas)
                lacunas.Add(new MetricaSemDado("anoAnteriorEmReais", semVendas));
            if (indicadores.MaquinasVendidas is not null && anterior.MotivoSemMaquinas is { } semMaquinas)
                lacunas.Add(new MetricaSemDado("anoAnteriorEmUnidades", semMaquinas));
        }

        // O FILTRO "TIPO DE PRODUTO" SÓ ALCANÇA AS UNIDADES: o que ele deixa de fora precisa estar dito.
        if (filtros.CategoriaDeMaquina is { } categoria)
            lacunas.Add(new MetricaSemDado(
                "filtroDeTipoDeProduto",
                $"O filtro de tipo de produto ({filtros.Categorias.First(c => c.Codigo == categoria).Nome}) vale para as máquinas " +
                "vendidas em UNIDADES (ART), para a captura e para a oportunidade — com a demanda da mesma categoria. O " +
                "faturamento em reais não tem o item da nota e não muda com ele, e as máquinas sem linha de produto ou de " +
                "linha ainda sem categoria ficam de fora: não se sabe de que tipo elas são."));

        // O FILTRO "CEN / GESTOR" É O RESPONSÁVEL DA CARTEIRA — e nenhum deles tem gestor cadastrado hoje.
        if (filtros.ResponsavelId is { } responsavel)
            lacunas.Add(new MetricaSemDado(
                "filtroDeCen",
                $"O filtro de CEN mostra só os clientes das carteiras comerciais de {filtros.Responsaveis.First(r => r.Id == responsavel).Nome} " +
                "e a cobertura dos vínculos dessas carteiras. A nota sem cliente no CRM não está em carteira nenhuma e sai " +
                "do recorte. Um cliente pode estar em carteiras de mais de um CEN, e aparece no recorte de cada um."));

        // O MÊS CORRENTE AINDA NÃO ACABOU. Pedir até ele é permitido, e a resposta diz que o último
        // mês é parcial — comparar um mês pela metade com meses cheios erra para baixo sem aviso.
        if (periodoParcial)
            lacunas.Add(new MetricaSemDado(
                "periodoParcial",
                $"O período termina no mês corrente ({indicadores.CompetenciaFinal:MM/yyyy}), que ainda não fechou: as vendas " +
                "desse mês estão incompletas e o total do período fica menor do que será."));

        if (!podeVerEmpresaInteira)
            lacunas.Add(new MetricaSemDado(
                "visaoDaEmpresa",
                $"A visão da empresa inteira existe e exige a permissão {ContextoAcesso.PermissaoDeAlcanceEntreEmpresas} em " +
                "profundidade Organização, concedida explicitamente num conjunto de permissões — o seu perfil não a tem. " +
                "Quem recebe esse acesso ainda não foi decidido; até lá, esta tela mostra a filial do cabeçalho (documento 32, P-10)."));

        if (indicadores.ForaDoMapa.Any(g => g.Grupo == "ClienteDeOutraFilial"))
            lacunas.Add(new MetricaSemDado(
                "clienteDeOutraFilial",
                "Parte das vendas e dos vínculos desta filial é com cliente cadastrado em outra filial. O endereço desses " +
                "clientes pertence ao cadastro da outra filial e aparece somado à parte, sem município; na visão da " +
                "empresa, cada venda vai para o município do cliente (documento 32, seção 8.5)."));

        // A NOTA SEM CLIENTE NO CRM ENTRA NO TOTAL, e a tela precisa dizer o que ela é: só a
        // contraparte sem cadastro é falha acionável; fábrica, grupo e revenda não são cliente final.
        var semCliente = indicadores.ForaDoMapa
            .Where(g => g.Grupo is "ContraparteSemCadastro" or "RepasseDeFabrica" or "EmpresaDoGrupo" or "OutraRevenda")
            .ToList();
        if (semCliente.Count > 0)
            lacunas.Add(new MetricaSemDado(
                "faturamentoSemClienteNoCrm",
                $"R$ {semCliente.Sum(g => g.Vendas.ValorLiquido).ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))} das notas do " +
                "período não têm cliente no CRM — contraparte sem cadastro, fábrica, empresa do grupo ou outra revenda. Sem " +
                "cadastro não há endereço: o valor fica somado à parte, fora dos municípios, e só a contraparte sem cadastro " +
                "é falha de cadastro a corrigir (documento 32, seção 8.5)."));

        if (!indicadores.Municipios.Any(m => m.PertenceAAdr))
            lacunas.Add(new MetricaSemDado(
                "areaDeAtuacao",
                "Nenhum município da ADR ao alcance desta consulta. A área de atuação entra pela carga " +
                "(--somente-territorio); sem ela, o mapa não tem o que destacar."));

        lacunas.Add(new MetricaSemDado(
            "visita",
            "A cobertura conta qualquer interação registrada como contato. Qual tipo de contato " +
            "caracteriza visita, e se a periodicidade é a declarada no CRM ou a de 30/60/90/120 dias " +
            "da maquete, ainda não foi decidido (documento 32, P-2)."));

        lacunas.Add(new MetricaSemDado(
            "potencialDosClientes",
            $"{indicadores.Enderecos - indicadores.EnderecosComArea:N0} de {indicadores.Enderecos:N0} " +
            "endereços de cliente não têm área nem cultura. Sem isso não existe potencial por cliente — " +
            "a base de propriedades está no ART e ainda não foi trazida para o CRM (documento 32, P-11)."));

        lacunas.Add(new MetricaSemDado(
            "potencialDosNaoClientes",
            "Não há cadastro de propriedade de não cliente. A área plantada do IBGE (PAM) serve só para estimar a " +
            "necessidade teórica da REGIÃO: ela não separa cliente de não cliente, não comprova a área de nenhuma " +
            "propriedade e não substitui as regras comerciais de potencial."));

        if (indicadores.AnoDaAreaPlantada is null)
            lacunas.Add(new MetricaSemDado(
                "areaPlantada", "A área plantada do IBGE não foi carregada; o mapa de potencial fica sem dado."));

        foreach (var regra in indicadores.Regras.Where(r => r.Situacao == nameof(SituacaoDaRegraDePotencial.AConfirmar)))
            lacunas.Add(new MetricaSemDado(
                "regraDePotencial",
                $"A regra \"1 {regra.ModeloDeReferencia} a cada {regra.HectaresPorMaquina:0.##} ha de " +
                $"{regra.ProdutoNome}\" foi informada como exemplo e não está confirmada. O resultado é " +
                "necessidade teórica de frota, não venda nem valor (documento 32, P-8)."));

        lacunas.Add(new MetricaSemDado(
            "tipoDeCliente",
            "SAM, KAM e Varejo não existem como classificação de cliente em nenhuma fonte carregada " +
            "(documento 32, seção 3.4)."));

        lacunas.Add(new MetricaSemDado(
            "tipoDeProdutoEModelo",
            "O faturamento EM REAIS é por cliente e mês, sem o item da nota: não há como abrir o valor por tipo de " +
            "produto nem por modelo (documento 32, seção 3.5). As vendas de máquina em UNIDADES, essas sim, têm " +
            "categoria — vêm do ART —, e é nelas que o filtro de tipo de produto age. As duas leituras não se misturam."));

        // AS UNIDADES DO ART (issue 69, D-P08). Três coisas diferentes podem faltar aqui, e cada uma
        // tem a sua frase: a carga inteira, a categoria de uma linha, e a data que põe a venda no período.
        if (indicadores.MaquinasVendidas is null)
            lacunas.Add(new MetricaSemDado(
                "vendasEmUnidades",
                "O ART não trouxe venda de máquina nenhuma ao alcance desta consulta, e é dele que saem as vendas em " +
                "UNIDADES (D-P08, decidida em 24/09/2026): a carga não rodou, ou não trouxe venda para este recorte. " +
                "Enquanto for assim, a captura e a oportunidade ficam vazias. Ausência de carga não é venda zero."));

        if (indicadores.MaquinasVendidas is { } maquinas)
        {
            if (maquinas.UnidadesEmLinhaSemCategoria > 0)
                lacunas.Add(new MetricaSemDado(
                    "categoriaDaLinhaDeProduto",
                    $"{maquinas.UnidadesEmLinhaSemCategoria:N0} máquinas vendidas são de uma linha que ainda não foi ligada a " +
                    "uma categoria — hoje a plataforma de corte, que é acessório de colheitadeira, e não máquina que o " +
                    "produtor compra sozinha: julgamento do comercial. Elas contam no total e somem da leitura POR " +
                    "categoria (documento 48, §5.3)."));

            if (maquinas.VendasSemAData > 0)
                lacunas.Add(new MetricaSemDado(
                    "dataDaVendaDeMaquina",
                    $"{maquinas.VendasSemAData:N0} vendas do ART não têm a data que define o período — com o critério do " +
                    "faturamento (D-P08.1), são vendas fechadas e ainda NÃO FATURADAS. Elas não cabem em período nenhum " +
                    $"e ficam fora da contagem, porque ainda não são máquina vendida. {maquinas.FraseDoCriterio}"));
        }

        lacunas.Add(new MetricaSemDado(
            "devolucoes",
            "As vendas são notas fiscais de saída; devolução e cancelamento não são abatidos " +
            "(documento 32, P-5)."));

        return lacunas;
    }

    /// <summary>
    /// COMO LER CADA NÚMERO (documento 32, seção 2). Pós-venda, visita e potencial dependem de
    /// definição comercial pendente e não podem aparecer como validados; vendas é medido, com os
    /// limites ditos; o CEN tem duas fontes sem vigência.
    /// </summary>
    private static List<ClassificacaoDeIndicador> Classificar(IndicadoresTerritoriais indicadores)
    {
        var regraAConfirmar = indicadores.Regras.Count == 0
                              || indicadores.Regras.Any(r => r.Situacao != nameof(SituacaoDaRegraDePotencial.Confirmada));

        return
        [
            // O PERÍODO DEIXOU DE ESTAR PENDENTE (27/09/2026): o padrão é o ano fiscal até o último mês fechado,
            // e a comparação é com o mesmo trecho do ano fiscal anterior. A frase dizia "FYTD ou 12 meses está
            // pendente" — e continuaria dizendo, se ninguém a trocasse junto com a decisão.
            new("vendas", "Medido", "Medido",
                "Notas de saída do Protheus, conferidas contra consulta independente. O período padrão é o ano fiscal até " +
                "o último mês fechado — novembro a outubro, decidido em 27/09/2026 —, comparado com o mesmo trecho do ano " +
                "fiscal anterior. Devolução e cancelamento não são abatidos (documento 32, P-5)."),
            new("coberturaDeVisita", "RegraComercialProvisoria", "Regra provisória",
                "Visita é qualquer interação registrada, e a periodicidade é a declarada no CRM. O que conta como visita, a " +
                "periodicidade e a unidade (cliente ou vínculo) aguardam decisão do comercial (documento 32, P-2 e P-3)."),
            new("posVenda", "RegraComercialProvisoria", "Composição provisória",
                "Pós-venda é peça mais serviço, pelo grupo do item da nota. O que a diretoria considera pós-venda ainda não foi " +
                "definido (documento 32, seção 5, grupo 11)."),
            new("potencial", "Estimativa", regraAConfirmar ? "Estimativa · regra a confirmar" : "Estimativa",
                "Área plantada do município (IBGE) dividida pelos hectares por máquina da regra. É necessidade teórica da região, " +
                "não área de cliente nem potencial comercial validado (documento 32, P-8)."),
            // PLANILHA É REQUISITO, NÃO FONTE (issue 107): quem atende o município é o que a carteira do CRM diz.
            // O SELO DIZIA "RESPONSÁVEL A CADASTRAR", e deixou de ser verdade com as carteiras do Vórtice (#236):
            // as carteiras comerciais têm dono. O que continua em aberto é a divisão OFICIAL do território — qual
            // CEN responde por qual município —, e é por isso que o selo não diz "medido" (D-P14).
            new("cenDoMunicipio", "FonteAConfirmar", "Da carteira do CRM",
                "Quem atende o município é o responsável de cada carteira comercial com clientes nele — o vendedor dono da " +
                "carteira no CRM, pelas carteiras do Vórtice. É quem tem os clientes, e não a divisão oficial do território: " +
                "qual CEN responde por qual município ainda não foi decidido (D-P14, issue 107).")
        ];
    }

    private static DateOnly LerCompetencia(string? texto, string campo, DateOnly padrao, ColetorDeErros erros)
    {
        if (string.IsNullOrWhiteSpace(texto)) return padrao;

        if (DateOnly.TryParseExact(texto.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mes))
            return mes;

        erros.Registrar(campo, "Use o formato aaaa-mm, por exemplo 2026-08.", texto);
        return padrao;
    }
}

/// <summary>
/// O HISTÓRICO DE UM MUNICÍPIO — a aba Histórico da ficha e a lavoura inteira da Visão geral (27/09/2026).
///
/// <para><b>As vendas por ano fiscal</b>, em reais (Protheus) e em unidades (ART), cada uma só nos anos que a
/// carga dela cobre inteiros, e o ano corrente ao lado do mesmo trecho do anterior. <b>A lavoura por ano da
/// PAM</b>, com todas as culturas (issue 168). <b>A cobertura não tem série</b> — ela é medida no instante da
/// leitura —, e a tela diz isso em vez de desenhar uma.</para>
///
/// <para><b>Os mesmos filtros de alcance do painel</b>, lidos pela mesma regra (<see cref="FiltrosDeAlcance"/>):
/// a ficha fica ao lado da linha da tabela, e os dois não podem discordar sobre o mesmo recorte.</para>
/// </summary>
public sealed class ObterHistoricoDoMunicipio(
    IRepositorioIndicadoresTerritoriais repositorio,
    IRepositorioHistoricoDoMunicipio historicos,
    IRelogio relogio,
    IProvedorContextoAcesso acesso)
{
    /// <summary>Executa a leitura.</summary>
    /// <param name="codigoIbge">O município.</param>
    /// <param name="visao"><c>Filial</c> (padrão) ou <c>Empresa</c>.</param>
    /// <param name="filialDaVenda">Código da filial que emitiu a nota.</param>
    /// <param name="filialDoCliente">Código da filial de cadastro do cliente.</param>
    /// <param name="categoriaDeMaquina">O código da categoria do filtro "Tipo de produto".</param>
    /// <param name="responsavel">O Id do responsável do filtro "CEN / gestor".</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<HistoricoDoMunicipio>>> ExecutarAsync(
        int codigoIbge,
        string? visao,
        string? filialDaVenda,
        string? filialDoCliente,
        string? categoriaDeMaquina,
        string? responsavel,
        CancellationToken ct)
    {
        var (filtros, erros, semPermissao) = await FiltrosDeAlcance.LerAsync(
            repositorio, acesso.Atual, visao, filialDaVenda, filialDoCliente, categoriaDeMaquina, responsavel, ct);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<HistoricoDoMunicipio>>("A consulta tem parâmetros que não valem.");
        if (semPermissao is not null)
            return Resultado<ComProcedencia<HistoricoDoMunicipio>>.SemPermissao(semPermissao);

        // O MÊS EM CURSO FICA DE FORA, como no painel: um ano fiscal com o mês pela metade se leria como queda. A
        // conta é a do domínio, com o mês de São Paulo.
        var ultimoFechado = AnoFiscal.AteOUltimoMesFechado(AnoFiscal.MesCorrenteEmSaoPaulo(relogio.Agora)).Final;

        var historico = await historicos.ApurarHistoricoDoMunicipioAsync(
            new ConsultaDoHistoricoDoMunicipio(
                codigoIbge, ultimoFechado, filtros!.Visao, filtros.FilialDaVendaId, filtros.FilialDoClienteId,
                filtros.CategoriaDeMaquina, filtros.ResponsavelId),
            ct);

        if (historico is null)
            return Resultado<ComProcedencia<HistoricoDoMunicipio>>.NaoEncontrado(
                $"Não há município de São Paulo com o código IBGE {codigoIbge}.");

        return Resultado<ComProcedencia<HistoricoDoMunicipio>>.Ok(
            ComProcedencia<HistoricoDoMunicipio>.DoNossoBanco(
                historico,
                "comercial.FaturamentoDoCliente · frota.VendaDeMaquina · organizacao.ProducaoAgricolaNoMunicipio",
                relogio));
    }
}

/// <summary>
/// O TERRITÓRIO DE CADA CARTEIRA — a carteira, a filial dela e as cidades que ela atende.
///
/// <para>É o segundo nível do mesmo agrupamento, e o que a tela de Cobertura abre quando alguém
/// clica numa filial. A carteira que não declara cidade nenhuma aparece com a lista vazia, de
/// propósito: escondê-la faria a tela mostrar uma operação menor do que ela é.</para>
/// </summary>
public sealed class ListarTerritorioPorCarteira(IRepositorioTerritorio repositorio, IRelogio relogio)
{
    /// <summary>Executa a listagem.</summary>
    /// <param name="empresaCodigo">Filtro pela filial; nulo traz todas as ao alcance.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<Agregado<TerritorioDeCarteira>>>> ExecutarAsync(
        string? empresaCodigo, CancellationToken ct)
    {
        var carteiras = await repositorio.ListarTerritorioPorCarteiraAsync(empresaCodigo, ct);

        if (carteiras.Count == 0 && !string.IsNullOrWhiteSpace(empresaCodigo))
            return Resultado<ComProcedencia<Agregado<TerritorioDeCarteira>>>.NaoEncontrado(
                $"Não há carteira da filial \"{empresaCodigo.Trim()}\" ao seu alcance. " +
                "Consulte /api/v1/cobertura/filiais para ver as filiais que você enxerga.");

        var semCidade = carteiras.Count(c => c.Municipios.Count == 0);

        var ausentes = new List<MetricaSemDado>();

        if (semCidade > 0)
            ausentes.Add(new MetricaSemDado(
                "municipiosDaCarteira",
                $"{semCidade} das {carteiras.Count} carteiras não têm nenhum município cadastrado " +
                "no sistema de origem. Elas aparecem com a lista vazia em vez de sumirem da " +
                "tela: a lacuna é do cadastro, não da operação."));

        return Resultado<ComProcedencia<Agregado<TerritorioDeCarteira>>>.Ok(
            ComProcedencia<Agregado<TerritorioDeCarteira>>.DoNossoBanco(
                new Agregado<TerritorioDeCarteira>(carteiras, ausentes),
                "organizacao.CarteiraMunicipio", relogio));
    }
}
