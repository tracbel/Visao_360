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

export type CreditoPorProduto = { codigo: number; nome: string; ehMaquina: boolean; janelas: JanelasDeCredito };

export type CreditoDeMaquinasNoMunicipio = { codigoIbge: number; nome: string; pertenceAAdr: boolean; janelas: JanelasDeCredito };

/** O crédito rural de investimento de SP, do SICOR (issue 68). */
export type PainelDeCreditoRural = {
  /** `aaaa-mm-01`; nulo quando nada foi carregado. */
  ultimoMes: string | null;
  porAno: CreditoNoAno[];
  porProduto: CreditoPorProduto[];
  porMunicipio: CreditoDeMaquinasNoMunicipio[];
};
