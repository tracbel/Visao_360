using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// De onde o painel olha: a filial escolhida no cabeçalho, ou a empresa inteira.
///
/// <para><b>A empresa inteira é permissão, não preferência.</b> Ela abre o alcance entre filiais
/// (<c>CrmDbContext.AbrirAlcanceEntreEmpresas</c>), e só quem tem
/// <c>Empresa.AlcanceEntreFiliais</c> em profundidade Organização pode pedir — cada abertura fica
/// no diário com o motivo.</para>
/// </summary>
public enum VisaoTerritorial
{
    /// <summary>A filial do contexto de acesso e as abaixo dela.</summary>
    Filial = 0,

    /// <summary>Todas as filiais, cada venda no município do cliente.</summary>
    Empresa = 1
}

/// <summary>O que o painel geográfico pede.</summary>
/// <param name="CompetenciaInicial">O primeiro mês das vendas, sempre no dia 1.</param>
/// <param name="CompetenciaFinal">O último mês das vendas, sempre no dia 1, inclusive.</param>
/// <param name="Regiao">Filtro pela região da ADR. Nulo é a área inteira.</param>
/// <param name="LojaCodigo">Filtro pela loja responsável, pelo código da filial. Nulo é todas.</param>
/// <param name="Visao">Filial ou empresa inteira.</param>
/// <param name="FilialDaVendaId">
/// Só as notas emitidas por esta filial. Vale para as vendas; não se aplica a cobertura nem a
/// potencial.
/// </param>
/// <param name="FilialDoClienteId">
/// Só os clientes cadastrados nesta filial. Vale para cobertura e vendas.
/// </param>
public sealed record ConsultaDeIndicadoresTerritoriais(
    DateOnly CompetenciaInicial,
    DateOnly CompetenciaFinal,
    RegiaoDaAreaDeAtuacao? Regiao,
    string? LojaCodigo,
    VisaoTerritorial Visao = VisaoTerritorial.Filial,
    int? FilialDaVendaId = null,
    int? FilialDoClienteId = null);

/// <summary>
/// A cobertura de visita de um recorte territorial, contada por VÍNCULO cliente × carteira comercial
/// (documento 27, seção 4; documento 32, seção 8.1).
///
/// <para><b>Os estados não se sobrepõem, e a ordem é esta:</b> vínculo em linha de negócio sem
/// cadência declarada é <see cref="SemCadencia"/>, qualquer que seja o contato; entre os que têm
/// cadência, sem interação nenhuma é <see cref="NuncaContatados"/>, dentro do prazo é
/// <see cref="Cobertos"/>, fora dele é <see cref="ForaDaCadencia"/>. Assim o denominador do
/// percentual só contém quem tem prazo contra o que ser medido.</para>
/// </summary>
/// <param name="Clientes">Clientes distintos com endereço principal no recorte.</param>
/// <param name="Vinculos">Vínculos em carteira comercial desses clientes.</param>
/// <param name="VinculosComCadencia">Os que estão em linha de negócio com cadência declarada — os elegíveis.</param>
/// <param name="Cobertos">Última interação dentro da cadência da classe.</param>
/// <param name="ForaDaCadencia">Com interação, mas mais antiga que o prazo.</param>
/// <param name="NuncaContatados">Elegíveis sem interação nenhuma.</param>
/// <param name="SemCadencia">Em linha de negócio que não declara prazo — fora da conta.</param>
public sealed record CoberturaTerritorial(
    int Clientes, int Vinculos, int VinculosComCadencia, int Cobertos, int ForaDaCadencia, int NuncaContatados, int SemCadencia)
{
    /// <summary>Fora da cadência mais nunca contatados.</summary>
    public int Pendentes => ForaDaCadencia + NuncaContatados;

    /// <summary>
    /// Pendentes sobre elegíveis, em pontos percentuais com uma casa. Nulo quando não há elegível —
    /// "sem dado", nunca zero.
    /// </summary>
    public decimal? PercentualPendente =>
        VinculosComCadencia == 0 ? null : decimal.Round(100m * Pendentes / VinculosComCadencia, 1);
}

