/**
 * Os contratos do painel geográfico, como `GET /api/v1/territorio/indicadores`
 * os devolve (documento 32, seção 8). Espelham os `record` de
 * `Dominio/Portas/PortasDeIndicadoresTerritoriais.cs` em camelCase.
 */

import type { MetricaSemDado } from './relacionamento';

/**
 * A SUB-REGIÃO da área de atuação — Norte ou Noroeste.
 *
 * NÃO É A REGIÃO TRACBEL. A hierarquia é São Paulo → Região Tracbel (a ADR
 * inteira) → sub-região → loja → município → cliente (issue 163). Chamar isto de
 * "região" na tela faria a diretoria ler "4,2% da região" como fatia da ADR
 * quando é fatia do Norte.
 */
export type RegiaoDaAdr = 'Norte' | 'Noroeste';

/** De onde veio UM indicador — não o painel inteiro (issue 167). */
export type ProcedenciaDoIndicador = {
  /** Quem publica — "IBGE/SIDRA", "BCB/SICOR", "ANP", "CRM Tracbel". */
  fonte: string;
  /** A pesquisa ou o conjunto — "PAM — Produção Agrícola Municipal". */
  pesquisa: string | null;
  /** A tabela na fonte, quando ela tem uma — "5457" no SIDRA. */
  tabela: string | null;
  /** A variável dentro da tabela — "Área plantada". */
  variavel: string | null;
  /** O período do dado — "2024", "set/2025 a ago/2026". */
  competencia: string | null;
  /** Quando o CRM leu a fonte pela última vez. */
  ultimaCargaUtc: string | null;
  /** O que quem lê precisa saber para não se enganar — sigilo, recorte, idade. */
  ressalva: string | null;
};

/**
 * Uma grandeza SOMÁVEL, com os denominadores que a tornam comparável.
 *
 * Somar os municípios dá o total do recorte: área, quantidade, valor, parque,
 * propriedades, rebanho, crédito contratado, demanda e vendas em unidades. Só
 * para essas a pergunta "que fatia isto é?" tem resposta.
 */
export type MedidaSomavel = {
  valor: number | null;
  totalRegiaoTracbel: number | null;
  totalSaoPaulo: number | null;
  /** Já em percentual, calculada no servidor. Nula sem numerador ou sem denominador. */
  fatiaRegiaoTracbel: number | null;
  fatiaSaoPaulo: number | null;
  motivoDaAusencia: string | null;
  procedencia: ProcedenciaDoIndicador | null;
};

/**
 * Uma grandeza do tipo RAZÃO, com as referências contra as quais ela se compara.
 *
 * Produtividade, preço, rentabilidade, captura, índices e o fator são razões.
 * Aqui não há fatia: o que sai são as referências, e a tela mostra a distância
 * até elas.
 */
export type MedidaDeRazao = {
  valor: number | null;
  referenciaRegiaoTracbel: number | null;
  referenciaSaoPaulo: number | null;
  /** Se a grandeza já é um percentual — então a distância é em pontos percentuais. */
  ehPercentual: boolean;
  unidade: string | null;
  motivoDaAusencia: string | null;
  procedencia: ProcedenciaDoIndicador | null;
};

/**
 * O FATOR DE CICLO DO RECORTE E AS PARCELAS QUE O EXPLICAM (fase T3).
 *
 * As parcelas são TRÊS, e não quatro: preço e rentabilidade, crédito e
 * percepção comercial (D-P05). O custo entra DENTRO da parcela de preço e
 * rentabilidade — ele não é uma quarta sensibilidade —, e o termo de troca ficou
 * de fora porque precisa do preço de máquina (issue 70).
 */
export type FatorDoCiclo = {
  fator: number | null;
  parcelaDePreco: number | null;
  parcelaDaPercepcao: number | null;
  parcelaDeCredito: number | null;
  fatorSemLimite: number | null;
  cortadoPeloLimite: boolean;
  indicadoresUsados: number;
  estimativa: boolean;
  motivo: string;
};

// `CenarioDoPotencial` já existe mais abaixo, vindo da calculadora (issue 161):
// é o mesmo contrato, e dois nomes para a mesma coisa deixariam a tela com duas
// verdades sobre o que é um cenário.

export type PotencialAjustado = {
  demandaEstrutural: number | null;
  fator: FatorDoCiclo;
  demandaAjustada: number | null;
  variacaoPercentual: number | null;
  cenarios: CenarioDoPotencial[];
  frase: string;
};

/**
 * O momento de UMA cultura — o fator dela, com a demanda que ela representa.
 *
 * O fator é por cultura (issue 74): a cana pode estar retraída enquanto o café
 * está aquecido. Crédito e percepção são do recorte e entram iguais em todas.
 */
export type MomentoDaCultura = {
  culturaCodigo: string;
  cultura: string;
  demandaEstrutural: number | null;
  areaUtilHectares: number | null;
  indiceDePreco: number | null;
  fator: FatorDoCiclo;
  demandaAjustada: number | null;
};

/** A cultura que domina a área — CONTEXTO, e não regra de agregação. */
export type CulturaPredominante = {
  cultura: string;
  /** A fatia dela na área útil das culturas com regra, em percentual. */
  fatia: number;
  /** O critério, dito por extenso — a tela não deduz qual foi. */
  criterio: string;
};

