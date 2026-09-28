/**
 * O forecast da gerência (28/09/2026) — `GET /api/v1/relatorios/forecast`. Uma leitura só: a API soma o PO e o realizado
 * pela filial do cabeçalho (ou por todas, em "Todas as filiais"), e o forecast é do gestor inteiro.
 */

import type { ComProcedencia } from '../../tipos/api';
import type { RelatorioDoForecast } from '../../tipos/forecast';
import { ler, type ContextoDeAcesso } from './http';

/** O forecast de um mês (`AAAA-MM`); sem mês, a API abre o corrente — ou o mais recente com forecast. */
export function obterForecastDaGerencia(
  contexto: ContextoDeAcesso,
  competencia: string | undefined,
  sinal?: AbortSignal,
): Promise<ComProcedencia<RelatorioDoForecast>> {
  return ler<RelatorioDoForecast>('/v1/relatorios/forecast', contexto, { sinal, parametros: { competencia } });
}
