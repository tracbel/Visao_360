import type { ProcedenciaDoIndicador, ResponsavelDeCarteira } from './territorio';
/**
 * A base de preços de mercado (issue 66) — como a rota `/v1/territorio/precos` a entrega.
 *
 * O valor vem na unidade da FONTE (a CONAB publica tudo por kg) e o fator para
 * a unidade em que o mercado negocia (saca de 60 kg, caixa de 40,8 kg, arroba).
 */

/** O preço de um mês. `valorEmDolares` é nulo quando o mês ainda não tem PTAX. */
export type PrecoNoMes = {
  /** O mês, `aaaa-mm-01`. */
  mes: string;
  valorEmReais: number;
  valorEmDolares: number | null;
};

/** Uma série: um produto, numa fonte, num nível. */
export type SerieDePreco = {
  fonte: string;
  codigoNaFonte: string;
  nivel: string;
  produto: string;
  classificacao: string;
  unidade: string;
  unidadeComercial: string;
  fatorComercial: number;
  /** Do mais antigo ao mais recente. */
  meses: PrecoNoMes[];
  /** De onde esta série veio e até quando ela vai (issue 167). */
  procedencia: ProcedenciaDoIndicador | null;
};

export type PrecosDeMercado = {
  series: SerieDePreco[];
  primeiroMesDoDolar: string | null;
  ultimoMesDoDolar: string | null;
  /**
   * O preço de cada categoria de máquina, mês a mês (issue 70, D-P12): a mediana das notas de venda do Protheus casadas
   * com as vendas do ART. Vazio enquanto a rotina PRECOS_DE_MAQUINA não rodou; ausente em resposta antiga.
   */
  maquinas?: SerieDePrecoDeMaquina[];
};

/** O preço de uma categoria de máquina num mês — só o agregado das notas. */
export type PrecoDeMaquinaNoMes = {
  /** O mês da emissão, `aaaa-mm-01`. */
  mes: string;
  mediana: number;
  menor: number;
  maior: number;
  /** Quantas notas entraram — o tamanho da base. */
  notas: number;
};

/** A série de preço de uma categoria de máquina (issue 70). Mês sem venda não aparece. */
export type SerieDePrecoDeMaquina = {
  categoriaCodigo: string;
  categoriaNome: string;
  /** Do mais antigo ao mais recente. */
  meses: PrecoDeMaquinaNoMes[];
  procedencia: ProcedenciaDoIndicador | null;
};

/** O custo de uma aba da série histórica da CONAB (issue 67). Nulo é "a CONAB parou no operacional". */
export type CustoNaSafra = {
  aba: string;
  safra: number;
  mesDoRelatorio: number | null;
  produtividade: number | null;
  unidadeDaProdutividade: string | null;
  custoVariavelHa: number;
  custoFixoHa: number;
  custoOperacionalHa: number;
  rendaDeFatoresHa: number | null;
  custoTotalHa: number | null;
  custoOperacionalUnidade: number;
  custoTotalUnidade: number | null;
};

/** A série de custo de uma cultura num local de referência da CONAB. */
export type SerieDeCusto = {
  cultura: string;
  local: string;
  variante: string | null;
  codigoIbge: number | null;
  unidadeComercial: string;
  /** Da safra mais antiga à mais recente. */
  safras: CustoNaSafra[];
  /** De onde esta série veio — CONAB, a localidade e a safra (issue 167). */
  procedencia: ProcedenciaDoIndicador | null;
};

/** Duas janelas de 12 meses do SICOR: a última e a anterior (issue 68). */
export type JanelasDeCredito = {
  linhas: number;
  valor: number;
  linhasAnteriores: number;
  valorAnterior: number;
};

export type CreditoNoAno = {
  ano: number;
  linhasDeMaquinas: number;
  valorDeMaquinas: number;
  linhasTotais: number;
  valorTotal: number;
  /** O último mês com dado no ano — no ano corrente, o ano ainda não fechou. */
  ultimoMes: number;
};


/**
 * O índice de crédito de um recorte (issue 73): 70% da quantidade de LINHAS do
 * SICOR e 30% do valor, comparando as duas janelas.
 *
 * LINHA NÃO É CONTRATO: o Banco Central não publica quantidade de contrato —
 * cada linha é a soma dos contratos de uma combinação. Chamar de contrato faria
 * a tela afirmar um número de produtores que a fonte não dá.
 */
export type IndiceDeCredito = {
  indice: number | null;
  faixa: FaixaDeMercado | null;
  indiceDeLinhas: number | null;
  indiceDeValor: number | null;
  linhas: number;
  linhasAnteriores: number;
  /** Valor ÷ linhas. NÃO é ticket médio por contrato. */
  valorMedioPorLinha: number | null;
  valorMedioAnterior: number | null;
  indiceDoValorMedio: number | null;
  /** Base pequena demais para o índice ser lido como tendência (D-P03). */
  basePequena: boolean;
  motivo: MotivoSemIndicador;
};

