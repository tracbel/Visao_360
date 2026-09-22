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
export type TotaisDoEstado = {
  /** O ano da PAM — o da área e do valor, e só deles. */
  ano: number;
  areaPlantadaHectares: number | null;
  valorDaProducaoMilReais: number | null;
  tratores: number | null;
  estabelecimentos: number | null;
  /** O ano do Censo dos tratores e dos estabelecimentos (issue 152). */
  anoDoCenso: number | null;
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
