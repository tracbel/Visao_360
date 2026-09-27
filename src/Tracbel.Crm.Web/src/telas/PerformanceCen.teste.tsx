/**
 * A PERFORMANCE DE CEN MOSTRA A META DE VENDA DE CADA CONSULTOR (#138, 27/09/2026).
 *
 * Até aqui o rodapé dizia que a meta não tinha fonte. Tem: a cota da API Gestão de Negócios, em máquinas, contra as
 * vendas do ART em que o consultor é o vendedor (D-M2). Este teste prende:
 *
 * - a tabela meta × realizado por consultor, com o atingimento refeito na tela e o consultor sem conta nomeado;
 * - o cadastro não lido diz isso, e não "meta zero";
 * - o 403 é falta de permissão, sem o botão de tentar de novo;
 * - o "Atingimento de meta" saiu do rodapé do que a tela não sustenta.
 *
 * O CAMINHO É O DE PRODUÇÃO: a rota é lida por um `fetch` de mentira que responde com as amostras do harness da Visão
 * 360. Os gráficos entram como dublê — o jsdom não tem canvas.
 */

import { render, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { metasDeVenda, respostaDaVisao360, type EstadoDaVisao360 } from '../dev/amostrasDaVisao360';
import { PerformanceCen } from './PerformanceCen';

vi.setConfig({ testTimeout: 20_000 });

vi.mock('../componentes/GraficoBarrasEmpilhadas', () => ({ GraficoBarrasEmpilhadas: () => <div data-grafico="empilhadas" /> }));
vi.mock('../componentes/GraficoBarrasHorizontais', () => ({ GraficoBarrasHorizontais: () => <div data-grafico="barras" /> }));
vi.mock('../componentes/MolduraDeGrafico', () => ({
  MolduraDeGrafico: ({ children, altura }: { children: (l: number, a: number) => React.ReactNode; altura: number }) => (
    <div>{children(600, altura)}</div>
  ),
}));

const guardado = new Map<string, string>();

/** A filial do contexto padrão; a amostra responde a ela com a primeira filial fictícia. */
const FILIAL = '010101';

function instalarApi(estado: EstadoDaVisao360, metasRecusadas = false) {
  vi.stubGlobal('localStorage', {
    getItem: (chave: string) => guardado.get(chave) ?? null,
    setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
    removeItem: (chave: string) => void guardado.delete(chave),
    clear: () => guardado.clear(),
  });
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string, init?: RequestInit) => {
      const [caminho, consulta = ''] = entrada.replace(/^.*\/api/, '').split('?');
      if (metasRecusadas && caminho === '/v1/relatorios/metas') {
        return new Response(JSON.stringify({ title: 'Sem a permissão Meta.Ler' }), { status: 403 });
      }
      const empresa = new Headers(init?.headers).get('X-Tracbel-Empresa') ?? '';
      const dados = respostaDaVisao360(caminho, new URLSearchParams(consulta), empresa, estado);
      return dados === undefined
        ? new Response('{"title":"não simulado"}', { status: 404 })
        : new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 });
    }),
  );
}

/** Monta a tela e devolve o bloco da meta quando a leitura dela voltou. */
async function montar(estado: EstadoDaVisao360, metasRecusadas = false) {
  instalarApi(estado, metasRecusadas);
  const tela = render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <PerformanceCen />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
  const bloco = await waitFor(() => {
    const b = [...tela.container.querySelectorAll('details')].find((d) => /meta de venda/i.test(d.querySelector('summary')?.textContent ?? ''));
    expect(b).toBeDefined();
    expect(b!.textContent).not.toContain('Carregando a meta de venda');
    return b!;
  });
  return { ...tela, bloco };
}

beforeEach(() => guardado.clear());
afterEach(() => vi.unstubAllGlobals());

describe('Performance de CEN — a meta de venda por consultor (#138)', () => {
  it('desenha meta × realizado de cada consultor, com o atingimento refeito na tela', async () => {
    const { bloco } = await montar('completo');
    const amostra = metasDeVenda('completo', FILIAL);
    const [um, dois] = amostra.porConsultor;

    expect(bloco.querySelector('summary')).toHaveTextContent('Meta de venda × realizado, por consultor');
    expect(bloco.querySelector('summary')).toHaveTextContent(
      `${amostra.totais.realizadoMaquinas.toLocaleString('pt-BR')} de ${amostra.totais.metaMaquinas.toLocaleString('pt-BR')} máquinas · nov/2025 a ago/2026 · 2 consultores`,
    );

    const linhas = within(bloco).getAllByRole('row').slice(1);
    expect(linhas).toHaveLength(2);
    expect(linhas[0]).toHaveTextContent(um!.consultor);
    expect(linhas[0]).toHaveTextContent(`${Math.round((um!.realizado / um!.meta) * 100)}%`);
    expect(linhas[0]).not.toHaveTextContent('sem conta no CRM');
    // A conta que a GN nomeia e o CRM não tem aparece nomeada — o número dela conta do mesmo jeito.
    expect(linhas[1]).toHaveTextContent(dois!.consultor);
    expect(linhas[1]).toHaveTextContent('sem conta no CRM');

    expect(bloco).toHaveTextContent('a venda sem vendedor conta no total da filial e em ninguém');
    expect(bloco).toHaveTextContent('o ano fiscal até o último mês fechado');
  });

  it('o "Atingimento de meta" saiu do rodapé do que a tela não sustenta', async () => {
    const { container } = await montar('completo');

    const rodape = [...container.querySelectorAll('details')].find((d) =>
      (d.querySelector('summary')?.textContent ?? '').includes('O que esta tela mostrava'),
    );
    expect(rodape).toBeDefined();
    expect(rodape!.querySelector('summary')).not.toHaveTextContent(/\bmeta\b/);
    expect(rodape).not.toHaveTextContent('Atingimento de meta');
    expect(container.textContent).not.toMatch(/organizacao\.Meta\b/);
  });

  it('sem o cadastro lido, diz que a rotina não rodou — e não desenha meta zero', async () => {
    const { bloco } = await montar('vazio');

    expect(bloco.querySelector('summary')).toHaveTextContent('o cadastro de metas da API Gestão de Negócios ainda não foi lido');
    expect(bloco).toHaveTextContent('O cadastro de metas ainda não foi lido');
    expect(within(bloco).queryByRole('table')).toBeNull();
  });

  it('o 403 é falta de permissão, e o bloco não oferece tentar de novo', async () => {
    const { bloco } = await montar('completo', true);

    expect(bloco).toHaveTextContent('Sem permissão para esta consulta');
    expect(within(bloco).queryByRole('button', { name: 'Tentar de novo' })).toBeNull();
    expect(within(bloco).queryByRole('table')).toBeNull();
  });
});