/** Por que um indicador não saiu (issue 73). */
export type MotivoSemIndicador = 'Nenhum' | 'SerieCurta' | 'SemBaseDeComparacao' | 'SemFonte' | 'SemParametro';

/** A faixa de mercado de um índice (D-P02). */
export type FaixaDeMercado = 'Retraido' | 'Intermediaria' | 'Aquecido' | 'Superaquecido';
export type CreditoPorProduto = { codigo: number; nome: string; ehMaquina: boolean; janelas: JanelasDeCredito };

export type CreditoDeMaquinasNoMunicipio = {
  codigoIbge: number;
  nome: string;
  pertenceAAdr: boolean;
  janelas: JanelasDeCredito;
  /** O índice da issue 73; nulo sem parâmetro vigente. */
  indice: IndiceDeCredito | null;
};

/**
 * A janela de comparação, vinda pronta do servidor (issue 157).
 *
 * Ela NÃO termina no último mês com dado: o Banco Central acrescenta contrato
 * registrado com atraso nos meses recentes, e comparar 12 meses cheios com 12
 * que ainda estão enchendo mostraria o crédito caindo sem ter caído.
 */
export type JanelaDoCredito = {
  /** `aaaa-mm-01` — o mês mais recente que o SICOR trouxe. */
  ultimoMesComDado: string;
  /** Quantos meses recentes ficaram de fora; zero quando a carência não foi decidida. */
  mesesDeCarencia: number;
  /** Se o parâmetro vigente traz um valor de carência (D-IM-03). */
  carenciaDecidida: boolean;
  inicio: string;
  /** O mês de corte da janela recente, já descontada a carência. */
  fim: string;
  inicioAnterior: string;
  fimAnterior: string;
  mesesPorJanela: number;
};

/** O crédito de máquinas de um recorte — a Região e São Paulo, somados no servidor. */
export type CreditoNoRecorte = {
  recorte: string;
  municipios: number;
  janelas: JanelasDeCredito;
  indice: IndiceDeCredito | null;
};

/** O crédito rural de investimento de SP, do SICOR (issue 68). */
export type PainelDeCreditoRural = {
  /** `aaaa-mm-01`; nulo quando nada foi carregado. */
  ultimoMes: string | null;
  /** O intervalo exato das duas janelas; nulo sem dado. */
  janela: JanelaDoCredito | null;
  porAno: CreditoNoAno[];
  porProduto: CreditoPorProduto[];
  porMunicipio: CreditoDeMaquinasNoMunicipio[];
  /** A Região (ADR) somada; nulo sem dado. */
  regiao: CreditoNoRecorte | null;
  /** São Paulo inteiro — o denominador da comparação. */
  saoPaulo: CreditoNoRecorte | null;
  /** De onde o crédito veio — SICOR, a janela e a ressalva do registro com atraso (issue 167). */
  procedencia: ProcedenciaDoIndicador | null;
  /**
   * O crédito de máquinas MÊS A MÊS nas duas janelas, por recorte (issue 68, 27/09/2026) — a
   * evolução com a janela recente sobre a anterior. Nulo em resposta antiga.
   */
  porMes: CreditoDeMaquinasNoMes[] | null;
};

/** As linhas e o valor de um recorte num mês. Linha do SICOR não é contrato. */
export type CreditoNoMes = { linhas: number; valor: number };

/**
 * O crédito de máquinas de um mês. A Região Tracbel é a ADR inteira; Norte e Noroeste são os municípios dela com
 * a sub-região informada — o que estiver sem sub-região conta só na Região. Com todos informados, os dois somam a
 * Região mês a mês; São Paulo é o estado inteiro.
 */
export type CreditoDeMaquinasNoMes = {
  /** `aaaa-mm-01`. */
  mes: string;
  /** Se o mês é da janela recente (e não da anterior). */
  janelaRecente: boolean;
  regiaoTracbel: CreditoNoMes;
  norte: CreditoNoMes;
  noroeste: CreditoNoMes;
  saoPaulo: CreditoNoMes;
};

/** O recorte da evolução mensal do crédito. */
export type RecorteDoCredito = 'regiaoTracbel' | 'norte' | 'noroeste' | 'saoPaulo';

/**
 * A rentabilidade de uma cultura (issue 159): receita, custo e margem por hectare.
 *
 * Cada número vem com a competência dele — o ano da PAM, quantos meses de preço
 * entraram na média e a safra do custo —, porque elas costumam ser diferentes e
 * um número que não diz de quando é não pode ser conferido.
 */
