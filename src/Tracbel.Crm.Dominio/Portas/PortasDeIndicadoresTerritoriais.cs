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
/// <param name="CategoriaDeMaquina">
/// Só as máquinas desta categoria (o código do catálogo), pelo de-para da linha de produto. Vale para as
/// vendas em UNIDADES do ART e para a captura; o faturamento em reais não tem o item da nota e não muda.
/// </param>
/// <param name="ResponsavelId">
/// Só os clientes das carteiras comerciais deste responsável — o CEN dono da carteira. Vale para cobertura,
/// vendas e unidades.
/// </param>
/// <param name="MesCorrente">
/// O mês em curso em São Paulo, no dia 1. Quando o período termina nele, o último mês está pela metade e a
/// comparação com o ano anterior fica vazia, com o motivo; nulo é não conferir.
/// </param>
public sealed record ConsultaDeIndicadoresTerritoriais(
    DateOnly CompetenciaInicial,
    DateOnly CompetenciaFinal,
    RegiaoDaAreaDeAtuacao? Regiao,
    string? LojaCodigo,
    VisaoTerritorial Visao = VisaoTerritorial.Filial,
    int? FilialDaVendaId = null,
    int? FilialDoClienteId = null,
    string? CategoriaDeMaquina = null,
    long? ResponsavelId = null,
    DateOnly? MesCorrente = null)
{
    /// <summary>O mês em curso, quando o período termina nele ou depois — o mês pela metade; nulo quando não.</summary>
    public DateOnly? MesEmCurso => MesCorrente is { } corrente && CompetenciaFinal >= corrente ? CompetenciaFinal : null;

    /// <summary>A janela pedida.</summary>
    public Comum.JanelaDeCompetencia Janela => new(CompetenciaInicial, CompetenciaFinal);

    /// <summary>
    /// O MESMO TRECHO DO ANO ANTERIOR — a comparação decidida em 27/09/2026. No padrão (ano fiscal até o
    /// último mês fechado) é o mesmo trecho do ano fiscal anterior: nov/2024 a ago/2025 contra nov/2025 a
    /// ago/2026.
    /// </summary>
    public Comum.JanelaDeCompetencia JanelaAnterior => Janela.DoAnoAnterior();
}

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
/// <param name="PorClasse">
/// Os mesmos clientes pela classe ABC apurada do faturamento (28/09/2026, o Diagnóstico Comercial). Nulo onde a apuração
/// não os separa.
/// </param>
/// <param name="ClientesEmCarteira">
/// Clientes distintos com pelo menos um vínculo em carteira comercial — os que têm dono no comercial. Nulo onde a apuração
/// não os conta.
/// </param>
public sealed record CoberturaTerritorial(
    int Clientes,
    int Vinculos,
    int VinculosComCadencia,
    int Cobertos,
    int ForaDaCadencia,
    int NuncaContatados,
    int SemCadencia,
    ClientesPorClasse? PorClasse = null,
    int? ClientesEmCarteira = null)
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
/// Os clientes de um recorte pela classe ABC — a curva do faturamento do Protheus. <see cref="SemClasse"/> é o cliente sem
/// faturamento apurado; nas contas de cadência ele vale como D, mas aqui ele é contado à parte, para a tela não dizer
/// que é D quem nunca comprou.
/// </summary>
/// <param name="A">Classe A.</param>
/// <param name="B">Classe B.</param>
/// <param name="C">Classe C.</param>
/// <param name="D">Classe D.</param>
/// <param name="SemClasse">Sem classe apurada.</param>
public sealed record ClientesPorClasse(int A, int B, int C, int D, int SemClasse);

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
/// QUAL DAS TRÊS DATAS DO ART põe uma venda dentro do período — a D-P08.1, <b>decidida em
/// 24/09/2026</b>: o <b>faturamento</b> (documento 48, §5.4).
///
/// <para><b>Porque é isso que o ART é:</b> a view é de máquina faturada — traz o número da nota e a
/// data dela —, e máquina faturada é máquina vendida. A entrega é evento posterior e fica vazia
/// quando a origem manda data zerada; contar por ela sumiria com a máquina faturada e ainda não
/// entregue. E o faturamento é o mesmo relógio do dinheiro do Protheus, então as duas medidas do
/// mesmo período falam do mesmo evento.</para>
///
/// <para>O critério continua viajando na resposta e a tela o escreve ao lado do número: <b>decidido
/// não é o mesmo que implícito</b>.</para>
/// </summary>
public enum DataQueDefineOPeriodoDaVenda
{
    /// <summary>Quando a máquina chegou na fazenda. Evento posterior, e o que mais vem vazio.</summary>
    Entrega = 0,

    /// <summary>Quando o negócio fechou, antes da nota.</summary>
    Venda = 1,

    /// <summary>Quando a nota saiu. <b>É o critério em uso</b> (D-P08.1).</summary>
    Faturamento = 2
}

/// <summary>As máquinas que a Tracbel vendeu numa categoria, em unidades.</summary>
/// <param name="CategoriaCodigo">O código da categoria no catálogo (issue 165).</param>
/// <param name="CategoriaNome">O nome de exibição.</param>
/// <param name="Unidades">Quantas máquinas.</param>
public sealed record UnidadesNaCategoria(string CategoriaCodigo, string CategoriaNome, int Unidades);

