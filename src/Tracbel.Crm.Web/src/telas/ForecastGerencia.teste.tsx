/**
 * O FORECAST DA GERÊNCIA NA TELA (28/09/2026): cada gestor abre o grupo com a soma do time, o forecast não informado
 * mostra o traço com o motivo (e não zero), trocar o mês pede de novo, a recusa da API aparece como falta de permissão,
 * e o CSV leva o que está na tela.
 *
 * O caminho é o de produção: a rota é lida por um `fetch` de mentira, que anota o que a tela pediu. Nomes inventados.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { RelatorioDoForecast } from '../tipos/forecast';
import { doPo, ForecastGerencia, linhasDoCsv, somar } from './ForecastGerencia';

const guardado = new Map<string, string>();
const pedidos: string[] = [];

const FORECAST: RelatorioDoForecast = {
  competencia: '2026-09-01',
  texto: 'set/2026',
  mesesDisponiveis: ['2026-08-01', '2026-09-01'],
  alcance: 'Filiais',
  gestores: [
    {
      gestor: 'GESTOR.NORTE',
      consultores: 4,
      linhas: [
        { codigo: 'TRATOR_MEDIO', nome: 'TRATOR MÉDIO', meta: 10, forecast: 8, bestGuess: 9, realizado: 6 },
        { codigo: 'PULVERIZADOR', nome: 'PULVERIZADOR', meta: 2, forecast: null, bestGuess: 1, realizado: 1 },
      ],
    },
    {
      gestor: 'GESTOR.SUL',
      consultores: 2,
      linhas: [{ codigo: 'TRATOR_MEDIO', nome: 'TRATOR MÉDIO', meta: 0, forecast: null, bestGuess: null, realizado: 2 }],
    },
  ],
  total: [
    { codigo: 'TRATOR_MEDIO', nome: 'TRATOR MÉDIO', meta: 10, forecast: 8, bestGuess: 9, realizado: 9 },
    { codigo: 'PULVERIZADOR', nome: 'PULVERIZADOR', meta: 2, forecast: null, bestGuess: 1, realizado: 1 },
  ],
  vendasSemGestor: 1,
  lidoEm: '2026-09-28T09:00:00Z',
  geradoNaOrigemEm: '2026-09-28T04:30:00Z',
  metricasSemDado: [
    { metrica: 'alcanceDasFiliais', motivo: 'O PO e o realizado são só da filial escolhida.' },
    { metrica: 'observacao', motivo: 'A observação que o gestor escreve na GN não é lida pelo CRM.' },
  ],
};

function montar(resposta: () => Response = () => new Response(JSON.stringify({ dados: FORECAST, procedencia: null }), { status: 200 })) {
  vi.stubGlobal('localStorage', {
    getItem: (chave: string) => guardado.get(chave) ?? null,
    setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
    removeItem: (chave: string) => void guardado.delete(chave),
    clear: () => guardado.clear(),
  });
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string) => {
      pedidos.push(entrada);
      return entrada.includes('/v1/relatorios/forecast') ? resposta() : new Response('{"title":"não simulado"}', { status: 404 });
    }),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <ForecastGerencia />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
}

beforeEach(() => {
  guardado.clear();
  pedidos.length = 0;
});
afterEach(() => vi.unstubAllGlobals());

describe('Forecast da Gerência', () => {
  it('cada gestor abre o grupo com a soma do time, e o total fecha a tabela', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());

    const grupos = within(screen.getByRole('table')).getAllByRole('rowgroup').slice(1);
    expect(grupos).toHaveLength(3);

    const norte = within(grupos[0]!).getAllByRole('row')[0]!;
    expect(norte).toHaveTextContent('GESTOR.NORTE');
    expect(norte).toHaveTextContent('4 consultor(es) no time');
    expect(within(norte).getAllByRole('cell').map((c) => c.textContent)).toEqual(['12', '8', '10', '7', '58%']);

    const total = within(grupos[2]!).getAllByRole('row')[0]!;
    expect(total).toHaveTextContent('com 1 venda(s) sem gestor');
    expect(screen.getByText('Previsão por gestor · set/2026')).toBeInTheDocument();
  });

  it('o forecast não informado mostra o traço com o motivo, e não zero', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());

    expect(screen.getAllByRole('button', { name: 'Por que o forecast não aparece' }).length).toBeGreaterThan(0);
    expect(screen.getAllByRole('button', { name: 'Por que o realizado sobre o PO não aparece' })).toHaveLength(2);
  });

  it('trocar o mês pede o forecast daquele mês', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());

    fireEvent.change(screen.getByLabelText('Mês'), { target: { value: '2026-08-01' } });

    await waitFor(() => expect(pedidos.some((p) => p.includes('competencia=2026-08'))).toBe(true));
  });

  it('a recusa da API aparece como falta de permissão, e não como tabela vazia', async () => {
    montar(() => new Response(JSON.stringify({ title: 'Sem permissão', detail: 'O forecast é da gerência.' }), { status: 403 }));

    await waitFor(() => expect(screen.getByText('Sem permissão para esta consulta')).toBeInTheDocument());
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });
});

describe('funções do forecast', () => {
  it('a soma deixa nulo o que ninguém informou', () => {
    expect(somar(FORECAST.gestores[1]!.linhas)).toEqual({ meta: 0, forecast: null, bestGuess: null, realizado: 2 });
    expect(somar(FORECAST.gestores[0]!.linhas)).toEqual({ meta: 12, forecast: 8, bestGuess: 10, realizado: 7 });
  });

  it('sem PO não há razão', () => {
    expect(doPo(2, 0)).toBeNull();
    expect(doPo(null, 10)).toBeNull();
    expect(doPo(7, 12)).toBe('58%');
  });

  it('o CSV leva uma linha por gestor e produto, o total no fim, e o não informado vazio', () => {
    const linhas = linhasDoCsv(FORECAST);
    expect(linhas).toHaveLength(5);
    expect(linhas[1]).toEqual(['GESTOR.NORTE', 'PULVERIZADOR', 2, '', 1, 1]);
    expect(linhas[3]![0]).toBe('TOTAL');
  });
});