export type RentabilidadeDaCultura = {
  culturaCodigo: string;
  culturaNome: string;
  unidadeComercial: string;
  anoDaProdutividade: number | null;
  /** Quilos por hectare colhido; nula quando a PAM não permite calcular. */
  produtividadeKgPorHa: number | null;
  precoMedioPorKg: number | null;
  mesesDePrecoNaMedia: number;
  receitaPorHectare: number | null;
  localDoCusto: string | null;
  /** `Operacional` ou `Total`; nula enquanto ninguém decide (D-P07). */
  camadaDoCusto: string | null;
  safraDoCusto: number | null;
  custoPorHectare: number | null;
  margemPorHectare: number | null;
  /**
   * A mesma margem na unidade em que o mercado negocia — por saca, caixa ou
   * tonelada (issue 73). A margem por hectare responde "a terra paga a conta?";
   * esta responde "cada saca que eu vendo sobra quanto?".
   */
  margemPorUnidade: number | null;
  areaColhidaHectares: number | null;
  margemTotal: number | null;
  /** `Nenhum` quando a margem saiu. */
  motivo: string;
  /** A frase que a tela mostra no lugar do número. */
  fraseDoMotivo: string;
  /** O mesmo ano contra o anterior, os dois pelo preço recebido da PAM; nula sem o ano anterior. */
  tendencia?: TendenciaDaRentabilidade | null;
  /**
   * A safra de custo publicada antes da usada — a anterior DA SÉRIE, porque a
   * CONAB não publica todo ano em todo local — e o custo dela.
   */
  safraAnteriorDoCusto?: number | null;
  custoPorHectareDaSafraAnterior?: number | null;
  /** Da safra anterior da série para a atual, em fração. */
  variacaoDoCusto?: number | null;
};
/**
 * A RENTABILIDADE DE UM ANO CONTRA A DO ANTERIOR (28/09/2026). O preço dos dois
 * anos é o recebido pelo produtor na PAM (valor ÷ área colhida), porque a CONAB
 * guarda só 12 meses; o custo de cada ano é a safra mais recente até ele.
 */
export type TendenciaDaRentabilidade = {
  ano: number;
  anoAnterior: number;
  margemPorHectare: number | null;
  margemPorHectareAnterior: number | null;
  safraDoCusto: number | null;
  custoPorHectare: number | null;
  safraDoCustoAnterior: number | null;
  custoPorHectareAnterior: number | null;
  /** Em fração; nula sem as duas margens ou com a anterior não positiva. */
  variacaoDaMargem: number | null;
};
/** Por que o preço implícito de um ano não saiu (issue 198). */
export type MotivoSemPrecoImplicito =
  | 'Nenhum'
  | 'SemValorDaProducao'
  | 'SemQuantidadeProduzida'
  | 'SemColheita';

/** O preço implícito de um ano — `valor da produção × 1000 ÷ quantidade produzida`. */
export type PrecoImplicitoNoAno = {
  ano: number;
  /** Reais por unidade da quantidade; nulo com motivo. */
  precoPorUnidade: number | null;
  /** "toneladas", "mil frutos", "mil cachos" — vem do produto E do ano (issue 152). */
  unidade: string;
  /** O numerador e o denominador, para a conta poder ser conferida na tela. */
  valorDaProducaoMilReais: number | null;
  quantidadeProduzida: number | null;
  motivo: MotivoSemPrecoImplicito;
};

/**
 * Uma média plurianual — ela SÓ SAI com todos os anos da janela.
 *
 * Uma média de dois anos apresentada como de três é mais enganosa que ausência nenhuma, porque parece
 * completa. Faltando um ano, `preco` é nulo e `anosFaltando` diz quais.
 */
export type MediaPlurianual = { anos: number; preco: number | null; anosFaltando: number[] };

/** A série anual de um produto, com as médias. */
export type SerieDoPrecoImplicito = {
  produtoCodigoIbge: number;
  produto: string;
  unidade: string;
  /** Do ano mais recente para o mais antigo. */
  anos: PrecoImplicitoNoAno[];
  mediaDeTresAnos: MediaPlurianual;
  mediaDeCincoAnos: MediaPlurianual;
  /** Quantos municípios sustentam o ano mais recente. */
  municipiosComDadoNoUltimoAno: number;
};

/**
 * O PREÇO RECEBIDO PELO PRODUTOR, DA PAM (issue 198) — anual e por município, desde 2010.
 *
 * NÃO SE EMENDA À SÉRIE MENSAL DA CONAB: são conceitos da mesma família e não intercambiáveis. Por isso
 * este é um tipo próprio, e não um `SerieDePreco` a mais — assim ninguém concatena os dois por acidente.
 */
export type PrecoImplicitoDoRecorte = { series: SerieDoPrecoImplicito[]; ressalva: string };

// ------------------------------------------------------------------------------------------------
// Diagnóstico Comercial — o IOC por município (issue 257), `GET /api/v1/mercado/diagnostico`
// ------------------------------------------------------------------------------------------------

/** Os sete componentes do IOC, de 0 a 1 (1 é muita oportunidade); nulo é componente sem dado neste município. */
export type ComponentesDoIoc = {
  potencial: number | null;
  cobertura: number | null;
  credito: number | null;
  rentabilidade: number | null;
  clientes: number | null;
  realizacao: number | null;
  penetracao: number | null;
};

export type ClasseDePrioridade = 'Maxima' | 'Alta' | 'Moderada' | 'Baixa' | 'Manutencao';

