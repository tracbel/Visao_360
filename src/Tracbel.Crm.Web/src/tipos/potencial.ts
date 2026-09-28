/**
 * Os parâmetros do potencial de mercado e o painel de fontes públicas — o espelho dos contratos de
 * `/api/v1/admin/parametros-do-potencial` e `/api/v1/integracoes/fontes-publicas` (issues 71 e 77).
 *
 * Datas de vigência chegam como `aaaa-mm-dd`; instantes, como ISO-8601 em UTC. Números chegam como número.
 */

/** Desde quando vale, por quê e por quem. `informadoPor` nulo é a semente da migração. */
export type VigenciaDoParametro = {
  vigenteDesde: string;
  justificativa: string;
  informadoPor: string | null;
  informadoEm: string;
  revogadoEm: string | null;
  revogadoPor: string | null;
  motivoDaRevogacao: string | null;
};

export type SituacaoDaRegra = 'AConfirmar' | 'Confirmada';

export type RegraDePotencialDetalhe = {
  produtoCodigoIbge: number;
  produtoNome: string;
  hectaresPorMaquina: number;
  anosDeRenovacao: number | null;
  modeloDeReferencia: string;
  situacao: SituacaoDaRegra;
  vigencia: VigenciaDoParametro;
  /** A cultura do catálogo; nula nas vigências anteriores a ele. */
  culturaCodigo: string | null;
  culturaNome: string | null;
  /** A categoria de máquina (D-P01); nula nas vigências anteriores ao catálogo. */
  categoriaDeMaquinaCodigo: string | null;
  categoriaDeMaquinaNome: string | null;
};

export type ParametrosGeraisDetalhe = {
  mesesDaJanela: number;
  pesoDosContratosNoCredito: number;
  pesoDoValorNoCredito: number;
  limiteDeRetracao: number;
  limiteDeAquecimento: number;
  limiteDeSuperaquecimento: number;
  nomeDaFaixaIntermediaria: string | null;
  limiteDaPercepcao: number;
  pesoDoIndicadorDePreco: number | null;
  pesoDoIndicadorDeCredito: number | null;
  pesoDoIndicadorComercial: number | null;
  fatorMinimo: number | null;
  fatorMaximo: number | null;
  /** Meses recentes do SICOR fora da janela; nulo é "não decidida" (D-IM-03, issue 157). */
  mesesDeCarenciaDoSicor: number | null;
  /** Abaixo disto a base do crédito é pequena; nulo é "não decidido" (D-P03, issue 73). */
  minimoDeLinhasNoCredito: number | null;
  /** Máquinas por ano a partir das quais o município é de mercado médio; nulo é "não registrada" (issue 166). */
  porteMedioAPartirDe: number | null;
  /** Máquinas por ano a partir das quais o município é de mercado grande (issue 166). */
  porteGrandeAPartirDe: number | null;
  vigencia: VigenciaDoParametro;
};

/**
 * As bandas de porte pelos tercis dos municípios da ADR — a sugestão que preenche o formulário (issue 166). Não
 * grava nada; nulas quando não há como cortar, com o motivo.
 */
export type SugestaoDasBandasDePorte = {
  porteMedioAPartirDe: number | null;
  porteGrandeAPartirDe: number | null;
  municipiosDaAdr: number;
  municipiosNaConta: number;
  anoDaAreaPlantada: number | null;
  justificativa: string | null;
  motivo: string | null;
};

export type PercepcaoDoGestorDetalhe = {
  municipioCodigoIbge: number;
  municipioNome: string;
  uf: string;
  percentual: number;
  vigencia: VigenciaDoParametro;
  /** Para onde o gestor acha que o município vai nos próximos 3 meses (28/09/2026); nula quando não declarada. */
  tendenciaParaTresMeses?: TendenciaDaPercepcao | null;
};

/** A tendência declarada pelo gestor para os próximos 3 meses. */
export type TendenciaDaPercepcao = 'Alta' | 'Estavel' | 'Queda';

/** A leitura de um município no fim de um mês — a série, reconstruída das vigências. */
export type PercepcaoDoGestorNoMes = {
  /** `aaaa-mm-01`. */
  mes: string;
  municipioCodigoIbge: number;
  percentual: number;
};

/** O que vale numa data, com o que falta decidir. */
export type ParametrosDoPotencialVigentes = {
  em: string;
  geral: ParametrosGeraisDetalhe | null;
  culturas: RegraDePotencialDetalhe[];
  percepcoes: PercepcaoDoGestorDetalhe[];
  pendencias: string[];
  /** A leitura de cada município no fim de cada um dos 12 meses até `em`. */
  serieDasPercepcoes?: PercepcaoDoGestorNoMes[] | null;
};

