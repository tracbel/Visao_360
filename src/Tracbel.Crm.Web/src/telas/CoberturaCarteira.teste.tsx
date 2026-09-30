/**
 * A COBERTURA DE CARTEIRA NA MAQUETE (30/09/2026) — o que a tela afirma, pelo caminho de produção: um `fetch` de mentira
 * que responde com as MESMAS amostras do harness `#/dev/visao360-visual?rota=/cobertura`. O desenho é da conferência
 * visual (`testes-visuais/telas-no-padrao.spec.ts`); aqui ficam as regras:
 *
 * - "Contato em 90 dias" é SÓ A FAIXA de 31 a 90 dias (decisão do Ricardo) — os cartões e a rosca somam o total;
 * - o maior risco é a carteira COMERCIAL com a maior fatia há mais de 90 dias ou nunca contatada;
 * - a prioridade é a curva ABC do cliente, com a classe dita ao leitor de tela;
 * - a busca vai ao servidor, e o Exportar leva a lista com os mesmos filtros;
 * - o menu de cada linha leva à ficha e às máquinas do cliente.
 */

import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { respostaDaVisao360, type EstadoDaVisao360 } from '../dev/amostrasDaVisao360';
import { CoberturaCarteira } from './CoberturaCarteira';

vi.setConfig({ testTimeout: 20_000 });

vi.mock('../componentes/GraficoDonutCentro', () => ({ GraficoDonutCentro: () => <div data-grafico="rosca" /> }));

const baixados: { nome: string; cabecalho: string[]; linhas: unknown[][] }[] = [];
vi.mock('../dados/exportarCsv', () => ({
  baixarCsv: (nome: string, cabecalho: string[], linhas: unknown[][]) => void baixados.push({ nome, cabecalho, linhas }),
  carimboDeData: () => '20260930',
}));

const guardado = new Map<string, string>();
let pedidos: string[] = [];

function instalarApi(estado: EstadoDaVisao360) {
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
      pedidos.push(entrada.replace(/^.*\/api/, ''));
      const empresa = new Headers(init?.headers).get('X-Tracbel-Empresa') ?? '';
      const dados = respostaDaVisao360(caminho, new URLSearchParams(consulta), empresa, estado);
      return dados === undefined
        ? new Response('{"title":"não simulado"}', { status: 404 })
        : new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 });
    }),
  );
}

async function montar(estado: EstadoDaVisao360 = 'completo') {
  instalarApi(estado);
  const tela = render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <CoberturaCarteira />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
  await waitFor(() => expect(tela.container.querySelector('[data-kpi="Contato em 90 dias"]')).toHaveTextContent(/\d/));
  await screen.findAllByText('PRODUTOR FICTÍCIO EPSILON');
  return tela;
}

/** A BUSCA ESPERA 350ms DEPOIS DA ÚLTIMA TECLA, e a suíte inteira roda com a máquina carregada: folga de 5 segundos. */
const ESPERA = { timeout: 5_000 };

const numero = (texto: string | null | undefined) => Number((texto ?? '').replace(/\D/g, ''));

beforeEach(() => {
  guardado.clear();
  pedidos = [];
  baixados.length = 0;
});
afterEach(() => vi.unstubAllGlobals());

