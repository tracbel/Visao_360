/**
 * O ESTADO DA COMPARAÇÃO COM O MESMO TRECHO DO ANO ANTERIOR — o contexto que a
 * casca da tela monta (`ProvedorDaComparacao`, em `comparacao.tsx`) e que
 * cartões, mapas e ficha leem.
 *
 * MORA SEPARADO DOS COMPONENTES para o recarregamento rápido do Vite: um arquivo
 * que exporta componente e função junto perde o recarregamento a quente.
 */

import { createContext, useContext } from 'react';
import type { ComparacaoComOAnoAnterior, PeriodoAnterior } from '../../tipos/territorio';
import { mes } from './indicadoresDaAdr';

/** Δ % é a variação relativa; Δ absoluto é a diferença na unidade do próprio número. */
export type UnidadeDaVariacao = 'percentual' | 'absoluta';

export type EstadoDaComparacao = {
  ligada: boolean;
  unidade: UnidadeDaVariacao;
  aoLigar: (ligada: boolean) => void;
  aoEscolherUnidade: (unidade: UnidadeDaVariacao) => void;
  /** A janela anterior e a cobertura das fontes; nulo antes da resposta. */
  periodoAnterior: PeriodoAnterior | null;
  /** Os quatro números de decisão no ano anterior; nulo antes da resposta. */
  numeros: ComparacaoComOAnoAnterior | null;
};

export const ContextoDaComparacao = createContext<EstadoDaComparacao>({
  ligada: true,
  unidade: 'percentual',
  aoLigar: () => undefined,
  aoEscolherUnidade: () => undefined,
  periodoAnterior: null,
  numeros: null,
});

export function useComparacao(): EstadoDaComparacao {
  return useContext(ContextoDaComparacao);
}

/** "nov/2024 a ago/2025" — a janela anterior por extenso, para a dica. */
export function janelaAnteriorPorExtenso(periodo: PeriodoAnterior | null): string | null {
  return periodo ? `${mes(periodo.competenciaInicial)} a ${mes(periodo.competenciaFinal)}` : null;
}