/** Todas as vigências já registradas, inclusive revogadas e futuras. */
export type HistoricoDosParametrosDoPotencial = {
  gerais: ParametrosGeraisDetalhe[];
  culturas: RegraDePotencialDetalhe[];
  percepcoes: PercepcaoDoGestorDetalhe[];
};

export type ProdutoDaPam = {
  codigoIbge: number;
  nome: string;
  ano: number;
  areaPlantadaNaAdrHectares: number | null;
};

export type MunicipioDaAdrOpcao = { codigoIbge: number; nome: string };

export type OpcoesDosParametros = {
  produtos: ProdutoDaPam[];
  municipios: MunicipioDaAdrOpcao[];
};

/**
 * As entradas — todas em texto, como a API as recebe: números com vírgula ou ponto, datas `aaaa-mm-dd`. É a
 * API que recusa campo a campo, com a frase certa.
 */
export type NovoParametroDoPotencial = {
  vigenteDesde: string;
  mesesDaJanela: string;
  pesoDosContratosNoCredito: string;
  limiteDeRetracao: string;
  limiteDeAquecimento: string;
  limiteDeSuperaquecimento: string;
  nomeDaFaixaIntermediaria: string;
  limiteDaPercepcao: string;
  pesoDoIndicadorDePreco: string;
  pesoDoIndicadorDeCredito: string;
  pesoDoIndicadorComercial: string;
  fatorMinimo: string;
  fatorMaximo: string;
  mesesDeCarenciaDoSicor: string;
  minimoDeLinhasNoCredito: string;
  porteMedioAPartirDe: string;
  porteGrandeAPartirDe: string;
  justificativa: string;
};

export type NovaRegraDePotencial = {
  produtoCodigoIbge: string;
  hectaresPorMaquina: string;
  anosDeRenovacao: string;
  modeloDeReferencia: string;
  situacao: string;
  vigenteDesde: string;
  justificativa: string;
  /** A cultura do catálogo — `CAFE`, `CANA`… (issue 165). Obrigatória na regra nova. */
  culturaCodigo: string;
  /** A categoria de máquina — `TRATOR`, `COLHEITADEIRA`… (D-P01). Obrigatória na regra nova. */
  categoriaDeMaquinaCodigo: string;
};

export type NovaPercepcaoDoGestor = {
  municipioCodigoIbge: string;
  percentual: string;
  vigenteDesde: string;
  justificativa: string;
  /** `Alta`, `Estavel` ou `Queda`; vazia é "não declarada". */
  tendenciaParaTresMeses: string;
};

// ------------------------------------------------------------------------------------------------
// Planejamento comercial (issue 256) — sazonalidade, pesos do IOC e share-alvo por categoria
// ------------------------------------------------------------------------------------------------

/** Os sete pesos do Índice de Oportunidade Comercial, normalizados pela soma no cálculo. */
export type PesosDoIoc = {
  potencial: number;
  cobertura: number;
  credito: number;
  rentabilidade: number;
  clientes: number;
  realizacao: number;
  penetracao: number;
};

/** A sazonalidade (doze percentuais, janeiro a dezembro, somando 100) e os pesos do IOC, numa vigência. */
export type ParametroDoPlanejamentoDetalhe = {
  sazonalidade: number[];
  pesos: PesosDoIoc;
  vigencia: VigenciaDoParametro;
};

/** O share-alvo de uma categoria de máquina, numa vigência. */
export type ShareAlvoDetalhe = {
  categoriaDeMaquinaCodigo: string;
  categoriaDeMaquinaNome: string;
  percentual: number;
  vigencia: VigenciaDoParametro;
};

export type ParametrosDoPlanejamentoVigentes = {
  em: string;
  planejamento: ParametroDoPlanejamentoDetalhe | null;
  shares: ShareAlvoDetalhe[];
  pendencias: string[];
};

export type HistoricoDoPlanejamento = {
  planejamentos: ParametroDoPlanejamentoDetalhe[];
  shares: ShareAlvoDetalhe[];
};

/** Uma vigência nova da sazonalidade e dos pesos — o conjunto inteiro, números como texto. */
export type NovoParametroDoPlanejamento = {
  vigenteDesde: string;
  sazonalidade: string[];
  pesoDoPotencial: string;
  pesoDaCobertura: string;
  pesoDoCredito: string;
  pesoDaRentabilidade: string;
  pesoDosClientes: string;
  pesoDaRealizacao: string;
  pesoDaPenetracao: string;
  justificativa: string;
};

export type NovoShareAlvo = {
  categoriaDeMaquinaCodigo: string;
  percentual: string;
  vigenteDesde: string;
  justificativa: string;
};

// ------------------------------------------------------------------------------------------------
// Painel de fontes públicas
// ------------------------------------------------------------------------------------------------

export type SituacaoDaFonte = 'EmDia' | 'Atrasada' | 'SemDado';