/// <summary>
/// As vendas de um recorte, pelo endereço principal do cliente, com a quebra do grupo do item.
///
/// <para><b>Sem dupla contagem por construção:</b> as quatro parcelas somam o valor líquido, e o
/// pós-venda é peça mais serviço — máquina e "outros" ficam fora dele.</para>
/// </summary>
/// <param name="ClientesQueCompraram">Clientes com faturamento diferente de zero no período.</param>
/// <param name="ValorLiquido">O total do período.</param>
/// <param name="Maquina">Grupo VEIC.</param>
/// <param name="Peca">Faixa de grupos de peça.</param>
/// <param name="Servico">Serviço e mão de obra.</param>
/// <param name="Outros">Repasse de fábrica e grupos não classificados.</param>
public sealed record VendasTerritoriais(
    int ClientesQueCompraram, decimal ValorLiquido, decimal Maquina, decimal Peca, decimal Servico, decimal Outros)
{
    /// <summary>Peça mais serviço.</summary>
    public decimal PosVenda => Peca + Servico;
}

/// <summary>
/// O potencial teórico de um produto num município, por uma regra — com a PAM do produto que o sustenta.
///
/// <para><b>Cada cultura tem o seu ano</b> (issue 152): o último em que a área plantada DELA foi divulgada. Um ano
/// único para todas as culturas — o maior da tabela — misturaria anos em silêncio no dia em que a PAM nova
/// entrasse incompleta.</para>
/// </summary>
/// <param name="ProdutoCodigoIbge">O produto da regra.</param>
/// <param name="AreaPlantadaHectares">A área plantada; nulo quando o IBGE não divulga ou não foi carregada.</param>
/// <param name="MaquinasTeoricas">Área dividida pelos hectares por máquina; nulo quando a área é nula.</param>
/// <param name="AreaColhidaHectares">A área colhida do mesmo produto e ano — abaixo da plantada em cultura perene nova ou em frustração de safra.</param>
/// <param name="ValorDaProducaoMilReais">O valor da produção do mesmo produto e ano, em MIL reais.</param>
/// <param name="Ano">O ano da PAM das quatro medidas; nulo quando a cultura não tem área divulgada em ano nenhum.</param>
/// <param name="QuantidadeProduzida">A quantidade produzida, na <paramref name="UnidadeDaQuantidade"/>.</param>
/// <param name="UnidadeDaQuantidade">"toneladas", "mil frutos" ou "mil cachos" (<see cref="UnidadesDaPam"/>).</param>
/// <param name="Produtividade">Quantidade sobre área colhida, no mesmo ano; nula sem colheita.</param>
/// <param name="UnidadeDaProdutividade">"t/ha", "mil frutos/ha" ou "mil cachos/ha".</param>
public sealed record PotencialTerritorial(
    int ProdutoCodigoIbge,
    decimal? AreaPlantadaHectares,
    decimal? MaquinasTeoricas,
    decimal? AreaColhidaHectares,
    decimal? ValorDaProducaoMilReais,
    short? Ano,
    decimal? QuantidadeProduzida,
    string? UnidadeDaQuantidade,
    decimal? Produtividade,
    string? UnidadeDaProdutividade);

/// <summary>
/// UMA CULTURA DE REGRA NO TOTAL DE SÃO PAULO, como o IBGE publica — o termo de comparação da mesma cultura no
/// município (issue 152). O ano é o mesmo da cultura no potencial, para a comparação não misturar anos.
/// </summary>
/// <param name="ProdutoCodigoIbge">O produto.</param>
/// <param name="ProdutoNome">O rótulo oficial.</param>
/// <param name="Ano">O ano da PAM.</param>
/// <param name="AreaPlantadaHectares">A área plantada no estado.</param>
/// <param name="AreaColhidaHectares">A área colhida no estado.</param>
/// <param name="QuantidadeProduzida">A quantidade produzida no estado, na unidade do produto.</param>
/// <param name="UnidadeDaQuantidade">A unidade da quantidade.</param>
/// <param name="ValorDaProducaoMilReais">O valor da produção no estado, em MIL reais.</param>
/// <param name="Produtividade">Quantidade sobre área colhida — o rendimento médio do IBGE, na unidade por hectare.</param>
/// <param name="UnidadeDaProdutividade">A unidade da produtividade.</param>
public sealed record CulturaNoEstado(
    int ProdutoCodigoIbge,
    string ProdutoNome,
    short Ano,
    decimal? AreaPlantadaHectares,
    decimal? AreaColhidaHectares,
    decimal? QuantidadeProduzida,
    string UnidadeDaQuantidade,
    decimal? ValorDaProducaoMilReais,
    decimal? Produtividade,
    string UnidadeDaProdutividade);

