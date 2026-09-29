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

/**
 * O faturamento do ano — nunca uma previsão. A meta saiu daqui (#138): a meta de VENDA, em unidades, da API Gestão de
 * Negócios, tem rota própria (`tipos/metas.ts`).
 *
 * O ANO É O FISCAL POR PADRÃO (27/09/2026): novembro a outubro, com o nome do ano
 * em que termina. `calendario` diz qual foi aplicado, e `inicio`/`fim` os meses.
 */
export type FaturamentoDoAno = {
  ano: number;
  calendario: 'Fiscal' | 'Civil';
  /** `aaaa-mm-dd` — o primeiro mês do ano no calendário aplicado. */
  inicio: string | null;
  /** `aaaa-mm-dd` — o último mês do ano, mesmo quando ele ainda corre. */
  fim: string | null;
  primeiraCompetencia: string | null;
  ultimaCompetencia: string | null;
  mesesComFaturamento: number;
  comCliente: number;
  semCliente: number;
  total: number;
};

/**
 * O FATURAMENTO PELO ART (29/09/2026, decisão do Ricardo): as máquinas ENTREGUES no ART — a data de entrega preenchida —
 * numa janela de meses, com e sem comprador no CRM, pela filial da unidade que vendeu. Uma máquina por registro, como a
 * Gestão de Negócios conta; o valor é o de venda do ART. Não se soma com a nota do Protheus.
 */
export type MaquinasEntreguesNoArt = {
  /** `aaaa-mm-dd` — o primeiro mês da janela. */
  inicio: string;
  /** `aaaa-mm-dd` — o último mês da janela. */
  fim: string;
  maquinas: number;
  /** Em reais. A máquina sem valor no ART conta em `maquinas`, e não aqui. */
  valor: number;
  semValor: number;
  /** As que ainda não viraram venda no CRM (comprador sem cadastro, chassi…) — entram na conta mesmo assim. */
  aguardandoNoCrm: number;
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
  /** O faturamento do ano pelo ART, nos mesmos meses de `ano`. Ausente numa API anterior a 29/09/2026. */
  entreguesNoAno?: MaquinasEntreguesNoArt | null;
  /** O mesmo trecho do ano anterior. */
  entreguesNoMesmoTrechoDoAnoAnterior?: MaquinasEntreguesNoArt | null;
  /** O mês em curso, à parte e parcial. */
  entreguesNoMesEmCurso?: MaquinasEntreguesNoArt | null;
  /**
   * O faturamento MÊS A MÊS pelo ART (29/09/2026): os doze meses que terminam no mês em curso, um item por mês (zero no
   * mês sem entrega). O último é o mês em curso, parcial. Não depende do ano pedido. Ausente no servidor anterior.
   */
  entreguesPorMes?: MaquinasEntreguesNoArt[] | null;
};

export type PainelExecutivoDaFilial = {
  indicadores: IndicadoresExecutivosDaFilial;
  metricasSemDado: MetricaSemDado[];
};
