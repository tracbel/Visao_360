/**
 * As cores das séries do gráfico combinado da Rentabilidade — as mesmas na
 * legenda (HTML) e no desenho (canvas). O canvas não enxerga `var(--…)`, e por
 * isso elas são escritas aqui, uma vez, para as duas pontas lerem da mesma.
 */
export const CORES_DA_RENTABILIDADE = {
  receita: '#0B5D2A',
  custo: '#A5D6A7',
  margem: '#2563EB',
  margemPercentual: '#8B5CF6',
} as const;