/**
 * PORTE E MOMENTO SÃO DOIS NÚMEROS, NUNCA UM (documento 50, §4.2).
 *
 * O porte é o tamanho do mercado e muda devagar; o momento é o fator de ciclo e
 * muda todo mês. O `porte` nasce **nulo** até a issue 166 ter bandas: nomear
 * exige um corte, e corte sem dono é parâmetro inventado.
 *
 * O FATOR AGREGADO NÃO É UM ÍNDICE MÉDIO DE COMMODITY (fase T3.1). Ele é
 * `Σ demandaAjustada ÷ Σ demandaEstrutural` — a razão entre dois números que o
 * motor já calcula. Cada cultura pesa pela demanda que representa, e nenhuma
 * fórmula nova entrou.
 */
export type MomentoDoRecorte = {
  /** Demanda ajustada total ÷ estrutural total; nulo com motivo. */
  fatorAgregado: number | null;
  /** `Nenhum`, `SemDemandaEstrutural` ou `SemFatorPorCultura`. */
  motivoSemFator: string;
  demandaEstruturalTotal: number | null;
  demandaAjustadaTotal: number | null;
  /** A composição do agregado — o fator de cada cultura, com as parcelas. */
  porCultura: MomentoDaCultura[];
  /** Contexto: o que se planta aqui. Não decide o fator. */
  predominante: CulturaPredominante | null;
  indiceDeCredito: number | null;
  percepcaoPercentual: number | null;
  /** Nulo enquanto a issue 166 não tiver bandas — e nulo não é "pequeno". */
  porte: string | null;
  faixaDoMomento: string | null;
  /** "Mercado grande, agora retraído." — vazia quando falta os dois lados. */
  leitura: string;
  procedencia: ProcedenciaDoIndicador | null;
};

/**
 * De onde veio cada indicador da tela de território (issue 167).
 *
 * Um registro tipado, e não um dicionário por texto: a tela não pode errar a
 * chave em silêncio e ficar sem carimbo justamente no número que alguém vai
 * conferir. Campo nulo = fonte não carregada, e aí não há carimbo nenhum.
 */
export type ProcedenciasDoTerritorio = {
  areaPlantada: ProcedenciaDoIndicador | null;
  valorDaProducao: ProcedenciaDoIndicador | null;
  tratores: ProcedenciaDoIndicador | null;
  estabelecimentos: ProcedenciaDoIndicador | null;
  rebanho: ProcedenciaDoIndicador | null;
  usinas: ProcedenciaDoIndicador | null;
  /** As vendas de máquina em unidades, do ART (D-P08); nula quando o ART não trouxe nada. */
  maquinasVendidas: ProcedenciaDoIndicador | null;
};

/**
 * Os totais da REGIÃO TRACBEL — a ADR inteira, e não a sub-região filtrada.
 *
 * É o denominador de "que fatia da Região Tracbel este município é?", e ele NÃO
 * muda quando o filtro muda: se fosse a soma do recorte consultado, escolher a
 * sub-região Norte faria cada município do Norte virar uma fatia maior de si
 * mesmo.
 */
export type TotaisDaRegiaoTracbel = {
  anoDaLavoura: number | null;
  areaPlantadaHectares: number | null;
  areaColhidaHectares: number | null;
  valorDaProducaoMilReais: number | null;
  anoDoCenso: number | null;
  tratores: number | null;
  estabelecimentos: number | null;
  anoDoRebanho: number | null;
  bovinos: number | null;
  /** Quantos municípios compõem a ADR — o que dá sentido às somas acima. */
  municipios: number;
};

/** Cobertura por VÍNCULO cliente × carteira comercial. */
export type CoberturaTerritorial = {
  clientes: number;
  vinculos: number;
  vinculosComCadencia: number;
  cobertos: number;
  foraDaCadencia: number;
  nuncaContatados: number;
  semCadencia: number;
  pendentes: number;
  /** Nulo quando não há vínculo elegível — sem dado, nunca zero. */
  percentualPendente: number | null;
};

/** Vendas pelo endereço principal do cliente; as quatro parcelas somam o líquido. */
export type VendasTerritoriais = {
  clientesQueCompraram: number;
  valorLiquido: number;
  maquina: number;
  peca: number;
  servico: number;
  outros: number;
  /** Peça + serviço. */
  posVenda: number;
};

/**
 * As máquinas teóricas de um produto numa categoria (issue 240) — a regra que as dimensionou e
 * quantas deram.
 */
export type MaquinasTeoricasNaCategoria = {
  categoriaCodigo: string;
  categoriaNome: string;
  hectaresPorMaquina: number;
  modeloDeReferencia: string;
  maquinas: number | null;
};

