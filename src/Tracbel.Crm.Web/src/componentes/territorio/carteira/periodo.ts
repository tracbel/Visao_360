/**
 * O PERÍODO DA LEITURA, escrito como a maquete o escreve: "12 meses".
 *
 * ELE VEM DA RESPOSTA, e não do filtro: é a competência que o servidor APLICOU.
 * Recalcular aqui a janela a partir do preset do filtro seria uma segunda conta
 * para a mesma data — e é assim que duas contas passam a discordar.
 *
 * POR QUE UM CONTEXTO. A ficha do município é montada pela casca da tela e
 * entregue pronta à aba Território; o seletor "12 meses" do resumo executivo
 * dela precisa do período, e a casca não o passa. O contexto leva o período da
 * aba até a ficha sem mudar o contrato da casca — e, fora da aba (no teste da
 * ficha sozinha), ele é nulo e o seletor diz só "Período da página".
 */

import { createContext, useContext } from 'react';
import { mes } from '../indicadoresDaAdr';

export type PeriodoDaLeitura = {
  /** Quantos meses a janela cobre, contando os dois das pontas. */
  meses: number;
  /** "12 meses" — o texto do seletor da maquete. */
  rotulo: string;
  /** "out/2025 a set/2026" — o intervalo que o servidor aplicou. */
  intervalo: string;
};

/** A janela entre duas competências (`aaaa-mm` ou `aaaa-mm-dd`); nula se alguma não se lê. */
export function periodoDaLeitura(competenciaInicial: string, competenciaFinal: string): PeriodoDaLeitura | null {
  const emMeses = (c: string) => {
    const [ano, m] = c.split('-').map(Number);
    return Number.isInteger(ano) && Number.isInteger(m) && m >= 1 && m <= 12 ? ano * 12 + m - 1 : null;
  };
  const inicio = emMeses(competenciaInicial);
  const fim = emMeses(competenciaFinal);
  if (inicio === null || fim === null || fim < inicio) return null;

  const meses = fim - inicio + 1;
  return {
    meses,
    rotulo: meses === 1 ? '1 mês' : `${meses} meses`,
    intervalo: `${mes(competenciaInicial)} a ${mes(competenciaFinal)}`,
  };
}

const ContextoDoPeriodo = createContext<PeriodoDaLeitura | null>(null);

/** A aba Território declara o período; a ficha, lá dentro, o lê. */
export const ProvedorDoPeriodo = ContextoDoPeriodo.Provider;

export function usePeriodoDaLeitura(): PeriodoDaLeitura | null {
  return useContext(ContextoDoPeriodo);
}