/** Um município no diagnóstico — a linha da tabela. */
export type MunicipioNoDiagnostico = {
  codigoIbge: number;
  nome: string;
  regiao: string;
  /** O código da filial responsável — o que o filtro de loja envia. */
  lojaCodigo: string | null;
  loja: string | null;
  culturaPrincipal: string | null;
  indiceDePreco: number | null;
  indiceDeCredito: number | null;
  creditoBasePequena: boolean;
  demandaEstrutural: number | null;
  demandaAjustada: number | null;
  metaDePlanejamento: number | null;
  vendidasNoPeriodo: number | null;
  vendidasNoAno: number | null;
  clientes: number;
  /** Os mesmos clientes pela classe ABC; "sem classe" é quem não tem faturamento apurado — e não é D. */
  clientesPorClasse: ClientesPorClasse | null;
  /** Deles, os com vínculo em carteira comercial — os que têm dono no comercial. */
  clientesEmCarteira: number | null;
  /** Deles, os com faturamento no período. */
  clientesQueCompraram: number;
  vinculosComCadencia: number;
  cobertos: number;
  cobertura: number | null;
  penetracao: number | null;
  componentes: ComponentesDoIoc | null;
  ioc: number | null;
  classe: ClasseDePrioridade | null;
  situacao: string;
  planoDeAcao: string;
  componentesAusentes: string[];
  estimativa: boolean;
  /** O CEN do município: o responsável da carteira comercial com mais vínculos aqui; nulo sem carteira. */
  responsavel: string | null;
};

export type ResumoDoDiagnostico = {
  maxima: number;
  alta: number;
  moderada: number;
  baixa: number;
  manutencao: number;
  semIndice: number;
  total: number;
  iocMedio: number | null;
  /** As somas só dos municípios que têm o número; nulas quando nenhum tem. */
  demandaEstrutural: number | null;
  demandaAjustada: number | null;
  municipiosComDemanda: number;
  metaDePlanejamento: number | null;
  /** Nula sem carga do ART — ausência de carga não é venda zero. */
  vendidasNoPeriodo: number | null;
  vendidasNoAno: number | null;
  /** Vendidas no ano ÷ demanda estrutural, de 0 a 1. */
  penetracao: number | null;
  clientes: number;
  clientesQueCompraram: number;
};

export type ClientesPorClasse = { a: number; b: number; c: number; d: number; semClasse: number };

export type ShareDoDiagnostico = { categoriaCodigo: string; categoriaNome: string; percentual: number; doPrototipo: boolean };

export type DiagnosticoComercialDaRegiao = {
  competenciaInicial: string;
  competenciaFinal: string;
  fracaoDoAnoNoPeriodo: number;
  categoria: string;
  categoriaNome: string;
  categorias: { codigo: string; nome: string; ordem: number }[];
  pesos: PesosDoIocUsados | null;
  pesosVigentesDesde: string | null;
  pesosDoPrototipo: boolean;
  shares: ShareDoDiagnostico[];
  percentil90: number | null;
  resumo: ResumoDoDiagnostico;
  municipios: MunicipioNoDiagnostico[];
  lacunas: { metrica: string; motivo: string }[];
  /** Os CENs que o filtro "CEN / Gestor" oferece: os responsáveis de carteira comercial ao alcance. */
  responsaveis: ResponsavelDeCarteira[];
};

/** Os pesos que o diagnóstico usou (os mesmos nomes dos componentes). */
export type PesosDoIocUsados = {
  potencial: number;
  cobertura: number;
  credito: number;
  rentabilidade: number;
  clientes: number;
  realizacao: number;
  penetracao: number;
};

/** Os filtros do diagnóstico; ausente é o padrão do servidor (12 meses fechados, trator, a ADR inteira). */
export type FiltrosDoDiagnostico = {
  competenciaInicial?: string;
  competenciaFinal?: string;
  regiao?: string;
  /** A filial responsável pelo município, pelo código. */
  lojaCodigo?: string;
  categoria?: string;
  /** `Filial` (padrão) ou `Empresa` — o mesmo alcance dos Indicadores Geográficos. */
  visao?: 'Filial' | 'Empresa';
  /** O CEN da carteira, pelo id do responsável — o filtro "CEN / Gestor" (02/10/2026). */
  responsavel?: string;
};

// ------------------------------------------------------------------------------------------------
// Demanda e previsão — `GET /api/v1/mercado/demanda` (issue 258)
// ------------------------------------------------------------------------------------------------