export type PotencialTerritorial = {
  produtoCodigoIbge: number;
  areaPlantadaHectares: number | null;
  /** SOMADAS as categorias com regra: o trator e a colheitadeira da soja são duas contas, e as duas contam. */
  maquinasTeoricas: number | null;
  /** O detalhe de `maquinasTeoricas`, uma linha por categoria, na ordem do catálogo. */
  porCategoria: MaquinasTeoricasNaCategoria[];
  /** A área colhida do mesmo produto e ano — abaixo da plantada em cultura perene nova. */
  areaColhidaHectares: number | null;
  /** O valor da produção do mesmo produto e ano, em MIL reais. */
  valorDaProducaoMilReais: number | null;
  /** O ano da PAM desta cultura — cada cultura no seu (issue 152); nulo sem área divulgada. */
  ano: number | null;
  /** Na `unidadeDaQuantidade`: tonelada quase sempre, mil frutos no abacaxi e no coco. */
  quantidadeProduzida: number | null;
  unidadeDaQuantidade: string | null;
  /** Quantidade sobre área COLHIDA, calculada no servidor; nula sem colheita. */
  produtividade: number | null;
  unidadeDaProdutividade: string | null;
};

/** Uma cultura de regra no total de São Paulo, no mesmo ano da cultura no potencial — a comparação da ficha. */
export type CulturaNoEstado = {
  produtoCodigoIbge: number;
  produtoNome: string;
  ano: number;
  areaPlantadaHectares: number | null;
  areaColhidaHectares: number | null;
  quantidadeProduzida: number | null;
  unidadeDaQuantidade: string;
  valorDaProducaoMilReais: number | null;
  produtividade: number | null;
  unidadeDaProdutividade: string;
};

/**
 * A lavoura inteira de um município, somando as culturas da PAM.
 *
 * A quantidade produzida NÃO tem total, e é de propósito: o IBGE publica cada
 * produto na unidade dele (tonelada, e mil frutos no abacaxi e no coco), e somar isso daria
 * um número sem unidade. O café entra uma vez só — o "Total" fica, Arábica e
 * Canephora saem da soma.
 */
export type ProducaoAgricolaDoMunicipio = {
  ano: number;
  areaPlantadaHectares: number | null;
  areaColhidaHectares: number | null;
  /** Em MIL reais, como o IBGE publica. */
  valorDaProducaoMilReais: number | null;
  culturasComArea: number;
};

/** Uma usina de etanol autorizada pela ANP. */
export type UsinaDoMunicipio = {
  razaoSocial: string;
  /** Anidro + hidratado; nulo quando a ANP não informou nenhum dos dois. */
  capacidadeM3Dia: number | null;
};

/** Os estabelecimentos numa faixa de tamanho, no vocabulário do comercial. */
export type FaixaDeArea = {
  ordem: number;
  rotulo: string;
  /** Nulo quando todas as faixas do IBGE que a compõem vieram sob sigilo. */
  estabelecimentos: number | null;
};

/**
 * O que já existe num município para mecanizar.
 *
 * Cada medida traz o ANO dela, porque as idades diferem muito: o Censo
 * Agropecuário é de 2017 e só sai de novo em 2028; a Pesquisa da Pecuária
 * Municipal é anual.
 *
 * Nulo é sigilo do IBGE, nunca zero. E somar as três faixas de potência conta o
 * parque duas vezes: o "Total" é uma categoria ao lado das outras duas.
 */
export type EstruturaDoMunicipio = {
  anoDoCenso: number | null;
  tratores: number | null;
  tratoresAbaixoDe100Cv: number | null;
  tratoresDe100CvEMais: number | null;
  estabelecimentosComTrator: number | null;
  estabelecimentos: number | null;
  faixasDeArea: FaixaDeArea[];
  anoDoRebanho: number | null;
  bovinos: number | null;
  areaKm2: number | null;
  usinas: UsinaDoMunicipio[];
  /** Densidade do parque — sem ela, o mapa de tratores é quase um mapa de tamanho do município. */
  tratoresPorMilKm2: number | null;
  capacidadeDeEtanolM3Dia: number | null;
};

/**
 * Os totais de São Paulo como o IBGE os publica — o denominador da comparação.
 *
 * Não é a soma dos municípios: o valor municipal sigiloso entra no total do
 * estado sem aparecer embaixo.
 */
export type MedidaDoEstado = {
  /** O total publicado pelo IBGE para a UF; nulo quando a fonte não foi carregada. */
  publicado: number | null;
  /** A soma dos municípios — menor que o publicado onde há sigilo (issue 155). */
  somaDosMunicipios: number | null;
};

export type TotaisDoEstado = {
  /** O ano da PAM — o da área e do valor, e só deles. */
  ano: number;
  areaPlantadaHectares: number | null;
  valorDaProducaoMilReais: number | null;
  /** A área colhida do estado (issue 72) — o denominador da fatia de área colhida. */
  areaColhidaHectares: number | null;
  tratores: MedidaDoEstado;
  estabelecimentos: MedidaDoEstado;
  /** O ano do Censo dos tratores e dos estabelecimentos (issue 152). */
  anoDoCenso: number | null;
  /** O efetivo bovino do estado (issue 155). */
  rebanho: MedidaDoEstado;
  /** O ano da PPM, que anda sozinho — ela é anual e o Censo é decenal. */
  anoDoRebanho: number | null;
};