/// <summary>
/// AS VENDAS DE MÁQUINA DA TRACBEL EM UNIDADES — a fonte é o ART (D-P08, decidida em 24/09/2026).
///
/// <para><b>Não confundir com <see cref="VendasTerritoriais"/></b>, que é o faturamento em REAIS, do
/// Protheus. Os dois medem coisas diferentes e não se somam: a captura é uma razão de unidades sobre
/// demanda estimada em unidades, e reais no numerador a tornariam incomparável.</para>
///
/// <para><b>Uma venda é uma máquina.</b> A contagem é de vendas, e não da soma da coluna de
/// quantidade da origem: cada linha do ART tem um chassi, e somar a quantidade declarada contaria
/// duas vezes a máquina que a origem lançasse em lote.</para>
///
/// <para><b>Nulo é o ART não ter trazido venda nenhuma</b> — serviço desligado, carga não rodada.
/// Zero, aí sim, é medida: a filial existe, o ART trouxe dado e ela não vendeu máquina no período.</para>
/// </summary>
/// <param name="CriterioDeData">Um <see cref="DataQueDefineOPeriodoDaVenda"/> como texto.</param>
/// <param name="FraseDoCriterio">O critério em português, para a tela escrever ao lado do número.</param>
/// <param name="Unidades">As máquinas vendidas a clientes dos municípios DO RECORTE.</param>
/// <param name="PorCategoria">A quebra por categoria de máquina, pelo de-para da linha de produto (issue 69).</param>
/// <param name="UnidadesForaDoMapa">
/// As vendidas a cliente sem município, sem código IBGE ou de outra UF. Ficam fora da captura porque
/// o denominador — a demanda do recorte — também não as cobre.
/// </param>
/// <param name="UnidadesSemClassificacao">
/// Vendas cuja máquina não tem classificação de produto no CRM, ou cuja máquina está fora do alcance
/// de filial de quem lê. Contam no total e não aparecem em categoria nenhuma.
/// </param>
/// <param name="UnidadesEmLinhaSemCategoria">
/// Vendas de uma linha que existe e <b>ainda não foi ligada a uma categoria</b> — hoje a colhedora de
/// cana e a plataforma de corte, que são julgamento do comercial (documento 48, §5.3).
/// </param>
/// <param name="VendasSemAData">
/// Vendas sem a data do critério, que não cabem em período nenhum. Com o faturamento (D-P08.1), é a
/// venda fechada e <b>ainda não faturada</b> — ela existe no ART e não é uma máquina vendida ainda.
/// </param>
/// <param name="VendaMaisRecente">A data mais recente, pelo critério — diz até quando o ART trouxe venda.</param>
/// <param name="CarregadoAte">Quando a carga mais recente entrou no CRM (UTC).</param>
public sealed record VendasDeMaquinaDoRecorte(
    string CriterioDeData,
    string FraseDoCriterio,
    int Unidades,
    IReadOnlyList<UnidadesNaCategoria> PorCategoria,
    int UnidadesForaDoMapa,
    int UnidadesSemClassificacao,
    int UnidadesEmLinhaSemCategoria,
    int VendasSemAData,
    DateOnly? VendaMaisRecente,
    DateTime? CarregadoAte);

/// <summary>
/// O potencial teórico de um produto num município, por uma regra — com a PAM do produto que o sustenta.
///
/// <para><b>Cada cultura tem o seu ano</b> (issue 152): o último em que a área plantada DELA foi divulgada. Um ano
/// único para todas as culturas — o maior da tabela — misturaria anos em silêncio no dia em que a PAM nova
/// entrasse incompleta.</para>
/// </summary>
/// <param name="ProdutoCodigoIbge">O produto da regra.</param>
/// <param name="AreaPlantadaHectares">A área plantada; nulo quando o IBGE não divulga ou não foi carregada.</param>
/// <param name="MaquinasTeoricas">
/// As máquinas que a área comporta, SOMADAS as categorias com regra (issue 240): um hectare de soja pede um
/// trator a cada tantos e uma colheitadeira a cada outros tantos, e as duas contam. Nulo quando a área é nula.
/// </param>
/// <param name="AreaColhidaHectares">A área colhida do mesmo produto e ano — abaixo da plantada em cultura perene nova ou em frustração de safra.</param>
/// <param name="ValorDaProducaoMilReais">O valor da produção do mesmo produto e ano, em MIL reais.</param>
/// <param name="Ano">O ano da PAM das quatro medidas; nulo quando a cultura não tem área divulgada em ano nenhum.</param>
/// <param name="QuantidadeProduzida">A quantidade produzida, na <paramref name="UnidadeDaQuantidade"/>.</param>
/// <param name="UnidadeDaQuantidade">"toneladas", "mil frutos" ou "mil cachos" (<see cref="UnidadesDaPam"/>).</param>
/// <param name="Produtividade">Quantidade sobre área colhida, no mesmo ano; nula sem colheita.</param>
/// <param name="UnidadeDaProdutividade">"t/ha", "mil frutos/ha" ou "mil cachos/ha".</param>
/// <param name="PorCategoria">
/// O detalhe de <paramref name="MaquinasTeoricas"/>: uma linha por categoria de máquina com regra para este
/// produto, na ordem de exibição do catálogo.
/// </param>
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
    string? UnidadeDaProdutividade,
    IReadOnlyList<MaquinasTeoricasNaCategoria>? PorCategoria = null);

