/**
 * O TERMO DE TROCA (fidelidade às maquetes, fase 3): o preço da commodity sai
 * de verdade; o preço do trator é a mediana das notas de venda (issue 70, D-P06
 * e D-P12). Sem a série do trator, tudo que depende dele sai com o traço e o
 * motivo; com ela, as sacas são a divisão das duas no MESMO mês.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../../dados/api/contexto';
import type { SerieDePreco, SerieDePrecoDeMaquina } from '../../../tipos/mercado';
import type { CulturaNoCatalogo } from '../../../tipos/potencial';
import { AbaTermoDeTroca } from './AbaTermoDeTroca';

const obterPrecosDeMercado = vi.hoisted(() => vi.fn());
const obterCatalogoDoMercado = vi.hoisted(() => vi.fn());

vi.mock('../../../dados/api/territorio', async (original) => ({
  ...(await original<typeof import('../../../dados/api/territorio')>()),
  obterPrecosDeMercado,
}));
vi.mock('../../../dados/api/potencial', async (original) => ({
  ...(await original<typeof import('../../../dados/api/potencial')>()),
  obterCatalogoDoMercado,
}));

// O GRÁFICO TEM TAMANHO MEDIDO (ResizeObserver) E DESENHA EM CANVAS — o jsdom não tem nenhum dos dois. Aqui o que
// está sob prova são os números; a moldura diz só que o gráfico existe.
vi.mock('../../MolduraDeGrafico', () => ({ MolduraDeGrafico: () => <div data-grafico="termo-de-troca" /> }));

const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (c: string) => guardado.get(c) ?? null,
  setItem: (c: string, v: string) => void guardado.set(c, v),
  removeItem: (c: string) => void guardado.delete(c),
  clear: () => guardado.clear(),
});

function serie(codigo: string, porKg: number): SerieDePreco {
  return {
    fonte: 'CONAB',
    codigoNaFonte: codigo,
    nivel: 'Produtor',
    produto: codigo,
    classificacao: 'tipo único',
    unidade: 'kg',
    unidadeComercial: 'saca de 60 kg',
    fatorComercial: 60,
    meses: [
      { mes: '2025-08-01', valorEmReais: porKg * 0.9, valorEmDolares: null },
      { mes: '2026-08-01', valorEmReais: porKg, valorEmDolares: null },
    ],
    procedencia: null,
  };
}

function cultura(codigo: string, nome: string, produtoDoPreco: string | null): CulturaNoCatalogo {
  return {
    codigo,
    nome,
    segmento: 'lavoura',
    unidadeComercial: 'saca de 60 kg',
    quilosPorUnidade: 60,
    fonteDoPreco: 'CONAB',
    produtoDoPreco,
    serieDeCusto: null,
    estaAtiva: true,
    produtos: [],
  };
}

/** O trator base: a mediana dos tratores vendidos em agosto de 2025 e de 2026 — e uma colheitadeira, que não entra. */
const MAQUINAS: SerieDePrecoDeMaquina[] = [
  {
    categoriaCodigo: 'TRATOR',
    categoriaNome: 'Trator',
    meses: [
      { mes: '2025-08-01', mediana: 400_000, menor: 380_000, maior: 500_000, notas: 3 },
      { mes: '2026-08-01', mediana: 480_000, menor: 450_000, maior: 520_000, notas: 4 },
    ],
    procedencia: null,
  },
  {
    categoriaCodigo: 'COLHEITADEIRA',
    categoriaNome: 'Colheitadeira',
    meses: [{ mes: '2026-08-01', mediana: 3_000_000, menor: 3_000_000, maior: 3_000_000, notas: 1 }],
    procedencia: null,
  },
];

function abrir(maquinas: SerieDePrecoDeMaquina[] = []) {
  obterPrecosDeMercado.mockResolvedValue({
    dados: { series: [serie('SOJA-CONAB', 2), serie('MILHO-CONAB', 1)], primeiroMesDoDolar: null, ultimoMesDoDolar: null, maquinas },
    procedencia: null,
  });
  obterCatalogoDoMercado.mockResolvedValue({
    dados: {
      culturas: [
        cultura('SOJA', 'Soja', 'SOJA-CONAB'),
        cultura('MILHO', 'Milho', 'MILHO-CONAB'),
        // COM PREÇO DECLARADO E SEM SÉRIE CARREGADA: a linha fica, com o traço.
        cultura('CAFE', 'Café', 'CAFE-CONAB'),
        // SEM FONTE DE PREÇO: não é cultura de troca.
        cultura('PASTO', 'Pastagem', null),
      ],
      categorias: [],
    },
    procedencia: null,
  });
  render(
    <ProvedorDeContextoDeAcesso>
      <AbaTermoDeTroca />
    </ProvedorDeContextoDeAcesso>,
  );
}

