namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>
/// De onde veio UM indicador — não o painel inteiro.
///
/// <para><b>Por que por indicador:</b> a rota de indicadores territoriais carrega um selo de
/// procedência só para a leitura toda, e a apuração dela lê dezenas de tabelas com anos diferentes —
/// o Censo Agropecuário é de 2017, a PAM é anual, a PPM anda sozinha e a ANP é outra fonte. Um
/// carimbo único obriga a tela a escrever "Fonte: IBGE" à mão ao lado de cada número, e foi assim que
/// um parágrafo cinza de oito linhas nasceu embaixo de cada cartão.</para>
///
/// <para><b>A tela não escreve fonte.</b> Tudo o que o leitor precisa para conferir o número numa
/// fonte oficial sai daqui: pesquisa, tabela, variável e competência. Quando a carga melhorar, o
/// texto muda sozinho, sem release.</para>
/// </summary>
/// <param name="Fonte">Quem publica — "IBGE/SIDRA", "BCB/SICOR", "ANP", "CRM Tracbel".</param>
/// <param name="Pesquisa">A pesquisa ou o conjunto — "PAM — Produção Agrícola Municipal".</param>
/// <param name="Tabela">A tabela na fonte, quando ela tem uma — "5457" no SIDRA.</param>
/// <param name="Variavel">A variável dentro da tabela — "Área plantada".</param>
/// <param name="Competencia">O período do dado — "2024", "set/2025 a ago/2026".</param>
/// <param name="UltimaCargaUtc">Quando o CRM leu a fonte pela última vez.</param>
/// <param name="Ressalva">O que quem lê precisa saber para não se enganar — sigilo, recorte, idade.</param>
public sealed record ProcedenciaDoIndicador(
    string Fonte,
    string? Pesquisa = null,
    string? Tabela = null,
    string? Variavel = null,
    string? Competencia = null,
    DateTime? UltimaCargaUtc = null,
    string? Ressalva = null);

/// <summary>
/// A procedência de cada indicador da tela de território (issue 167).
///
/// <para><b>Um registro tipado, e não um dicionário por texto:</b> a tela não pode errar a chave em
/// silêncio e ficar sem carimbo justamente no número que alguém vai conferir.</para>
///
/// <para>Cada campo é nulo quando aquela fonte não foi carregada — e aí a tela não mostra carimbo
/// nenhum, em vez de carimbar uma origem que não leu.</para>
/// </summary>
/// <param name="AreaPlantada">A área plantada e a área colhida, da PAM.</param>
/// <param name="ValorDaProducao">O valor da produção agrícola, da PAM.</param>
/// <param name="Tratores">O parque de tratores, do Censo Agropecuário.</param>
/// <param name="Estabelecimentos">As propriedades, do Censo Agropecuário.</param>
/// <param name="Rebanho">O efetivo bovino, da Pesquisa da Pecuária Municipal.</param>
/// <param name="Usinas">As usinas de etanol autorizadas, da ANP.</param>
public sealed record ProcedenciasDoTerritorio(
    ProcedenciaDoIndicador? AreaPlantada = null,
    ProcedenciaDoIndicador? ValorDaProducao = null,
    ProcedenciaDoIndicador? Tratores = null,
    ProcedenciaDoIndicador? Estabelecimentos = null,
    ProcedenciaDoIndicador? Rebanho = null,
    ProcedenciaDoIndicador? Usinas = null);

/// <summary>
/// Uma grandeza <b>somável</b>, com os denominadores que a tornam comparável.
///
/// <para>Somável quer dizer que somar os municípios dá o total do recorte: área, quantidade, valor,
/// parque, propriedades, rebanho, crédito contratado, demanda e vendas em unidades. Só para essas a
/// pergunta "que fatia isto é?" tem resposta.</para>
///
/// <para><b>Os denominadores vêm do servidor.</b> A fatia da Região Tracbel é sobre a ADR inteira e
/// <b>não muda quando o filtro muda</b>: se ela fosse a soma do recorte consultado, escolher a
/// sub-região Norte faria todo município virar uma fatia maior de si mesmo.</para>
/// </summary>
/// <param name="Valor">O número do recorte; nulo é ausência, e nunca zero.</param>
/// <param name="TotalRegiaoTracbel">O total da ADR inteira, independente do filtro.</param>
/// <param name="TotalSaoPaulo">O total <b>publicado</b> para São Paulo — não a soma dos municípios.</param>
/// <param name="MotivoDaAusencia">Por que o valor não saiu, quando não saiu.</param>
/// <param name="Procedencia">De onde o valor veio.</param>
public sealed record MedidaSomavel(
    decimal? Valor,
    decimal? TotalRegiaoTracbel,
    decimal? TotalSaoPaulo,
    string? MotivoDaAusencia = null,
    ProcedenciaDoIndicador? Procedencia = null)
{
    /// <summary>A fatia da Região Tracbel, em percentual. Nula sem numerador ou sem denominador.</summary>
    public decimal? FatiaRegiaoTracbel => Fatia(Valor, TotalRegiaoTracbel);

    /// <summary>A fatia de São Paulo, em percentual. Nula sem numerador ou sem denominador.</summary>
    public decimal? FatiaSaoPaulo => Fatia(Valor, TotalSaoPaulo);

    /// <summary>
    /// Denominador ausente ou zerado não vira 0%.
    ///
    /// <para>"0% de São Paulo" é uma afirmação — diz que a região não tem nada disso. Fonte não
    /// carregada é outra coisa, e a tela precisa poder dizer qual das duas está olhando.</para>
    /// </summary>
    private static decimal? Fatia(decimal? parte, decimal? total) =>
        parte is null || total is null || total.Value == 0m ? null : 100m * parte.Value / total.Value;
}

