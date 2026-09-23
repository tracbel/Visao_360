/**
 * Um cartão de mapa (issue 170, parte A).
 *
 * O mapa de cobertura serve de prova da casca que os quatro compartilham: o
 * alternador troca a medida, a legenda acompanha, e a linha do cursor diz o
 * número do município sob ele.
 */

import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { CAFELANDIA, municipioDeTeste } from '../../../testes/territorio';
import type { LigacaoDoMapa } from './CartaoDeMapa';
import { MapaDeCobertura } from './MapaDeCobertura';
import { calcularTotais } from '../totaisDaAdr';

const MUNICIPIO = municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' });

function ligacao(parcial: Partial<LigacaoDoMapa> = {}): LigacaoDoMapa {
  return {
    enquadramento: { largura: 520, altura: 400, oeste: -50, norte: -20, escala: 100, cossenoDaLatitude: 0.94, margem: 10 },
    poligonos: [{ codigo: CAFELANDIA, nome: 'Cafelândia', caminho: 'M0,0 L10,0 L10,10 Z' }],
    adr: new Set([CAFELANDIA]),
    porCodigo: new Map([[CAFELANDIA, MUNICIPIO]]),
    nomeDoPoligono: new Map([[CAFELANDIA, 'Cafelândia']]),
    selecionado: null,
    aoSelecionar: vi.fn(),
    emFoco: null,
    aoPassar: vi.fn(),
    ...parcial,
  };
}

function montar(parcial: Partial<LigacaoDoMapa> = {}) {
  render(
    <MapaDeCobertura
      ligacao={ligacao(parcial)}
      totais={calcularTotais([MUNICIPIO])}
      coberturaDaAdr={(100 * 11) / 18}
      classificacao={{
        indicador: 'coberturaDeVisita',
        situacao: 'RegraComercialProvisoria',
        selo: 'Regra provisória',
        motivo: 'a cadência ainda não foi confirmada',
      }}
    />,
  );
}

/** A linha do resumo, acima do alternador. */
const resumo = () => document.querySelector('.terr-mapa-resumo')!.textContent ?? '';

/** A linha que segue o cursor, abaixo do mapa. */
const linhaDoCursor = () => document.querySelector('.terr-mapa-foco')!.textContent ?? '';

describe('o cartão do mapa de cobertura', () => {
  // O CONTRATO MUDOU NA FASE T2: o motivo do selo saía num `title=`. Agora o
  // próprio selo é o gatilho de uma dica, e abre pelo teclado (issue 167).
  it('mostra o selo de como ler o número, e o motivo abre pelo teclado', () => {
    montar();

    const selo = screen.getByRole('button', { name: 'Por que este número está marcado como Regra provisória' });
    expect(selo).toHaveTextContent('Regra provisória');

    fireEvent.focus(selo);
    expect(screen.getByRole('tooltip')).toHaveTextContent('a cadência ainda não foi confirmada');
  });

  it('o resumo traz elegíveis, no prazo e pendentes', () => {
    montar();
    // Escopado ao resumo: o mesmo texto aparece no `<title>` de cada polígono do
    // SVG, que é o que o leitor de tela lê ao entrar no mapa.
    expect(resumo()).toMatch(/18 elegíveis · 11 no prazo/);
  });

  it('o alternador troca a medida, e a legenda troca junto', () => {
    montar();

    expect(screen.getByText(/verde é mais coberto/)).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: '% pendente' }));
    expect(screen.getByText(/vermelho é mais pendente/)).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'pendentes (qtd.)' }));
    expect(screen.getByText(/vínculos pendentes \(fora do prazo \+ nunca contatados\)/)).toBeInTheDocument();
  });

  it('o aviso muda com a medida: quantidade favorece cidade grande', () => {
    montar();
    expect(screen.getByText(/Percentual compara municípios de tamanhos diferentes/)).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'pendentes (qtd.)' }));
    expect(screen.getByText(/Quantidade favorece cidades grandes/)).toBeInTheDocument();
  });

  it('sem cursor, a linha convida a passar por cima', () => {
    montar();
    expect(linhaDoCursor()).toMatch(/Passe o cursor sobre um município/);
  });

  it('com o município em foco, a linha do cursor traz o detalhe dele', () => {
    montar({ emFoco: CAFELANDIA });
    expect(linhaDoCursor()).toMatch(/^Cafelândia — 18 elegíveis/);
  });

  it('município fora da ADR é dito como fora, e não como sem dado', () => {
    const fora = municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia', pertenceAAdr: false });
    montar({ emFoco: CAFELANDIA, porCodigo: new Map([[CAFELANDIA, fora]]) });

    expect(linhaDoCursor()).toBe('Cafelândia — fora da ADR');
  });
});