export type IndicadoresDoMunicipio = {
  codigoIbge: number;
  nome: string;
  pertenceAAdr: boolean;
  listadoNaAreaDeAtuacao: boolean;
  regiao: string;
  lojaCodigo: string | null;
  lojaNome: string | null;
  lojaAtivaNoCrm: boolean | null;
  cobertura: CoberturaTerritorial;
  vendas: VendasTerritoriais;
  potencial: PotencialTerritorial[];
  /** Quem atende o município: os responsáveis das carteiras do CRM com vínculo aqui (issue 107). */
  responsaveisPelasCarteiras: ResponsavelPelaCarteira[];
  /** A lavoura inteira; nulo quando a PAM não foi carregada. */
  producao: ProducaoAgricolaDoMunicipio | null;
  estrutura: EstruturaDoMunicipio;
  /** O parque e a demanda pelo motor (issue 72); nulo quando não há regra vigente nenhuma. */
  potencialEstrutural: PotencialEstruturalDoMunicipio | null;
  /**
   * As máquinas que a Tracbel vendeu a clientes daqui, em UNIDADES, pelo ART (issue 69, D-P08).
   *
   * NULO É O ART NÃO TER TRAZIDO VENDA NENHUMA; zero é medida. O valor em reais continua em `vendas`,
   * que vem do Protheus e mede outra coisa — os dois não se somam.
   */
  maquinasVendidas: number | null;
  /**
   * As vendas em reais no MESMO TRECHO DO ANO ANTERIOR (27/09/2026) — a base do "vs. ano anterior".
   * Nulo quando o faturamento carregado não cobre a janela anterior inteira: o motivo está em
   * `periodoAnterior.motivoSemVendas`.
   */
  vendasNoPeriodoAnterior: VendasTerritoriais | null;
  /** As máquinas do mesmo trecho do ano anterior, em unidades (ART); nulo quando o ART não o cobre. */
  maquinasVendidasNoPeriodoAnterior: number | null;
};

/** As vendas de um mês dos municípios da ADR do recorte — um ponto do mini-gráfico. */
export type VendasNoMes = {
  /** `aaaa-mm-dd`, dia 1. */
  competencia: string;
  valorLiquido: number;
  maquina: number;
  posVenda: number;
  /** Em unidades (ART); nulo sem carga do ART. Não se soma aos reais. */
  maquinasVendidas: number | null;
};

/**
 * O MESMO TRECHO DO ANO ANTERIOR — a base de todo "vs. ano anterior" (decisão de 27/09/2026).
 *
 * É a janela pedida doze meses para trás: no padrão, o ano fiscal até agosto contra o ano fiscal
 * anterior até agosto. A cobertura das duas fontes é conferida no servidor — se a carga começa depois
 * da janela anterior, a variação fica vazia com o motivo, e não vira uma queda que não aconteceu.
 */
export type PeriodoAnterior = {
  competenciaInicial: string;
  competenciaFinal: string;
  primeiraCompetenciaDoFaturamento: string | null;
  primeiroMesDoArt: string | null;
  /** As unidades do recorte no ano anterior; nula quando o ART não cobre a janela. */
  maquinasVendidas: VendasDeMaquinaDoRecorte | null;
  /** Mês a mês da janela pedida, nos municípios da ADR do recorte. */
  serieAtual: VendasNoMes[];
  /** O mesmo da janela anterior; vazia quando o faturamento não a cobre. */
  serieAnterior: VendasNoMes[];
  vendasCobertas: boolean;
  maquinasCobertas: boolean;
  /** Por que a variação em reais não sai; nulo quando sai. */
  motivoSemVendas: string | null;
  /** Por que a variação em unidades não sai; nulo quando sai. */
  motivoSemMaquinas: string | null;
};

/**
 * A área de um produto da PAM nos municípios da ADR do recorte (issue 168) — de TODAS as culturas, e não
 * só das que têm regra de potencial. Cada produto no último ano em que a área dele foi divulgada.
 */
export type AreaDoProdutoNoRecorte = {
  produtoCodigoIbge: number;
  produtoNome: string;
  ano: number;
  areaPlantadaHectares: number | null;
  areaColhidaHectares: number | null;
  municipiosComArea: number;
};

/** Uma categoria de máquina do filtro "Tipo de produto" — do catálogo e do de-para da linha de produto. */
export type CategoriaParaFiltro = { codigo: string; nome: string; ordem: number };

/** Um responsável de carteira comercial — o CEN dono da carteira —, opção do filtro "CEN / gestor". */
export type ResponsavelDeCarteira = {
  id: number;
  nome: string;
  natureza: string;
  carteiras: number;
  /** O gestor direto no cadastro de usuário; nulo quando não há gestor cadastrado. */
  gestor: string | null;
};

/** Por que o parque ou a demanda não saiu — o enum do domínio, em texto. */
export type MotivoSemPotencial = 'Nenhum' | 'SemArea' | 'SemRegra' | 'SemCicloDeRenovacao';

/**
 * O potencial estrutural de um município, pelo motor (issue 72).
 *
 * É a soma das categorias de máquina — o mesmo hectare pede um trator e uma
 * colheitadeira —, e a área útil já vem sem a terra contada duas vezes.
 */
