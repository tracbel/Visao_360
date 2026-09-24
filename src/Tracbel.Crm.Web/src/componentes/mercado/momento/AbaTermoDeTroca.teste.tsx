/**
 * O TERMO DE TROCA (fidelidade às maquetes, fase 3): o preço da commodity sai
 * de verdade; tudo que depende do preço do trator (issue 70) sai com o traço e
 * o motivo — e as culturas são as reais, do catálogo.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../../dados/api/contexto';
import type { SerieDePreco } from '../../../tipos/mercado';
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

function abrir() {
  obterPrecosDeMercado.mockResolvedValue({
    dados: { series: [serie('SOJA-CONAB', 2), serie('MILHO-CONAB', 1)], primeiroMesDoDolar: null, ultimoMesDoDolar: null },
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

  it('sacas, variação, melhor cultura e tendência dependem do trator: traço e issue 70, nunca número', async () => {
    abrir();
    await screen.findByText('R$ 120,00');

    for (const rotulo of ['Sacas para comprar 1 trator', 'Variação (5 anos)', 'Melhor cultura de troca', 'Tendência atual']) {
      const cartao = document.querySelector<HTMLElement>(`[data-cartao="${rotulo}"]`)!;
      expect(cartao.querySelector('.mom-cartao-valor')!.textContent, rotulo).not.toMatch(/\d/);
    }

    const cartao = document.querySelector<HTMLElement>('[data-cartao="Sacas para comprar 1 trator"]')!;
    fireEvent.focus(within(cartao).getByRole('button'));
    expect(screen.getByRole('tooltip')).toHaveTextContent(/issue 70/);
    fireEvent.blur(within(cartao).getByRole('button'));

    // Na tabela, o preço do trator e as sacas: traço em todas as linhas.
    for (const tr of document.querySelectorAll<HTMLElement>('table.mom-tabela tbody tr')) {
      const celulas = tr.querySelectorAll('td');
      expect(celulas[1].textContent).toContain('—');
      expect(celulas[2].textContent).toContain('—');
    }
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