export type TotaisDaDemanda = {
  parque: number | null;
  /** A renovação anual do parque — 100% do mercado. */
  demandaEstrutural: number | null;
  demandaAjustada: number | null;
  /** Demanda × share-alvo — a meta anual da Tracbel. */
  aEntregar: number | null;
  aEntregarAjustada: number | null;
  culturasComRegra: string[];
  municipios: number;
  municipiosComDemanda: number;
  estimativa: boolean;
  /** O parque com a área do ano anterior da PAM (02/10/2026). */
  parqueAnoAnterior: number | null;
  /** A demanda estrutural com a área do ano anterior. */
  demandaEstruturalAnoAnterior: number | null;
  /** As culturas cuja área útil no recorte cresceu sobre o ano anterior. */
  culturasComAumentoDeArea: string[] | null;
  /** As máquinas entregues no ano fiscal até hoje (ART, pela entrega); nulo sem ART ou com cultura escolhida. */
  entreguesNoPeriodo: number | null;
  /** O mesmo trecho do ano fiscal anterior. */
  entreguesNoPeriodoAnterior: number | null;
  /** O último dia contado nas entregas (aaaa-mm-dd). */
  entregasAte: string | null;
};

export type DemandaDaCultura = {
  culturaCodigo: string;
  cultura: string;
  areaUtilHectares: number | null;
  hectaresPorMaquina: number | null;
  anosDeRenovacao: number | null;
  parque: number | null;
  demandaEstrutural: number | null;
  demandaAjustada: number | null;
  variacaoPercentual: number | null;
  /** A área, o parque e a demanda com a PAM do ano anterior. */
  areaAnoAnterior: number | null;
  parqueAnoAnterior: number | null;
  demandaAnoAnterior: number | null;
};

/** Um mês da previsão; `mes` é o do calendário (1 a 12), e a lista vem de novembro a outubro. */
export type PrevisaoDoMes = {
  mes: number;
  fracao: number;
  demandaEstrutural: number | null;
  demandaAjustada: number | null;
  aEntregar: number | null;
  aEntregarAjustada: number | null;
  /** As máquinas entregues no mês do ano fiscal escolhido (ART); nulo no mês que não começou, sem ART ou com cultura. */
  entregues: number | null;
};

export type EntregaDaLoja = { lojaCodigo: string | null; loja: string; aEntregarNoAno: number | null; porMes: (number | null)[] };

export type DemandaDoMunicipioNaPrevisao = {
  codigoIbge: number;
  nome: string;
  regiao: string;
  lojaCodigo: string | null;
  loja: string | null;
  areaUtilHectares: number | null;
  parque: number | null;
  porCultura: { culturaCodigo: string; demanda: number }[];
  demandaEstrutural: number | null;
  demandaAjustada: number | null;
  aEntregar: number | null;
  aEntregarAjustada: number | null;
  fatorDePreco: number | null;
  fatorDeCredito: number | null;
  culturaPredominante: string | null;
  variacaoPercentual: number | null;
  /** A demanda estrutural com a área do ano anterior da PAM, e a variação de hoje sobre ela (%). */
  demandaEstruturalAnoAnterior: number | null;
  variacaoAnoAnterior: number | null;
};

export type DemandaEPrevisaoDaRegiao = {
  categoria: string;
  categoriaNome: string;
  categorias: { codigo: string; nome: string; ordem: number }[];
  shareAlvo: number | null;
  shareDoPrototipo: boolean;
  sazonalidadeVigenteDesde: string | null;
  sazonalidadeDoPrototipo: boolean;
  anoDaAreaPlantada: number | null;
  totais: TotaisDaDemanda;
  porCultura: DemandaDaCultura[];
  previsaoMensal: PrevisaoDoMes[];
  porLoja: EntregaDaLoja[];
  municipios: DemandaDoMunicipioNaPrevisao[];
  culturas: { codigo: string; nome: string; demanda: number | null }[];
  lacunas: { metrica: string; motivo: string }[];
  /** O ano fiscal das entregas (2026 é nov/2025 a out/2026) e os que o filtro "Período" oferece. */
  anoFiscal: number;
  anosFiscais: number[];
  /** A cultura que recorta a demanda; nulo é todas. */
  cultura: string | null;
  /** As culturas com demanda na categoria e no recorte — o filtro "Cultura". */
  culturasDoFiltro: { codigo: string; nome: string; demanda: number | null }[];
  /** O ano da PAM da comparação. */
  anoDaAreaAnterior: number | null;
};

export type FiltrosDaDemanda = { regiao?: string; lojaCodigo?: string; categoria?: string; anoFiscal?: number; cultura?: string };

// ------------------------------------------------------------------------------------------------
// Dimensionamento da ADR — `GET /api/v1/mercado/dimensionamento` (issue 259)
// ------------------------------------------------------------------------------------------------

/** A parte, o todo e quanto a parte é do todo, em %; nulo sem os dois ou com o todo zero. */
export type Fatia = { parte: number | null; todo: number | null; percentual: number | null };

/** Uma medida no ano-base e no anterior; a variação já vem em %. */
export type MedidaComAnterior = { atual: number | null; anterior: number | null; variacaoPercentual: number | null };

/** As faixas ACUMULADAS de dias desde o último contato: quem foi contatado há 20 dias está nas quatro. */
export type FaixasDoUltimoContato = { ate30: number; ate60: number; ate90: number; ate120: number; sem120: number };