/// <summary>
/// AS MÁQUINAS TEÓRICAS DE UM PRODUTO NUMA CATEGORIA — a regra que as dimensionou, e quantas deram.
/// </summary>
/// <param name="CategoriaCodigo">A categoria de máquina, pelo código do catálogo.</param>
/// <param name="CategoriaNome">O nome de exibição da categoria.</param>
/// <param name="HectaresPorMaquina">Os hectares por máquina da regra vigente.</param>
/// <param name="ModeloDeReferencia">A máquina de referência, como o negócio a escreveu.</param>
/// <param name="Maquinas">As máquinas que a área comporta nesta categoria; nulo quando a área é nula.</param>
public sealed record MaquinasTeoricasNaCategoria(
    string CategoriaCodigo,
    string CategoriaNome,
    decimal HectaresPorMaquina,
    string ModeloDeReferencia,
    decimal? Maquinas);

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
/// <param name="MaquinasVendidas">
/// As máquinas que a Tracbel vendeu a clientes daqui, EM UNIDADES, pelo ART (issue 69, D-P08).
///
/// <para><b>Nulo é o ART não ter trazido venda nenhuma</b> ao alcance desta consulta; zero é medida.
/// O valor em reais continua em <see cref="Vendas"/>, que vem do Protheus e mede outra coisa.</para>
/// </param>
/// <param name="VendasNoPeriodoAnterior">
/// As vendas em reais no MESMO TRECHO DO ANO ANTERIOR (decisão de 27/09/2026) — a base do "vs. ano anterior".
///
/// <para><b>Nulo quando o faturamento carregado não cobre a janela anterior inteira</b>: comparar com meses
/// que não foram carregados daria uma queda que não aconteceu. O motivo está em
/// <see cref="PeriodoAnterior.MotivoSemVendas"/>.</para>
/// </param>
/// <param name="MaquinasVendidasNoPeriodoAnterior">
/// As máquinas vendidas no mesmo trecho do ano anterior, em unidades (ART). Nulo quando o ART não trouxe
/// venda nenhuma, ou quando a primeira venda que ele trouxe é posterior ao começo da janela anterior.
/// </param>
/// <param name="MaquinasPorCategoria">
/// As máquinas vendidas daqui, por categoria (ART) — a base da captura do município, que conta só as categorias
/// que têm demanda aqui (27/09/2026). Nulo quando o ART não trouxe venda nenhuma.
/// </param>
/// <param name="DemandaPorCategoriaECultura">
/// A demanda do motor por categoria e cultura — o ingrediente dos números de decisão do município. <b>Só existe
/// entre o repositório e a consulta</b>, que monta os números com ela e a esvazia antes de responder: a tela recebe
/// o resultado, e não centenas de linhas por município.
/// </param>
/// <param name="NumerosDeDecisao">
/// Demanda, captura e oportunidade DO MUNICÍPIO, pela mesma conta da página (27/09/2026). Até aqui a ficha
/// mostrava "—" e "só no recorte": a rota calculava a demanda por categoria e as vendas por categoria de cada
/// município e jogava as duas fora. O mercado anual segue sem preço (issue 70).
/// </param>
/// <param name="ParqueConectado">
/// As máquinas do parque do CRM cuja última posição da telemetria cai aqui (Operations Center, 28/09/2026). Nulo quando
/// nenhuma cai.
/// </param>
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
    PotencialEstruturalDoMunicipio? PotencialEstrutural = null,
    int? MaquinasVendidas = null,
    VendasTerritoriais? VendasNoPeriodoAnterior = null,
    int? MaquinasVendidasNoPeriodoAnterior = null,
    IReadOnlyList<UnidadesNaCategoria>? MaquinasPorCategoria = null,
    IReadOnlyList<DemandaNoMunicipio>? DemandaPorCategoriaECultura = null,
    NumerosDeDecisao? NumerosDeDecisao = null,
    ParqueConectadoNoMunicipio? ParqueConectado = null);

