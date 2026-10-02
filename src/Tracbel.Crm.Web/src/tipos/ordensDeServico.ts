/**
 * As ordens de serviço da oficina, do Protheus (02/10/2026) — `/api/v1/clientes/{chave}/ordens-de-servico` e
 * `/api/v1/equipamentos/{chave}/ordens-de-servico`, da tabela que a rotina 15 `POS_VENDA_PROTHEUS` mantém a partir das
 * views do BI. Sem nome de técnico nem de consultor.
 */

import type { MetricaSemDado } from './relacionamento';

/** A situação da capa da OS. */
export type SituacaoDaOrdemDeServico = 'Aberta' | 'Liberada' | 'Fechada' | 'Cancelada';

/** Uma ordem de serviço. */
export type OrdemDeServico = {
  chave: string;
  numero: string;
  filialCodigo: string;
  filialNome: string;
  situacao: SituacaoDaOrdemDeServico;
  tipoDeAtendimento: string | null;
  abertaEm: string;
  liberadaEm: string | null;
  fechadaEm: string | null;
  canceladaEm: string | null;
  chassi: string | null;
  modelo: string | null;
  horimetro: number | null;
  equipamentoChave: string | null;
  clienteChave: string | null;
  clienteNome: string | null;
  /** As peças, sem desconto — a régua do painel de pós-venda do BI. */
  valorDePecas: number;
  /** Os serviços, sem desconto — a régua do painel de pós-venda do BI. */
  valorDeServicos: number;
  itensDePeca: number;
  itensDeServico: number;
};

/** Uma OS da lista, com os dias em aberto já contados (nulo na fechada ou cancelada). */
export type OrdemDeServicoResumida = { ordem: OrdemDeServico; diasEmAberto: number | null };

/** As OS de um cliente ou de uma máquina, resumidas nos recortes do painel de pós-venda. */
export type OrdensDeServicoResumidas = {
  emAberto: number;
  emAbertoHaMaisDe45Dias: number;
  diasDaMaisAntigaEmAberto: number | null;
  valorEmAberto: number;
  nosUltimos12Meses: number;
  pecasNosUltimos12Meses: number;
  servicosNosUltimos12Meses: number;
  ultimaAbertaEm: string | null;
  totalDeOrdens: number;
  /** As abertas primeiro, da mais antiga; depois as outras, da mais recente. No máximo 100. */
  ordens: OrdemDeServicoResumida[];
  carregadoEm: string | null;
  metricasSemDado: MetricaSemDado[];
};
