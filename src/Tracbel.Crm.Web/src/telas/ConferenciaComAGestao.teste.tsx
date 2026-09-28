/**
 * A CONFERÊNCIA COM A GESTÃO DE NEGÓCIOS NA TELA (28/09/2026): a filial com os dois lados e a diferença, a diferença zero
 * escrita como "bate", o filtro das máquinas que não batem, e a tela que diz que não foi apurada. Chassis inventados.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { ConferenciaComAGestao as Conferencia } from '../tipos/conferencia';
import { ConferenciaComAGestao, diferenca, filtrar } from './ConferenciaComAGestao';

const guardado = new Map<string, string>();

const CONFERENCIA: Conferencia = {
  alcance: 'Filiais',
  totais: { metaNaGestao: 10, metaNoCrm: 10, realizadoNaGestao: 8, realizadoNoCrm: 6 },
  porFilial: [{ filial: 'Ribeirão Preto', numeros: { metaNaGestao: 10, metaNoCrm: 10, realizadoNaGestao: 8, realizadoNoCrm: 6 } }],
  porMes: [{ competencia: '2026-08-01', numeros: { metaNaGestao: 10, metaNoCrm: 10, realizadoNaGestao: 8, realizadoNoCrm: 6 } }],
  porTipo: [
    { tipo: 'RealizadoSoNaGestao', rotulo: 'Só na Gestão de Negócios', quantidade: 1 },
    { tipo: 'RealizadoPendenteNoArt', rotulo: 'Pendente na integração do ART', quantidade: 1 },
  ],
  divergencias: [
    { tipo: 'RealizadoPendenteNoArt', rotulo: 'Pendente na integração do ART', chassi: '1PY0000001', filial: 'Ribeirão Preto', descricao: 'A venda está no ART, mas pendente.', noCrm: 'pendente: COMPRADOR_AUSENTE_NO_CRM', naGestao: 'Ribeirão Preto · 2026-08', detectadaEm: '2026-09-28T10:00:00Z' },
    { tipo: 'RealizadoSoNaGestao', rotulo: 'Só na Gestão de Negócios', chassi: '1PY0000002', filial: 'Ribeirão Preto', descricao: 'Só a GN conta.', noCrm: null, naGestao: 'Ribeirão Preto · 2026-08', detectadaEm: '2026-09-28T10:00:00Z' },
  ],
  apuradaEm: '2026-09-28T10:15:00Z',
  geradaNaOrigemEm: null,
  metricasSemDado: [{ metrica: 'regua', motivo: 'Os dois lados contam pela mesma régua.' }],
};

function montar(dados: Conferencia = CONFERENCIA) {
  vi.stubGlobal('localStorage', {
    getItem: (chave: string) => guardado.get(chave) ?? null,
    setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
    removeItem: (chave: string) => void guardado.delete(chave),
    clear: () => guardado.clear(),
  });
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string) =>
      entrada.includes('/v1/integracoes/conferencia-gn')
        ? new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 })
        : new Response('{"title":"não simulado"}', { status: 404 }),
    ),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <ConferenciaComAGestao />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
}

beforeEach(() => guardado.clear());
afterEach(() => vi.unstubAllGlobals());

describe('Conferência com a Gestão de Negócios', () => {
  it('a filial mostra os dois lados, e a diferença zero é "bate"', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table', { name: /por filial/ })).toBeInTheDocument());

    const linha = within(screen.getByRole('table', { name: /por filial/ })).getAllByRole('row')[2]!;
    expect(within(linha).getAllByRole('cell').map((c) => c.textContent)).toEqual(['10', '10', 'bate', '8', '6', '−2']);
  });

  it('as máquinas que não batem filtram pelo tipo e pelo chassi, e o que o CRM não tem é traço', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table', { name: /não batem/ })).toBeInTheDocument());

    fireEvent.change(screen.getByLabelText('Tipo'), { target: { value: 'RealizadoSoNaGestao' } });

    const linhas = within(screen.getByRole('table', { name: /não batem/ })).getAllByRole('row').slice(1);
    expect(linhas).toHaveLength(1);
    expect(linhas[0]).toHaveTextContent('1PY0000002');
    expect(within(linhas[0]!).getAllByRole('cell')[3]).toHaveTextContent('—');
  });

  it('sem apuração, os indicadores dizem que não foi apurada', async () => {
    montar({ ...CONFERENCIA, apuradaEm: null, porFilial: [], porMes: [], divergencias: [] });
    await waitFor(() => expect(screen.getAllByText('ainda não apurada').length).toBeGreaterThan(0));
  });
});

describe('funções da conferência', () => {
  it('a diferença tem sinal, e zero bate', () => {
    expect(diferenca(6, 8)).toBe('−2');
    expect(diferenca(9, 8)).toBe('+1');
    expect(diferenca(8, 8)).toBe('bate');
  });

  it('a busca do chassi ignora a caixa', () => {
    expect(filtrar(CONFERENCIA.divergencias, '', '1py0000001').map((d) => d.chassi)).toEqual(['1PY0000001']);
  });
});
