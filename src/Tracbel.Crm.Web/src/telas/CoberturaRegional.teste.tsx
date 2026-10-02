/**
 * A COBERTURA POR FILIAL E CARTEIRA NA MAQUETE (02/10/2026) — o que a tela afirma, pelo caminho de produção: um `fetch` de
 * mentira que responde com as MESMAS amostras do harness `#/dev/visao360-visual?rota=/relatorios/cobertura`. O desenho é
 * da conferência visual (`testes-visuais/telas-no-padrao.spec.ts`); aqui ficam as regras:
 *
 * - as três partes embaixo do medidor (no período, fora dele, nunca contatados) somam o total de vínculos;
 * - o período troca a conta sem pedir de novo — a rota já traz 30 e 90 dias;
 * - filial, carteira, município e estado recortam os cartões, o medidor e as barras, pela chave da carteira;
 * - o tipo de cliente vai ao servidor, como a classe da curva ABC;
 * - a filial do nível 1 desdobra no nível 2; "Expandir todos" abre as cidades; o aviso fecha no X;
 * - a carteira sem cidade continua na lista, com zero.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { respostaDaVisao360, type EstadoDaVisao360 } from '../dev/amostrasDaVisao360';
import { CoberturaRegional } from './CoberturaRegional';

vi.setConfig({ testTimeout: 20_000 });

// O MEDIDOR É CANVAS, que o jsdom não desenha: no lugar dele, o valor e a legenda em texto.
vi.mock('../componentes/GraficoGauge', () => ({
  GraficoGauge: ({ valor, legenda }: { valor: number; legenda?: string }) => (
    <div data-grafico="medidor">
      {valor.toFixed(1)} {legenda}
    </div>
  ),
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
        <CoberturaRegional />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
  await waitFor(() => expect(tela.container.querySelector('[data-kpi="Carteiras"]')).toHaveTextContent(/\d/));
  await waitFor(() => expect(tela.container.querySelector('.cobf-partes')).not.toBeNull());
  return tela;
}

const numero = (texto: string | null | undefined) => Number((texto ?? '').replace(/\D/g, ''));
const kpi = (nome: string) => document.querySelector(`[data-kpi="${nome}"]`) as HTMLElement;
const campo = (bloco: string) => document.querySelector(`[data-bloco="filtros"] [data-bloco="${bloco}"] select`) as HTMLSelectElement;
const partes = () => [...document.querySelectorAll('.cobf-parte strong')].map((e) => numero(e.textContent));
const legenda = () => document.querySelector('.cobf-medidor .cad-medidor-legenda')?.textContent ?? '';
const barras = () => [...document.querySelectorAll('[data-bloco="por-carteira"] .cad-barras-titulo')].map((e) => e.textContent);

beforeEach(() => {
  guardado.clear();
  pedidos = [];
});
afterEach(() => vi.unstubAllGlobals());

describe('Cobertura por Filial e Carteira (maquete de 02/10/2026)', () => {
  it('os cinco números e as três partes do medidor, que somam o total de vínculos', async () => {
    await montar();

    expect(kpi('Filiais ao alcance')).toHaveTextContent('filial selecionada');
    expect(kpi('Carteiras')).toHaveTextContent('15carteiras');
    expect(kpi('Com cidade')).toHaveTextContent('11carteiras');
    expect(kpi('Com cidade')).toHaveTextContent('73,3% das carteiras');
    expect(kpi('Estados')).toHaveTextContent('SP');

    const [noPeriodo, fora, nunca] = partes();
    const total = Number(legenda().match(/de ([\d.]+) vínculos/)![1].replace(/\./g, ''));
    expect(noPeriodo + fora + nunca).toBe(total);
    expect(legenda()).toContain('tiveram contato nos últimos 90 dias');
  });

  it('o período de 30 dias troca o medidor e as barras, sem pedir de novo', async () => {
    await montar();
    const antes = pedidos.length;
    const [noPeriodoEm90] = partes();

    fireEvent.change(campo('periodo'), { target: { value: '30' } });

    expect(legenda()).toContain('nos últimos 30 dias');
    expect(partes()[0]).toBeLessThan(noPeriodoEm90);
    expect(screen.getByText('Visualize, nos últimos 30 dias, o alcance da sua carteira.')).toBeInTheDocument();
    expect(pedidos.length).toBe(antes);
  });

  it('a carteira recorta os cartões, o medidor, as barras e o nível 2', async () => {
    await montar();

    fireEvent.change(campo('carteira'), { target: { value: (screen.getByRole('option', { name: 'Carteira fictícia 3' }) as HTMLOptionElement).value } });

    expect(kpi('Carteiras')).toHaveTextContent('1carteiras');
    expect(barras()).toEqual(['Carteira fictícia 3']);
    const nivel2 = document.querySelector('[data-bloco="por-carteira-territorio"]') as HTMLElement;
    expect(within(nivel2).getAllByRole('button', { name: /Carteira fictícia/ })).toHaveLength(1);
    expect(legenda()).toMatch(/de 1\.952 vínculos/);
  });

  it('o município deixa só as carteiras que o atendem', async () => {
    await montar();
    const todas = numero(kpi('Carteiras').querySelector('.mv-kpi-valor')?.textContent);

    fireEvent.change(campo('municipio'), { target: { value: campo('municipio').options[1].value } });

    const doMunicipio = numero(kpi('Carteiras').querySelector('.mv-kpi-valor')?.textContent);
    expect(doMunicipio).toBeGreaterThan(0);
    expect(doMunicipio).toBeLessThan(todas);
  });

  it('o tipo de cliente pede a cobertura da classe ao servidor', async () => {
    await montar();

    fireEvent.change(campo('tipo'), { target: { value: 'A' } });

    await waitFor(() => expect(pedidos.some((p) => p.startsWith('/v1/relatorios/cobertura') && p.includes('classe=A'))).toBe(true));
  });

  it('a filial do nível 1 desdobra no nível 2: escolhe a filial no filtro', async () => {
    await montar();

    fireEvent.click(screen.getByRole('button', { name: /Ver as carteiras de/ }));

    expect(campo('filial').value).not.toBe('');
  });

  it('"Expandir todos" abre as cidades de todas as carteiras, e "Recolher todos" fecha', async () => {
    await montar();
    expect(document.querySelectorAll('.cad-cidade')).toHaveLength(0);

    fireEvent.click(screen.getByRole('button', { name: 'Expandir todos' }));
    expect(document.querySelectorAll('.cad-cidade').length).toBeGreaterThan(0);

    fireEvent.click(screen.getByRole('button', { name: 'Recolher todos' }));
    expect(document.querySelectorAll('.cad-cidade')).toHaveLength(0);
  });

  it('o aviso do topo fecha no X, e a carteira sem cidade continua na lista, com zero', async () => {
    await montar();

    fireEvent.click(screen.getByRole('button', { name: 'Fechar o aviso "O que estes números não dizem"' }));
    expect(document.querySelector('[data-bloco="aviso"]')).toBeNull();

    const deposito = screen.getByRole('button', { name: /Depósito de cadastro \(amostra\)/ });
    expect(deposito).toBeDisabled();
    expect(deposito).toHaveTextContent('0cidades');
  });

  it('sem carteira ao alcance, cada bloco diz por quê', async () => {
    instalarApi('vazio');
    render(
      <ProvedorDeContextoDeAcesso>
        <MemoryRouter>
          <CoberturaRegional />
        </MemoryRouter>
      </ProvedorDeContextoDeAcesso>,
    );

    expect(await screen.findByText('Sem vínculo de carteira para medir')).toBeInTheDocument();
    expect(await screen.findByText('Nenhuma filial com carteira ao seu alcance')).toBeInTheDocument();
    expect(kpi('Com cidade')).toHaveTextContent('sem carteira no recorte');
  });
});
