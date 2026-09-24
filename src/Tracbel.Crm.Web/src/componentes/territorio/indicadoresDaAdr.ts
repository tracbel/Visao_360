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
  quantidade: 'Pendentes (qtd.)',
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

/**
 * OS CINCO RECORTES DA ESTRUTURA NUMA LINHA SÓ (fidelidade às maquetes).
 *
 * A maquete mostra quatro — sem usinas —, e tirar as usinas é decisão que não
 * foi tomada: ficam os cinco, com os nomes curtos da maquete ("Rebanho") para
 * caberem num cartão de um quarto de linha. O bovino e o etanol, que saíram do
 * botão, estão no nome do mapa e na dica dele. A densidade mantém a unidade
 * VERDADEIRA — por mil km², e não por km² como a maquete escreve.
 *
 * "Por mil km²", e não "Tratores / mil km²": o botão fica logo depois de
 * "Tratores", que já diz de quê. Com o nome inteiro os cinco rótulos somavam
 * ~286 px num alternador de ~264 (cartão de um quarto de linha a 1216 px de
 * conteúdo): no Windows passava por arredondamento, e no Linux do CI cortava.
 */
export const ROTULO_DA_ESTRUTURA: Record<RecorteDaEstrutura, string> = {
  tratores: 'Tratores',
  densidade: 'Por mil km²',
  estabelecimentos: 'Propriedades',
  rebanho: 'Rebanho',
  usinas: 'Usinas',
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
 * O NOME DO RECORTE FILTRADO — para o texto que fala dos municípios da leitura.
 *
 * "REGIÃO TRACBEL" É A ÁREA DE ATUAÇÃO INTEIRA, e não muda com o filtro (issue
 * 163; é o que a dica da Sub-região diz). Os municípios da leitura, esses sim,
 * são só os da sub-região e da loja escolhidas: chamar de "Região Tracbel" a
 * média ou a contagem feitas sobre eles poria o nome da ADR num número do Norte.
 * Sem filtro a resposta é nula, e o texto diz "Região Tracbel".
 *
 * `da` vai no meio da frase ("Municípios da Sub-região Norte com leitura…");
 * `daCurto` vai num rótulo de cartão, onde "do recorte (Sub-região Norte · loja
 * Catanduva)" não cabe — e aí o nome inteiro fica na dica, em `nome`.
 */
export type RecorteFiltrado = { nome: string; da: string; daCurto: string };

export function recorteFiltrado(regiao: string, loja: string | null): RecorteFiltrado | null {
  if (regiao && loja) {
    const nome = `Sub-região ${regiao} · loja ${loja}`;
    return { nome, da: `do recorte (${nome})`, daCurto: 'do recorte' };
  }
  if (regiao) return { nome: `Sub-região ${regiao}`, da: `da Sub-região ${regiao}`, daCurto: `da Sub-região ${regiao}` };
  if (loja) return { nome: `loja ${loja}`, da: `da loja ${loja}`, daCurto: `da loja ${loja}` };
  return null;
}

/**
 * O ano civil até o último mês fechado — o recorte "acumulado do ano" pelo calendário de todo mundo.
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

/**
 * O MÊS EM QUE O ANO FISCAL DA TRACBEL COMEÇA: **novembro**.
 *
 * Confirmado pelo Ricardo em 24/09/2026. Até então o programa se contradizia — a issue 69 afirmava
 * novembro a outubro citando a planilha do comercial, e esta tela dizia, por escrito, que o calendário
 * fiscal não tinha sido confirmado. Agora há decisão, e ela mora aqui, numa constante só: quem precisar
 * do calendário fiscal usa esta, e não repete o número.
 */
export const MES_INICIAL_DO_ANO_FISCAL = 11;

/**
 * O ano fiscal até o último mês fechado — o mesmo recorte do ano civil, no calendário da Tracbel.
 *
 * **O ano fiscal leva o nome do ano em que TERMINA:** o FY2026 vai de novembro de 2025 a outubro de
 * 2026. É como a planilha do comercial conta, e é o que a diretoria compara de um ano para o outro.
 *
 * **Em novembro o recorte é o ano fiscal que acabou de fechar**, inteiro: o último mês fechado é
 * outubro, que é a última competência daquele ano. Não é um caso especial — é a mesma conta.
 */
export function anoFiscalFechado(hoje = new Date()): Pick<FiltrosTerritoriais, 'competenciaInicial' | 'competenciaFinal'> {
  const ultimoFechado = new Date(hoje.getFullYear(), hoje.getMonth() - 1, 1);
  const ano = ultimoFechado.getFullYear();
  const mes = ultimoFechado.getMonth() + 1;
  // O ano fiscal em curso começou em novembro DESTE ano quando já passamos de novembro; senão, no
  // novembro do ano passado. Novembro e dezembro são os dois meses em que o ano fiscal vai à frente
  // do civil, e é aí que contar pelo civil erraria o ano inteiro.
  const anoDeInicio = mes >= MES_INICIAL_DO_ANO_FISCAL ? ano : ano - 1;
  return {
    competenciaInicial: `${anoDeInicio}-${String(MES_INICIAL_DO_ANO_FISCAL).padStart(2, '0')}`,
    competenciaFinal: `${ano}-${String(mes).padStart(2, '0')}`,
  };
}

/**
 * O nome do ano fiscal de uma competência — "FY2026" para qualquer mês entre nov/2025 e out/2026.
 *
 * Ele aparece SEMPRE ao lado do intervalo escrito, nunca sozinho: "FY2026" sem "nov/2025 a out/2026"
 * é lido como ano civil por quem não conhece o calendário, e erra por dois meses sem avisar.
 */
export function nomeDoAnoFiscal(competencia: string): string | null {
  const [ano, mes] = competencia.split('-').map(Number);
  if (!Number.isInteger(ano) || !Number.isInteger(mes) || mes < 1 || mes > 12) return null;
  return `FY${mes >= MES_INICIAL_DO_ANO_FISCAL ? ano + 1 : ano}`;
}