/// <summary>
/// A LAVOURA INTEIRA DE UM MUNICÍPIO num ano — a soma de todas as culturas da PAM.
///
/// <para><b>A quantidade produzida NÃO tem total, e é de propósito.</b> O IBGE publica cada produto
/// na unidade dele: tonelada em quase tudo, mas <b>mil frutos</b> no abacaxi e no coco-da-baía (e, até 2000, nas frutas, com <b>mil cachos</b>
/// para a banana — <see cref="UnidadesDaPam"/>). Somar isso daria um número com unidade nenhuma. A quantidade aparece por produto, ao
/// lado do rótulo que diz a unidade — nunca agregada.</para>
///
/// <para><b>O café entra uma vez só.</b> A classificação do IBGE traz "Café (em grão) Total" ao lado
/// de Arábica e Canephora, e os três na mesma resposta: a soma exclui os dois detalhados e mantém o
/// total, que é o mesmo critério da planilha do comercial.</para>
/// </summary>
/// <param name="Ano">O ano da PAM somado.</param>
/// <param name="AreaPlantadaHectares">Soma da área plantada de todas as culturas.</param>
/// <param name="AreaColhidaHectares">Soma da área colhida.</param>
/// <param name="ValorDaProducaoMilReais">Soma do valor da produção, em MIL reais.</param>
/// <param name="CulturasComArea">Quantas culturas o IBGE divulgou com área maior que zero aqui.</param>
public sealed record ProducaoAgricolaDoMunicipio(
    short Ano,
    decimal? AreaPlantadaHectares,
    decimal? AreaColhidaHectares,
    decimal? ValorDaProducaoMilReais,
    int CulturasComArea);

/// <summary>Uma usina de etanol de um município, como a tela a lista.</summary>
/// <param name="RazaoSocial">A razão social publicada pela ANP.</param>
/// <param name="CapacidadeM3Dia">Anidro mais hidratado; nulo quando a ANP não informou nenhum dos dois.</param>
public sealed record UsinaDoMunicipio(string RazaoSocial, int? CapacidadeM3Dia);

/// <summary>
/// O QUE JÁ EXISTE NUM MUNICÍPIO PARA MECANIZAR — o parque, as propriedades, o rebanho e as usinas.
///
/// <para><b>Cada medida traz o ano dela</b>, porque elas têm idades muito diferentes: o Censo
/// Agropecuário é de 2017 e só sai de novo em 2028; a Pesquisa da Pecuária Municipal é anual. Uma
/// tela que mostrasse as duas sem dizer o ano faria o leitor comparar 2017 com 2024 sem perceber.</para>
///
/// <para><b>Nulo é sigilo do IBGE, nunca zero</b> — em São Paulo, 79 linhas de tratores vêm ocultas
/// porque poucos estabelecimentos as compõem. Zero trator é uma medida; sigilo é ausência dela.</para>
///
/// <para><b>Somar as três faixas de potência conta o parque duas vezes:</b> o "Total" do IBGE é uma
/// categoria ao lado de "menos de 100 cv" e "100 cv e mais", e nem sempre é a soma delas — quando o
/// sigilo esconde uma parte, o total continua publicado.</para>
/// </summary>
/// <param name="AnoDoCenso">O ano do Censo Agropecuário; nulo quando não carregado.</param>
/// <param name="Tratores">O total de tratores.</param>
/// <param name="TratoresAbaixoDe100Cv">Os de menos de 100 cv.</param>
/// <param name="TratoresDe100CvEMais">Os de 100 cv e mais.</param>
/// <param name="EstabelecimentosComTrator">Estabelecimentos que declararam ter trator.</param>
/// <param name="Estabelecimentos">O total de estabelecimentos agropecuários.</param>
/// <param name="FaixasDeArea">Os estabelecimentos nas faixas de tamanho que o comercial usa.</param>
/// <param name="AnoDoRebanho">O ano da PPM; nulo quando não carregado.</param>
/// <param name="Bovinos">O efetivo de bovinos, em cabeças.</param>
/// <param name="AreaKm2">A área territorial do município.</param>
/// <param name="Usinas">As usinas de etanol instaladas aqui.</param>
public sealed record EstruturaDoMunicipio(
    short? AnoDoCenso,
    int? Tratores,
    int? TratoresAbaixoDe100Cv,
    int? TratoresDe100CvEMais,
    int? EstabelecimentosComTrator,
    int? Estabelecimentos,
    IReadOnlyList<FaixaDeArea> FaixasDeArea,
    short? AnoDoRebanho,
    int? Bovinos,
    decimal? AreaKm2,
    IReadOnlyList<UsinaDoMunicipio> Usinas)
{
    /// <summary>
    /// Tratores por mil km² — a densidade do parque, que compara município grande com pequeno.
    ///
    /// <para>Sem ela, o mapa de tratores é quase um mapa de tamanho do município: quem tem mais
    /// terra tem mais máquina, e isso não diz nada sobre quem mecaniza mais.</para>
    /// </summary>
    public decimal? TratoresPorMilKm2 =>
        Tratores is null || AreaKm2 is null or 0 ? null : Math.Round(Tratores.Value * 1000m / AreaKm2.Value, 1);

    /// <summary>A capacidade somada das usinas daqui; nulo quando não há usina ou nenhuma informou.</summary>
    public int? CapacidadeDeEtanolM3Dia =>
        Usinas.Count == 0 || Usinas.All(u => u.CapacidadeM3Dia is null)
            ? null
            : Usinas.Sum(u => u.CapacidadeM3Dia ?? 0);
}

