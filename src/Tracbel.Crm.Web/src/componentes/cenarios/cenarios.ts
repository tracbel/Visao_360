/**
 * OS CENÁRIOS DE MERCADO (issue 263) — o que a tela e o CSV dizem de cada município: o número, o enquadramento do manual e
 * a recomendação, nas regras do protótipo da pasta 360.
 */

import type { CenarioDeMercado, CenarioDoMunicipio, CenariosDeMercado, EscolhaDoCenario } from '../../tipos/mercado';

export const n = (v: number, casas = 1) => v.toLocaleString('pt-BR', { maximumFractionDigits: casas });

/** O número, ou o traço. */
export const numeroOuTraco = (v: number | null | undefined, casas = 1) => (v === null || v === undefined ? '—' : n(v, casas));

/** Um percentual com o sinal: 3,2 vira "+3,2%"; nulo vira o traço. */
export function comSinal(v: number | null | undefined, casas = 1): string {
  if (v === null || v === undefined) return '—';
  const texto = n(Math.abs(v), casas);
  return texto === '0' ? '0%' : `${v > 0 ? '+' : '−'}${texto}%`;
}

/** A seta do momento, como a do protótipo: acima de 2% sobe, abaixo de −2% cai. */
export function setaDoEfeito(v: number | null | undefined): { seta: string; sentido: 'sobe' | 'desce' | 'estavel' | 'sem' } {
  if (v === null || v === undefined) return { seta: '→', sentido: 'sem' };
  if (v > 2) return { seta: '↑', sentido: 'sobe' };
  if (v < -2) return { seta: '↓', sentido: 'desce' };
  return { seta: '→', sentido: 'estavel' };
}

export const NOME_DO_CENARIO: Record<CenarioDeMercado, string> = {
  Conservador: 'Conservador',
  Moderado: 'Moderado',
  Otimista: 'Otimista',
  Manual: 'Manual',
};

/** A meta de um cenário no município, com o mercado de hoje. */
export function metaDoCenario(linha: CenarioDoMunicipio, cenario: CenarioDeMercado): number | null {
  if (cenario === 'Conservador') return linha.conservador;
  if (cenario === 'Otimista') return linha.otimista;
  if (cenario === 'Moderado') return linha.moderado;
  return null;
}

/**
 * O ENQUADRAMENTO DO NÚMERO DIGITADO — o cenário mais perto dele e quanto ele fica do moderado (a regra do protótipo). A
 * mesma conta do servidor, para a linha que acabou de ser gravada aparecer certa sem reler a tela.
 */
export function enquadramento(linha: CenarioDoMunicipio, escolha: EscolhaDoCenario | null): { nome: string; doModerado: number } | null {
  if (escolha?.cenario !== 'Manual' || linha.moderado === null || linha.moderado <= 0 || linha.conservador === null || linha.otimista === null) return null;
  const manual = escolha.metaCombinada;
  const [dCon, dMod, dOti] = [Math.abs(manual - linha.conservador), Math.abs(manual - linha.moderado), Math.abs(manual - linha.otimista)];
  const nome = dOti < dMod && dOti < dCon ? 'Otimista' : dCon < dMod ? 'Conservador' : 'Moderado';
  return { nome, doModerado: (manual / linha.moderado - 1) * 100 };
}

/** A recomendação em texto, e o tom dela. */
export function recomendacao(linha: CenarioDoMunicipio): { texto: string; tom: 'alerta' | 'neutro' | 'ok'; linhas?: string[] } | null {
  switch (linha.recomendacao) {
    case 'Gradual':
      return {
        texto: `Sugerido ${n(linha.acimaDoRealizado ?? 0, 0)}% acima do realizado — gradual: ${numeroOuTraco(linha.metaGradual)}`,
        tom: 'alerta',
        linhas: [`${n(linha.acimaDoRealizado ?? 0, 0)}% acima do realizado`, `gradual: ${numeroOuTraco(linha.metaGradual)}`],
      };
    case 'SemHistorico':
      return { texto: 'Sem histórico — validar', tom: 'neutro' };
    case 'Atingivel':
      return { texto: 'Meta atingível', tom: 'ok' };
    default:
      return null;
  }
}

/** A meta que vale no município: a combinada, ou o moderado enquanto ninguém escolheu. */
export const aEntregarDe = (linha: CenarioDoMunicipio, escolha: EscolhaDoCenario | null) => escolha?.metaCombinada ?? linha.moderado;

/** "FY2026 (nov/2025 a out/2026)". */
export const nomeDoAnoFiscal = (ano: number) => `FY${ano} (nov/${ano - 1} a out/${ano})`;

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

/** "2025-11" vira "nov/25". */
export function mesCurto(iso: string): string {
  const [ano, mes] = iso.split('-').map(Number);
  return `${MESES[mes - 1]}/${String(ano).slice(2)}`;
}

// ------------------------------------------------------------------------------------------------
// O CSV — todas as colunas, uma por medida
// ------------------------------------------------------------------------------------------------

export function cabecalhoDoCsv(dados: CenariosDeMercado): string[] {
  const share = dados.shareAlvo === null ? 'share-alvo' : `${n(dados.shareAlvo, 0)}%`;
  return [
    'Município', 'Código IBGE', 'Região', 'Loja', 'Cultura principal', 'Efeito do preço (%)', 'Efeito do crédito (%)',
    `Realizado FY${dados.anoFiscal}`, `Realizado no intervalo (${dados.intervaloDe} a ${dados.intervaloAte})`, `Realizado FY${dados.anoFiscal - 1}`,
    `Média FY${dados.anosDaMedia[0]}–FY${dados.anosDaMedia[dados.anosDaMedia.length - 1]}`, 'Clientes', 'Potencial', 'Mercado ajustado',
    `Meta estrutural (${share})`, 'Share estrutural (%)', 'Share ajustado (%)', 'Conservador', 'Moderado', 'Otimista',
    'Base dos cenários', 'Cenário gravado', 'Meta combinada', 'Gravada por', 'A entregar', 'Recomendação',
  ];
}

export function linhasDoCsv(dados: CenariosDeMercado, escolhas: ReadonlyMap<number, EscolhaDoCenario | null>): unknown[][] {
  const num = (v: number | null | undefined, casas = 2) =>
    v === null || v === undefined ? '' : v.toLocaleString('pt-BR', { maximumFractionDigits: casas, useGrouping: false });
  return dados.municipios.map((m) => {
    const escolha = escolhas.has(m.codigoIbge) ? (escolhas.get(m.codigoIbge) ?? null) : m.escolha;
    return [
      m.nome, m.codigoIbge, m.regiao, m.loja ?? '', m.culturaPrincipal ?? '', num(m.efeitoDoPreco, 1), num(m.efeitoDoCredito, 1),
      num(m.realizadoNoAno, 0), num(m.realizadoNoIntervalo, 0), num(m.realizadoNoAnoAnterior, 0), num(m.mediaDosAnosAnteriores),
      num(m.clientes, 0), num(m.potencial), num(m.mercadoAjustado), num(m.metaEstrutural), num(m.shareEstrutural, 1), num(m.shareAjustado, 1),
      num(m.conservador), num(m.moderado), num(m.otimista), m.baseAjustada ? 'Mercado ajustado' : 'Meta estrutural (sem fator de ciclo)',
      escolha ? NOME_DO_CENARIO[escolha.cenario] : '', num(escolha?.metaCombinada), escolha?.gravadaPor ?? '', num(aEntregarDe(m, escolha)),
      recomendacao(m)?.texto ?? '',
    ];
  });
}