/// <summary>
/// AS MÁQUINAS CONECTADAS NUM MUNICÍPIO — a telemetria do Operations Center da John Deere (decisão de 28/09/2026).
///
/// <para><b>É onde a máquina ESTÁ, e não onde o dono mora.</b> A máquina entra no município em que a última posição dela
/// cai, pelo contorno oficial do IBGE. Só as do parque do CRM (as que casaram pelo chassi), ao alcance de quem consulta:
/// a máquina conectada que o CRM não conhece não é contada.</para>
///
/// <para><b>"Sem uso há 30 dias" é contado a partir da leitura mais nova da telemetria</b>, e não do relógio: se a rotina
/// parar, o parque inteiro não vira "parado" de uma hora para a outra. A máquina sem horímetro fica fora dessa conta — não
/// se sabe se ela trabalhou.</para>
/// </summary>
/// <param name="Maquinas">As máquinas do parque do CRM com a última posição aqui.</param>
/// <param name="ComHorimetro">Quantas delas têm horímetro.</param>
/// <param name="SemUsoHa30Dias">Das com horímetro, quantas não mandaram leitura de horas nos 30 dias antes da referência.</param>
/// <param name="HorimetroMediano">A mediana do horímetro das que têm; nula sem nenhuma.</param>
/// <param name="Referencia">A leitura mais nova da telemetria ao alcance da consulta (UTC) — o "hoje" da conta dos 30 dias.</param>
public sealed record ParqueConectadoNoMunicipio(
    int Maquinas,
    int ComHorimetro,
    int SemUsoHa30Dias,
    decimal? HorimetroMediano,
    DateTime Referencia)
{
    /// <summary>A janela do "sem uso": 30 dias antes da leitura mais nova.</summary>
    public const int DiasSemUso = 30;

    /// <summary>Agrupa as máquinas conectadas por município, com a mesma referência para todos.</summary>
    /// <param name="maquinas">As máquinas com a última posição num município.</param>
    public static IReadOnlyDictionary<int, ParqueConectadoNoMunicipio> PorMunicipio(IReadOnlyList<MaquinaConectada> maquinas)
    {
        if (maquinas.Count == 0) return new Dictionary<int, ParqueConectadoNoMunicipio>();

        var referencia = maquinas.Max(m => m.HorimetroEm is { } h && h > m.PosicaoEm ? h : m.PosicaoEm);
        var corte = referencia.AddDays(-DiasSemUso);

        return maquinas
            .GroupBy(m => m.CodigoIbge)
            .ToDictionary(g => g.Key, g =>
            {
                var comHoras = g.Where(m => m.Horimetro is not null && m.HorimetroEm is not null).ToList();
                return new ParqueConectadoNoMunicipio(
                    g.Count(),
                    comHoras.Count,
                    comHoras.Count(m => m.HorimetroEm < corte),
                    Mediana([.. comHoras.Select(m => m.Horimetro!.Value)]),
                    referencia);
            });
    }

    private static decimal? Mediana(List<decimal> valores)
    {
        if (valores.Count == 0) return null;
        valores.Sort();
        var meio = valores.Count / 2;
        return decimal.Round(valores.Count % 2 == 1 ? valores[meio] : (valores[meio - 1] + valores[meio]) / 2m, 0);
    }
}

/// <summary>Uma máquina do parque com a última posição num município — o que a conta das conectadas usa.</summary>
/// <param name="CodigoIbge">O município onde a última posição cai.</param>
/// <param name="Horimetro">O horímetro, quando há.</param>
/// <param name="HorimetroEm">Quando o horímetro foi lido.</param>
/// <param name="PosicaoEm">Quando a máquina estava ali.</param>
public sealed record MaquinaConectada(int CodigoIbge, decimal? Horimetro, DateTime? HorimetroEm, DateTime PosicaoEm);

/// <summary>
/// A DEMANDA DE UMA CULTURA NUMA CATEGORIA DE MÁQUINA, DENTRO DE UM MUNICÍPIO — uma parcela do motor.
///
/// <para>É com ela que a consulta monta os números de decisão do município: a categoria diz o que entra na
/// captura (as vendas das categorias que têm demanda aqui), e a cultura diz que fator de ciclo ajusta a demanda.</para>
/// </summary>
/// <param name="CategoriaCodigo">A categoria de máquina.</param>
/// <param name="CategoriaNome">O nome de exibição da categoria.</param>
/// <param name="CulturaCodigo">A cultura dominante da parcela.</param>
/// <param name="Cultura">O nome da cultura.</param>
/// <param name="DemandaAnual">A demanda anual da parcela; nula sem ciclo de renovação.</param>
/// <param name="AreaUtilHectares">A área útil da parcela.</param>
/// <param name="Parque">
/// As máquinas que a área da parcela comporta (28/09/2026, a Demanda e previsão). Com ele e a área saem os hectares por
/// máquina, e com a demanda, o ciclo de renovação — sem reler as regras.
/// </param>
public sealed record DemandaNoMunicipio(
    string CategoriaCodigo,
    string CategoriaNome,
    string CulturaCodigo,
    string Cultura,
    decimal? DemandaAnual,
    decimal? AreaUtilHectares,
    decimal? Parque = null);

/// <summary>As vendas de UM MÊS dos municípios da ADR no recorte — um ponto do mini-gráfico.</summary>
/// <param name="Competencia">O mês, no dia 1.</param>
/// <param name="ValorLiquido">O faturamento líquido do mês, em reais (Protheus).</param>
/// <param name="Maquina">A parte de máquina.</param>
/// <param name="PosVenda">Peça mais serviço.</param>
/// <param name="MaquinasVendidas">As máquinas do mês em UNIDADES (ART); nulo sem carga do ART. Não se soma aos reais.</param>
public sealed record VendasNoMes(DateOnly Competencia, decimal ValorLiquido, decimal Maquina, decimal PosVenda, int? MaquinasVendidas);

