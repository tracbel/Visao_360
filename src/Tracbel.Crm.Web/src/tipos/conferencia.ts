/**
 * A conferência com a Gestão de Negócios (28/09/2026), como `GET /api/v1/integracoes/conferencia-gn` devolve. Espelha
 * `ConferenciaComAGestao` (`Aplicacao/Relacionamento/ObterConferenciaComAGestao.cs`) em camelCase.
 *
 * Os dois lados pela mesma régua: a meta sem consórcio, e o realizado só com a máquina entregue, no mês da entrega.
 */

import type { MetricaSemDado } from './relacionamento';

export type NumerosDaConferencia = { metaNaGestao: number; metaNoCrm: number; realizadoNaGestao: number; realizadoNoCrm: number };

export type ConferenciaDaFilial = { filial: string; numeros: NumerosDaConferencia };

export type ConferenciaDoMes = { competencia: string; numeros: NumerosDaConferencia };

export type ContagemDeDivergencia = { tipo: string; rotulo: string; quantidade: number };

export type DivergenciaNaTela = {
  tipo: string;
  rotulo: string;
  chassi: string;
  filial: string;
  descricao: string;
  /** "Filial · AAAA-MM", "venda sem entrega" ou os motivos da pendência; nulo quando o CRM não tem a máquina. */
  noCrm: string | null;
  /** "Filial · AAAA-MM"; nulo quando a GN não conta a máquina. */
  naGestao: string | null;
  detectadaEm: string;
};

export type ConferenciaComAGestao = {
  alcance: 'Organizacao' | 'Filiais';
  totais: NumerosDaConferencia;
  porFilial: ConferenciaDaFilial[];
  porMes: ConferenciaDoMes[];
  porTipo: ContagemDeDivergencia[];
  divergencias: DivergenciaNaTela[];
  apuradaEm: string | null;
  geradaNaOrigemEm: string | null;
  metricasSemDado: MetricaSemDado[];
};