/// <summary>
/// Os estabelecimentos numa faixa de tamanho, no vocabulário do comercial.
///
/// <para>O IBGE publica <b>18</b> faixas; o banco as guarda todas, e esta é a agregação que a
/// planilha do comercial usa. Fazer a conta aqui, e não na carga, é o que permite mudar o
/// reagrupamento sem recarregar nada.</para>
/// </summary>
/// <param name="Ordem">A ordem de exibição, da menor faixa para a maior.</param>
/// <param name="Rotulo">Como a faixa é chamada na tela.</param>
/// <param name="Estabelecimentos">Quantos; nulo quando todas as faixas do IBGE que a compõem vieram sob sigilo.</param>
public sealed record FaixaDeArea(int Ordem, string Rotulo, int? Estabelecimentos);

/// <summary>
/// OS TOTAIS DE SÃO PAULO, como o IBGE os publica — o denominador da comparação com a região.
///
/// <para><b>Não é a soma dos municípios.</b> O valor municipal sigiloso entra no total do estado sem
/// aparecer embaixo: em 2024 o estado publica R$ 118.021.046 mil e a soma dos 645 municípios dá
/// R$ 118.021.202 mil. A tela compara a região com o total publicado, que é o número que a diretoria
/// encontra em qualquer outra fonte.</para>
/// </summary>
/// <param name="Ano">O ano da PAM — o da área plantada e do valor, e só deles.</param>
/// <param name="AreaPlantadaHectares">A área plantada de São Paulo.</param>
/// <param name="ValorDaProducaoMilReais">O valor da produção de São Paulo, em MIL reais.</param>
/// <param name="AreaColhidaHectares">
/// A área colhida de São Paulo (issue 72) — o denominador da fatia de área colhida do recorte. A quantidade
/// produzida não tem total, e é de propósito: cada produto vem na unidade dele, e somar tonelada com mil
/// frutos daria um número sem unidade nenhuma.
/// </param>
/// <param name="Tratores">O parque de tratores do estado, do Censo Agropecuário.</param>
/// <param name="Estabelecimentos">Os estabelecimentos agropecuários do estado.</param>
/// <param name="AnoDoCenso">
/// O ano do Censo dos tratores e dos estabelecimentos (issue 152). Antes eles vinham sem ano, ao lado do da PAM —
/// e o leitor tomava 2017 por 2024.
/// </param>
/// <param name="Rebanho">O efetivo bovino do estado, da Pesquisa da Pecuária Municipal.</param>
/// <param name="AnoDoRebanho">O ano da PPM, que anda sozinho — ela é anual e o Censo é decenal.</param>
public sealed record TotaisDoEstado(
    short Ano,
    decimal? AreaPlantadaHectares,
    decimal? ValorDaProducaoMilReais,
    MedidaDoEstado Tratores,
    MedidaDoEstado Estabelecimentos,
    short? AnoDoCenso,
    MedidaDoEstado Rebanho,
    short? AnoDoRebanho,
    decimal? AreaColhidaHectares = null);

