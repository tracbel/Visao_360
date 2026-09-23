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
public sealed record PainelTerritorial(
    IndicadoresTerritoriais Indicadores,
    IReadOnlyList<MetricaSemDado> MetricasSemDado,
    bool PodeVerEmpresaInteira,
    IReadOnlyList<ClassificacaoDeIndicador> Classificacoes);

/// <summary>
/// OS TRÊS MAPAS DA ADR — cobertura de visita, vendas e potencial por área (documento 32, seção 8).
///
/// <para><b>O período padrão são os doze meses FECHADOS</b> até o mês anterior ao corrente. O mês em
/// curso fica de fora porque, no dia 13, ele tem 13 dias de nota — e um mapa com o mês parcial faria
/// toda cidade parecer ter vendido menos. A maquete dizia "FYTD" e "últimos 12 meses" ao mesmo
/// tempo; a decisão entre os dois está pendente, e o filtro de período deixa escolher.</para>
///
/// <para><b>Os filtros que não têm dado não são aceitos em silêncio.</b> Tipo de cliente, tipo de
/// produto e modelo não chegam aqui como parâmetro: a resposta os lista em <c>metricasSemDado</c>,
/// com o motivo, e a tela os mostra desligados.</para>
/// </summary>
public sealed class ObterIndicadoresTerritoriais(
    IRepositorioIndicadoresTerritoriais repositorio,
    IRelogio relogio,
    IProvedorContextoAcesso acesso,
    IRepositorioDeIndicadoresDeMercado indicadoresDeMercado,
    IRepositorioDeParametrosDoPotencial parametros)
{
    /// <summary>
    /// O MOMENTO DO MERCADO DO RECORTE — o fator de ciclo, suas parcelas e o porte (fase T3).
    ///
    /// <para><b>O índice de preço é o da cultura de maior área, e o resultado diz qual.</b> O momento
    /// de preço é apurado por cultura — o café pode subir enquanto a cana cai — e o fator pede um
    /// número. Uma média ponderada seria fórmula nova que ninguém decidiu; escolher a cultura que
    /// domina a área é uma <b>seleção declarada</b>, que a tela mostra.</para>
    ///
    /// <para><b>Sem município escolhido não há crédito nem percepção</b>, e o fator sai só com o
    /// preço: indicador ausente vale desvio zero (issue 74), e não fator indeterminado.</para>
    /// </summary>
    private async Task<MomentoDoRecorte?> MomentoDoMercadoAsync(
        IndicadoresTerritoriais indicadores, DateTime agoraUtc, CancellationToken ct)
    {
        if (indicadores.PotencialDoRecorte is not { } recorte) return null;

        var data = DateOnly.FromDateTime(agoraUtc);
        var vigente = ParametroComVigencia.VigenteEm(await parametros.ListarGeraisAsync(ct), data);
        var doRecorte = await indicadoresDeMercado.LerAsync(data, null, ct);

        // A CULTURA QUE DOMINA A ÁREA dá o índice de preço do recorte. Sem parcela com área, não há
        // escolha a declarar — e o fator sai sem preço, que é desvio zero.
        var dominante = recorte.PorCultura
            .Where(p => p.AreaUtilHectares is > 0)
            .OrderByDescending(p => p.AreaUtilHectares)
            .FirstOrDefault();

        decimal? indiceDePreco = null;
        string? culturaDoPreco = null;
        if (dominante is not null && doRecorte.PrecoPorCultura.TryGetValue(dominante.CulturaCodigo, out var momento))
        {
            indiceDePreco = momento.Indice;
            culturaDoPreco = momento.Indice is null ? null : dominante.Cultura;
        }

        var ajustado = FatorDeCiclo.Ajustar(
            recorte.DemandaAnualDeMaquinas,
            indiceDePreco,
            doRecorte.Credito?.Indice,
            doRecorte.PercepcaoDoGestor,
            vigente,
            recorte.Estimativa);

        var porte = vigente?.PorteDe(recorte.DemandaAnualDeMaquinas);
        var faixa = LeituraDoMercado.FaixaDoFator(ajustado.Fator.Fator, vigente);

        return new MomentoDoRecorte(
            ajustado,
            indiceDePreco,
            culturaDoPreco,
            doRecorte.Credito?.Indice,
            doRecorte.PercepcaoDoGestor,
            porte,
            faixa,
            LeituraDoMercado.Frase(porte, faixa),
            new ProcedenciaDoIndicador(
                "CRM Tracbel",
                "Fator de ciclo de mercado (issue 74)",
                null,
                "Preço e rentabilidade, crédito e percepção comercial",
                doRecorte.UltimoMesDePreco is { } mes ? $"preço até {mes:MM/yyyy}" : null,
                agoraUtc,
                "O fator é 1,00 quando nada desvia. Indicador ausente vale desvio ZERO, e não fator " +
                "indeterminado. O custo entra dentro da parcela de preço e rentabilidade — ele não é uma " +
                "quarta sensibilidade. O termo de troca ficou de fora (D-P05): precisa do preço de máquina, " +
                "que é a issue 70."));
    }

    /// <summary>O período mais longo aceito: três anos, a janela da curva ABC (documento 27).</summary>
    private const int MesesNoMaximo = 36;

    /// <summary>Executa a apuração.</summary>
    /// <param name="competenciaInicial">O primeiro mês, <c>aaaa-mm</c>. Nulo usa o padrão.</param>
    /// <param name="competenciaFinal">O último mês, <c>aaaa-mm</c>, inclusive. Nulo usa o padrão.</param>
    /// <param name="regiao">Norte ou Noroeste. Nulo é a área inteira.</param>
    /// <param name="lojaCodigo">O código da filial responsável. Nulo é todas.</param>
    /// <param name="visao"><c>Filial</c> (padrão) ou <c>Empresa</c>.</param>
    /// <param name="filialDaVenda">Código da filial que emitiu a nota. Nulo é todas ao alcance.</param>
    /// <param name="filialDoCliente">Código da filial de cadastro do cliente. Nulo é todas ao alcance.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PainelTerritorial>>> ExecutarAsync(
        string? competenciaInicial,
        string? competenciaFinal,
        string? regiao,
        string? lojaCodigo,
        string? visao,
        string? filialDaVenda,
        string? filialDoCliente,
        CancellationToken ct)
    {
        var contextoDeAcesso = acesso.Atual;
        var errosDeVisao = new ColetorDeErros();

        var visaoEscolhida = VisaoTerritorial.Filial;
        if (!string.IsNullOrWhiteSpace(visao)
            && !(Enum.TryParse(visao.Trim(), ignoreCase: true, out visaoEscolhida) && Enum.IsDefined(visaoEscolhida)))
            errosDeVisao.Registrar("visao", "A visão é Filial ou Empresa.", visao);

        int? filialDaVendaId = null, filialDoClienteId = null;
        if (!string.IsNullOrWhiteSpace(filialDaVenda) || !string.IsNullOrWhiteSpace(filialDoCliente))
        {
            var filiais = await repositorio.ListarFiliaisAsync(ct);
            filialDaVendaId = ResolverFilial(filialDaVenda, "filialDaVenda", filiais, errosDeVisao);
            filialDoClienteId = ResolverFilial(filialDoCliente, "filialDoCliente", filiais, errosDeVisao);
        }

        if (errosDeVisao.TemErro)
            return errosDeVisao.Recusar<ComProcedencia<PainelTerritorial>>("A consulta tem parâmetros que não valem.");

        // A PERMISSÃO REAL DECIDE, e não a tela. A empresa inteira exige a permissão de alcance
        // entre filiais em profundidade Organização; na visão da filial, pedir uma filial que o
        // contexto não alcança seria pedir para ver o que não é seu.
        if (visaoEscolhida == VisaoTerritorial.Empresa && !contextoDeAcesso.PodeAlcancarTodasAsEmpresas)
            return Resultado<ComProcedencia<PainelTerritorial>>.SemPermissao(
                $"A visão da empresa inteira exige a permissão {ContextoAcesso.PermissaoDeAlcanceEntreEmpresas} " +
                "em profundidade Organização, e o seu perfil não a tem (documento 32, P-10).");

        if (visaoEscolhida == VisaoTerritorial.Filial
            && new[] { (Id: filialDaVendaId, Codigo: filialDaVenda), (Id: filialDoClienteId, Codigo: filialDoCliente) }
                .FirstOrDefault(f => f.Id is { } id && !contextoDeAcesso.EmpresasVisiveis.Contains(id)) is { Id: not null } foraDoAlcance)
            return Resultado<ComProcedencia<PainelTerritorial>>.SemPermissao(
                $"A filial {foraDoAlcance.Codigo!.Trim()} não está ao alcance do seu contexto de acesso. " +
                "Troque a filial no cabeçalho, ou use a visão da empresa se o seu perfil permitir.");

        var agora = relogio.Agora;
        var mesCorrente = new DateOnly(agora.Year, agora.Month, 1);
        var erros = new ColetorDeErros();

        var final = LerCompetencia(competenciaFinal, "competenciaFinal", mesCorrente.AddMonths(-1), erros);
        var inicial = LerCompetencia(competenciaInicial, "competenciaInicial", final.AddMonths(-11), erros);

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
                visaoEscolhida,
                filialDaVendaId,
                filialDoClienteId),
            agora,
            ct);

        // O MOMENTO DO MERCADO DO RECORTE (fase T3): o fator de ciclo e as parcelas que o explicam.
        // Ele é montado aqui, e não no repositório, pelo mesmo motivo da calculadora — o fator é conta
        // de DOMÍNIO sobre índices e vigências, e não uma leitura de banco.
        indicadores = indicadores with { Momento = await MomentoDoMercadoAsync(indicadores, agora, ct) };

        return Resultado<ComProcedencia<PainelTerritorial>>.Ok(
            ComProcedencia<PainelTerritorial>.DoNossoBanco(
                new PainelTerritorial(
                    indicadores,
                    Lacunas(indicadores, contextoDeAcesso.PodeAlcancarTodasAsEmpresas, final >= mesCorrente),
                    contextoDeAcesso.PodeAlcancarTodasAsEmpresas,
                    Classificar(indicadores)),
                "organizacao.MunicipioDaAreaDeAtuacao · comercial.ClienteCarteira · comercial.FaturamentoDoCliente · organizacao.ProducaoAgricolaNoMunicipio",
                relogio));
    }

    /// <summary>
    /// O que os mapas pedem e o dado não sustenta — cada frase com a medida que a prova.
    /// </summary>
    private static List<MetricaSemDado> Lacunas(IndicadoresTerritoriais indicadores, bool podeVerEmpresaInteira, bool periodoParcial)
    {
        var lacunas = new List<MetricaSemDado>();

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
            "a base de propriedades está no ART, que não responde desta rede (documento 32, P-11)."));

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
            "O faturamento carregado é por cliente e mês, sem o item da nota: não há como filtrar as " +
            "vendas por tipo de produto nem por modelo (documento 32, seção 3.5)."));

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
            new("vendas", "Medido", "Medido",
                "Notas de saída do Protheus, conferidas contra consulta independente. Devolução e cancelamento não são abatidos, " +
                "e o período (FYTD ou 12 meses) está pendente (documento 32, P-4 e P-5)."),
            new("coberturaDeVisita", "RegraComercialProvisoria", "Regra provisória",
                "Visita é qualquer interação registrada, e a periodicidade é a declarada no CRM. O que conta como visita, a " +
                "periodicidade e a unidade (cliente ou vínculo) aguardam decisão do comercial (documento 32, P-2 e P-3)."),
            new("posVenda", "RegraComercialProvisoria", "Composição provisória",
                "Pós-venda é peça mais serviço, pelo grupo do item da nota. O que a diretoria considera pós-venda ainda não foi " +
                "definido (documento 32, seção 5, grupo 11)."),
            new("potencial", "Estimativa", regraAConfirmar ? "Estimativa · regra a confirmar" : "Estimativa",
                "Área plantada do município (IBGE) dividida pelos hectares por máquina da regra. É necessidade teórica da região, " +
                "não área de cliente nem potencial comercial validado (documento 32, P-8)."),
            // PLANILHA É REQUISITO, NÃO FONTE (issue 107): quem atende o município é o que a carteira do CRM diz —
            // e ela ainda não tem responsável cadastrado. A tela não mostra nem compara o que as planilhas afirmam.
            new("cenDoMunicipio", "FonteAConfirmar", "Da carteira · responsável a cadastrar",
                "Quem atende o município é o responsável da carteira do CRM com clientes nele. Enquanto as carteiras não tiverem " +
                "responsável cadastrado, o CEN do município fica sem dado (issue 107).")
        ];
    }

    private static int? ResolverFilial(
        string? codigo, string campo, IReadOnlyDictionary<string, int> filiais, ColetorDeErros erros)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        if (filiais.TryGetValue(codigo.Trim(), out var id)) return id;

        erros.Registrar(campo, "Não há filial com este código. Consulte /api/v1/catalogos/EMPRESA.", codigo);
        return null;
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