export type PotencialEstruturalDoMunicipio = {
  parqueDeMaquinas: number | null;
  demandaAnualDeMaquinas: number | null;
  areaUtilHectares: number | null;
  /** Alguma regra usada aqui ainda não foi confirmada pelo comercial (D-P01). */
  estimativa: boolean;
  motivoSemParque: MotivoSemPotencial;
  motivoSemDemanda: MotivoSemPotencial;
};

/** Uma parcela do parque: um grupo que divide a terra, ou uma cultura sozinha. */
export type ParcelaDoParque = {
  culturaCodigo: string;
  cultura: string;
  /** As outras culturas da parcela, que não somam área de novo (issue 160). */
  compartilhada: string[];
  areaUtilHectares: number | null;
  parque: number | null;
  demandaAnual: number | null;
  motivo: MotivoSemPotencial;
};

/** O parque de uma categoria de máquina no recorte — "30.000 tratores e 900 colheitadeiras". */
export type PotencialPorCategoria = {
  categoriaCodigo: string;
  categoriaNome: string;
  parqueDeMaquinas: number | null;
  demandaAnualDeMaquinas: number | null;
  areaUtilHectares: number | null;
  estimativa: boolean;
  motivoSemParque: MotivoSemPotencial;
  motivoSemDemanda: MotivoSemPotencial;
  frase: string;
  porCultura: ParcelaDoParque[];
};

/** As medidas da lavoura de um recorte — os dois lados da comparação com São Paulo. */
export type MedidasDaLavoura = {
  areaPlantadaHectares: number | null;
  areaColhidaHectares: number | null;
  quantidadeProduzida: number | null;
  valorDaProducaoMilReais: number | null;
};

/**
 * A relevância de um recorte dentro de São Paulo.
 *
 * As fatias vêm em percentual; a razão de produtividade é adimensional — 1,10 é
 * "a terra daqui rende 10% acima da média do estado".
 */
export type RelevanciaNoEstado = {
  fatiaDaAreaPlantada: number | null;
  fatiaDaAreaColhida: number | null;
  fatiaDaQuantidade: number | null;
  fatiaDoValor: number | null;
  produtividadeDoRecorte: number | null;
  produtividadeNoEstado: number | null;
  razaoDeProdutividade: number | null;
};

/** A relevância de UMA cultura contra o estado — a aba "Relevância vs SP" do protótipo. */
export type RelevanciaDaCultura = {
  produtoCodigoIbge: number;
  produtoNome: string;
  ano: number;
  unidadeDaQuantidade: string;
  unidadeDaProdutividade: string;
  aqui: MedidasDaLavoura;
  emSaoPaulo: MedidasDaLavoura;
  relevancia: RelevanciaNoEstado;
};

/**
 * O potencial do recorte consultado — município, loja, região da ADR ou tudo.
 *
 * É a SOMA DOS MUNICÍPIOS, e não o motor rodado sobre as áreas somadas: o
 * compartilhamento de terra acontece dentro do município. Some a coluna da
 * tabela e dá este número.
 */
export type PotencialDoRecorteNoMapa = {
  parqueDeMaquinas: number | null;
  demandaAnualDeMaquinas: number | null;
  areaUtilHectares: number | null;
  estimativa: boolean;
  motivoSemParque: MotivoSemPotencial;
  motivoSemDemanda: MotivoSemPotencial;
  /** O selo de estimativa e o que falta, prontos para a tela — vazio quando não há o que ressalvar. */
  frase: string;
  municipiosComParque: number;
  porCultura: ParcelaDoParque[];
  porCategoria: PotencialPorCategoria[];
  relevanciaNoEstado: RelevanciaNoEstado | null;
  relevanciaPorCultura: RelevanciaDaCultura[];
};

/** O responsável cadastrado de uma carteira comercial com clientes do município. */
export type ResponsavelPelaCarteira = {
  nome: string;
  natureza: string;
  vinculos: number;
  carteiras: number;
};

/** Como ler um indicador: medido, regra comercial provisória, estimativa ou fonte a confirmar. */
export type ClassificacaoDeIndicador = {
  indicador: 'vendas' | 'coberturaDeVisita' | 'posVenda' | 'potencial' | 'cenDoMunicipio';
  situacao: 'Medido' | 'RegraComercialProvisoria' | 'Estimativa' | 'FonteAConfirmar';
  selo: string;
  motivo: string;
};

export type IndicadoresForaDoMapa = {
  /** As máquinas do grupo em UNIDADES (issue 69); nulo quando o ART não trouxe venda nenhuma. */
  maquinasVendidas: number | null;
  grupo: string;
  descricao: string;
  cobertura: CoberturaTerritorial;
  vendas: VendasTerritoriais;
};

export type RegraDePotencialAplicada = {
  produtoCodigoIbge: number;
  produtoNome: string;
  hectaresPorMaquina: number;
  modeloDeReferencia: string;
  situacao: 'AConfirmar' | 'Confirmada';
  justificativa: string;
  /** aaaa-mm-dd — desde quando a regra vale (issue 71). */
  vigenteDesde: string;
  anosDeRenovacao: number | null;
  /**
   * A categoria de máquina da regra (issue 240). Um produto pode ter uma regra por categoria — é ela que
   * diz qual é qual. Nula só na regra anterior ao catálogo.
   */
  categoriaCodigo: string | null;
  categoriaNome: string | null;
};