describe('Cobertura de Carteira na maquete (30/09/2026)', () => {
  it('"Contato em 90 dias" é só a faixa de 31 a 90 — e as faixas somam o total de vínculos', async () => {
    const { container } = await montar();
    const valor = (kpi: string) => numero(container.querySelector(`[data-kpi="${kpi}"] .mv-kpi-valor strong`)?.textContent);

    const total = valor('Clientes carteira');
    const em30 = valor('Contato em 30 dias');
    const entre31e90 = valor('Contato em 90 dias');
    const nunca = valor('Nunca contatados');

    // A LEGENDA DA ROSCA traz as quatro faixas; a de "acima de 90 dias" completa a soma.
    const legenda = container.querySelector('.cob-legenda-tabela')!;
    const acima90 = numero(within(legenda as HTMLElement).getByText('Acima de 90 dias').closest('tr')!.querySelector('td')?.textContent);
    expect(numero(within(legenda as HTMLElement).getByText('Entre 31 e 90 dias').closest('tr')!.querySelector('td')?.textContent)).toBe(
      entre31e90,
    );
    expect(em30 + entre31e90 + acima90 + nunca).toBe(total);
  });

  it('o maior risco é a carteira comercial com a maior fatia há mais de 90 dias ou nunca contatada', async () => {
    const { container } = await montar();

    const risco = container.querySelector('.cob-risco')!;
    // O DEPÓSITO DE CADASTRO E A CARTEIRA DE TESTE têm 100% "nunca contatados" por construção, e ficam fora.
    expect(risco).not.toHaveTextContent('Depósito');
    expect(risco).not.toHaveTextContent('teste');
    expect(risco).toHaveTextContent(/Maior risco: .+/);
    expect(risco).toHaveTextContent(/\d+% dos clientes há mais de 90 dias ou nunca contatados\./);

    // O PRIMEIRO DO RANKING DE EXPOSIÇÃO é o mesmo do maior risco.
    const primeiro = container.querySelector('.cob-exposicao-lista li .cob-exposicao-nome')!.textContent!;
    expect(risco).toHaveTextContent(primeiro);
  });

  it('as barras escrevem o percentual de cada faixa e o total de cada carteira', async () => {
    const { container } = await montar();

    const linha = container.querySelector('.cob-barras-linha')!;
    expect(linha.querySelectorAll('.cob-barras-faixa').length).toBeGreaterThan(0);
    expect(linha.querySelector('.cob-barras-numero')).toHaveTextContent(/\d/);
    // O LEITOR DE TELA OUVE A LINHA INTEIRA: o nome, os vínculos e as quatro faixas.
    expect(linha.querySelector('.cad-so-leitor')).toHaveTextContent(/vínculos — em até 30 dias: \d+%, entre 31 e 90 dias: \d+%/);
  });

  it('a prioridade é a curva ABC do cliente, e o leitor de tela ouve a classe', async () => {
    await montar();

    const epsilon = screen.getAllByText('PRODUTOR FICTÍCIO EPSILON')[0].closest('tr')!;
    expect(within(epsilon).getByText('Alta').closest('.cob-prioridade')).toHaveTextContent('classe A da curva ABC');

    const eta = screen.getAllByText('PRODUTOR FICTÍCIO ETA')[0].closest('tr')!;
    expect(within(eta).getByText('Média')).toBeInTheDocument();

    const teta = screen.getAllByText('PRODUTOR FICTÍCIO TETA')[0].closest('tr')!;
    expect(within(teta).getByText('Baixa').closest('.cob-prioridade')).toHaveTextContent('cliente sem classe na curva ABC');
  });

  it('a busca vai ao servidor e filtra a lista', async () => {
    await montar();

    fireEvent.change(screen.getByPlaceholderText('Buscar cliente, carteira ou responsável...'), { target: { value: 'Lambda' } });

    await waitFor(() => expect(pedidos.some((p) => p.startsWith('/v1/cobertura?') && p.includes('termo=Lambda'))).toBe(true), ESPERA);
    await waitFor(() => expect(screen.queryAllByText('PRODUTOR FICTÍCIO EPSILON')).toHaveLength(0), ESPERA);
    expect(screen.getAllByText('PRODUTOR FICTÍCIO LAMBDA').length).toBeGreaterThan(0);
    expect(screen.getByRole('button', { name: 'Limpar filtros' })).toBeInTheDocument();
  });

  it('o Exportar leva a lista com a busca, com a classe e a prioridade de cada linha', async () => {
    await montar();

    fireEvent.change(screen.getByPlaceholderText('Buscar cliente, carteira ou responsável...'), { target: { value: 'Lambda' } });
    await waitFor(() => expect(screen.queryAllByText('PRODUTOR FICTÍCIO EPSILON')).toHaveLength(0), ESPERA);

    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: 'Exportar' }));
    });
    await waitFor(() => expect(baixados).toHaveLength(1), ESPERA);

    const [{ cabecalho, linhas }] = baixados;
    expect(cabecalho).toContain('Prioridade');
    expect(cabecalho).toContain('Classe do cliente (curva ABC)');
    expect(linhas).toHaveLength(1);
    expect(linhas[0][0]).toBe('PRODUTOR FICTÍCIO LAMBDA');
    expect(linhas[0][2]).toBe('Baixa');
    expect(pedidos.some((p) => p.includes('tamanho=200') && p.includes('termo=Lambda'))).toBe(true);
  });

  it('o menu de cada linha leva à ficha e às máquinas do cliente', async () => {
    await montar();

    fireEvent.click(screen.getByRole('button', { name: 'Ações de PRODUTOR FICTÍCIO EPSILON' }));
    const menu = screen.getByRole('menu');
    expect(within(menu).getByRole('menuitem', { name: 'Abrir a ficha do cliente' })).toHaveAttribute('href', '/clientes/cliente-ficticio-11');
    expect(within(menu).getByRole('menuitem', { name: 'Ver as máquinas do cliente' })).toHaveAttribute(
      'href',
      '/equipamentos?cliente=cliente-ficticio-11',
    );

    fireEvent.keyDown(document, { key: 'Escape' });
    expect(screen.queryByRole('menu')).toBeNull();
  });

  it('nada da tela antiga sumiu: a cobertura em número e o que a tela deixou de afirmar, com a tendência dos cartões', async () => {
    const { container } = await montar();

    expect(container).toHaveTextContent('Cobertura por carteira, em número');
    // OS DOIS BLOCOS SÃO `<details>` FECHADOS: o conteúdo está no documento, e abre pelo resumo.
    const lacunas = [...container.querySelectorAll('details')].find((d) => d.textContent?.includes('O que esta tela deixou de afirmar'))!;
    expect(lacunas).toHaveTextContent('A tendência de cada cartão');
    expect(lacunas).toHaveTextContent('Mapa da carteira');
    expect(lacunas).toHaveTextContent('Interações por canal');
  });
});
