/**
 * Tipos da tela Performance de CEN — espelham as estruturas `PERF_*` de
 * `prototipo/referencia/assets/app.js` (linhas 5919-6570) e o formato dos
 * JSONs em `public/dados/{performance-cens,performance-series,fytd-meses}.json`.
 */

export type PersonaPerf = 'cen' | 'regional' | 'nacional';

export type MetricaBreakdown =
  | 'vendas_fyd'
  | 'atingimento'
  | 'pipeline'
  | 'conversao'
  | 'cobertura'
  | 'ticket';

/** `FYTD_MESES` — calendário fiscal Tracbel nov-out (fytd-meses.json). */
export type FytdMes = {
  key: string;
  label: string;
  dias_uteis: number;
  parcial?: boolean;
};

/** `PERF_CENS` — CENs com metas para o painel (performance-cens.json). */
export type PerfCen = {
  id: string;
  nome: string;
  regional: string;
  foto: string;
  avatar: string;
  meta_mes: number;
  meta_fytd: number;
  admissao: string;
};

/** Um mês da série de um CEN, já materializada em performance-series.json (gerada por `gerarSerieCEN`/`mulberry32` no protótipo). */
export type PerfSerieMes = {
  mes: string;
  label: string;
  parcial: boolean;
  vendas: number;
  pipeline_criado: number;
  oport_ganhas: number;
  oport_perdidas: number;
  oport_totais: number;
  conversao: number;
  visitas: number;
  ticket_medio: number;
  cobertura: number;
};

/** `PERF_SERIES` — mapa id do CEN -> série mensal FYTD (performance-series.json). */
export type PerfSeries = Record<string, PerfSerieMes[]>;

/** Retorno de `calcularPerfCEN` (perf-block.js linha 108). */
export type PerfDetalheCen = {
  cen: PerfCen;
  serie: PerfSerieMes[];
  mes_atual: PerfSerieMes;
  mes_vendas: number;
  mes_meta: number;
  mes_atingimento: number;
  fytd_vendas: number;
  fytd_meta: number;
  fytd_atingimento: number;
  pipeline_aberto: number;
  conversao_fytd: number;
  cobertura: number;
  ticket_medio_fytd: number;
  fytd_visitas: number;
  fytd_ganhas: number;
  fytd_totais: number;
};

/** Retorno de `calcularPerfGrupo` (perf-block.js linha 144). */
export type PerfGrupo = {
  detalhes: PerfDetalheCen[];
  mes_vendas: number;
  mes_meta: number;
  mes_atingimento: number;
  fytd_vendas: number;
  fytd_meta: number;
  fytd_atingimento: number;
  pipeline_aberto: number;
  conversao_fytd: number;
  cobertura: number;
  ticket_medio_fytd: number;
  n_cens: number;
};

/** Campos comuns entre `PerfDetalheCen` e `PerfGrupo` usados pelos cartões de KPI. */
export type PerfResumo = {
  mes_vendas: number;
  mes_meta: number;
  mes_atingimento: number;
  fytd_vendas: number;
  fytd_meta: number;
  fytd_atingimento: number;
  pipeline_aberto: number;
  conversao_fytd: number;
  cobertura: number;
  ticket_medio_fytd: number;
};