/// <summary>
/// O MESMO TRECHO DO ANO ANTERIOR — a base de todo "vs. ano anterior" da tela (decisão de 27/09/2026).
///
/// <para><b>Não é "os meses de antes"</b>: é a janela pedida doze meses para trás. No padrão, o ano fiscal até
/// agosto contra o ano fiscal anterior até agosto — cada mês contra ele mesmo, sem misturar safra nem o fim de
/// ano.</para>
///
/// <para><b>A cobertura é conferida, e não suposta.</b> O faturamento do Protheus foi carregado a partir de um
/// mês, e o ART a partir de outro: se a janela anterior começa antes do que foi carregado, a comparação sairia
/// como uma queda que não aconteceu. Aí a variação fica vazia, com o motivo — e as duas fontes são conferidas
/// separadas, porque R$ e unidades nunca se somam (D-P08).</para>
///
/// <para><b>Por que só o primeiro mês, e não cada mês</b> (revisão de 27/09/2026): as duas cargas releem a
/// janela INTEIRA a cada execução — o faturamento, os 36 meses da SD2; o ART, a view toda —, e uma execução que
/// falhe é refeita pela seguinte. Um mês sem linha no meio da janela não é carga que faltou: é o ERP sem nota ao
/// alcance naquele mês, e isso é medida. O que a carga não alcança é o que vem antes do primeiro mês dela.</para>
///
/// <para><b>O mês em curso também tira a comparação</b>: um período que termina no mês corrente tem o último mês
/// pela metade, e contra o mesmo mês inteiro do ano anterior a variação erraria para baixo.</para>
/// </summary>
/// <param name="CompetenciaInicial">O primeiro mês da janela anterior.</param>
/// <param name="CompetenciaFinal">O último mês da janela anterior.</param>
/// <param name="PrimeiraCompetenciaDoFaturamento">O primeiro mês com faturamento ao alcance; nulo sem faturamento.</param>
/// <param name="PrimeiroMesDoArt">O mês da primeira venda que o ART trouxe, pelo critério de data; nulo sem ART.</param>
/// <param name="MaquinasVendidas">As unidades do recorte na janela anterior, com a quebra; nula quando o ART não a cobre.</param>
/// <param name="SerieAtual">Mês a mês da janela pedida, nos municípios da ADR do recorte — o mini-gráfico.</param>
/// <param name="SerieAnterior">O mesmo, na janela anterior; vazia quando o faturamento não a cobre.</param>
/// <param name="MesEmCurso">O último mês do período pedido quando ele ainda está em curso; nulo quando não.</param>
public sealed record PeriodoAnterior(
    DateOnly CompetenciaInicial,
    DateOnly CompetenciaFinal,
    DateOnly? PrimeiraCompetenciaDoFaturamento,
    DateOnly? PrimeiroMesDoArt,
    VendasDeMaquinaDoRecorte? MaquinasVendidas,
    IReadOnlyList<VendasNoMes> SerieAtual,
    IReadOnlyList<VendasNoMes> SerieAnterior,
    DateOnly? MesEmCurso = null)
{
    /// <summary>Se o faturamento carregado cobre a janela anterior inteira — e o período pedido não termina no mês em curso.</summary>
    public bool VendasCobertas =>
        MesEmCurso is null && PrimeiraCompetenciaDoFaturamento is { } primeira && primeira <= CompetenciaInicial;

    /// <summary>Se o ART cobre a janela anterior inteira — e o período pedido não termina no mês em curso.</summary>
    public bool MaquinasCobertas => MesEmCurso is null && PrimeiroMesDoArt is { } primeiro && primeiro <= CompetenciaInicial;

    /// <summary>Por que o mês em curso tira a comparação.</summary>
    private string? MotivoDoMesEmCurso =>
        MesEmCurso is { } mes
            ? $"O último mês do período, {Comum.JanelaDeCompetencia.Mes(mes)}, está em curso: ele tem só parte das notas, " +
              "e contra o mesmo mês inteiro do ano anterior a variação erraria para baixo. Termine o período no último " +
              "mês fechado para comparar."
            : null;

    /// <summary>Por que a variação em reais não sai; nulo quando sai.</summary>
    public string? MotivoSemVendas =>
        VendasCobertas
            ? null
            : MotivoDoMesEmCurso ?? (PrimeiraCompetenciaDoFaturamento is { } primeira
                ? $"O mesmo trecho do ano anterior começa em {Comum.JanelaDeCompetencia.Mes(CompetenciaInicial)}, e o " +
                  $"faturamento carregado ao seu alcance começa em {Comum.JanelaDeCompetencia.Mes(primeira)}: comparar " +
                  "com meses que não foram carregados mostraria uma queda que não aconteceu."
                : "Não há faturamento carregado ao alcance desta consulta — sem ele não há ano anterior para comparar.");

    /// <summary>Por que a variação em unidades não sai; nulo quando sai.</summary>
    public string? MotivoSemMaquinas =>
        MaquinasCobertas
            ? null
            : MotivoDoMesEmCurso ?? (PrimeiroMesDoArt is { } primeiro
                ? $"O mesmo trecho do ano anterior começa em {Comum.JanelaDeCompetencia.Mes(CompetenciaInicial)}, e a " +
                  $"primeira venda que o ART trouxe é de {Comum.JanelaDeCompetencia.Mes(primeiro)}: antes disso o ART " +
                  "não tem venda carregada, e a comparação mostraria uma queda que não aconteceu."
                : "O ART não trouxe venda de máquina ao alcance desta consulta — sem ela não há ano anterior em " +
                  "unidades para comparar.");
}

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
/// <param name="Vendas">As vendas do grupo, em reais.</param>
/// <param name="MaquinasVendidas">
/// As máquinas do grupo, em unidades (issue 69); nulo quando o ART não trouxe venda nenhuma.
///
/// <para>Ele está aqui pelo mesmo motivo que o valor em reais: é o que faz <b>mapa + fora do mapa =
/// o total da consulta</b> fechar. Sem ele, somar a coluna de unidades daria menos que o total e
/// ninguém saberia por quê.</para>
/// </param>
public sealed record IndicadoresForaDoMapa(
    string Grupo,
    string Descricao,
    CoberturaTerritorial Cobertura,
    VendasTerritoriais Vendas,
    int? MaquinasVendidas = null);