export type ClientesDaCarteira = {
  clientes: number;
  a: number;
  b: number;
  c: number;
  d: number;
  /** O cliente sem faturamento apurado — contado à parte, e não como D. */
  semClasse: number;
  faixas: FaixasDoUltimoContato;
  /** A e B sem contato nos últimos 120 dias. */
  abSemContato: number;
  ultimoContatoEm: string | null;
};

export type CulturaDoDimensionamento = { codigo: string; nome: string };

export type TotaisDaProducao = {
  /** Hectares plantados. */
  area: MedidaComAnterior;
  /** Toneladas — só os produtos publicados em toneladas. */
  quantidade: MedidaComAnterior;
  /** Mil reais. */
  valor: MedidaComAnterior;
  /** Reais por hectare plantado. */
  densidade: MedidaComAnterior;
  /** Com uma cultura, toneladas por hectare colhido. */
  produtividade: number | null;
  municipiosComProducao: number;
};

export type MunicipioNoMapaDoDimensionamento = {
  codigoIbge: number;
  nome: string;
  emFoco: boolean;
  area: number | null;
  quantidade: number | null;
  valor: number | null;
  clientes: number | null;
};

export type CulturaTracbelNoEstado = {
  codigo: string;
  nome: string;
  area: Fatia;
  quantidade: Fatia;
  valor: Fatia;
  fatiaDaLojaNaTracbel: number | null;
};

export type PerfilDaLoja = {
  lojaCodigo: string | null;
  loja: string;
  municipios: number;
  area: number | null;
  valor: number | null;
  reaisPorHectare: number | null;
  /** A produtividade da loja em % da média do recorte. */
  tecnificacao: number | null;
  culturaDominante: string | null;
};

export type MunicipioPrioritario = {
  codigoIbge: number;
  nome: string;
  valor: number | null;
  clientes: number;
  coberturaAte90Percentual: number;
  lacunaPercentual: number;
  /** Valor × lacuna: o valor de produção sem cobertura, em mil reais. */
  prioridade: number | null;
};

export type LinhaDaMatrizMunicipal = {
  codigoIbge: number;
  nome: string;
  regiao: string | null;
  lojaCodigo: string | null;
  loja: string | null;
  vendedor: string | null;
  culturaPrincipal: string | null;
  area: MedidaComAnterior;
  quantidade: MedidaComAnterior;
  valor: MedidaComAnterior;
  fatiaNaRegiao: number | null;
  fatiaNoEstado: number | null;
  clientes: ClientesDaCarteira;
  usinas: number;
};

export type CarteiraDoResponsavelNoDimensionamento = {
  responsavelId: number;
  nome: string;
  natureza: string;
  naRegiao: number;
  fora: number;
  clientes: ClientesDaCarteira;
};

export type DimensionamentoDaAdr = {
  anoBase: number | null;
  anoAnterior: number | null;
  anosDisponiveis: number[];
  cultura: string | null;
  culturaNome: string;
  culturas: CulturaDoDimensionamento[];
  lojas: { codigo: string; nome: string; municipios: number }[];
  responsaveis: ResponsavelDeCarteira[];
  recorte: string;
  municipiosNoRecorte: number;
  tracbelNoEstado: { area: Fatia; quantidade: Fatia; valor: Fatia };
  representatividade: { lojaCodigo: string; loja: string; area: Fatia; quantidade: Fatia; valor: Fatia } | null;
  totais: TotaisDaProducao;
  momento: { indice: number | null; faixa: string | null; motivo: string; ultimoMes: string | null } | null;
  mapa: MunicipioNoMapaDoDimensionamento[];
  porCultura: CulturaTracbelNoEstado[];
  porLoja: PerfilDaLoja[];
  carteira: { clientes: ClientesDaCarteira; naRegiao: number; fora: number | null; potencialMedio: number | null };
  prioritarios: MunicipioPrioritario[];
  matriz: LinhaDaMatrizMunicipal[];
  porVendedor: CarteiraDoResponsavelNoDimensionamento[];
  lacunas: { metrica: string; motivo: string }[];
};

/** Os filtros do Dimensionamento; ausente é o padrão do servidor (o ano mais novo da PAM, todas as culturas, a ADR). */
export type FiltrosDoDimensionamento = {
  anoBase?: string;
  cultura?: string;
  regiao?: string;
  lojaCodigo?: string;
  visao?: 'Filial' | 'Empresa';
  responsavel?: string;
  classe?: string;
  /** `com` ou `sem` usina de etanol. */
  usina?: string;
};

// ------------------------------------------------------------------------------------------------
// Gestão de Financiamentos — `GET /api/v1/mercado/financiamentos` (issue 261)
// ------------------------------------------------------------------------------------------------

/** Um código do catálogo do SICOR com o nome do Banco Central. */
export type OpcaoDoSicor = { codigo: number; nome: string };

/** As linhas e o valor de máquina num mês. Linha do SICOR não é contrato. */
export type LinhasDoSicorNoMes = { mes: string; linhas: number; valor: number };

