/**
 * A meta de venda × o realizado (#138), como `GET /api/v1/relatorios/metas` devolve — uma resposta por filial.
 * Espelha `MetaERealizadoDaFilial` (`Aplicacao/Relacionamento/ObterMetaERealizado.cs`) em camelCase.
 *
 * A META é a cota da API Gestão de Negócios, em UNIDADES (máquinas); o REALIZADO são as máquinas vendidas que o CRM
 * tem (o ART, D-M3). O consórcio é à parte, em cotas, sem realizado (D-M4). Todo número se soma entre filiais.
 */

import type { MetricaSemDado } from './relacionamento';

/** O período — sempre escrito junto com o número. */
export type PeriodoDaMeta = {
  inicial: string;
  final: string;
  meses: number;
  /** O ano fiscal (novembro a outubro, com o nome do ano em que termina). */
  anoFiscal: number;
  /** "nov/2025 a ago/2026". */
  texto: string;
  /** O ano fiscal até o último mês fechado — o padrão decidido em 27/09/2026. */
  ehOPadrao: boolean;
  inicialDoAnterior: string;
  finalDoAnterior: string;
};

export type TotaisDaMeta = {
  metaMaquinas: number;
  realizadoMaquinas: number;
  /**
   * As vendas que aguardam na integração do ART (cadastro, chassi ou outro motivo); nulo no alcance Próprios (a pendente
   * ainda não tem vendedor).
   */
  pendentesNoArt: number | null;
  /** Em cotas — à parte, sem realizado. */
  metaConsorcio: number;
  /** As vendas do período sem vendedor no ART: contam no total e em consultor nenhum. Em número, para somar as filiais. */
  vendasSemVendedor: number;
};

export type MetaERealizadoNoMes = {
  competencia: string;
  metaMaquinas: number;
  realizadoMaquinas: number;
  metaConsorcio: number;
};

export type MetaERealizadoNaLinha = { codigo: string; nome: string; meta: number; realizado: number };

export type MetaERealizadoDoConsultor = { consultor: string; temConta: boolean; meta: number; realizado: number };

export type OrigemDaMetaDeVenda = { sistema: string; rota: string; lidaEm: string; geradaNaOrigemEm: string | null };

/** `Proprios`: só a meta e as vendas da própria pessoa. `Filial`: a filial inteira (D-M5). */
export type AlcanceDaMeta = 'Proprios' | 'Filial';

export type MetaERealizadoDaFilial = {
  periodo: PeriodoDaMeta;
  alcance: AlcanceDaMeta;
  totais: TotaisDaMeta;
  porMes: MetaERealizadoNoMes[];
  porLinha: MetaERealizadoNaLinha[];
  porConsultor: MetaERealizadoDoConsultor[];
  /** O mês em curso, quando ficou fora do período — meta e realizado até aqui. */
  mesEmCurso: MetaERealizadoNoMes | null;
  mesmoTrechoDoFyAnterior: { realizadoMaquinas: number };
  origem: OrigemDaMetaDeVenda | null;
  metricasSemDado: MetricaSemDado[];
};
