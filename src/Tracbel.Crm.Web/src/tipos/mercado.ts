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
};

export type PrecosDeMercado = {
  series: SerieDePreco[];
  primeiroMesDoDolar: string | null;
  ultimoMesDoDolar: string | null;
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
};

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
};