export type TotaisDoCredito = {
  linhas: number;
  valor: number;
  /** Nulo quando o mesmo período do ano anterior começa antes do SICOR. */
  linhasAnteriores: number | null;
  valorAnterior: number | null;
  indice: IndiceDeCredito | null;
};

export type MomentoDoCredito = { meses: number; janelas: JanelasDeCredito | null; indice: IndiceDeCredito | null };

export type RecorteNoEstado = {
  linhas: number;
  linhasDoEstado: number;
  fatiaDasLinhas: number | null;
  valor: number;
  valorDoEstado: number;
  fatiaDoValor: number | null;
};

export type SituacoesDoCredito = {
  retraidos: number;
  intermediarios: number;
  aquecidos: number;
  superaquecidos: number;
  /** Sem crédito no mesmo período do ano anterior — sem faixa. */
  semBase: number;
  basePequena: number;
};

export type CreditoDaLoja = { lojaCodigo: string; loja: string; municipios: number; janelas: JanelasDeCredito; indice: IndiceDeCredito | null };

export type CreditoDoMunicipio = {
  codigoIbge: number;
  nome: string;
  pertenceAAdr: boolean;
  lojaCodigo: string | null;
  loja: string | null;
  janelas: JanelasDeCredito;
  /** Quanto do valor de São Paulo no período é deste município, em %. */
  fatiaNoEstado: number | null;
  /** Quantos pontos a fatia mudou contra o ano anterior. */
  variacaoDaFatia: number | null;
  indice: IndiceDeCredito | null;
};

export type FinanciamentosDoSicor = {
  primeiroMesDoSicor: string | null;
  ultimoMesDoSicor: string | null;
  mesesDeCarencia: number;
  carenciaDecidida: boolean;
  de: string | null;
  ate: string | null;
  meses: number;
  anteriorDe: string | null;
  anteriorAte: string | null;
  produto: number | null;
  programa: number | null;
  produtos: OpcaoDoSicor[];
  programas: OpcaoDoSicor[];
  recorte: string;
  recorteNome: string;
  municipiosNoRecorte: number;
  lojas: { codigo: string; nome: string; municipios: number }[];
  totais: TotaisDoCredito;
  momento: MomentoDoCredito[];
  serie: LinhasDoSicorNoMes[];
  noEstado: RecorteNoEstado | null;
  situacoes: SituacoesDoCredito;
  porLoja: CreditoDaLoja[];
  municipios: CreditoDoMunicipio[];
  lacunas: { metrica: string; motivo: string }[];
};

/** Os filtros dos Financiamentos; ausente é o padrão do servidor (12 meses fechados, os três produtos, a Região). */
export type FiltrosDosFinanciamentos = {
  /** aaaa-mm */
  de?: string;
  ate?: string;
  produto?: string;
  programa?: string;
  /** `adr`, `norte`, `noroeste`, `sp` ou `fora`. */
  recorte?: string;
  lojaCodigo?: string;
  usina?: string;
};

/**
 * O SHARE DA TRACBEL NO CRÉDITO DE MECANIZAÇÃO (issue 262, decisões de 28/09/2026): o crédito rural que a Tracbel
 * financiou (formulários da venda do Vórtice, sem recurso próprio e sem consórcio) dividido pelo crédito de máquinas
 * do SICOR, por filial e na Região, na janela do painel do crédito. Estimativa.
 */
export interface ShareNoRecorte {
  valorTracbel: number;
  financiamentos: number;
  valorSicor: number;
  linhasSicor: number;
  /** De 0 a 1 (pode passar de 1 no município); nulo quando o SICOR não tem crédito. */
  share: number | null;
  valorDaConcorrencia: number;
}

export interface ShareDaFilial {
  empresaId: number;
  filial: string;
  municipios: number;
  share: ShareNoRecorte;
}

export interface ShareNoMunicipio {
  codigoIbge: number;
  nome: string;
  filial: string | null;
  share: ShareNoRecorte;
  /** A Tracbel financiou mais que o SICOR registrou: o limite da leitura por município. */
  acimaDoSicor: boolean;
}

export interface FinanciadoPorLinha {
  linha: string;
  contaNoShare: boolean;
  financiamentos: number;
  valor: number;
}

export interface ShareNoMes {
  mes: string;
  valorTracbel: number;
  valorSicor: number;
}

export interface FinanciadoForaDaRegiao {
  semMunicipio: number;
  valorSemMunicipio: number;
  foraDaAdr: number;
  valorForaDaAdr: number;
}

export interface ShareNoCreditoDeMecanizacao {
  inicio: string | null;
  fim: string | null;
  ultimoPedido: string | null;
  regiao: ShareNoRecorte | null;
  porFilial: ShareDaFilial[];
  porMunicipio: ShareNoMunicipio[];
  porLinha: FinanciadoPorLinha[];
  porMes: ShareNoMes[];
  foraDaRegiao: FinanciadoForaDaRegiao;
  procedencia: ProcedenciaDoIndicador | null;
}

