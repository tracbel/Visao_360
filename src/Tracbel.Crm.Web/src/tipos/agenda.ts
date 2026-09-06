/**
 * Tipos da tela Agenda do CEN — espelham `AGENDA_DATA` (linha 1125) e
 * `TIPO_META` (linha 1173) de `prototipo/referencia/assets/app.js`.
 */
import type { Classe } from './clientes';

export type TipoTarefa = 'monitorar' | 'visitar' | 'ligar' | 'proposta';
export type OrigemTarefa = 'auto' | 'manual';

/** Item de `AGENDA_DATA.tarefas` (`agenda.json`). */
export type Tarefa = {
  id: number;
  data: string;
  tipo: TipoTarefa;
  titulo: string;
  cliente: string;
  cliente_id: number;
  cidade: string;
  classe: Classe;
  prioridade: 0 | 1 | 2;
  origem: OrigemTarefa;
  agendado_por: string;
  ultima_interacao: string;
  obs?: string;
};

export type CenAgendaAtual = { user: string; nome: string; regional: string; avatar: string };
export type CenAgendaDisponivel = { user: string; nome: string; regional: string };

/** `AGENDA_DATA` completo (`agenda.json`). */
export type AgendaData = {
  cen_atual: CenAgendaAtual;
  cens_disponiveis: CenAgendaDisponivel[];
  tarefas: Tarefa[];
};

/** Nome do ícone usado por `iconSvg()` (app.js:1209) — um por tipo de tarefa. */
export type NomeIconeTarefa = 'eye' | 'nav' | 'phone' | 'file';

/** Item de `TIPO_META` (`tipo-meta.json`). */
export type MetaTipoTarefa = { label: string; icon: NomeIconeTarefa; color: string; bg: string };
export type MapaTipoMeta = Record<TipoTarefa, MetaTipoTarefa>;

/** Grupo de agrupamento por data — espelha `classifyByDate()` (app.js:1180). */
export type GrupoAgendaChave = 'atrasada' | 'hoje' | 'amanha' | 'semana' | 'futura';

/** Visão ativa da tela — espelha o `data-view` do `.view-toggle` (app.js:1243). */
export type VisaoAgenda = 'lista' | 'semana';

/* ------------------------------------------------------------------------ */
/* Andamento da tarefa — o que a tela de conclusão grava                      */
/* ------------------------------------------------------------------------ */

/**
 * Um andamento gravado ao concluir uma tarefa. Espelha o que a tela
 * `IVS7AGE02_AndamentoAgendaTab` do Vórtice grava em `IV_Historico` na mesma
 * transação em que fecha a agenda (pesquisa 16 §5.1): quem, quando, por qual
 * canal, com que desfecho e o que nasce em seguida.
 *
 * Os três campos de código (`categoria`, `resultado`, `passo_seguinte`) vêm de
 * catálogo — nunca de digitação (documento 05 §2). Só `observacao` é texto
 * livre, a exceção justificada.
 */
export type AndamentoTarefa = {
  tarefa_id: number;
  cliente_id: number;
  cliente: string;
  titulo: string;
  /** Data e hora em que o contato aconteceu, no formato do `datetime-local`. */
  data_contato: string;
  /** Código de `catalogo-categoria-interacao.json`. */
  categoria: string;
  /** Código de `catalogo-resultado.json`. */
  resultado: string;
  /** Código de `catalogo-passo-seguinte.json`. */
  passo_seguinte: string;
  observacao: string;
  /** Instante da gravação (ISO), equivalente a `IV_Historico.DtaRealizacao`. */
  concluida_em: string;
};

/** Um reagendamento gravado — equivale a mexer em `IV_Agenda.DtaAgenda`. */
export type ReagendamentoTarefa = {
  tarefa_id: number;
  /** Data original da tarefa, preservada para a trilha de auditoria. */
  data_anterior: string;
  /** Nova data agendada (ISO). */
  data_nova: string;
  motivo: string;
  reagendada_em: string;
};