export type IndicadoresTerritoriais = {
  competenciaInicial: string;
  competenciaFinal: string;
  referenciaDaCobertura: string;
  interacaoMaisRecente: string | null;
  anoDaAreaPlantada: number | null;
  regras: RegraDePotencialAplicada[];
  municipios: IndicadoresDoMunicipio[];
  foraDoMapa: IndicadoresForaDoMapa[];
  enderecos: number;
  enderecosComArea: number;
  /** A visão aplicada. */
  visao: VisaoTerritorial;
  /** Os totais de São Paulo publicados pelo IBGE; nulo quando não carregados. */
  estado: TotaisDoEstado | null;
  /** As culturas das regras no total de SP, cada uma no ano dela. */
  culturasNoEstado: CulturaNoEstado[];
  /** O parque, a demanda e a relevância do recorte consultado (issue 72); nulo sem regra vigente. */
  potencialDoRecorte: PotencialDoRecorteNoMapa | null;
  /** Os totais da ADR inteira — o denominador da fatia da Região Tracbel (issue 163). */
  regiaoTracbel: TotaisDaRegiaoTracbel | null;
  /** De onde veio cada indicador — a tela não escreve fonte à mão (issue 167). */
  procedencias: ProcedenciasDoTerritorio | null;
  /** O fator de ciclo do recorte, as parcelas e o porte estrutural (fase T3). */
  momento: MomentoDoRecorte | null;
  /** As vendas de máquina do recorte em UNIDADES, pelo ART (issue 69); nulo quando o ART não trouxe nada. */
  maquinasVendidas: VendasDeMaquinaDoRecorte | null;
  /** O mesmo trecho do ano anterior — a base do "vs. ano anterior" (27/09/2026). */
  periodoAnterior: PeriodoAnterior | null;
  /** A área de todas as culturas da PAM nos municípios da ADR do recorte (issue 168). */
  lavouraDoRecorte: AreaDoProdutoNoRecorte[] | null;
  /** As opções do filtro "Tipo de produto". */
  categoriasDeMaquina: CategoriaParaFiltro[] | null;
  /** As opções do filtro "CEN / gestor". */
  responsaveisDasCarteiras: ResponsavelDeCarteira[] | null;
};

/** Qual das três datas do ART põe a venda no período — a sub-decisão aberta da D-P08. */
export type DataQueDefineOPeriodoDaVenda = 'Entrega' | 'Venda' | 'Faturamento';

/** As máquinas vendidas numa categoria, em unidades. */
export type UnidadesNaCategoria = { categoriaCodigo: string; categoriaNome: string; unidades: number };

/**
 * AS VENDAS DE MÁQUINA DA TRACBEL EM UNIDADES — o ART (D-P08, decidida em 24/09/2026).
 *
 * NÃO CONFUNDIR COM `vendas`, que é o faturamento em REAIS, do Protheus. A captura é uma razão de
 * unidades sobre demanda estimada em unidades: reais no numerador a tornariam incomparável.
 */
export type VendasDeMaquinaDoRecorte = {
  criterioDeData: DataQueDefineOPeriodoDaVenda;
  /** O critério em português — a tela escreve qual data contou, em vez de deixá-la implícita. */
  fraseDoCriterio: string;
  unidades: number;
  porCategoria: UnidadesNaCategoria[];
  /** Vendidas a cliente sem município, sem código IBGE ou de outra UF — fora da captura. */
  unidadesForaDoMapa: number;
  /** Máquina sem classificação de produto, ou fora do alcance de filial de quem lê. */
  unidadesSemClassificacao: number;
  /** Linha que existe e ainda não foi ligada a uma categoria (colhedora de cana, plataforma de corte). */
  unidadesEmLinhaSemCategoria: number;
  /** Vendas em que a data do critério é vazia: não cabem em período nenhum. */
  vendasSemAData: number;
  /** A data mais recente, pelo critério — até quando o ART trouxe venda. */
  vendaMaisRecente: string | null;
  /** Quando a carga mais recente entrou no CRM. */
  carregadoAte: string | null;
};

// ---------------------------------------------------------------------------
// A calculadora de máquinas (issue 161) — `POST /api/v1/mercado/calculadora`
// ---------------------------------------------------------------------------

/** Uma área informada na simulação. Texto, como o resto da API: a conversão é do servidor. */
export type AreaSimulada = { culturaCodigo: string; areaHectares: string };

/**
 * O que se pergunta à calculadora.
 *
 * Tudo é opcional, e cada ausência tem sentido: sem município, a conta parte do
 * zero; **sem áreas, ela devolve o número medido do município** — que é o
 * critério de aceite da issue; sem categoria, entram todas; sem data, vale hoje.
 */
export type SimulacaoDeMaquinas = {
  municipioCodigoIbge?: string;
  categoriaCodigo?: string;
  data?: string;
  areas?: AreaSimulada[];
};

/** Uma cultura que a calculadora oferece — a lista vem do servidor, não do código daqui. */
export type CulturaNaCalculadora = {
  codigo: string;
  nome: string;
  categoriaCodigo: string;
  categoriaNome: string;
  hectaresPorMaquina: number;
  anosDeRenovacao: number | null;
  regraConfirmada: boolean;
  /** A área que o IBGE divulgou no município; nula sem município ou sob sigilo. */
  areaMedidaHectares: number | null;
  anoDaArea: number | null;
  areaInformadaHectares: number | null;
};