/// <summary>
/// Uma grandeza do tipo <b>razão</b>, com as referências contra as quais ela se compara.
///
/// <para>Produtividade, preço, rentabilidade, captura, índices e o fator de mercado são razões. Somar
/// a produtividade de municípios não dá a produtividade do estado, e uma captura de 14,8% não é fatia
/// de coisa alguma: dizer "0,8% de São Paulo" sobre uma razão é um número sem significado.</para>
///
/// <para>Por isso aqui não há fatia. O que sai são as <b>referências</b>, e a tela mostra a distância
/// até elas — em percentual quando a grandeza é contínua, em pontos percentuais quando ela já é um
/// percentual.</para>
/// </summary>
/// <param name="Valor">O número do recorte; nulo é ausência.</param>
/// <param name="ReferenciaRegiaoTracbel">O mesmo indicador, apurado para a ADR inteira.</param>
/// <param name="ReferenciaSaoPaulo">O mesmo indicador, apurado para São Paulo.</param>
/// <param name="EhPercentual">Se a grandeza já é um percentual — então a distância é em p.p.</param>
/// <param name="Unidade">A unidade, quando há — "t/ha", "R$/sc".</param>
/// <param name="MotivoDaAusencia">Por que o valor não saiu, quando não saiu.</param>
/// <param name="Procedencia">De onde o valor veio.</param>
public sealed record MedidaDeRazao(
    decimal? Valor,
    decimal? ReferenciaRegiaoTracbel,
    decimal? ReferenciaSaoPaulo,
    bool EhPercentual = false,
    string? Unidade = null,
    string? MotivoDaAusencia = null,
    ProcedenciaDoIndicador? Procedencia = null);

/// <summary>
/// Os totais da <b>Região Tracbel</b> — a ADR inteira, e não a sub-região filtrada.
///
/// <para><b>Por que a ADR inteira, sempre:</b> este é o denominador de "que fatia da Região Tracbel
/// este município é?". Se ele fosse a soma do recorte consultado, escolher a sub-região Norte faria
/// cada município do Norte virar uma fatia maior de si mesmo — e o mesmo município mostraria dois
/// números diferentes conforme o filtro.</para>
///
/// <para><b>Região Tracbel não é "região":</b> no código, <c>RegiaoDaAreaDeAtuacao</c> é Norte ou
/// Noroeste, que são <b>sub-regiões</b>. A hierarquia é São Paulo → Região Tracbel → sub-região →
/// loja → município (issue 163).</para>
/// </summary>
/// <param name="AnoDaLavoura">O ano da PAM carregado — o mesmo dos municípios, para a fatia comparar iguais.</param>
/// <param name="AreaPlantadaHectares">A área plantada somada da ADR, sem os produtos que duplicam.</param>
/// <param name="AreaColhidaHectares">A área colhida somada da ADR.</param>
/// <param name="ValorDaProducaoMilReais">O valor da produção somado da ADR, em mil reais.</param>
/// <param name="AnoDoCenso">O ano do Censo Agropecuário dos tratores e das propriedades.</param>
/// <param name="Tratores">Os tratores somados da ADR; o município sob sigilo fica de fora, e não vira zero.</param>
/// <param name="Estabelecimentos">As propriedades somadas da ADR.</param>
/// <param name="AnoDoRebanho">O ano da PPM, que anda sozinho — ela é anual e o Censo é decenal.</param>
/// <param name="Bovinos">As cabeças de bovino somadas da ADR.</param>
/// <param name="Municipios">Quantos municípios compõem a ADR — o que dá sentido às somas acima.</param>
public sealed record TotaisDaRegiaoTracbel(
    short? AnoDaLavoura,
    decimal? AreaPlantadaHectares,
    decimal? AreaColhidaHectares,
    decimal? ValorDaProducaoMilReais,
    short? AnoDoCenso,
    int? Tratores,
    int? Estabelecimentos,
    short? AnoDoRebanho,
    int? Bovinos,
    int Municipios);