/// <summary>A regra de potencial vigente hoje, como a tela a cita junto do mapa.</summary>
/// <param name="ProdutoCodigoIbge">O produto.</param>
/// <param name="ProdutoNome">O rótulo do produto.</param>
/// <param name="HectaresPorMaquina">Hectares por máquina de referência.</param>
/// <param name="ModeloDeReferencia">O modelo de referência.</param>
/// <param name="Situacao">A confirmar ou confirmada.</param>
/// <param name="Justificativa">Por que estes valores — a decisão ou a fonte.</param>
/// <param name="VigenteDesde">Desde quando a regra vale.</param>
/// <param name="AnosDeRenovacao">Anos de renovação, quando informado.</param>
/// <param name="CategoriaCodigo">
/// A categoria de máquina da regra (issue 240); nula só na regra anterior ao catálogo. Um produto pode ter uma
/// regra por categoria, e é ela que diz qual é qual.
/// </param>
/// <param name="CategoriaNome">O nome de exibição da categoria.</param>
public sealed record RegraDePotencialAplicada(
    int ProdutoCodigoIbge,
    string ProdutoNome,
    decimal HectaresPorMaquina,
    string ModeloDeReferencia,
    string Situacao,
    string Justificativa,
    DateOnly VigenteDesde,
    decimal? AnosDeRenovacao,
    string? CategoriaCodigo = null,
    string? CategoriaNome = null);

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
/// <param name="Momento">
/// O fator de ciclo do recorte e as parcelas que o explicam, ao lado do porte estrutural (fase T3).
///
/// <para><b>Porte e momento são dois números</b>, e o porte nasce sem nome até a issue 166 ter
/// bandas: nomear exige um corte, e um corte sem dono é parâmetro inventado.</para>
/// </param>
/// <param name="MaquinasVendidas">
/// As vendas de máquina do recorte EM UNIDADES, pelo ART (issue 69, D-P08 decidida em 24/09/2026).
///
/// <para><b>Nulo é o ART não ter trazido venda nenhuma</b> ao alcance desta consulta — serviço
/// desligado para ajuste de dados, carga não rodada. É o que faz a captura sair vazia COM MOTIVO em
/// vez de sair 0%, que afirmaria que a Tracbel não vendeu máquina na região.</para>
/// </param>
/// <param name="PeriodoAnterior">
/// O mesmo trecho do ano anterior — a base de todo "vs. ano anterior" (decisão de 27/09/2026), com a cobertura
/// das duas fontes conferida e o mês a mês dos municípios da ADR para o mini-gráfico.
/// </param>
/// <param name="LavouraDoRecorte">
/// A área de TODAS as culturas da PAM nos municípios da ADR do recorte (issue 168) — e não só das que têm regra
/// de potencial, que era o que a leitura trazia até aqui. Cada produto no último ano em que a área dele foi
/// divulgada, como no resto da tela.
/// </param>
/// <param name="CategoriasDeMaquina">As categorias que o filtro "Tipo de produto" oferece — as do de-para da linha de produto.</param>
/// <param name="ResponsaveisDasCarteiras">Os responsáveis das carteiras comerciais ao alcance — as opções do filtro "CEN / gestor".</param>
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
    ProcedenciasDoTerritorio? Procedencias = null,
    MomentoDoRecorte? Momento = null,
    VendasDeMaquinaDoRecorte? MaquinasVendidas = null,
    PeriodoAnterior? PeriodoAnterior = null,
    IReadOnlyList<AreaDoProdutoNoRecorte>? LavouraDoRecorte = null,
    IReadOnlyList<CategoriaParaFiltro>? CategoriasDeMaquina = null,
    IReadOnlyList<ResponsavelDeCarteira>? ResponsaveisDasCarteiras = null);

