/**
 * OS QUATRO NÚMEROS DE DECISÃO — o que a revisão de 24/09/2026 achou neles.
 *
 * A composição dos quatro cartões está provada no teste da tela inteira. Aqui
 * ficam duas regras miúdas que a tela inteira não alcança: o "-0%" de um fator
 * que quase não mexe na demanda, e a diferença entre "ainda carregando" e "não
 * existe".
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { MomentoDoRecorte, NumerosDeDecisao, VendasDeMaquinaDoRecorte } from '../../tipos/territorio';
import { KpisExecutivos } from './KpisExecutivos';

const momento = (fatorAgregado: number | null) => ({ fatorAgregado, procedencia: null }) as unknown as MomentoDoRecorte;

/** O motivo vem do servidor (issue 69, parte A): a frase aqui é a da API, e a tela só a mostra. */
const NUMEROS: NumerosDeDecisao = {
  demandaAnual: { valor: null, motivo: 'SemDemandaAnual', frase: 'Falta o ciclo de renovação (D-P01, issue 63).' },
  mercadoAnual: { valor: null, motivo: 'SemPrecoDeMaquina', frase: 'Sem preço de máquina.', parcial: false, categoriasSemPreco: [] },
  capturaPercentual: { valor: null, motivo: 'SemVendasEmUnidades', frase: 'Sem vendas em máquinas.' },
  oportunidade: { valor: null, motivo: 'SemVendasEmUnidades', frase: 'Sem vendas em máquinas.' },
  baseDaCaptura: null,
};

function abrir({ fator = null as number | null, demanda = 400 as number | null, carregando = false } = {}) {
  render(
    <KpisExecutivos
      numeros={carregando ? null : NUMEROS}
      momento={momento(fator)}
      demandaEstrutural={demanda}
      demandaDeSaoPaulo={null}
      carregando={carregando}
      procedenciaDaDemanda={null}
      maquinasVendidas={null}
      procedenciaDasVendas={null}
    />,
  );
}

const cartao = (rotulo: string) => document.querySelector<HTMLElement>(`[data-kpi="${rotulo}"]`)!;

function lerDica(rotulo: string): string {
  const gatilho = screen.getByRole('button', { name: rotulo });
  fireEvent.focus(gatilho);
  const texto = screen.getByRole('tooltip').textContent ?? '';
  fireEvent.blur(gatilho);
  return texto;
}

describe('os quatro números de decisão', () => {
  it('um fator que arredonda para zero diz "0%" — e nunca "-0%", que leria como queda', () => {
    // 0,998 é −0,2%: arredonda para zero.
    abrir({ fator: 0.998 });
    const dica = lerDica('Fonte e método: Demanda anual');
    expect(dica).toContain('0% com o momento do mercado');
    expect(dica).not.toContain('-0%');
    expect(dica).not.toContain('+0%');
  });

  it('o sinal continua onde há variação: 0,88 é "-12%"', () => {
    abrir({ fator: 0.88 });
    expect(lerDica('Fonte e método: Demanda anual')).toContain('-12% com o momento do mercado');
  });

  it('ENQUANTO CARREGA, a demanda anual mostra a espera — e não o motivo de uma falta que ninguém viu', () => {
    abrir({ demanda: null, carregando: true });
    const demanda = cartao('Demanda anual');
    expect(demanda).toHaveTextContent('carregando…');
    expect(within(demanda).queryByRole('button', { name: 'Por que demanda anual não aparece' })).toBeNull();
  });

  it('carregada e sem demanda, ela diz o motivo e a decisão que falta', () => {
    abrir({ demanda: null });
    expect(lerDica('Por que demanda anual não aparece')).toMatch(/D-P01/);
    expect(cartao('Demanda anual')).not.toHaveTextContent('carregando');
  });

  it('a dica da captura escreve a conta do SERVIDOR — só as categorias com demanda — e não o total do ART', () => {
    // QUATRO MÁQUINAS VENDIDAS, DUAS NA CONTA: a regra é só de trator (D-P01), e a colhedora e a máquina
    // sem classificação não têm demanda do outro lado. A frase vem pronta; a tela só junta o critério.
    const conta =
      'FRASE DO SERVIDOR — 2 máquinas vendidas da categoria Trator ÷ a demanda anual estimada da mesma categoria.';
    render(
      <KpisExecutivos
        numeros={{
          ...NUMEROS,
          capturaPercentual: { valor: 12.5, motivo: 'Nenhum', frase: '' },
          baseDaCaptura: { unidades: 2, categorias: ['Trator'], unidadesForaDaConta: 2, frase: conta },
        }}
        momento={momento(null)}
        demandaEstrutural={16}
        demandaDeSaoPaulo={null}
        carregando={false}
        procedenciaDaDemanda={null}
        maquinasVendidas={
          { unidades: 4, fraseDoCriterio: 'Pela data do faturamento (D-P08.1).' } as unknown as VendasDeMaquinaDoRecorte
        }
        procedenciaDasVendas={null}
      />,
    );

    const dica = lerDica('Fonte e método: Captura Tracbel');
    expect(dica).toContain(conta);
    expect(dica).toContain('D-P08.1');
    expect(dica).not.toContain('4 máquinas vendidas');
  });

  it('a captura e a oportunidade dizem que a demanda é a do período, proporcional aos meses (27/09/2026)', () => {
    abrir();
    expect(lerDica('Fonte e método: Captura Tracbel')).toMatch(/demanda estimada DO PERÍODO.*proporcional aos meses/);
    expect(lerDica('Fonte e método: Oportunidade')).toMatch(/proporcional aos meses do período/);
  });

  it('com o filtro de tipo de produto, o fator agregado é dito como o do recorte inteiro, e não o desta demanda', () => {
    render(
      <KpisExecutivos
        numeros={NUMEROS}
        momento={momento(0.88)}
        demandaEstrutural={400}
        demandaDeSaoPaulo={null}
        carregando={false}
        procedenciaDaDemanda={null}
        maquinasVendidas={null}
        procedenciaDasVendas={null}
        tipoDeProduto="Trator"
      />,
    );

    const dica = lerDica('Fonte e método: Demanda anual');
    expect(dica).toContain('recorte inteiro, com todas as categorias, é -12%');
    expect(dica).toContain('Esta demanda é só a de Trator');
    expect(dica).not.toContain('aplicado a esta demanda');
  });
});