/** O resultado de uma categoria de máquina na simulação. */
export type CategoriaNaCalculadora = {
  codigo: string;
  nome: string;
  parqueDeMaquinas: number | null;
  demandaAnualDeMaquinas: number | null;
  areaUtilHectares: number | null;
  estimativa: boolean;
  motivoSemParque: MotivoSemPotencial;
  motivoSemDemanda: MotivoSemPotencial;
  frase: string;
  porCultura: ParcelaDoParque[];
};

/** O que a calculadora responde. Nada disto é gravado. */
export type ResultadoDaCalculadora = {
  /** aaaa-mm-dd — a data cujas vigências valeram. */
  data: string;
  municipioCodigoIbge: number | null;
  municipioNome: string | null;
  parqueDeMaquinas: number | null;
  demandaAnualDeMaquinas: number | null;
  areaUtilHectares: number | null;
  estimativa: boolean;
  motivoSemParque: MotivoSemPotencial;
  motivoSemDemanda: MotivoSemPotencial;
  frase: string;
  porCultura: ParcelaDoParque[];
  porCategoria: CategoriaNaCalculadora[];
  culturas: CulturaNaCalculadora[];
  /** O que o ajuste por cenário de mercado faz — a frase que acompanha os três. */
  sobreOsCenarios: string;
  /** O momento do mercado e a demanda ajustada por ele (issue 74); nulo sem regra vigente. */
  mercado: MercadoNaCalculadora | null;
};

/** O ajuste de uma cultura pelo momento do mercado (issue 74). */
export type AjusteDaCultura = {
  culturaCodigo: string;
  cultura: string;
  indiceDePreco: number | null;
  faixaDoPreco: string | null;
  fator: number | null;
  demandaAnual: number | null;
  demandaAjustada: number | null;
  frase: string;
};

/** Um cenário — conservador, moderado ou otimista. */
export type CenarioDoPotencial = {
  nome: string;
  fator: number | null;
  demandaAjustada: number | null;
  variacaoPercentual: number | null;
};

/**
 * O momento do mercado no recorte simulado.
 *
 * NADA AQUI MUDA COM A ÁREA DIGITADA: preço, crédito e percepção são do mercado
 * e do município, não da simulação. O que a área muda é a demanda sobre a qual o
 * fator incide.
 */
export type MercadoNaCalculadora = {
  /** aaaa-mm-dd — a data cujas vigências valeram. */
  data: string;
  /** aaaa-mm-01 — a competência da série de preço. */
  ultimoMesDePreco: string | null;
  indiceDeCredito: number | null;
  faixaDoCredito: string | null;
  /** Linhas do SICOR na janela recente — o tamanho da base. NÃO é contagem de contrato. */
  linhasDoCredito: number;
  basePequenaNoCredito: boolean;
  /** Em pontos percentuais; nula quando ninguém informou (D-P04). */
  percepcaoDoGestor: number | null;
  porCultura: AjusteDaCultura[];
  demandaAjustadaTotal: number | null;
  cenarios: CenarioDoPotencial[];
  frase: string;
};

/** Filial do cabeçalho, ou empresa inteira (só para quem tem a permissão). */
export type VisaoTerritorial = 'Filial' | 'Empresa';

/** Por que um dos quatro números de decisão não saiu. `Nenhum` é "saiu". */
export type MotivoSemNumeroDeDecisao =
  | 'Nenhum'
  | 'SemDemandaAnual'
  | 'SemVendasEmUnidades'
  | 'SemPrecoDeMaquina';

/**
 * Um dos quatro números do topo: o valor, ou o motivo de não haver valor.
 *
 * `frase` vem PRONTA do servidor — mandar só o código e montar o texto aqui devolveria a redação para o
 * TypeScript, que é de onde ela está saindo. Vazia quando o número saiu.
 */
export type NumeroDeDecisao = { valor: number | null; motivo: MotivoSemNumeroDeDecisao; frase: string };

/**
 * O mercado anual — o único dos quatro que pode sair PELA METADE.
 *
 * Categoria sem preço não é somada como zero e também não derruba o total: sai a soma das que têm,
 * marcada como `parcial`, com o nome das que ficaram de fora (documento 50, §4.1).
 */
export type MercadoAnual = NumeroDeDecisao & {
  parcial: boolean;
  categoriasSemPreco: string[];
};

/**
 * O QUE A CAPTURA CONTOU (D-P01, 27/09/2026): as máquinas vendidas das categorias que têm demanda, e o
 * que ficou de fora. Com regra só de trator, a colheitadeira e a colhedora de cana que o ART traz não
 * entram — não há demanda delas do outro lado da conta.
 *
 * `frase` vem PRONTA do servidor, como a dos motivos: o cartão e a aba dizem a mesma coisa porque é o
 * mesmo texto.
 */
export type BaseDaCaptura = {
  unidades: number;
  categorias: string[];
  unidadesForaDaConta: number;
  frase: string;
};

