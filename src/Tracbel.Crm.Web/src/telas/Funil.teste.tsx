/**
 * O FUNIL NA TELA (27/09/2026, documento 52) — os seis estágios do Vórtice, a chave coorte × fluxo e o vazio com o
 * motivo verdadeiro.
 *
 * O CAMINHO É O DE PRODUÇÃO: as rotas são lidas por um `fetch` de mentira que responde com as amostras do harness da
 * Visão 360, e anota o que a tela pediu.
 */

import { fireEvent, render, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { respostaDaVisao360, type EstadoDaVisao360 } from '../dev/amostrasDaVisao360';
import { Funil } from './Funil';

vi.setConfig({ testTimeout: 20_000 });

const guardado = new Map<string, string>();
const pedidos: string[] = [];

function montar(estado: EstadoDaVisao360) {
  vi.stubGlobal('localStorage', {
    getItem: (chave: string) => guardado.get(chave) ?? null,
    setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
    removeItem: (chave: string) => void guardado.delete(chave),
    clear: () => guardado.clear(),
  });
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string, init?: RequestInit) => {
      pedidos.push(entrada);
      const [caminho, consulta = ''] = entrada.replace(/^.*\/api/, '').split('?');
      const empresa = new Headers(init?.headers).get('X-Tracbel-Empresa') ?? '';
      const dados = respostaDaVisao360(caminho, new URLSearchParams(consulta), empresa, estado);
      return dados === undefined
        ? new Response('{"title":"não simulado"}', { status: 404 })
        : new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 });
    }),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <Funil />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
}

beforeEach(() => {
  guardado.clear();
  pedidos.length = 0;
});
afterEach(() => vi.unstubAllGlobals());

describe('Funil de Vendas — o funil por estágio (documento 52)', () => {
  it('abre na coorte do ano fiscal, com os seis estágios e os dois percentuais', async () => {
    const { container } = montar('completo');
    const bloco = await waitFor(() => {
      const b = container.querySelector('[data-bloco="funil-por-estagio"]') as HTMLElement;
      expect(b.querySelector('#funil-svg')).not.toBeNull();
      return b;
    });

    expect(bloco).toHaveTextContent('Coorte · nov/2025 a ago/2026 · ano fiscal até o último mês fechado');
    const linhas = within(bloco).getAllByRole('row').slice(1);
    expect(linhas.map((l) => l.querySelector('td')?.textContent)).toEqual([
      'Lead', 'Qualificado', 'Cobertura', 'Negociação', 'Pedido', 'Faturamento',
    ]);
    expect(linhas[0]).toHaveTextContent('100%');
    expect(pedidos.some((p) => p.includes('/relatorios/funil-por-estagio?base=abertura'))).toBe(true);
    expect(container).toHaveTextContent('Para quem perdemos');
  });

  it('a chave troca para o fluxo', async () => {
    const { container, getByRole } = montar('completo');
    await waitFor(() => expect(container.querySelector('#funil-svg')).not.toBeNull());

    fireEvent.click(getByRole('button', { name: 'Fluxo' }));

    await waitFor(() => expect(pedidos.some((p) => p.includes('base=etapa'))).toBe(true));
    await waitFor(() => expect(container.querySelector('[data-bloco="funil-por-estagio"]')).toHaveTextContent('Fluxo ·'));
  });

  it('sem a rotina ter rodado, cada estágio é "—" com o motivo, e não zero', async () => {
    const { container } = montar('vazio');
    const bloco = await waitFor(() => {
      const b = container.querySelector('[data-bloco="funil-por-estagio"]') as HTMLElement;
      expect(within(b).getAllByRole('row')).toHaveLength(7);
      return b;
    });

    expect(bloco.querySelector('#funil-svg')).toBeNull();
    expect(bloco.querySelectorAll('.cad-ausente').length).toBeGreaterThanOrEqual(6);
    expect(container).toHaveTextContent('ainda não rodou');
  });
});