/// <summary>
/// A ÁREA DE UM PRODUTO DA PAM NOS MUNICÍPIOS DA ADR DO RECORTE (issue 168).
///
/// <para><b>Todos os produtos, e não só os de regra.</b> A rentabilidade pesa a margem pela área colhida da
/// região, e a cultura destaque é a de maior área: com a área só das culturas que têm regra de potencial, a
/// "média ponderada" era a de poucas culturas com o nome do todo, e uma cultura nova do catálogo sem regra ficava
/// sem área na tela. Esta leitura não passa pelas regras.</para>
///
/// <para><b>Sigilo não vira zero:</b> o município sem área divulgada fica fora da soma, e
/// <see cref="MunicipiosComArea"/> diz quantos entraram.</para>
/// </summary>
/// <param name="ProdutoCodigoIbge">O produto da classificação 782.</param>
/// <param name="ProdutoNome">O rótulo oficial.</param>
/// <param name="Ano">O último ano em que a área DESTE produto foi divulgada (issue 152).</param>
/// <param name="AreaPlantadaHectares">A soma da área plantada; nula quando nenhum município divulgou.</param>
/// <param name="AreaColhidaHectares">A soma da área colhida; nula quando nenhum município divulgou.</param>
/// <param name="MunicipiosComArea">Quantos municípios do recorte entraram na soma da área plantada.</param>
public sealed record AreaDoProdutoNoRecorte(
    int ProdutoCodigoIbge,
    string ProdutoNome,
    short Ano,
    decimal? AreaPlantadaHectares,
    decimal? AreaColhidaHectares,
    int MunicipiosComArea);

/// <summary>
/// Uma categoria de máquina que o filtro "Tipo de produto" oferece.
///
/// <para><b>A lista vem do catálogo e do de-para da linha de produto</b>, e não de uma constante: a categoria
/// nova que o comercial ligar a uma linha aparece sozinha no filtro.</para>
/// </summary>
/// <param name="Codigo">O código da categoria no catálogo.</param>
/// <param name="Nome">O nome de exibição.</param>
/// <param name="Ordem">A ordem de exibição do catálogo.</param>
public sealed record CategoriaParaFiltro(string Codigo, string Nome, short Ordem);

/// <summary>
/// Um responsável de carteira comercial — o CEN dono da carteira no CRM —, para o filtro "CEN / gestor".
/// </summary>
/// <param name="Id">O usuário, que é o valor do filtro.</param>
/// <param name="Nome">O nome de exibição.</param>
/// <param name="Natureza">Pessoa, departamento… — a carteira de área aparece como área.</param>
/// <param name="Carteiras">Quantas carteiras comerciais dele estão ao alcance.</param>
/// <param name="Gestor">O nome do gestor direto no cadastro de usuários; nulo quando não há gestor cadastrado.</param>
public sealed record ResponsavelDeCarteira(long Id, string Nome, string Natureza, int Carteiras, string? Gestor);

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

    /// <summary>
    /// As categorias de máquina que têm ao menos uma linha de produto no de-para — as opções do filtro
    /// "Tipo de produto", na ordem do catálogo.
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<CategoriaParaFiltro>> ListarCategoriasDeMaquinaAsync(CancellationToken ct);

    /// <summary>
    /// Os responsáveis das carteiras comerciais ao alcance do contexto — as opções do filtro "CEN / gestor".
    /// </summary>
    /// <param name="empresaInteira">
    /// Se a leitura é da visão da empresa: aí o alcance entre filiais é aberto, como no painel, e a lista traz os
    /// responsáveis de todas as filiais. Quem chama já conferiu a permissão.
    /// </param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<ResponsavelDeCarteira>> ListarResponsaveisDasCarteirasAsync(bool empresaInteira, CancellationToken ct);
}

/// <summary>
/// O MUNICÍPIO AO LONGO DO TEMPO — a porta da rota do histórico (27/09/2026).
///
/// <para><b>Uma porta própria, e não o quinto membro da dos indicadores</b> (documento 22, seção 6.2): o
/// painel lê uma janela de todos os municípios; o histórico lê todos os anos de um município só. São duas
/// perguntas, e quem implementa as duas é o mesmo repositório, que já tem as junções e o critério de data.</para>
/// </summary>
public interface IRepositorioHistoricoDoMunicipio
{
    /// <summary>
    /// O HISTÓRICO DE UM MUNICÍPIO — as vendas por ano fiscal e a lavoura por ano da PAM, com todas as
    /// culturas. Nulo quando o código não é de um município de São Paulo.
    /// </summary>
    /// <param name="consulta">O município e os mesmos filtros de alcance da tela.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<HistoricoDoMunicipio?> ApurarHistoricoDoMunicipioAsync(ConsultaDoHistoricoDoMunicipio consulta, CancellationToken ct);
}

/// <summary>O que o histórico de um município pede — os mesmos filtros de alcance do painel, sem o período.</summary>
/// <param name="CodigoIbge">O município.</param>
/// <param name="UltimoMesFechado">O último mês que entra — o mês em curso fica de fora, como no painel.</param>
/// <param name="Visao">Filial ou empresa inteira.</param>
/// <param name="FilialDaVendaId">Só as notas desta filial.</param>
/// <param name="FilialDoClienteId">Só os clientes cadastrados nesta filial.</param>
/// <param name="CategoriaDeMaquina">Só as máquinas desta categoria (unidades).</param>
/// <param name="ResponsavelId">Só os clientes das carteiras deste responsável.</param>
public sealed record ConsultaDoHistoricoDoMunicipio(
    int CodigoIbge,
    DateOnly UltimoMesFechado,
    VisaoTerritorial Visao = VisaoTerritorial.Filial,
    int? FilialDaVendaId = null,
    int? FilialDoClienteId = null,
    string? CategoriaDeMaquina = null,
    long? ResponsavelId = null);

