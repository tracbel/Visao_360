/**
 * Os contratos do painel geográfico, como `GET /api/v1/territorio/indicadores`
 * os devolve (documento 32, seção 8). Espelham os `record` de
 * `Dominio/Portas/PortasDeIndicadoresTerritoriais.cs` em camelCase.
 */

import type { MetricaSemDado } from './relacionamento';

export type RegiaoDaAdr = 'Norte' | 'Noroeste';

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

export type PotencialTerritorial = {
  produtoCodigoIbge: number;
  areaPlantadaHectares: number | null;
  maquinasTeoricas: number | null;
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

export type PainelTerritorial = {
  indicadores: IndicadoresTerritoriais;
  metricasSemDado: MetricaSemDado[];
  /** Se o perfil tem a permissão de alcance entre filiais em profundidade Organização. */
  podeVerEmpresaInteira: boolean;
  /** Como ler cada indicador — a tela mostra o selo junto dele. */
  classificacoes: ClassificacaoDeIndicador[];
};

/** Os filtros que a rota aceita. Vazio é "sem filtro" / "padrão". */
export type FiltrosTerritoriais = {
  competenciaInicial: string;
  competenciaFinal: string;
  regiao: '' | RegiaoDaAdr;
  lojaCodigo: string;
  visao: VisaoTerritorial;
  /** A filial que emitiu a nota — só as vendas. */
  filialDaVenda: string;
  /** A filial de cadastro do cliente — cobertura e vendas. */
  filialDoCliente: string;
};
