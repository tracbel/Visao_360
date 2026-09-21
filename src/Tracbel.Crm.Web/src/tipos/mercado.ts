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
