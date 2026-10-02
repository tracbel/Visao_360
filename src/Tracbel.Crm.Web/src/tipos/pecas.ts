/**
 * As peças do cliente, do Protheus (02/10/2026) — `/api/v1/clientes/{chave}/pecas`: o faturamento de peças por mês e os
 * orçamentos que o modo de peças da rotina 15 `POS_VENDA_PROTHEUS` traz das views do BI. Sem custo nem margem.
 */

import type { MetricaSemDado } from './relacionamento';

/** Uma fatia dos doze meses: um setor (balcão, oficina…) ou um grupo comercial (peças, pneus, lubrificantes…). */
export type FatiaDasPecas = { nome: string; valor: number };

/** Um mês da série. */
export type MesDasPecas = { competencia: string; valor: number };

/** Um orçamento de peças em aberto. */
export type OrcamentoDePecas = {
  chave: string;
  numero: string;
  filialCodigo: string;
  filialNome: string;
  situacao: string;
  prazo: string | null;
  reserva: string | null;
  orcadoEm: string;
  validoAte: string | null;
  vendedorNome: string | null;
  valorTotal: number;
  itens: number;
};

/** As peças de um cliente, resumidas para a ficha. */
export type PecasDoCliente = {
  de: string;
  ate: string;
  /** O valor líquido dos doze meses, com as devoluções descontadas. */
  dozeMeses: number;
  devolucoesNosDozeMeses: number;
  descontoNosDozeMeses: number;
  itensNosDozeMeses: number;
  porSetor: FatiaDasPecas[];
  porGrupo: FatiaDasPecas[];
  serie: MesDasPecas[];
  ultimaCompraEm: string | null;
  vendedorPrincipal: string | null;
  orcamentosEmAberto: number;
  valorEmOrcamentosAbertos: number;
  orcamentosVencidos: number;
  /** Os em aberto, os que vencem antes primeiro (no máximo dez). */
  orcamentos: OrcamentoDePecas[];
  carregadoEm: string | null;
  orcamentosCarregadosEm: string | null;
  metricasSemDado: MetricaSemDado[];
};
