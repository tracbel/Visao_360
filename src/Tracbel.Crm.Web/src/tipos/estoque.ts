/**
 * O estoque de máquinas e a cobertura (28/09/2026), como `GET /api/v1/relatorios/estoque` devolve. Espelha
 * `EstoqueECobertura` (`Aplicacao/Relacionamento/ObterEstoqueECobertura.cs`) em camelCase.
 *
 * O estoque é o painel "Estoque & Pedidos" do TOTVS, lido pela API Gestão de Negócios: o que está no pátio e o que vem da
 * fábrica. Não há custo nem cliente — o CRM não os lê. A cobertura é da empresa inteira.
 */

import type { MetricaSemDado } from './relacionamento';

export type TotaisDoEstoque = {
  noPatio: number;
  disponiveis: number;
  reservadas: number;
  pagas: number;
  maisDe180Dias: number;
  pedidosAFabrica: number;
};

export type GrupoNoEstoque = {
  grupo: string;
  noPatio: number;
  disponiveis: number;
  reservadas: number;
  pedidosAFabrica: number;
  /** Nula sem data de entrada. */
  idadeMediaEmDias: number | null;
  /** A cobertura do grupo, da empresa inteira; nula quando a GN não a calcula. */
  coberturaEmMeses: number | null;
};

export type MaquinaNaLista = {
  filial: string;
  grupo: string;
  descricao: string;
  configuracao: string | null;
  situacao: string;
  tipo: string;
  ehUsado: boolean;
  anoModelo: string | null;
  chassi: string | null;
  entradaEm: string | null;
  /** De hoje, pela data de entrada; nulo no pedido à fábrica. */
  diasNoPatio: number | null;
  chegadaPrevistaEm: string | null;
  faturamentoPrevistoEm: string | null;
  pago: boolean;
  reservado: boolean;
  ehPedidoAFabrica: boolean;
  situacaoNaFabrica: string | null;
};

export type ItemDaCobertura = { chave: string; competencia: string | null; meses: number; vendas: number };

export type CoberturaDoEstoque = {
  porMes: ItemDaCobertura[];
  porGrupo: ItemDaCobertura[];
  mediaPorMes: number | null;
  mediaPorGrupo: number | null;
  lidaEm: string | null;
  geradaNaOrigemEm: string | null;
};

/** `Organizacao`: "Todas as filiais" no seletor. `Filiais`: só a filial escolhida. */
export type AlcanceDoEstoque = 'Organizacao' | 'Filiais';

export type EstoqueECobertura = {
  alcance: AlcanceDoEstoque;
  hoje: string;
  totais: TotaisDoEstoque;
  porGrupo: GrupoNoEstoque[];
  maquinas: MaquinaNaLista[];
  cobertura: CoberturaDoEstoque;
  lidoEm: string | null;
  geradoNaOrigemEm: string | null;
  metricasSemDado: MetricaSemDado[];
};