/// <summary>
/// UMA MEDIDA DO ESTADO NAS DUAS LEITURAS: a que o IBGE publica e a soma dos municípios (issue 155).
///
/// <para><b>As duas, e não só a publicada.</b> A publicada é o denominador honesto — é o número que a
/// diretoria encontra em qualquer outra fonte. A soma fica ao lado para que a tela possa dizer quanto
/// o sigilo esconde, em vez de apresentar uma diferença sem explicação a quem conferir na mão.</para>
///
/// <para>Publicado nulo é fonte não carregada; nesse caso a tela não tem denominador e não mostra
/// fatia nenhuma — nunca cai na soma sem avisar.</para>
/// </summary>
/// <param name="Publicado">O total publicado pelo IBGE para a UF, ou nulo se a linha não foi carregada.</param>
/// <param name="SomaDosMunicipios">A soma dos municípios do estado, ou nulo quando a fonte não tem linha nenhuma.</param>
public sealed record MedidaDoEstado(int? Publicado, int? SomaDosMunicipios);

/// <summary>
/// Quem responde pelos vínculos de um município NA CARTEIRA — o responsável cadastrado de cada carteira
/// comercial que tem cliente com endereço ali, ao alcance da consulta.
///
/// <para><b>É a única fonte de responsável que a tela mostra</b> (issue 107): planilha é requisito, não
/// fonte. A carteira diz quem hoje tem os clientes do município no CRM.</para>
/// </summary>
/// <param name="Nome">O nome de exibição do usuário no CRM.</param>
/// <param name="Natureza">Pessoa, departamento, sistema… — a carteira de área aparece como área.</param>
/// <param name="Vinculos">Vínculos em carteira comercial deste responsável com clientes do município.</param>
/// <param name="Carteiras">Em quantas carteiras dele esses vínculos estão.</param>
public sealed record ResponsavelPelaCarteira(string Nome, string Natureza, int Vinculos, int Carteiras);

/// <summary>Os três indicadores de um município, com o território e os responsáveis.</summary>
/// <param name="CodigoIbge">O código IBGE — é por ele que o mapa encontra o polígono.</param>
/// <param name="Nome">O nome oficial.</param>
/// <param name="PertenceAAdr">Se o município é da ADR.</param>
/// <param name="ListadoNaAreaDeAtuacao">Se ele está na área de atuação, dentro ou fora da ADR.</param>
/// <param name="Regiao">A região da ADR.</param>
/// <param name="LojaCodigo">O código da loja responsável.</param>
/// <param name="LojaNome">O nome da loja responsável.</param>
/// <param name="LojaAtivaNoCrm">Se a filial está ativa no CRM; nulo quando não há loja.</param>
/// <param name="Cobertura">A cobertura de visita.</param>
/// <param name="Vendas">As vendas no período.</param>
/// <param name="Potencial">O potencial teórico, uma linha por regra ativa.</param>
/// <param name="ResponsaveisPelasCarteiras">Os responsáveis das carteiras com vínculo aqui, dos com mais vínculos para os com menos.</param>
/// <param name="Producao">A lavoura inteira do município, somando as culturas; nulo quando a PAM não foi carregada.</param>
/// <param name="Estrutura">O parque, as propriedades, o rebanho e as usinas.</param>
/// <param name="PotencialEstrutural">O parque e a demanda do município pelo motor (issue 72), somando as categorias de máquina.</param>
public sealed record IndicadoresDoMunicipio(
    int CodigoIbge,
    string Nome,
    bool PertenceAAdr,
    bool ListadoNaAreaDeAtuacao,
    string Regiao,
    string? LojaCodigo,
    string? LojaNome,
    bool? LojaAtivaNoCrm,
    CoberturaTerritorial Cobertura,
    VendasTerritoriais Vendas,
    IReadOnlyList<PotencialTerritorial> Potencial,
    IReadOnlyList<ResponsavelPelaCarteira> ResponsaveisPelasCarteiras,
    ProducaoAgricolaDoMunicipio? Producao,
    EstruturaDoMunicipio Estrutura,
    PotencialEstruturalDoMunicipio? PotencialEstrutural = null);

