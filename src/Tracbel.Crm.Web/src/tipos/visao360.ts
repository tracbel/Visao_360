/**
 * Tipos dos dados usados pela Visão 360 — espelham os JSONs extraídos do
 * protótipo (`public/dados/*.json`) e as constantes correspondentes em
 * `prototipo/referencia/assets/app.js` (linhas 7203-7302).
 */

export type PerfilId360 = 'cen' | 'gerente' | 'diretor';

/** `perfis-360.json` (PERFIS_360, app.js:7273). */
export interface Perfil360 {
  id: PerfilId360;
  nome: string;
  cargo: string;
  avatar: string;
  cor: string;
  escopo_label: string;
}
export type Perfis360 = Record<PerfilId360, Perfil360>;

/** `faturamento-12m.json` (FATURAMENTO_12M, app.js:7222). Valores em R$ milhões. */
export interface Faturamento12Meses {
  labels: string[];
  realizado_global: number[];
  previsto_global: number[];
  meta_global: number[];
}

/** `mix-linhas.json` (MIX_LINHAS, app.js:7233). */
export interface MixLinha {
  linha: string;
  pct: number;
  valor: number;
  cor: string;
}

/** `top-clientes.json` (TOP_CLIENTES_GLOBAL, app.js:7242) — usado para os perfis gerente/diretor. */
export interface TopClienteGlobal {
  razao: string;
  fat_fytd: number;
  cidade: string;
  cen: string;
}

export type TipoAlerta360 = 'critico' | 'aviso' | 'info';

/** `alertas.json` (ALERTAS_POR_PERFIL, app.js:7254). */
export interface AlertaGerencialDado {
  tipo: TipoAlerta360;
  titulo: string;
  detalhe: string;
  acao: string;
}
export type AlertasPorPerfil360 = Record<PerfilId360, AlertaGerencialDado[]>;

/* `mercado-pracas.json` SAIU (issue 169).
 *
 * Eram quatro praças fictícias — MT Norte, MT Sul, GO e BA —, com "total de
 * mercado", "vendemos" e "indicamos perdida" inventados, e elas continuavam no
 * pacote publicado mesmo sem nada carregá-las. A Inteligência de Mercado real
 * responde essas perguntas com dado do IBGE e do CRM, por município de São
 * Paulo; deixar as praças ali era manter no ar um número que ninguém mediu, em
 * estados onde a Tracbel Agro não atua. */

/** `vendas-perdidas-motivos.json` (VENDAS_PERDIDAS_MOTIVOS, app.js:7211). */
export interface VendaPerdidaMotivo {
  motivo: string;
  qtd: number;
  valor: number;
  cor: string;
}

/**
 * `performance-cens.json` (PERF_CENS, app.js:5945) — usado pelo Top CENs.
 * Observação: o campo `avatar` aqui é uma cor hex (não um rótulo); as iniciais
 * ficam em `foto`. Nomenclatura herdada do protótipo original.
 */
export interface PerfilCen {
  id: string;
  nome: string;
  regional: string;
  foto: string;
  avatar: string;
  meta_mes: number;
  meta_fytd: number;
  admissao: string;
}

/** Última interação registrada com o cliente (dentro de `ClienteCarteira.ult_int`). */
export interface UltimaInteracao {
  data: string;
  cat: string;
  autor: string;
}

/** `carteira-cen.json` (CARTEIRA_CEN, app.js:3389) — carteira do CEN "cen" (João Ribeiro). */
export interface ClienteCarteira {
  id: number;
  razao: string;
  apelido: string;
  classe: 'A' | 'B' | 'C' | 'D';
  cidade: string;
  uf: string;
  lat: number;
  lng: number;
  exige_visita: boolean;
  ult_int: UltimaInteracao | null;
  fat_12m: number;
  oportunidades: number;
  oportunidades_valor?: number;
  obs: string;
}

/** `meta-frequencia.json` (META_FREQ, app.js:3376) — dias-meta de contato por classe. */
export type MetaFrequencia = Record<ClienteCarteira['classe'], number>;

/**
 * `constantes-escalares.json` — subconjunto de escalares soltos do app.js.
 * Só os campos usados pela Visão 360 estão tipados com precisão.
 */
export interface ConstantesEscalares {
  NOVAS_STORAGE_KEY: { valor: string };
  CONTADOR_STORAGE_KEY: { valor: string };
  AGENDA_HOJE: { valor: string };
  /** Data "hoje" congelada usada nos cálculos de cobertura da carteira (app.js:3373). */
  HOJE_CARTEIRA: { valor: string };
  /** Multiplicador do faturamento global visto pelo Gerente Regional MT Norte (app.js:7229). */
  FAT_MULT_MT_NORTE: { valor: number };
  /** Multiplicador do faturamento global visto pelo CEN João Ribeiro (app.js:7230). */
  FAT_MULT_CEN_JOAO: { valor: number };
}

/** Estado de drill-down do dashboard (filial → CEN → cliente; só "filial" é usado hoje). */
export type Drill360 = { nivel: 'filial'; valor: string };

export type StatusCobertura = 'em_dia' | 'aviso' | 'atraso' | 'critico' | 'nunca';
