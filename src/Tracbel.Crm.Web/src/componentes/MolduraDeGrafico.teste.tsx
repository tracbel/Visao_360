/**
 * MEDIDA NOVA, GRÁFICO NOVO (29/09/2026).
 *
 * O defeito: na Visão 360, depois que o funil chegava, o alerta dos processos
 * parados esticava a linha do faturamento. A moldura media a altura nova, mas o
 * gráfico, de tamanho fixo, continuava com a medida com que nasceu. O desenho
 * saía cortado e o balão não aparecia mais.
 *
 * O jsdom não tem `ResizeObserver` nem medidas de layout: o observador é um
 * dublê que este teste dispara, e `clientWidth`/`clientHeight` dizem a medida da
 * vez.
 */

import { act, render, screen } from '@testing-library/react';
import { useState } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MolduraDeGrafico } from './MolduraDeGrafico';

let medida = { largura: 739, altura: 219 };
let avisarDaMudanca: (() => void) | null = null;

class ObservadorDeMentira {
  constructor(aviso: () => void) {
    avisarDaMudanca = aviso;
  }
  observe() {}
  disconnect() {}
}

beforeEach(() => {
  medida = { largura: 739, altura: 219 };
  vi.stubGlobal('ResizeObserver', ObservadorDeMentira);
  vi.spyOn(Element.prototype, 'clientWidth', 'get').mockImplementation(() => medida.largura);
  vi.spyOn(Element.prototype, 'clientHeight', 'get').mockImplementation(() => medida.altura);
});

afterEach(() => {
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
  avisarDaMudanca = null;
});

/** Anota cada vez que nasce, com a medida com que nasceu — como o Chart.js, que guarda o tamanho do primeiro desenho. */
function GraficoDeTamanhoFixo({ largura, altura, nascimentos }: { largura: number; altura: number; nascimentos: string[] }) {
  const [comQueNasceu] = useState(() => {
    nascimentos.push(`${largura}x${altura}`);
    return `${largura}x${altura}`;
  });
  return <canvas data-testid="grafico" data-nasceu={comQueNasceu} width={largura} height={altura} />;
}

function mudarAMedida(nova: { largura: number; altura: number }) {
  medida = nova;
  act(() => avisarDaMudanca?.());
}

describe('MolduraDeGrafico', () => {
  it('com preencher, monta o gráfico de novo quando a altura muda depois do desenho', () => {
    const nascimentos: string[] = [];
    render(
      <MolduraDeGrafico altura={150} preencher>
        {(l, a) => <GraficoDeTamanhoFixo largura={l} altura={a} nascimentos={nascimentos} />}
      </MolduraDeGrafico>,
    );
    expect(nascimentos).toEqual(['739x219']);

    // A linha cresceu: o alerta ganhou o texto dos processos parados.
    mudarAMedida({ largura: 739, altura: 380 });

    expect(nascimentos).toEqual(['739x219', '739x380']);
    expect(screen.getByTestId('grafico')).toHaveAttribute('data-nasceu', '739x380');
  });

  it('sem preencher, monta o gráfico de novo quando a largura muda', () => {
    const nascimentos: string[] = [];
    render(
      <MolduraDeGrafico altura={150}>
        {(l, a) => <GraficoDeTamanhoFixo largura={l} altura={a} nascimentos={nascimentos} />}
      </MolduraDeGrafico>,
    );
    expect(nascimentos).toEqual(['739x150']);

    mudarAMedida({ largura: 632, altura: 219 });

    expect(nascimentos).toEqual(['739x150', '632x150']);
    expect(screen.getByTestId('grafico')).toHaveAttribute('data-nasceu', '632x150');
  });

  it('a mesma medida não monta o gráfico de novo', () => {
    const nascimentos: string[] = [];
    render(
      <MolduraDeGrafico altura={150} preencher>
        {(l, a) => <GraficoDeTamanhoFixo largura={l} altura={a} nascimentos={nascimentos} />}
      </MolduraDeGrafico>,
    );

    mudarAMedida({ largura: 739, altura: 219 });

    expect(nascimentos).toEqual(['739x219']);
  });
});
