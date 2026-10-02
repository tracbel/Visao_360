/**
 * O que as peças da Gestão de Financiamentos (issue 261) compartilham: o nome e o tom das faixas do índice, as variações, a
 * granularidade da série e o CSV da tabela analítica.
 *
 * LINHA NÃO É CONTRATO: o Banco Central não publica quantidade de contrato, e a tela fala em "linhas do SICOR" onde o
 * protótipo dizia "contratos (chassi)".
 */

import type { FaixaDeMercado, FinanciamentosDoSicor, IndiceDeCredito, JanelasDeCredito, LinhasDoSicorNoMes } from '../../tipos/mercado';

export const NOME_DA_FAIXA: Record<FaixaDeMercado, string> = {
  Retraido: 'Retraído',
  Intermediaria: 'Intermediária',
  Aquecido: 'Aquecido',
  Superaquecido: 'Superaquecido',
};

/** O tom de cada faixa — a cor só reforça o nome escrito ao lado. */
export const TOM_DA_FAIXA: Record<FaixaDeMercado, string> = {
  Retraido: 'retraido',
  Intermediaria: 'intermediaria',
  Aquecido: 'aquecido',
  Superaquecido: 'superaquecido',
};

/** A cor de cada faixa na dispersão, do vermelho ao verde escuro. */
export const COR_DA_FAIXA: Record<FaixaDeMercado, string> = {
  Retraido: '#C62828',
  Intermediaria: '#B08900',
  Aquecido: '#4A8A4F',
  Superaquecido: '#1B5E20',
};

export const COR_SEM_FAIXA = '#9AA39A';

export const n = (v: number, casas = 0) => v.toLocaleString('pt-BR', { maximumFractionDigits: casas });

/** Uma razão entre janelas como variação: 1,12 vira "+12%"; nulo vira o traço. */
export function variacaoDaRazao(razao: number | null | undefined, casas = 1): string {
  if (razao === null || razao === undefined) return '—';
  const d = (razao - 1) * 100;
  const texto = n(Math.abs(d), casas);
  return texto === '0' ? '0%' : `${d > 0 ? '+' : '−'}${texto}%`;
}

/** A variação de duas grandezas, já como razão; nula sem base. */
export const razao = (atual: number, anterior: number | null | undefined) => (anterior && anterior > 0 ? atual / anterior : null);

/** O sentido de uma razão, para a seta. */
export const sentidoDaRazao = (r: number | null | undefined) => (r === null || r === undefined ? '' : r > 1.0005 ? 'sobe' : r < 0.9995 ? 'desce' : '');

/** O valor médio por linha de uma janela — e não ticket médio: a linha é uma soma de contratos. */
export const medioPorLinha = (valor: number, linhas: number) => (linhas > 0 ? valor / linhas : null);

/** A faixa de um índice, em palavras; sem índice, o motivo curto. */
export function situacaoDe(indice: IndiceDeCredito | null, janelas: JanelasDeCredito): string {
  if (janelas.linhasAnteriores === 0 || janelas.valorAnterior <= 0) return 'Sem base';
  if (!indice || indice.faixa === null) return '—';
  return NOME_DA_FAIXA[indice.faixa];
}

/** "set/25 a ago/26" a partir de duas datas `aaaa-mm-dd`. */
export function textoDoPeriodo(de: string | null, ate: string | null): string {
  if (!de || !ate) return '—';
  return de === ate ? mesCurtoDe(de) : `${mesCurtoDe(de)} a ${mesCurtoDe(ate)}`;
}

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

export function mesCurtoDe(iso: string): string {
  const [ano, mes] = iso.split('-').map(Number);
  return `${MESES[mes - 1]}/${String(ano).slice(2)}`;
}

/** O mês `aaaa-mm-dd` como `aaaa-mm`, o formato do filtro. */
export const comoFiltro = (iso: string | null) => (iso ? iso.slice(0, 7) : undefined);

// ------------------------------------------------------------------------------------------------
// A série por granularidade
// ------------------------------------------------------------------------------------------------

export type Granularidade = 'mes' | 'tri' | 'sem' | 'ano';
export type MetricaDoCredito = 'linhas' | 'valor';

export const GRANULARIDADES: readonly { id: Granularidade; rotulo: string }[] = [
  { id: 'mes', rotulo: 'Mês' },
  { id: 'tri', rotulo: 'Trimestre' },
  { id: 'sem', rotulo: 'Semestre' },
  { id: 'ano', rotulo: 'Ano' },
];

export const METRICAS: readonly { id: MetricaDoCredito; rotulo: string }[] = [
  { id: 'valor', rotulo: 'Valor financiado' },
  { id: 'linhas', rotulo: 'Linhas do SICOR' },
];