export type FontePublicaResumo = {
  fluxo: string;
  nome: string;
  orgao: string;
  oQueTraz: string;
  tabela: string;
  /** O nome da rotina que carrega a fonte (issue 136: a agenda é do banco, editável em Integrações). */
  rotina: string;
  rotinaCodigo: string;
  rotinaLigada: boolean;
  /** A agenda em português, como o servidor a descreve. */
  agenda: string;
  cadencia: 'Anual' | 'Mensal' | 'Diaria' | 'Intervalo';
  situacao: SituacaoDaFonte;
  motivo: string;
  ultimaAtualizacaoEm: string | null;
  ultimaExecucaoPrevistaEm: string;
  /** Nula com a rotina desligada. */
  proximaExecucaoEm: string | null;
  linhas: number;
  periodoInicial: string | null;
  periodoFinal: string | null;
  municipiosCobertos: number | null;
  registrosLidos: number;
  registrosGravados: number;
  recusados: number;
  recusasPendentes: number;
  exemplosDeRecusa: string[];
};

export type PainelDeFontesPublicas = {
  municipiosDaAdr: number;
  atrasadas: number;
  semDado: number;
  fontes: FontePublicaResumo[];
};

// ------------------------------------------------------------------------------------------------
// Cobertura dos dados do motor (issue 150) — `/api/v1/integracoes/cobertura-do-motor`
// ------------------------------------------------------------------------------------------------

export type SituacaoDaCobertura = 'Completa' | 'Parcial' | 'Vazia';

/** Uma medida de cobertura. Só contagem: nenhum valor em reais, nenhum nome de cliente. */
export type ItemDaCoberturaDoMotor = {
  codigo: string;
  nome: string;
  /** O que se conta, no plural: "municípios da ADR", "meses", "vendas". */
  unidade: string;
  total: number;
  cobertos: number;
  /** Calculado no servidor; nulo quando não há total. */
  percentual: number | null;
  situacao: SituacaoDaCobertura;
  /** A frase pronta, do servidor: o que falta e para quê. */
  motivo: string;
  paraQue: string;
  periodoInicial: string | null;
  periodoFinal: string | null;
  detalhe: string | null;
};

export type BlocoDaCoberturaDoMotor = {
  codigo: string;
  nome: string;
  fonte: string;
  /** Dado da Tracbel, contado dentro da fronteira de filial de quem lê. */
  ehInterno: boolean;
  incompletos: number;
  itens: ItemDaCoberturaDoMotor[];
};

export type PainelDeCoberturaDoMotor = {
  municipiosDaAdr: number;
  filiaisNoAlcance: number;
  todasAsFiliais: boolean;
  incompletos: number;
  grupos: BlocoDaCoberturaDoMotor[];
};

/** Um produto da PAM dentro de uma cultura do catálogo (issue 165). */
export type ProdutoDaPamNoCatalogo = {
  codigoIbge: number;
  nome: string;
  /** Falso nos detalhados de um total — o café tem "Total", "Arábica" e "Canephora". */
  entraNaSomaDaLavoura: boolean;
};

/** Uma cultura do catálogo — o que substitui as listas fixas de cultura no código das telas. */
export type CulturaNoCatalogo = {
  codigo: string;
  nome: string;
  segmento: string;
  unidadeComercial: string;
  /** Quantos quilos tem a unidade — é o que converte o R$/kg da CONAB. */
  quilosPorUnidade: number;
  fonteDoPreco: string | null;
  produtoDoPreco: string | null;
  serieDeCusto: string | null;
  estaAtiva: boolean;
  produtos: ProdutoDaPamNoCatalogo[];
};

/** Uma classificação de produto do CRM ligada a uma categoria de máquina (issue 69). */
export type LinhaDeProdutoNaCategoria = { codigo: string; nome: string };

/** Uma categoria de máquina. Lista de produtos do SICOR vazia não é erro. */
export type CategoriaNoCatalogo = {
  codigo: string;
  nome: string;
  ordem: number;
  estaAtiva: boolean;
  produtosDoSicor: number[];
  /**
   * AS CLASSIFICAÇÕES DE PRODUTO DO CRM QUE CAEM NESTA CATEGORIA (issue 69, D-P08).
   *
   * É por elas que a venda de máquina do ART chega à categoria:
   * `ART (linha) → ClassificacaoDoArt → frota.LinhaDeProduto → esta ligação → categoria`.
   *
   * Vazia não é erro: "Agricultura de precisão" não tem linha no ART.
   */
  linhasDeProduto: LinhaDeProdutoNaCategoria[];
};

/** O catálogo de mercado: culturas e categorias de máquina (issue 165). */
export type CatalogoDoMercado = { culturas: CulturaNoCatalogo[]; categorias: CategoriaNoCatalogo[] };