describe('a aba Termo de troca', () => {
  afterEach(() => {
    obterPrecosDeMercado.mockReset();
    obterCatalogoDoMercado.mockReset();
    guardado.clear();
  });

  it('o preço da commodity sai de verdade, na unidade do mercado', async () => {
    abrir();
    const soja = await screen.findByText('R$ 120,00');
    expect(soja.closest('tr')).toHaveTextContent('Soja');
    expect(soja.closest('tr')).toHaveTextContent('saca de 60 kg');
  });

  it('as culturas são as do catálogo com fonte de preço — com ou sem série', async () => {
    abrir();
    await screen.findByText('R$ 120,00');

    const nomes = [...document.querySelectorAll<HTMLElement>('table.mom-tabela tbody th')].map((n) => n.textContent);
    expect(nomes).toEqual(['Soja', 'Milho', 'Café']);
  });

  it('sem a série do trator, sacas, variação, melhor cultura e tendência saem com o traço e o motivo, nunca número', async () => {
    abrir();
    await screen.findByText('R$ 120,00');

    for (const rotulo of ['Sacas para comprar 1 trator', 'Variação (5 anos)', 'Melhor cultura de troca', 'Tendência atual']) {
      const cartao = document.querySelector<HTMLElement>(`[data-cartao="${rotulo}"]`)!;
      expect(cartao.querySelector('.mom-cartao-valor')!.textContent, rotulo).not.toMatch(/\d/);
    }

    const cartao = document.querySelector<HTMLElement>('[data-cartao="Sacas para comprar 1 trator"]')!;
    fireEvent.focus(within(cartao).getByRole('button', { name: 'Por que as sacas por trator não aparece' }));
    expect(screen.getByRole('tooltip')).toHaveTextContent(/issue 70/);
    fireEvent.blur(within(cartao).getByRole('button', { name: 'Por que as sacas por trator não aparece' }));

    // Na tabela, o preço do trator e as sacas: traço em todas as linhas.
    for (const tr of document.querySelectorAll<HTMLElement>('table.mom-tabela tbody tr')) {
      const celulas = tr.querySelectorAll('td');
      expect(celulas[1].textContent).toContain('—');
      expect(celulas[2].textContent).toContain('—');
    }
  });

  it('com o trator base, as sacas são a divisão das duas séries no MESMO mês', async () => {
    abrir(MAQUINAS);
    await screen.findByText('R$ 120,00');

    // SOJA A R$ 120 A SACA E TRATOR A R$ 480 MIL EM AGOSTO DE 2026: 4.000 sacas. A colheitadeira não entra — o trator
    // base é a mediana dos tratores (D-P06).
    const sacas = document.querySelector<HTMLElement>('[data-cartao="Sacas para comprar 1 trator"]')!;
    expect(sacas.querySelector('.mom-cartao-valor')).toHaveTextContent('4.000');
    expect(sacas.querySelector('.mom-cartao-valor')).toHaveTextContent('saca de 60 kg');
    expect(sacas.querySelector('.mom-cartao-apoio')).toHaveTextContent('de Soja · trator de ago/26');

    // O MILHO A R$ 60 PEDE O DOBRO: a melhor troca é a soja.
    expect(document.querySelector('[data-cartao="Melhor cultura de troca"] .mom-cartao-valor')).toHaveTextContent('Soja');

    // CINCO ANOS E O TRIMESTRE NÃO EXISTEM NAS DUAS SÉRIES: traço, e não o mês mais perto.
    for (const rotulo of ['Variação (5 anos)', 'Tendência atual'])
      expect(document.querySelector(`[data-cartao="${rotulo}"] .mom-cartao-valor`)!.textContent, rotulo).not.toMatch(/\d/);

    // O ANO: soja a R$ 108 e trator a R$ 400 mil em agosto de 2025 — 3.703,7 sacas; hoje 4.000, +8%.
    const soja = document.querySelector<HTMLElement>('table.mom-tabela tbody tr[data-cultura="SOJA"]')!;
    const celulas = soja.querySelectorAll('td');
    expect(celulas[1]).toHaveTextContent('R$ 480.000');
    expect(celulas[2]).toHaveTextContent('4.000');
    expect(celulas[3]).toHaveTextContent('+8%');

    // A CULTURA SEM SÉRIE DE PREÇO CONTINUA COM O TRAÇO.
    const cafe = document.querySelector<HTMLElement>('table.mom-tabela tbody tr[data-cultura="CAFE"]')!;
    expect(cafe.querySelectorAll('td')[2].textContent).toContain('—');

    expect(document.querySelector('[data-grafico="termo-de-troca"]')).not.toBeNull();
  });

  it('o ⋮ diz o mês e a variação do PREÇO — e que a do termo espera o trator', async () => {
    abrir();
    await screen.findByText('R$ 120,00');

    fireEvent.focus(screen.getByRole('button', { name: 'O preço de Soja' }));
    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent('ago/26');
    expect(dica).toHaveTextContent('+11,1%');
    expect(dica).toHaveTextContent(/issue 70/);
  });
});