/// <summary>
/// O POTENCIAL ESTRUTURAL DE UM MUNICÍPIO, pelo motor (issue 72) — o que o mapa C colore.
///
/// <para><b>É a soma das categorias de máquina</b>: o mesmo hectare pede um trator a cada tantos
/// hectares e uma colheitadeira a cada outros tantos, e as duas contam. O detalhe por categoria e por
/// cultura fica no recorte, e não em cada uma das centenas de linhas do mapa.</para>
///
/// <para><b>A área útil já vem sem a terra contada duas vezes</b> (issue 160): dentro do município, as
/// culturas que dividem o talhão entram uma vez só.</para>
/// </summary>
/// <param name="ParqueDeMaquinas">As máquinas que a área do município comporta; nulo com motivo.</param>
/// <param name="DemandaAnualDeMaquinas">Quantas por ano o parque pede; nula com motivo.</param>
/// <param name="AreaUtilHectares">A área que entrou na conta.</param>
/// <param name="Estimativa">Se alguma regra usada aqui ainda não foi confirmada pelo comercial (D-P01).</param>
/// <param name="MotivoSemParque">Por que o parque não saiu, como TEXTO; <c>Nenhum</c> quando saiu.</param>
/// <param name="MotivoSemDemanda">Por que a demanda anual não saiu, como TEXTO.</param>
public sealed record PotencialEstruturalDoMunicipio(
    decimal? ParqueDeMaquinas,
    decimal? DemandaAnualDeMaquinas,
    decimal? AreaUtilHectares,
    bool Estimativa,
    string MotivoSemParque,
    string MotivoSemDemanda);

/// <summary>
/// O POTENCIAL DE UM RECORTE NUMA CATEGORIA DE MÁQUINA — o detalhe de "30.000 tratores e 900
/// colheitadeiras", que um total só não conta.
/// </summary>
/// <param name="CategoriaCodigo">O código da categoria no catálogo (issue 165).</param>
/// <param name="CategoriaNome">O nome de exibição.</param>
/// <param name="ParqueDeMaquinas">O parque desta categoria no recorte.</param>
/// <param name="DemandaAnualDeMaquinas">A demanda anual desta categoria.</param>
/// <param name="AreaUtilHectares">A área que entrou na conta desta categoria.</param>
/// <param name="Estimativa">Se alguma regra desta categoria ainda não foi confirmada.</param>
/// <param name="MotivoSemParque">Por que o parque não saiu, como texto.</param>
/// <param name="MotivoSemDemanda">Por que a demanda não saiu, como texto.</param>
/// <param name="Frase">O que a tela mostra ao lado do número, ou no lugar dele.</param>
/// <param name="PorCultura">Uma linha por cultura dominante, com quem divide a terra com ela.</param>
public sealed record PotencialPorCategoria(
    string CategoriaCodigo,
    string CategoriaNome,
    decimal? ParqueDeMaquinas,
    decimal? DemandaAnualDeMaquinas,
    decimal? AreaUtilHectares,
    bool Estimativa,
    string MotivoSemParque,
    string MotivoSemDemanda,
    string Frase,
    IReadOnlyList<ParcelaDoParque> PorCultura);

/// <summary>
/// A RELEVÂNCIA DE UMA CULTURA DO RECORTE DENTRO DE SÃO PAULO — a aba "Relevância vs SP" do protótipo.
///
/// <para><b>Os dois lados vêm do mesmo produto e do mesmo ano</b>: a fatia compara o que é comparável, e a
/// produtividade daqui contra a do estado só faz sentido na mesma unidade.</para>
/// </summary>
/// <param name="ProdutoCodigoIbge">O produto da classificação 782.</param>
/// <param name="ProdutoNome">O rótulo oficial.</param>
/// <param name="Ano">O ano da PAM desta cultura.</param>
/// <param name="UnidadeDaQuantidade">A unidade em que o IBGE publica a quantidade.</param>
/// <param name="UnidadeDaProdutividade">A unidade da produtividade.</param>
/// <param name="Aqui">As medidas do recorte.</param>
/// <param name="EmSaoPaulo">As medidas publicadas para o estado.</param>
/// <param name="Relevancia">As fatias e a razão de produtividade.</param>
public sealed record RelevanciaDaCultura(
    int ProdutoCodigoIbge,
    string ProdutoNome,
    short Ano,
    string UnidadeDaQuantidade,
    string UnidadeDaProdutividade,
    MedidasDaLavoura Aqui,
    MedidasDaLavoura EmSaoPaulo,
    RelevanciaNoEstado Relevancia);