const MESES_DA_GRANULARIDADE: Record<Granularidade, number> = { mes: 1, tri: 3, sem: 6, ano: 12 };

export type PontoDaSerie = { rotulo: string; linhas: number; valor: number; meses: number; completo: boolean };

/**
 * AGRUPA A SÉRIE MENSAL no calendário: trimestre e semestre do ano civil, como o protótipo. O mês sem crédito entra como
 * zero — ele existe na série do SICOR, só não teve operação. O último grupo que não fechou os meses dele sai marcado como
 * incompleto, para o gráfico desenhá-lo tracejado.
 */
export function agrupar(serie: readonly LinhasDoSicorNoMes[], granularidade: Granularidade): PontoDaSerie[] {
  if (serie.length === 0) return [];
  const tamanho = MESES_DA_GRANULARIDADE[granularidade];
  const [anoIni, mesIni] = serie[0].mes.split('-').map(Number);
  const [anoFim, mesFim] = serie[serie.length - 1].mes.split('-').map(Number);
  const porMes = new Map(serie.map((m) => [m.mes.slice(0, 7), m]));
  const grupos = new Map<string, PontoDaSerie>();

  for (let ano = anoIni, mes = mesIni; ano < anoFim || (ano === anoFim && mes <= mesFim); mes === 12 ? ((mes = 1), ano++) : mes++) {
    const chave = `${ano}-${String(mes).padStart(2, '0')}`;
    const doMes = porMes.get(chave);
    const indice = Math.floor((mes - 1) / tamanho);
    const rotulo =
      granularidade === 'mes'
        ? `${MESES[mes - 1]}/${String(ano).slice(2)}`
        : granularidade === 'tri'
          ? `${indice + 1}º tri/${String(ano).slice(2)}`
          : granularidade === 'sem'
            ? `${indice + 1}º sem/${String(ano).slice(2)}`
            : String(ano);
    const grupo = grupos.get(rotulo) ?? { rotulo, linhas: 0, valor: 0, meses: 0, completo: false };
    grupo.linhas += doMes?.linhas ?? 0;
    grupo.valor += doMes?.valor ?? 0;
    grupo.meses += 1;
    grupo.completo = grupo.meses === tamanho;
    grupos.set(rotulo, grupo);
  }

  return [...grupos.values()];
}

// ------------------------------------------------------------------------------------------------
// O CSV da tabela analítica
// ------------------------------------------------------------------------------------------------

export function cabecalhoDaAnalitica(dados: FinanciamentosDoSicor): string[] {
  const atual = textoDoPeriodo(dados.de, dados.ate);
  const anterior = textoDoPeriodo(dados.anteriorDe, dados.anteriorAte);
  return [
    'Município', 'Da Região Tracbel', 'Loja',
    `Valor ${atual} (R$)`, `Valor ${anterior} (R$)`, 'Δ valor (%)',
    `Linhas ${atual}`, `Linhas ${anterior}`, 'Δ linhas (%)',
    'Valor médio por linha atual (R$)', 'Valor médio por linha anterior (R$)', 'Δ valor médio (%)',
    'Fatia em SP (%)', 'Δ fatia (p.p.)', 'Índice de crédito', 'Situação', 'Base pequena',
  ];
}

export function linhasDaAnalitica(dados: FinanciamentosDoSicor): unknown[][] {
  const num = (v: number | null | undefined, casas = 2) =>
    v === null || v === undefined ? '' : v.toLocaleString('pt-BR', { maximumFractionDigits: casas, useGrouping: false });
  const delta = (r: number | null) => (r === null ? '' : num((r - 1) * 100, 1));
  return dados.municipios.map((m) => {
    const j = m.janelas;
    const medio = medioPorLinha(j.valor, j.linhas);
    const medioAntes = medioPorLinha(j.valorAnterior, j.linhasAnteriores);
    return [
      m.nome, m.pertenceAAdr ? 'sim' : 'não', m.loja ?? '',
      num(j.valor), num(j.valorAnterior), delta(razao(j.valor, j.valorAnterior)),
      j.linhas, j.linhasAnteriores, delta(razao(j.linhas, j.linhasAnteriores)),
      num(medio), num(medioAntes), delta(medio !== null && medioAntes ? medio / medioAntes : null),
      num(m.fatiaNoEstado, 3), num(m.variacaoDaFatia, 3), num(m.indice?.indice, 3), situacaoDe(m.indice, j),
      m.indice?.basePequena ? 'sim' : '',
    ];
  });
}
