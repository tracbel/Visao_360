/**
 * O ESTOQUE E A COBERTURA NA TELA (28/09/2026): os grupos com a cobertura ao lado, a lista no recorte escolhido, o pedido
 * à fábrica com a chegada no lugar dos dias, a máquina parada em destaque, o que não foi lido dito como tal, e o CSV com
 * o que está na tela. O caminho é o de produção, com um `fetch` de mentira. Modelos e chassis inventados.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { EstoqueECobertura as Estoque, MaquinaNaLista } from '../tipos/estoque';
import { EstoqueECobertura, filtrar, linhasDoCsv } from './EstoqueECobertura';

const guardado = new Map<string, string>();
const pedidos: string[] = [];

function maquina(parcial: Partial<MaquinaNaLista> & { descricao: string }): MaquinaNaLista {
  return {
    filial: 'Ribeirão Preto', grupo: 'TRATOR 6000', configuracao: null, situacao: 'Estoque', tipo: 'MÁQUINA', ehUsado: false,
    anoModelo: '2026/2026', chassi: '1PY000001', entradaEm: '2026-08-29', diasNoPatio: 30, chegadaPrevistaEm: null,
    faturamentoPrevistoEm: null, pago: true, reservado: false, ehPedidoAFabrica: false, situacaoNaFabrica: null, ...parcial,
  };
}

const ESTOQUE: Estoque = {
  alcance: 'Filiais',
  hoje: '2026-09-28',
  totais: { noPatio: 2, disponiveis: 1, reservadas: 1, pagas: 2, maisDe180Dias: 1, pedidosAFabrica: 1 },
  porGrupo: [{ grupo: 'TRATOR 6000', noPatio: 2, disponiveis: 1, reservadas: 1, pedidosAFabrica: 1, idadeMediaEmDias: 115, coberturaEmMeses: 2.25 }],
  maquinas: [
    maquina({ descricao: 'TR 6155M' }),
    maquina({ descricao: 'TR 6190M', reservado: true, diasNoPatio: 200 }),
    maquina({ descricao: 'TR 6125J', situacao: 'PEDIDO', ehPedidoAFabrica: true, diasNoPatio: null, entradaEm: null, chegadaPrevistaEm: '2026-11-30', situacaoNaFabrica: 'Confirmado', chassi: null }),
  ],
  cobertura: {
    porMes: [
      { chave: '2026-07', competencia: '2026-07-01', meses: 3.59, vendas: 143 },
      { chave: '2026-08', competencia: '2026-08-01', meses: 3.12, vendas: 152 },
    ],
    porGrupo: [{ chave: 'TRATOR 6000', competencia: null, meses: 2.25, vendas: 165 }],
    mediaPorMes: 3.36,
    mediaPorGrupo: 2.25,
    lidaEm: '2026-09-28T12:00:00Z',
    geradaNaOrigemEm: null,
  },
  lidoEm: '2026-09-28T12:00:00Z',
  geradoNaOrigemEm: null,
  metricasSemDado: [{ metrica: 'valor', motivo: 'O valor do estoque não aparece: está a custo no TOTVS.' }],
};

function montar(dados: Estoque = ESTOQUE) {
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
      return entrada.includes('/v1/relatorios/estoque')
        ? new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 })
        : new Response('{"title":"não simulado"}', { status: 404 });
    }),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <EstoqueECobertura />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
}

beforeEach(() => {
  guardado.clear();
  pedidos.length = 0;
});
afterEach(() => vi.unstubAllGlobals());

const modelos = () =>
  within(screen.getByRole('table', { name: /Máquinas do estoque/ })).getAllByRole('row').slice(1).map((l) => l.querySelector('td')?.firstChild?.textContent);

describe('Estoque e Cobertura', () => {
  it('abre no pátio, com a máquina parada em destaque e a reserva escrita', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table', { name: /Máquinas do estoque/ })).toBeInTheDocument());

    expect(modelos()).toEqual(['TR 6155M', 'TR 6190M']);
    expect(screen.getByText('200 dias')).toHaveClass('est-velha');
    expect(screen.getByText('reservada')).toBeInTheDocument();
    expect(pedidos.some((p) => p.includes('/v1/relatorios/estoque'))).toBe(true);
  });

  it('o grupo mostra a cobertura ao lado, e as barras da cobertura trazem a média', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table', { name: /por grupo/ })).toBeInTheDocument());

    const grupo = within(screen.getByRole('table', { name: /por grupo/ })).getAllByRole('row')[1]!;
    expect(grupo).toHaveTextContent('115 dias');
    expect(grupo).toHaveTextContent('2,3 meses');
    expect(screen.getByText('média 3,4 meses')).toBeInTheDocument();
    expect(screen.getByText('ago/26')).toBeInTheDocument();
  });

  it('os pedidos à fábrica mostram a chegada, e não os dias', async () => {
    montar();
    await waitFor(() => expect(screen.getByLabelText('Mostrar')).toBeInTheDocument());

    fireEvent.change(screen.getByLabelText('Mostrar'), { target: { value: 'pedidos' } });

    expect(modelos()).toEqual(['TR 6125J']);
    expect(screen.getByText('chega 30/11/2026')).toBeInTheDocument();
    expect(screen.getByText('Confirmado')).toBeInTheDocument();
  });

  it('sem leitura, os indicadores dizem que o estoque não foi lido, e não zero', async () => {
    montar({ ...ESTOQUE, lidoEm: null, maquinas: [], porGrupo: [], totais: { ...ESTOQUE.totais, noPatio: 0 } });
    await waitFor(() => expect(screen.getAllByText('o estoque ainda não foi lido').length).toBeGreaterThan(0));
  });
});

describe('funções do estoque', () => {
  it('o recorte e a busca agem juntos', () => {
    expect(filtrar(ESTOQUE.maquinas, 'disponiveis', '', '').map((m) => m.descricao)).toEqual(['TR 6155M']);
    expect(filtrar(ESTOQUE.maquinas, 'todas', '', '6190').map((m) => m.descricao)).toEqual(['TR 6190M']);
    expect(filtrar(ESTOQUE.maquinas, 'patio', 'AMS', '')).toEqual([]);
  });

  it('o CSV escreve o pedido à fábrica por extenso e deixa vazio o que não há', () => {
    const [pedido] = linhasDoCsv([ESTOQUE.maquinas[2]!]);
    expect(pedido![4]).toBe('Pedido à fábrica');
    expect(pedido![9]).toBe('');
    expect(pedido![12]).toBe('30/11/2026');
  });
});