/// <summary>
/// O POTENCIAL DO RECORTE CONSULTADO — município, loja, região da ADR ou tudo o que a consulta deixou
/// passar (issue 72).
///
/// <para><b>É a soma dos municípios da consulta</b>, e não o motor rodado sobre as áreas somadas: o
/// compartilhamento de terra acontece dentro do município. Some a coluna da tabela e dá este número.</para>
///
/// <para><b>A fatia da quantidade não tem total</b>, de propósito — ela aparece por cultura, em
/// <see cref="RelevanciaPorCultura"/>, onde a unidade é a mesma dos dois lados.</para>
/// </summary>
/// <param name="ParqueDeMaquinas">O parque do recorte, somando as categorias.</param>
/// <param name="DemandaAnualDeMaquinas">A demanda anual do recorte.</param>
/// <param name="AreaUtilHectares">A área que entrou na conta.</param>
/// <param name="Estimativa">Se alguma regra usada ainda não foi confirmada (D-P01).</param>
/// <param name="MotivoSemParque">Por que o parque não saiu, como texto.</param>
/// <param name="MotivoSemDemanda">Por que a demanda não saiu, como texto.</param>
/// <param name="Frase">O que a tela mostra ao lado do número — o selo de estimativa e o que falta.</param>
/// <param name="MunicipiosComParque">Quantos municípios do recorte entraram na soma.</param>
/// <param name="PorCultura">O parque do recorte por cultura dominante.</param>
/// <param name="PorCategoria">O parque do recorte por categoria de máquina.</param>
/// <param name="RelevanciaNoEstado">A fatia do recorte em São Paulo — área plantada, área colhida e valor.</param>
/// <param name="RelevanciaPorCultura">A fatia e a produtividade de cada cultura contra o estado.</param>
public sealed record PotencialDoRecorteNoMapa(
    decimal? ParqueDeMaquinas,
    decimal? DemandaAnualDeMaquinas,
    decimal? AreaUtilHectares,
    bool Estimativa,
    string MotivoSemParque,
    string MotivoSemDemanda,
    string Frase,
    int MunicipiosComParque,
    IReadOnlyList<ParcelaDoParque> PorCultura,
    IReadOnlyList<PotencialPorCategoria> PorCategoria,
    RelevanciaNoEstado? RelevanciaNoEstado,
    IReadOnlyList<RelevanciaDaCultura> RelevanciaPorCultura);

/// <summary>
/// O que não tem lugar no mapa, somado à parte — é o que faz o total da tela fechar com o banco.
/// </summary>
/// <param name="Grupo">Código estável do grupo.</param>
/// <param name="Descricao">O que o grupo é, em português.</param>
/// <param name="Cobertura">A cobertura do grupo.</param>
/// <param name="Vendas">As vendas do grupo.</param>
public sealed record IndicadoresForaDoMapa(string Grupo, string Descricao, CoberturaTerritorial Cobertura, VendasTerritoriais Vendas);

/// <summary>A regra de potencial vigente hoje, como a tela a cita junto do mapa.</summary>
/// <param name="ProdutoCodigoIbge">O produto.</param>
/// <param name="ProdutoNome">O rótulo do produto.</param>
/// <param name="HectaresPorMaquina">Hectares por máquina de referência.</param>
/// <param name="ModeloDeReferencia">O modelo de referência.</param>
/// <param name="Situacao">A confirmar ou confirmada.</param>
/// <param name="Justificativa">Por que estes valores — a decisão ou a fonte.</param>
/// <param name="VigenteDesde">Desde quando a regra vale.</param>
/// <param name="AnosDeRenovacao">Anos de renovação, quando informado.</param>
public sealed record RegraDePotencialAplicada(
    int ProdutoCodigoIbge,
    string ProdutoNome,
    decimal HectaresPorMaquina,
    string ModeloDeReferencia,
    string Situacao,
    string Justificativa,
    DateOnly VigenteDesde,
    decimal? AnosDeRenovacao);

