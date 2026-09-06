/**
 * Tipos da tela Pipeline de Vendas — espelham `PIPELINE_MOCK` (linha 4081) e
 * `PIPELINE_STATE` (linha 4130) de `prototipo/referencia/assets/app.js`.
 * Reaproveita `Classe`/`CenInfo`/`MapaCens` (já usados por Clientes) e
 * `FasePipeline` (já usado por Nova Oportunidade) em vez de redefinir.
 */
import type { Classe } from './clientes';

export type { CenInfo, MapaCens } from './clientes';
export type { FasePipeline } from './oportunidade';

export type ResultadoOportunidade = 'ganho' | 'perdido';

/** Espelha `op.aprovacao_travada` — bloqueio de avanço até aprovação. */
export type AprovacaoTravada = { por: string; motivo: string; dias: number };

/** Item de `PIPELINE_MOCK` (`pipeline.json`) — um card do kanban. */
export type OportunidadePipeline = {
  id: string;
  titulo: string;
  cliente: string;
  classe: Classe;
  cidade: string;
  cen: string;
  linha: string;
  modelo: string;
  valor: number;
  probabilidade: number;
  previsao: string;
  dias_fase: number;
  fase: string;
  /** Só existe na oportunidade de exemplo ligada à Ficha de Oportunidade. */
  ficha_id?: number;
  aprovacao_travada?: AprovacaoTravada;
  resultado?: ResultadoOportunidade;
  motivo_perda?: string;
  fechado_em?: string;
  /** Oportunidade criada nesta sessão via Nova Oportunidade — mostra o badge "NOVA". */
  criada_agora?: boolean;
};

export type PersonaPipeline = 'cen' | 'regional' | 'nacional';

/** Mapa de emoji por linha de produto (`linha-icon.json`, `LINHA_ICON`). */
export type MapaLinhaIcon = Record<string, string>;
