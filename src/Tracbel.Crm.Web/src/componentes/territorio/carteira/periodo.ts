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
 *
 * O ANO FISCAL TEM NOME PRÓPRIO (27/09/2026). Desde que ele virou o período
 * padrão, a janela de nov/2025 a ago/2026 não é "10 meses" para quem lê: é o
 * ano fiscal até agosto. O rótulo curto diz "Ano fiscal", e a descrição — que
 * vai nas dicas — diz o nome do ano ao lado do intervalo, nunca sozinho.
 */

import { createContext, useContext } from 'react';
import { MES_INICIAL_DO_ANO_FISCAL, mes, nomeDoAnoFiscal } from '../indicadoresDaAdr';

export type PeriodoDaLeitura = {
  /** Quantos meses a janela cobre, contando os dois das pontas. */
  meses: number;
  /** "Ano fiscal" ou "12 meses" — o texto curto do seletor e do cabeçalho da tabela. */
  rotulo: string;
  /** "out/2025 a set/2026" — o intervalo que o servidor aplicou. */
  intervalo: string;
  /**
   * O período por extenso, para as dicas: "o ano fiscal FY2026 até ago/2026" ou
   * "12 meses" — com o intervalo sempre ao lado, na mesma frase de quem o usa.
   */
  descricao: string;
};

/** A janela entre duas competências (`aaaa-mm` ou `aaaa-mm-dd`); nula se alguma não se lê. */
export function periodoDaLeitura(competenciaInicial: string, competenciaFinal: string): PeriodoDaLeitura | null {
  const partes = (c: string) => c.split('-').map(Number);
  const emMeses = (c: string) => {
    const [ano, m] = partes(c);
    return Number.isInteger(ano) && Number.isInteger(m) && m >= 1 && m <= 12 ? ano * 12 + m - 1 : null;
  };
  const inicio = emMeses(competenciaInicial);
  const fim = emMeses(competenciaFinal);
  if (inicio === null || fim === null || fim < inicio) return null;

  const meses = fim - inicio + 1;
  const intervalo = `${mes(competenciaInicial)} a ${mes(competenciaFinal)}`;

  // É O ANO FISCAL QUANDO COMEÇA EM NOVEMBRO E NÃO PASSA DE OUTUBRO: até o
  // último mês fechado (o padrão) ou inteiro. Doze meses de novembro a outubro
  // são as duas coisas ao mesmo tempo, e o nome que fica é o do ano fiscal.
  const doAnoFiscal = partes(competenciaInicial)[1] === MES_INICIAL_DO_ANO_FISCAL && meses <= 12;
  const nome = nomeDoAnoFiscal(competenciaFinal);

  if (doAnoFiscal && nome)
    return {
      meses,
      rotulo: 'Ano fiscal',
      intervalo,
      descricao:
        meses === 12 ? `o ano fiscal ${nome} inteiro` : `o ano fiscal ${nome} até ${mes(competenciaFinal)}`,
    };

  const rotulo = meses === 1 ? '1 mês' : `${meses} meses`;
  return { meses, rotulo, intervalo, descricao: rotulo };
}

const ContextoDoPeriodo = createContext<PeriodoDaLeitura | null>(null);

/** A aba Território declara o período; a ficha, lá dentro, o lê. */
export const ProvedorDoPeriodo = ContextoDoPeriodo.Provider;

export function usePeriodoDaLeitura(): PeriodoDaLeitura | null {
  return useContext(ContextoDoPeriodo);
}
