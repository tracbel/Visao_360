/**
 * O vocabulário compartilhado da tela de Indicadores Geográficos (issue 170, parte A).
 *
 * Rótulos, faixas, unidades e formatadores que os quatro mapas, a tabela e os
 * cartões usam. Ficavam no topo de `IndicadoresGeograficos.tsx`; saíram para cá
 * quando a tela foi quebrada em componentes, sem mudar um caractere do que
 * aparece na tela.
 */

import {
  FAIXAS_AREA_PLANTADA,
  FAIXAS_COBERTURA_PERCENTUAL,
  FAIXAS_DENSIDADE_DE_TRATORES,
  FAIXAS_ESTABELECIMENTOS,
  FAIXAS_PENDENCIA_PERCENTUAL,
  FAIXAS_PENDENCIA_QUANTIDADE,
  FAIXAS_POTENCIAL,
  FAIXAS_REBANHO,
  FAIXAS_TRATORES,
  FAIXAS_USINAS,
  FAIXAS_VALOR_DA_PRODUCAO,
} from './escalas';
import type { FiltrosTerritoriais, MotivoSemPotencial } from '../../tipos/territorio';

/** A largura de referência do desenho; o SVG escala para o cartão. */
export const LARGURA_DO_DESENHO = 520;

export type ModoDeCobertura = 'cobertura' | 'pendencia' | 'quantidade';
export type RecorteDeVendas = 'valorLiquido' | 'maquina' | 'posVenda';

/** O que o mapa D pinta. Cada recorte vem de uma pesquisa diferente, com ano próprio. */
export type RecorteDaEstrutura = 'tratores' | 'densidade' | 'estabelecimentos' | 'rebanho' | 'usinas';

/** O que o mapa C pinta: a regra, ou a lavoura que a sustenta. */
export type RecorteDoPotencial = 'maquinas' | 'areaPlantada' | 'valorDaProducao';

export const ROTULO_DE_COBERTURA: Record<ModoDeCobertura, string> = {
  cobertura: '% no prazo',
  pendencia: '% pendente',
  quantidade: 'pendentes (qtd.)',
};

export const UNIDADE_DE_COBERTURA: Record<ModoDeCobertura, string> = {
  cobertura: '% dos vínculos elegíveis com contato no prazo da cadência — verde é mais coberto',
  pendencia: '% dos vínculos elegíveis fora do prazo ou nunca contatados — vermelho é mais pendente',
  quantidade: 'vínculos pendentes (fora do prazo + nunca contatados)',
};

export const FAIXAS_DE_COBERTURA = {
  cobertura: FAIXAS_COBERTURA_PERCENTUAL,
  pendencia: FAIXAS_PENDENCIA_PERCENTUAL,
  quantidade: FAIXAS_PENDENCIA_QUANTIDADE,
} as const;

export const ROTULO_DE_VENDAS: Record<RecorteDeVendas, string> = {
  valorLiquido: 'Total líquido',
  maquina: 'Máquina',
  posVenda: 'Pós-venda',
};

export const ROTULO_DA_ESTRUTURA: Record<RecorteDaEstrutura, string> = {
  tratores: 'Tratores',
  densidade: 'Tratores / mil km²',
  estabelecimentos: 'Propriedades',
  rebanho: 'Rebanho bovino',
  usinas: 'Usinas de etanol',
};

export const FAIXAS_DA_ESTRUTURA = {
  tratores: FAIXAS_TRATORES,
  densidade: FAIXAS_DENSIDADE_DE_TRATORES,
  estabelecimentos: FAIXAS_ESTABELECIMENTOS,
  rebanho: FAIXAS_REBANHO,
  usinas: FAIXAS_USINAS,
} as const;

export const UNIDADE_DA_ESTRUTURA: Record<RecorteDaEstrutura, string> = {
  tratores: 'tratores existentes (Censo Agropecuário) — hachurado é sigilo do IBGE, não zero',
  densidade: 'tratores por mil km² — a densidade, que compara município grande com pequeno',
  estabelecimentos: 'estabelecimentos agropecuários (Censo Agropecuário)',
  rebanho: 'cabeças de bovino (Pesquisa da Pecuária Municipal)',
  usinas: 'capacidade autorizada de etanol, em m³/dia (ANP) — hachurado é município sem usina de etanol',
};

export const ROTULO_DO_POTENCIAL: Record<RecorteDoPotencial, string> = {
  maquinas: 'Máquinas teóricas',
  areaPlantada: 'Área plantada',
  valorDaProducao: 'Valor da produção',
};

export const FAIXAS_DO_POTENCIAL = {
  maquinas: FAIXAS_POTENCIAL,
  areaPlantada: FAIXAS_AREA_PLANTADA,
  valorDaProducao: FAIXAS_VALOR_DA_PRODUCAO,
} as const;

export const UNIDADE_DO_POTENCIAL: Record<RecorteDoPotencial, string> = {
  maquinas: 'máquinas teóricas na região (necessidade de frota, não venda nem valor)',
  areaPlantada: 'hectares plantados de TODAS as culturas do município (IBGE/PAM)',
  valorDaProducao: 'valor da produção agrícola do município — o que ele COLHE, não o que a Tracbel vende',
};

/**
 * Por que o parque não saiu, em português (issue 72).
 *
 * O motor devolve o motivo, e não um traço mudo: "sem área" é sigilo do IBGE e
 * "sem regra" é parâmetro que ninguém decidiu — quem lê o mapa precisa saber
 * qual dos dois está olhando.
 */
export const MOTIVO_SEM_PARQUE: Record<MotivoSemPotencial, string> = {
  Nenhum: '—',
  SemArea: 'área plantada não divulgada aqui (sigilo do IBGE)',
  SemRegra: 'há área plantada, mas nenhuma cultura daqui tem regra de hectares por máquina',
  SemCicloDeRenovacao: 'falta o ciclo de renovação',
};

export const nº = (v: number) => v.toLocaleString('pt-BR');
export const porcento = (v: number) => `${v.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`;

/** `2025-09-01` vira `set/2025`. */
export function mes(competencia: string): string {
  const [ano, m] = competencia.split('-');
  return `${['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'][Number(m) - 1]}/${ano}`;
}

/**
 * O ano civil até o último mês fechado — o recorte "acumulado do ano" que existe sem calendário fiscal.
 * Em janeiro, o último mês fechado é dezembro: o recorte vira o ano anterior inteiro.
 */
export function anoCivilFechado(hoje = new Date()): Pick<FiltrosTerritoriais, 'competenciaInicial' | 'competenciaFinal'> {
  const ultimoFechado = new Date(hoje.getFullYear(), hoje.getMonth() - 1, 1);
  const ano = ultimoFechado.getFullYear();
  return {
    competenciaInicial: `${ano}-01`,
    competenciaFinal: `${ano}-${String(ultimoFechado.getMonth() + 1).padStart(2, '0')}`,
  };
}
