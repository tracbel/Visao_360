/**
 * O que as peças do Preço de Commodities (issue 260) compartilham: os horizontes, o nome e o tom das faixas, a variação do
 * índice e o preço na unidade da fonte.
 */

import type { FaixaDeMercado, HorizonteDoPreco, PrecoDaCultura } from '../../tipos/mercado';

export type Horizonte = 1 | 3 | 6 | 12;

export const HORIZONTES: readonly { id: Horizonte; rotulo: string; extenso: string }[] = [
  { id: 1, rotulo: 'Mês', extenso: 'o mês' },
  { id: 3, rotulo: 'R3', extenso: 'o trimestre (R3)' },
  { id: 6, rotulo: 'R6', extenso: 'o semestre (R6)' },
  { id: 12, rotulo: 'R12', extenso: 'o ciclo (R12)' },
];

export const NOME_DA_FAIXA: Record<FaixaDeMercado, string> = {
  Retraido: 'Retraído',
  Intermediaria: 'Intermediária',
  Aquecido: 'Aquecido',
  Superaquecido: 'Superaquecido',
};

export const TOM_DA_FAIXA: Record<FaixaDeMercado, string> = {
  Retraido: 'retraido',
  Intermediaria: 'intermediaria',
  Aquecido: 'aquecido',
  Superaquecido: 'superaquecido',
};

/** O horizonte de uma cultura. */
export const horizonteDe = (c: PrecoDaCultura, meses: Horizonte): HorizonteDoPreco | undefined => c.horizontes.find((h) => h.meses === meses);

/** Um índice como variação: 1,12 vira "+12,0%"; nulo vira o traço. */
export function variacao(indice: number | null | undefined): string {
  if (indice === null || indice === undefined) return '—';
  const d = (indice - 1) * 100;
  const texto = Math.abs(d).toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
  return texto === '0,0' ? '0,0%' : `${d > 0 ? '+' : '−'}${texto}%`;
}

/** A seta do sentido: a cor só reforça. */
export const sentido = (indice: number | null | undefined) =>
  indice === null || indice === undefined ? { seta: '→', classe: '' } : indice > 1.005 ? { seta: '↑', classe: 'sobe' } : indice < 0.995 ? { seta: '↓', classe: 'desce' } : { seta: '→', classe: '' };

/** O preço na unidade da fonte: três casas abaixo de R$ 10 (o kg de ATR), duas acima (a saca). */
export function preco(valor: number | null | undefined, unidade: string): string {
  if (valor === null || valor === undefined) return '—';
  const casas = valor < 10 ? 3 : 2;
  const texto = valor.toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas });
  return unidade ? `R$ ${texto} / ${unidade}` : `R$ ${texto}`;
}

/** Por que um horizonte não saiu, em palavras. */
export function motivoDoHorizonte(meses: number, motivo: string): string {
  if (motivo === 'SerieCurta') return `A série ainda não tem ${2 * meses} meses para comparar ${meses === 1 ? 'o último mês com o anterior' : `${meses} meses com os ${meses} anteriores`}.`;
  if (motivo === 'SemBaseDeComparacao') return 'A janela anterior está zerada — não há do que variar.';
  if (motivo === 'SemFonte') return 'Nenhuma fonte carregada publica esta cultura.';
  return 'O índice não saiu.';
}
