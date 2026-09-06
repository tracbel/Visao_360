/**
 * Tipos do relatório Funil de Vendas — espelham `FUNIL_DATA` do protótipo de
 * referência (`prototipo/referencia/assets/app.js`, linha 281 / `funil.json`).
 */

/** Um estágio do funil (faixa do gráfico + linha de KPI). */
export type EstagioFunil = {
  key: string;
  label: string;
  qty: number;
  value: number;
  opps: number;
  color: string;
};

/** Linha da tabela de detalhamento por oportunidade. */
export type LinhaDetalhamentoFunil = {
  proprietario: string;
  fase: string;
  linha: string;
  oportunidade: string;
  conta: string;
  modelo: string;
  valor: number;
  qtd: number;
};

export type TotaisFunil = {
  registros: number;
  valor: number;
  quantidade: number;
  propostas: number;
  pedidosAlocados: number;
  pedidos: number;
  faturamento: number;
};

export type FunilData = {
  stages: EstagioFunil[];
  totals: TotaisFunil;
  detalhamento: LinhaDetalhamentoFunil[];
};

/** Métrica exibida no funil e na legenda (segmented control). */
export type MetricaFunil = 'qty' | 'value' | 'opps';