/** Os quatro números de decisão do recorte (documento 50, §4.1). */
export type NumerosDeDecisao = {
  demandaAnual: NumeroDeDecisao;
  mercadoAnual: MercadoAnual;
  /** Em pontos percentuais. É CAPTURA, não market share (issue 162). */
  capturaPercentual: NumeroDeDecisao;
  /** `max(0, demanda ajustada − vendas)`, em máquinas. */
  oportunidade: NumeroDeDecisao;
  /** O numerador da captura e da oportunidade; nulo quando o ART não trouxe venda nenhuma. */
  baseDaCaptura: BaseDaCaptura | null;
};

export type PainelTerritorial = {
  indicadores: IndicadoresTerritoriais;
  metricasSemDado: MetricaSemDado[];
  /** Se o perfil tem a permissão de alcance entre filiais em profundidade Organização. */
  podeVerEmpresaInteira: boolean;
  /** Como ler cada indicador — a tela mostra o selo junto dele. */
  classificacoes: ClassificacaoDeIndicador[];
  /**
   * OS QUATRO NÚMEROS DO TOPO, COM O MOTIVO — vindos da API (issue 69, parte A).
   *
   * Eles eram `valor={null}` escrito no TypeScript, e o motivo de cada um era uma constante repetida em
   * dezesseis lugares de três arquivos. Nenhum número mudou: o que mudou é que a ausência passou a ser
   * uma afirmação do servidor, com teste, e com uma redação só para a página e para a ficha.
   */
  numerosDeDecisao: NumerosDeDecisao;
  /**
   * OS QUATRO NÚMEROS NO MESMO TRECHO DO ANO ANTERIOR (27/09/2026). Só a captura e a oportunidade têm
   * número: a demanda e o mercado anual são estruturais, e a frase diz isso.
   */
  comparacaoComOAnoAnterior: ComparacaoComOAnoAnterior | null;
};

/** Por que um número de decisão não tem o valor do mesmo trecho do ano anterior. */
export type MotivoSemComparacao = 'Nenhum' | 'NumeroEstrutural' | 'SemNumeroNoPeriodo' | 'AnoAnteriorSemVendas';

/** Um número de decisão no mesmo trecho do ano anterior — ou por que ele não existe. */
export type NumeroNoAnoAnterior = { valor: number | null; motivo: MotivoSemComparacao; frase: string };

export type ComparacaoComOAnoAnterior = {
  demandaAnual: NumeroNoAnoAnterior;
  mercadoAnual: NumeroNoAnoAnterior;
  capturaPercentual: NumeroNoAnoAnterior;
  oportunidade: NumeroNoAnoAnterior;
  baseDaCaptura: BaseDaCaptura | null;
};

/** Os filtros que a rota aceita. Vazio é "sem filtro" / "padrão". */
export type FiltrosTerritoriais = {
  /** Vazio com o final vazio é o padrão do servidor: o ano fiscal até o último mês fechado. */
  competenciaInicial: string;
  competenciaFinal: string;
  regiao: '' | RegiaoDaAdr;
  lojaCodigo: string;
  visao: VisaoTerritorial;
  /** A filial que emitiu a nota — só as vendas. */
  filialDaVenda: string;
  /** A filial de cadastro do cliente — cobertura e vendas. */
  filialDoCliente: string;
  /** O "Tipo de produto": o código da categoria de máquina — só as unidades do ART e a captura. */
  categoriaDeMaquina: string;
  /** O "CEN / gestor": o Id do responsável da carteira comercial. */
  responsavel: string;
};

/** Um ano fiscal de um município — reais e unidades, cada um com a sua cobertura. */
export type AnoFiscalDoMunicipio = {
  anoFiscal: number;
  inicio: string;
  fim: string;
  /** O ano ainda não fechou: o número é até `fim`. */
  emCurso: boolean;
  vendas: VendasTerritoriais | null;
  motivoSemVendas: string | null;
  maquinasVendidas: number | null;
  motivoSemMaquinas: string | null;
};

/** Uma cultura da PAM num município e num ano. */
export type CulturaDaLavoura = {
  produtoCodigoIbge: number;
  produtoNome: string;
  areaPlantadaHectares: number | null;
  areaColhidaHectares: number | null;
  valorDaProducaoMilReais: number | null;
};

/** A lavoura de um município num ano — o total e todas as culturas (issue 168). */
export type LavouraNoAno = {
  ano: number;
  areaPlantadaHectares: number | null;
  areaColhidaHectares: number | null;
  valorDaProducaoMilReais: number | null;
  culturas: CulturaDaLavoura[];
};

/** O município ao longo do tempo — `GET /api/v1/territorio/municipios/{codigo}/historico`. */
export type HistoricoDoMunicipio = {
  codigoIbge: number;
  nome: string;
  primeiraCompetenciaDoFaturamento: string | null;
  primeiroMesDoArt: string | null;
  anosFiscais: AnoFiscalDoMunicipio[];
  /** O ano fiscal anterior nos mesmos meses do corrente; nulo quando o corrente já fechou. */
  mesmoTrechoDoAnoAnterior: AnoFiscalDoMunicipio | null;
  lavoura: LavouraNoAno[];
};