// ------------------------------------------------------------------------------------------------
// Preço de commodities — `GET /api/v1/mercado/precos` (issue 260)
// ------------------------------------------------------------------------------------------------

/** Um índice de momento de preço: a média recente sobre a anterior; 1,10 é 10% acima. */
export type IndiceDeMomento = {
  indice: number | null;
  faixa: FaixaDeMercado | null;
  mediaRecente: number | null;
  mediaAnterior: number | null;
  mesesRecentes: number;
  mesesAnteriores: number;
  motivo: MotivoSemIndicador;
  /** `Mensal`, ou `AnualPam` quando o R12 é o preço anual da PAM. */
  serie: string;
  anoRecente: number | null;
};

export type HorizonteDoPreco = { meses: number; indice: IndiceDeMomento };

export type PrecoDaCultura = {
  codigo: string;
  nome: string;
  fonte: string;
  unidade: string;
  ultimoMes: string | null;
  ultimoValor: number | null;
  /** Mês, R3, R6 e R12. */
  horizontes: HorizonteDoPreco[];
  serie: { mes: string; valor: number }[];
};

export type PrecosDasCulturas = {
  ultimoMesDePreco: string | null;
  culturas: PrecoDaCultura[];
  lacunas: { metrica: string; motivo: string }[];
};

// ------------------------------------------------------------------------------------------------
// Cenários de mercado — `GET /api/v1/mercado/cenarios` e `PUT /api/v1/mercado/cenarios/{codigoIbge}` (issue 263)
// ------------------------------------------------------------------------------------------------

export type CenarioDeMercado = 'Conservador' | 'Moderado' | 'Otimista' | 'Manual';

/** A meta gravada de um município: o cenário, o número combinado, e o mesmo cenário com o mercado de hoje. */
export type EscolhaDoCenario = {
  cenario: CenarioDeMercado;
  valorManual: number | null;
  metaCombinada: number;
  /** O mesmo cenário com o mercado de hoje; nulo no manual. */
  metaDoCenarioHoje: number | null;
  gravadaPor: string | null;
  gravadaEm: string;
};

export type CenarioDoMunicipio = {
  codigoIbge: number;
  nome: string;
  regiao: string;
  lojaCodigo: string | null;
  loja: string | null;
  culturaPrincipal: string | null;
  /** O quanto o preço das culturas move o mercado do município, em % (o fator de ciclo). */
  efeitoDoPreco: number | null;
  efeitoDoCredito: number | null;
  realizadoNoAno: number | null;
  realizadoNoIntervalo: number | null;
  realizadoNoAnoAnterior: number | null;
  mediaDosAnosAnteriores: number | null;
  clientes: number | null;
  potencial: number | null;
  mercadoAjustado: number | null;
  metaEstrutural: number | null;
  shareEstrutural: number | null;
  shareAjustado: number | null;
  conservador: number | null;
  moderado: number | null;
  otimista: number | null;
  /** Se os cenários partem do mercado ajustado; falso é a meta estrutural (fator sem dado). */
  baseAjustada: boolean;
  escolha: EscolhaDoCenario | null;
  aEntregar: number | null;
  enquadramento: string | null;
  diferencaParaOModerado: number | null;
  recomendacao: 'Gradual' | 'SemHistorico' | 'Atingivel' | null;
  acimaDoRealizado: number | null;
  metaGradual: number | null;
  gravavel: boolean;
};

export type TotaisDosCenarios = {
  realizadoNoAno: number | null;
  realizadoNoAnoAnterior: number | null;
  mediaDosAnosAnteriores: number | null;
  realizadoNoIntervalo: number | null;
  clientes: number | null;
  potencial: number | null;
  mercadoAjustado: number | null;
  metaEstrutural: number | null;
  realizacaoSobrePotencial: number | null;
  aEntregar: number | null;
  municipiosComMeta: number;
  municipios: number;
  entregasAte: string | null;
};

export type CenariosDeMercado = {
  categoria: string;
  categoriaNome: string;
  categorias: { codigo: string; nome: string; ordem: number }[];
  anoFiscal: number;
  anosFiscais: number[];
  anosDaMedia: number[];
  situacaoDoAno: 'Corrente' | 'Fechado' | 'Futuro';
  intervaloDe: string;
  intervaloAte: string;
  shareAlvo: number | null;
  shareDoPrototipo: boolean;
  podeGravar: boolean;
  porQueNaoGrava: string | null;
  alcanceDaGravacao: string | null;
  totais: TotaisDosCenarios;
  municipios: CenarioDoMunicipio[];
  lacunas: { metrica: string; motivo: string }[];
};

export type FiltrosDosCenarios = {
  categoria?: string;
  anoFiscal?: number;
  de?: string;
  ate?: string;
  regiao?: string;
  lojaCodigo?: string;
};

/** O corpo da escolha — o número dos três cenários é calculado no servidor; só o manual leva `valorManual`. */
export type EscolhaDeMetaDoCenario = {
  categoria: string;
  anoFiscal: string;
  cenario: CenarioDeMercado;
  valorManual?: string;
};