/**
 * O PREÇO DE COMMODITIES NA TELA (issue 260): os quatro números da cultura no horizonte escolhido, o R12 que vem da PAM dito
 * no cartão e na tabela, a tabela ordenada pelo horizonte, o horizonte sem série com o motivo, e a escolha da cultura pela
 * tabela.
 */

import { fireEvent, render, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { IndiceDeMomento, PrecosDasCulturas } from '../tipos/mercado';
import { PrecoDeCommodities } from './PrecoDeCommodities';

// O GRÁFICO TEM TAMANHO MEDIDO (ResizeObserver) E DESENHA EM CANVAS — o jsdom não tem nenhum dos dois.
vi.mock('../componentes/MolduraDeGrafico', () => ({ MolduraDeGrafico: () => <div data-grafico="preco" /> }));

const indice = (valor: number | null, recente: number | null, anterior: number | null, faixa: IndiceDeMomento['faixa'], serie = 'Mensal'): IndiceDeMomento => ({
  indice: valor,
  faixa,
  mediaRecente: recente,
  mediaAnterior: anterior,
  mesesRecentes: 12,
  mesesAnteriores: 12,
  motivo: valor === null ? 'SerieCurta' : 'Nenhum',
  serie,
  anoRecente: serie === 'AnualPam' ? 2025 : null,
});

const PRECOS: PrecosDasCulturas = {
  ultimoMesDePreco: '2026-08-01',
  culturas: [
    {
      codigo: 'SOJA', nome: 'Soja', fonte: 'CONAB', unidade: 'kg', ultimoMes: '2026-08-01', ultimoValor: 2.2,
      horizontes: [
        { meses: 1, indice: indice(1, 2.2, 2.2, 'Intermediaria') },
        { meses: 3, indice: indice(1.02, 2.2, 2.15, 'Intermediaria') },
        { meses: 6, indice: indice(1.1, 2.2, 2.0, 'Intermediaria') },
        { meses: 12, indice: indice(0.95, 2.1, 2.21, 'Retraido', 'AnualPam') },
      ],
      serie: [{ mes: '2026-07-01', valor: 2.2 }, { mes: '2026-08-01', valor: 2.2 }],
    },
    {
      codigo: 'CANA', nome: 'Cana-de-açúcar', fonte: 'SOCICANA', unidade: 'kg de ATR', ultimoMes: '2026-08-01', ultimoValor: 1.1,
      horizontes: [
        { meses: 1, indice: indice(1, 1.1, 1.1, 'Intermediaria') },
        { meses: 3, indice: indice(null, null, null, null) },
        { meses: 6, indice: indice(1.3, 1.1, 0.85, 'Aquecido') },
        { meses: 12, indice: indice(1.1, 1.1, 1.0, 'Intermediaria') },
      ],
      serie: [{ mes: '2026-07-01', valor: 1.1 }, { mes: '2026-08-01', valor: 1.1 }],
    },
  ],
  lacunas: [{ metrica: 'r12Anual', motivo: 'O R12 da Soja é o preço anual da PAM.' }],
};

function montar() {
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string) =>
      entrada.includes('/v1/mercado/precos')
        ? new Response(JSON.stringify({ dados: PRECOS, procedencia: null }), { status: 200 })
        : new Response('{"title":"não simulado"}', { status: 404 }),
    ),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <PrecoDeCommodities />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
}

afterEach(() => vi.unstubAllGlobals());

const aTabela = () => waitFor(() => expect(document.querySelector('[data-bloco="momento-por-cultura"] table')).not.toBeNull());
const kpi = (rotulo: RegExp) => within(document.querySelector('[data-bloco="kpis"]') as HTMLElement).getByText(rotulo).closest('[data-kpi]');
const bloco = (nome: string) => document.querySelector(`[data-bloco="${nome}"]`) as HTMLElement;

describe('Preço de Commodities (issue 260)', () => {
  it('os quatro números da primeira cultura no R12, com o R12 da PAM dito', async () => {
    montar();
    await aTabela();

    expect(kpi(/Preço médio atual/)).toHaveTextContent('R$ 2,100 / kg');
    expect(kpi(/Variação — R12/)).toHaveTextContent('−5,0%');
    expect(kpi(/R12 — renda do ciclo/)).toHaveTextContent('preço anual da PAM');
    expect(kpi(/R12 — renda do ciclo/)).toHaveTextContent('tendência R3 ↑ +2,0%');
  });

  it('a tabela ordena pelo horizonte escolhido e marca o R12 que vem da PAM', async () => {
    montar();
    await aTabela();

    const tabela = bloco('momento-por-cultura');
    const nomes = () => [...tabela.querySelectorAll('tbody .prc-cultura')].map((e) => e.textContent);
    // NO R12, A CANA (+10%) VEM ANTES DA SOJA (−5%).
    expect(nomes()).toEqual(['Cana-de-açúcar', 'Soja']);
    expect(within(tabela).getByText('Soja').closest('tr')).toHaveTextContent('PAM');

    fireEvent.change(bloco('horizonte').querySelector('select')!, { target: { value: '1' } });
    expect(within(tabela).getByText('Cana-de-açúcar').closest('tr')).toHaveTextContent('Intermediária');
  });

  it('escolher a cultura na tabela troca os números do topo, e o horizonte sem série diz o motivo', async () => {
    montar();
    await aTabela();

    fireEvent.click(within(bloco('momento-por-cultura')).getByText('Cana-de-açúcar'));
    expect(kpi(/Preço médio atual/)).toHaveTextContent('Cana-de-açúcar');
    expect(kpi(/Variação — R12/)).toHaveTextContent('+10,0%');

    fireEvent.change(bloco('horizonte').querySelector('select')!, { target: { value: '3' } });
    expect(kpi(/Variação — R3/)).toHaveTextContent('—');
  });
});