/// <summary>
/// UM ANO FISCAL DE UM MUNICÍPIO — as vendas em reais e as máquinas em unidades, cada uma com a sua cobertura.
///
/// <para><b>O ano que a carga não cobre inteiro não é somado pela metade</b>: o faturamento foi carregado a partir
/// de um mês, e o ART de outro; um ano fiscal que começa antes disso sai sem o número, com o motivo — um total de
/// seis meses ao lado de um de doze se leria como queda.</para>
/// </summary>
/// <param name="AnoFiscal">O ano fiscal — o ano civil em que ele termina.</param>
/// <param name="Inicio">O primeiro mês (novembro).</param>
/// <param name="Fim">O último mês que entrou — outubro, ou o último mês fechado quando o ano ainda corre.</param>
/// <param name="EmCurso">Se o ano ainda não fechou: o número é do ano até <paramref name="Fim"/>.</param>
/// <param name="Vendas">As vendas em reais; nulas quando o faturamento carregado não cobre o ano.</param>
/// <param name="MotivoSemVendas">Por que as vendas não saíram.</param>
/// <param name="MaquinasVendidas">As máquinas vendidas, em unidades; nulas quando o ART não cobre o ano.</param>
/// <param name="MotivoSemMaquinas">Por que as unidades não saíram.</param>
public sealed record AnoFiscalDoMunicipio(
    int AnoFiscal,
    DateOnly Inicio,
    DateOnly Fim,
    bool EmCurso,
    VendasTerritoriais? Vendas,
    string? MotivoSemVendas,
    int? MaquinasVendidas,
    string? MotivoSemMaquinas);

/// <summary>Uma cultura da PAM num município e num ano.</summary>
/// <param name="ProdutoCodigoIbge">O produto.</param>
/// <param name="ProdutoNome">O rótulo oficial.</param>
/// <param name="AreaPlantadaHectares">A área plantada; nula sob sigilo.</param>
/// <param name="AreaColhidaHectares">A área colhida.</param>
/// <param name="ValorDaProducaoMilReais">O valor da produção, em mil reais.</param>
public sealed record CulturaDaLavoura(
    int ProdutoCodigoIbge,
    string ProdutoNome,
    decimal? AreaPlantadaHectares,
    decimal? AreaColhidaHectares,
    decimal? ValorDaProducaoMilReais);

/// <summary>
/// A LAVOURA DE UM MUNICÍPIO NUM ANO DA PAM — o total e TODAS as culturas (issue 168), e não só as que têm regra.
///
/// <para><b>O café entra uma vez só</b>, pelo Total do IBGE: Arábica e Canephora ficam fora do total e da
/// lista, porque somariam a mesma terra de novo.</para>
/// </summary>
/// <param name="Ano">O ano da PAM.</param>
/// <param name="AreaPlantadaHectares">A soma das culturas.</param>
/// <param name="AreaColhidaHectares">A soma das culturas.</param>
/// <param name="ValorDaProducaoMilReais">A soma das culturas, em mil reais.</param>
/// <param name="Culturas">As culturas com área plantada divulgada, da maior para a menor.</param>
public sealed record LavouraNoAno(
    short Ano,
    decimal? AreaPlantadaHectares,
    decimal? AreaColhidaHectares,
    decimal? ValorDaProducaoMilReais,
    IReadOnlyList<CulturaDaLavoura> Culturas);

/// <summary>
/// O MUNICÍPIO AO LONGO DO TEMPO — a aba Histórico da ficha e a lavoura inteira da Visão geral.
///
/// <para><b>Uma rota própria, e não mais um campo dos 645 municípios do painel</b>: o ano a ano e todas as
/// culturas de cada município multiplicariam a resposta do painel por um número que só um município — o
/// escolhido — usa.</para>
///
/// <para><b>A cobertura de visita não tem série</b>, e isto não a inventa: ela é medida no instante da
/// leitura, e o CRM não guarda a de antes.</para>
/// </summary>
/// <param name="CodigoIbge">O município.</param>
/// <param name="Nome">O nome oficial.</param>
/// <param name="PrimeiraCompetenciaDoFaturamento">Desde quando há faturamento carregado ao alcance.</param>
/// <param name="PrimeiroMesDoArt">Desde quando o ART trouxe venda.</param>
/// <param name="AnosFiscais">Os anos fiscais, do mais antigo ao corrente.</param>
/// <param name="MesmoTrechoDoAnoAnterior">
/// O ano fiscal anterior cortado nos mesmos meses do corrente — a comparação que a diretoria faz. Nulo quando o
/// ano corrente já fechou (aí a comparação é com o ano anterior inteiro, que já está na lista).
/// </param>
/// <param name="Lavoura">A lavoura de cada ano da PAM carregado, do mais antigo ao mais recente.</param>
public sealed record HistoricoDoMunicipio(
    int CodigoIbge,
    string Nome,
    DateOnly? PrimeiraCompetenciaDoFaturamento,
    DateOnly? PrimeiroMesDoArt,
    IReadOnlyList<AnoFiscalDoMunicipio> AnosFiscais,
    AnoFiscalDoMunicipio? MesmoTrechoDoAnoAnterior,
    IReadOnlyList<LavouraNoAno> Lavoura);
