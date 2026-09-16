/**
 * Os contratos dos cinco cartões da Visão 360, como
 * `GET /api/v1/relatorios/indicadores-executivos` os devolve (documento 36).
 * Espelham os `record` de `Dominio/Portas/PortasDeIndicadoresExecutivos.cs` em
 * camelCase. Uma resposta por filial; todo número se soma entre filiais, exceto
 * `clientesNasCarteirasDaFilial`.
 */

import type { MetricaSemDado } from './relacionamento';

/** O faturamento de uma competência, com a nota sem cliente separada por natureza. */
export type FaturamentoDaCompetencia = {
  competencia: string;
  comCliente: number;
  contraparteSemCadastro: number;
  repasseDeFabrica: number;
  empresaDoGrupo: number;
  outraRevenda: number;
  maquina: number;
  peca: number;
  servico: number;
  outros: number;
  notas: number;
  /** Quando a carga gravou a linha mais recente da competência (UTC). */
  carregadoEm: string | null;
  semCliente: number;
  total: number;
};

/** O realizado do ano civil e a meta de faturamento da filial — nunca uma previsão. */
export type FaturamentoDoAno = {
  ano: number;
  primeiraCompetencia: string | null;
  ultimaCompetencia: string | null;
  mesesComFaturamento: number;
  comCliente: number;
  semCliente: number;
  metasDaFilial: number;
  /** Nulo quando não há meta — sem meta, e não meta zero. */
  alvoDaFilial: number | null;
  metasDetalhadas: number;
  metasQueCruzamOAno: number;
  total: number;
};

/** Clientes únicos pela filial de cadastro, e vínculos pela filial da carteira. */
export type CarteiraDaFilial = {
  clientesCadastradosComVinculo: number;
  clientes: number;
  prospects: number;
  suspects: number;
  outrasSituacoes: number;
  semDocumento: number;
  /** O que a filial enxerga nas carteiras dela. NÃO se soma entre filiais. */
  clientesNasCarteirasDaFilial: number;
  vinculos: number;
  vinculosComerciais: number;
  carteiras: number;
  carteirasComerciais: number;
};

/** Cobertura por vínculo em carteira comercial, contra a cadência declarada da linha. */
export type CoberturaDaFilial = {
  vinculosComerciais: number;
  elegiveis: number;
  cobertos: number;
  foraDaCadencia: number;
  nuncaContatados: number;
  semCadencia: number;
  contatoMaisRecente: string | null;
  tiposDeAtividade: number;
  tiposMarcadosComoVisita: number;
  pendentes: number;
};

/** As vendas perdidas registradas no formulário. Sem percentual de mercado. */
export type MercadoDaFilial = {
  vendasPerdidasRegistradas: number;
  comConcorrente: number;
  comModeloDoConcorrente: number;
  comOsDoisPrecos: number;
  unidades: number;
  primeiraEm: string | null;
  ultimaEm: string | null;
};

export type IndicadoresExecutivosDaFilial = {
  referenciaUtc: string;
  faturamentoDoMes: FaturamentoDaCompetencia | null;
  ano: FaturamentoDoAno;
  carteira: CarteiraDaFilial;
  cobertura: CoberturaDaFilial;
  mercado: MercadoDaFilial;
};

export type PainelExecutivoDaFilial = {
  indicadores: IndicadoresExecutivosDaFilial;
  metricasSemDado: MetricaSemDado[];
};