/// <summary>O painel geográfico inteiro.</summary>
/// <param name="CompetenciaInicial">O primeiro mês das vendas.</param>
/// <param name="CompetenciaFinal">O último mês das vendas, inclusive.</param>
/// <param name="ReferenciaDaCobertura">O instante contra o qual a cadência foi medida (UTC).</param>
/// <param name="InteracaoMaisRecente">A interação mais recente carregada — diz a idade do dado de visita.</param>
/// <param name="AnoDaAreaPlantada">O ano da PAM usado no potencial.</param>
/// <param name="Regras">As regras de potencial ativas.</param>
/// <param name="Municipios">Os municípios de SP da área de atuação ou com cliente, depois dos filtros.</param>
/// <param name="ForaDoMapa">Os grupos sem polígono: sem município, sem código IBGE, outra UF.</param>
/// <param name="Enderecos">Endereços de cliente ativos ao alcance.</param>
/// <param name="EnderecosComArea">Deles, quantos têm área e cultura.</param>
/// <param name="Visao">A visão aplicada — filial ou empresa.</param>
/// <param name="Estado">Os totais de São Paulo publicados pelo IBGE; nulo quando não carregados.</param>
/// <param name="CulturasNoEstado">As culturas das regras no total de São Paulo, no ano de cada uma — a comparação da ficha.</param>
/// <param name="PotencialDoRecorte">O parque, a demanda e a relevância do recorte consultado, pelo motor (issue 72).</param>
/// <param name="RegiaoTracbel">
/// Os totais da ADR inteira — o denominador de "que fatia da Região Tracbel isto é?" (issue 163).
///
/// <para><b>Ele não muda quando o filtro muda</b>, de propósito: se fosse a soma do recorte
/// consultado, escolher a sub-região Norte faria cada município do Norte virar uma fatia maior de
/// si mesmo, e o mesmo município mostraria dois números conforme o filtro.</para>
/// </param>
/// <param name="Procedencias">
/// De onde veio cada indicador — fonte, pesquisa, tabela, variável e competência (issue 167).
///
/// <para><b>A tela não escreve fonte à mão.</b> Escrever "Fonte: IBGE" no front foi o que produziu
/// os parágrafos cinza embaixo de cada cartão, e eles envelhecem sem ninguém notar.</para>
/// </param>
public sealed record IndicadoresTerritoriais(
    DateOnly CompetenciaInicial,
    DateOnly CompetenciaFinal,
    DateTime ReferenciaDaCobertura,
    DateTime? InteracaoMaisRecente,
    short? AnoDaAreaPlantada,
    IReadOnlyList<RegraDePotencialAplicada> Regras,
    IReadOnlyList<IndicadoresDoMunicipio> Municipios,
    IReadOnlyList<IndicadoresForaDoMapa> ForaDoMapa,
    int Enderecos,
    int EnderecosComArea,
    string Visao,
    TotaisDoEstado? Estado,
    IReadOnlyList<CulturaNoEstado> CulturasNoEstado,
    PotencialDoRecorteNoMapa? PotencialDoRecorte = null,
    TotaisDaRegiaoTracbel? RegiaoTracbel = null,
    ProcedenciasDoTerritorio? Procedencias = null);

/// <summary>
/// O acesso aos INDICADORES TERRITORIAIS — a leitura que alimenta os três mapas.
///
/// <para><b>Porta própria, separada de <see cref="IRepositorioTerritorio"/></b>: aquela lista o
/// território; esta cruza o território com cliente, carteira, faturamento e área plantada. Juntas
/// dariam uma porta com duas razões para mudar.</para>
///
/// <para><b>A fronteira de filial continua valendo</b> para o que tem dono — cliente, carteira e
/// faturamento entram filtrados pelo contexto de acesso. A área de atuação, os responsáveis e a
/// área plantada são mapa da empresa e não são filtrados.</para>
/// </summary>
public interface IRepositorioIndicadoresTerritoriais
{
    /// <summary>Apura os indicadores.</summary>
    /// <param name="consulta">O período e os filtros, já validados.</param>
    /// <param name="agoraUtc">O instante contra o qual a cadência é medida.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IndicadoresTerritoriais> ApurarAsync(
        ConsultaDeIndicadoresTerritoriais consulta, DateTime agoraUtc, CancellationToken ct);

    /// <summary>
    /// As filiais pelo código — ativas e inativas —, para validar os filtros de filial da venda e
    /// de cadastro do cliente antes de apurar.
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyDictionary<string, int>> ListarFiliaisAsync(CancellationToken ct);
}
