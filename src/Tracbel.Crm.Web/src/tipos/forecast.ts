/**
 * O forecast da gerência (28/09/2026), como `GET /api/v1/relatorios/forecast` devolve. Espelha `RelatorioDoForecast`
 * (`Aplicacao/Relacionamento/ObterForecastDaGerencia.cs`) em camelCase.
 *
 * O FORECAST E O BEST GUESS são a previsão de cada gestor, lida da API Gestão de Negócios (o Forecast é revisto uma vez
 * por mês; o Best Guess, toda segunda). O PO é a meta do time do gestor; o REALIZADO, as máquinas vendidas pelo time (o
 * ART). Tudo em unidades.
 */

import type { MetricaSemDado } from './relacionamento';

/** Uma linha de produto de um gestor — ou do total. */
export type LinhaDoForecast = {
  codigo: string;
  nome: string;
  /** O PO: a meta de máquinas do time na linha. */
  meta: number;
  /** Nulo quando o gestor não informou — e não zero. */
  forecast: number | null;
  /** Nulo quando o gestor não informou. */
  bestGuess: number | null;
  realizado: number;
};

export type ForecastDoGestor = {
  gestor: string;
  /** Quantos consultores o de-para põe no time dele. */
  consultores: number;
  linhas: LinhaDoForecast[];
};

/** `Organizacao`: "Todas as filiais" no seletor. `Filiais`: o PO e o realizado são só da filial escolhida. */
export type AlcanceDoForecast = 'Organizacao' | 'Filiais';

export type RelatorioDoForecast = {
  competencia: string;
  /** "set/2026". */
  texto: string;
  /** Os meses que têm forecast — é entre eles que a tela troca. */
  mesesDisponiveis: string[];
  alcance: AlcanceDoForecast;
  gestores: ForecastDoGestor[];
  total: LinhaDoForecast[];
  /** As vendas do mês cujo vendedor não está no de-para: entram no total e em gestor nenhum. */
  vendasSemGestor: number;
  lidoEm: string | null;
  geradoNaOrigemEm: string | null;
  metricasSemDado: MetricaSemDado[];
};
