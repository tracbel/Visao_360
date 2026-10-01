/**
 * O FORECAST DA GERÊNCIA NA TELA (28/09/2026; na maquete do Ricardo desde 01/10/2026): cada gestor abre o grupo com a
 * soma do time, o forecast não informado mostra o traço com o motivo (e não zero), trocar o mês pede de novo, a recusa da
 * API aparece como falta de permissão, e o CSV leva o que está na tela.
 *
 * Da maquete: os cartões comparam com o mês anterior inteiro; a "Regional / Gerência" escolhe um gestor e recorta a tela;
 * a "Visão" agrupa por linha de produto; o alerta faz a conta do forecast contra o realizado; a tabela mostra os cinco
 * primeiros e "Demais gestores".
 *
 * O caminho é o de produção: a rota é lida por um `fetch` de mentira, que anota o que a tela pediu. Nomes inventados.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { respostaDaVisao360 } from '../dev/amostrasDaVisao360';
import type { RelatorioDoForecast } from '../tipos/forecast';
import { nomeCurto, nomeProprio } from './cadastro/formato';
import { doPo, ForecastGerencia, linhasDoCsv, somar } from './ForecastGerencia';

vi.mock('../componentes/GraficoDonutCentro', () => ({ GraficoDonutCentro: () => <div data-grafico="rosca" /> }));

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

/** O mês anterior do teste: PG 10 (agosto), contra 12 em setembro. */
const AGOSTO: RelatorioDoForecast = {
  ...FORECAST,
  competencia: '2026-08-01',
  texto: 'ago/2026',
  gestores: [{ ...FORECAST.gestores[0]!, linhas: [{ ...FORECAST.gestores[0]!.linhas[0]!, meta: 10, forecast: 10, bestGuess: 12, realizado: 5 }] }],
  total: [{ codigo: 'TRATOR_MEDIO', nome: 'TRATOR MÉDIO', meta: 10, forecast: 10, bestGuess: 12, realizado: 5 }],
};

const responder = (dados: RelatorioDoForecast) => new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 });

function montar(resposta: (url: string) => Response = (url) => responder(url.includes('competencia=2026-08') ? AGOSTO : FORECAST)) {
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
      return entrada.includes('/v1/relatorios/forecast') ? resposta(entrada) : new Response('{"title":"não simulado"}', { status: 404 });
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

/** A tabela da previsão — a última da tela; as legendas dos gráficos também são tabelas. */
const tabela = () => screen.getAllByRole('table').find((t) => t.classList.contains('fc-tabela'))!;

const grupos = () => within(tabela()).getAllByRole('rowgroup').slice(1);

beforeEach(() => {
  guardado.clear();
  pedidos.length = 0;
});
afterEach(() => vi.unstubAllGlobals());

describe('Forecast da Gerência', () => {
  it('cada gestor abre o grupo com a soma do time, e o total fecha a tabela', async () => {
    montar();
    await waitFor(() => expect(tabela()).toBeInTheDocument());

    expect(grupos()).toHaveLength(3);

    const norte = within(grupos()[0]!).getAllByRole('row')[0]!;
    expect(norte).toHaveTextContent('GESTOR.NORTE');
    expect(norte).toHaveTextContent('4 consultores');
    expect(within(norte).getAllByRole('cell').map((c) => c.textContent)).toEqual(['12', '8', '10', '7', '58%']);

    const total = within(grupos()[2]!).getAllByRole('row')[0]!;
    expect(total).toHaveTextContent('Total da gerência');
    expect(total).toHaveTextContent('com 1 venda(s) sem gestor');
    expect(screen.getByText('Previsão por gestor · set/2026')).toBeInTheDocument();
  });

  it('o forecast não informado mostra o traço com o motivo, e não zero', async () => {
    montar();
    await waitFor(() => expect(tabela()).toBeInTheDocument());

    expect(screen.getAllByRole('button', { name: 'Por que o forecast não aparece' }).length).toBeGreaterThan(0);
    expect(within(tabela()).getAllByRole('button', { name: 'Por que o atingimento não aparece' })).toHaveLength(1);
  });

  it('trocar o mês pede o forecast daquele mês', async () => {
    montar();
    await waitFor(() => expect(tabela()).toBeInTheDocument());

    fireEvent.change(screen.getByLabelText('Mês de referência'), { target: { value: '2026-08-01' } });

    await waitFor(() => expect(pedidos.filter((p) => p.includes('competencia=2026-08')).length).toBeGreaterThan(1));
  });

  it('a recusa da API aparece como falta de permissão, e não como tabela vazia', async () => {
    montar(() => new Response(JSON.stringify({ title: 'Sem permissão', detail: 'O forecast é da gerência.' }), { status: 403 }));

    await waitFor(() => expect(screen.getByText('Sem permissão para esta consulta')).toBeInTheDocument());
    expect(document.querySelector('.fc-tabela')).toBeNull();
  });
});

describe('Forecast da Gerência na maquete (01/10/2026)', () => {
  it('a meta se chama PG, e cada cartão compara com o mês anterior inteiro', async () => {
    const { container } = montar();
    await waitFor(() => expect(container.querySelector('[data-kpi="PG"] .fc-variacao')).not.toBeNull());

    // O MÊS ANTERIOR É PEDIDO À PARTE: agosto, para comparar com setembro.
    expect(pedidos.some((p) => p.includes('competencia=2026-08'))).toBe(true);
    // PG 12 contra 10: +20%. Forecast 8 contra 10: -20%. Realizado 10 contra 5: +100%.
    expect(container.querySelector('[data-kpi="PG"] .mv-kpi-contexto')).toHaveTextContent('+20% vs. mês anterior');
    expect(container.querySelector('[data-kpi="PG"] .fc-variacao')).toHaveAttribute('data-sentido', 'sobe');
    expect(container.querySelector('[data-kpi="Forecast"] .mv-kpi-contexto')).toHaveTextContent('-20% vs. mês anterior');
    expect(container.querySelector('[data-kpi="Forecast"] .fc-variacao')).toHaveAttribute('data-sentido', 'desce');
    expect(container.querySelector('[data-kpi="Realizado"] .mv-kpi-contexto')).toHaveTextContent('+100% vs. mês anterior');
    expect(container.querySelector('[data-kpi="PG"] .mv-kpi-contexto')).toHaveTextContent('Meta comercial da gerência');
  });

  it('o alerta faz a conta do forecast contra o realizado e do best guess contra o mês anterior', async () => {
    const alto: RelatorioDoForecast = {
      ...FORECAST,
      total: [{ codigo: 'TRATOR_MEDIO', nome: 'TRATOR MÉDIO', meta: 20, forecast: 15, bestGuess: 9, realizado: 10 }],
    };
    const { container } = montar((url) => responder(url.includes('competencia=2026-08') ? AGOSTO : alto));
    await waitFor(() => expect(container.querySelector('[data-bloco="atencao-da-gerencia"]')).toHaveTextContent('mês anterior'));

    const alerta = container.querySelector('[data-bloco="atencao-da-gerencia"]')!;
    expect(alerta).toHaveTextContent('O forecast (15) está acima do realizado (10), com uma diferença de 5 máquinas (+50%).');
    // BEST GUESS 9 contra o forecast 15 e contra os 12 de agosto: 25% abaixo.
    expect(alerta).toHaveTextContent('O Best Guess (9) indica um cenário mais conservador, 25% abaixo do mês anterior.');
  });

  it('a regional / gerência escolhe um gestor e recorta os cartões e a tabela', async () => {
    const { container } = montar();
    await waitFor(() => expect(tabela()).toBeInTheDocument());

    fireEvent.change(container.querySelector('[data-bloco="gerencia"] select')!, { target: { value: 'GESTOR.SUL' } });

    await waitFor(() => expect(container.querySelector('[data-kpi="Gestores"] .mv-kpi-valor strong')).toHaveTextContent('1'));
    expect(container.querySelector('[data-kpi="Gestores"] .mv-kpi-contexto')).toHaveTextContent('de 2 gestores no mês');
    expect(container.querySelector('[data-kpi="PG"] .mv-kpi-valor strong')).toHaveTextContent('0');
    expect(container.querySelector('[data-kpi="Realizado"] .mv-kpi-valor strong')).toHaveTextContent('2');
    expect(grupos()).toHaveLength(2);
    expect(within(grupos()[1]!).getByRole('row')).toHaveTextContent('Total do gestor');
    // SEM PREVISÃO INFORMADA, NÃO HÁ ALERTA: não há o que comparar.
    expect(container.querySelector('[data-bloco="atencao-da-gerencia"]')).toBeNull();
  });

  it('a visão por linha agrupa pelas linhas de produto, com os gestores e as vendas sem gestor dentro', async () => {
    const { container } = montar();
    await waitFor(() => expect(tabela()).toBeInTheDocument());

    fireEvent.change(container.querySelector('[data-bloco="visao"] select')!, { target: { value: 'linha' } });

    await waitFor(() => expect(screen.getByText('Previsão por linha de produto · set/2026')).toBeInTheDocument());
    const trator = grupos()[0]!;
    const linhas = within(trator).getAllByRole('row');
    expect(linhas[0]).toHaveTextContent('Trator Médio');
    expect(linhas.slice(1).map((l) => l.querySelector('td')?.textContent)).toEqual(['GESTOR.NORTE', 'GESTOR.SUL', 'Vendas sem gestor']);
  });

  it('o grupo abre e fecha pela seta, e o primeiro abre aberto', async () => {
    montar();
    await waitFor(() => expect(tabela()).toBeInTheDocument());

    expect(within(grupos()[0]!).getAllByRole('row')).toHaveLength(3);
    expect(within(grupos()[1]!).getAllByRole('row')).toHaveLength(1);

    fireEvent.click(screen.getByRole('button', { name: 'Abrir as linhas de GESTOR.SUL' }));
    expect(within(grupos()[1]!).getAllByRole('row')).toHaveLength(2);

    fireEvent.click(screen.getByRole('button', { name: 'Fechar as linhas de GESTOR.NORTE' }));
    expect(within(grupos()[0]!).getAllByRole('row')).toHaveLength(1);
  });

  it('com mais de seis gestores, os cinco primeiros ficam à vista e o resto em "Demais gestores"', async () => {
    montar((url) => {
      const competencia = new URL(url, 'http://x').searchParams;
      return responder(respostaDaVisao360('/v1/relatorios/forecast', competencia, '010101', 'completo') as RelatorioDoForecast);
    });
    await waitFor(() => expect(tabela()).toBeInTheDocument());

    // A AMOSTRA TEM NOVE GESTORES: cinco, "Demais gestores (4)" e o total.
    expect(grupos()).toHaveLength(7);
    expect(within(grupos()[5]!).getAllByRole('row')[0]).toHaveTextContent('Demais gestores (4)');
    expect(screen.getAllByText('Demais gestores').length).toBeGreaterThan(0);
  });
});

describe('funções do forecast', () => {
  it('a soma deixa nulo o que ninguém informou', () => {
    expect(somar(FORECAST.gestores[1]!.linhas)).toEqual({ meta: 0, forecast: null, bestGuess: null, realizado: 2 });
    expect(somar(FORECAST.gestores[0]!.linhas)).toEqual({ meta: 12, forecast: 8, bestGuess: 10, realizado: 7 });
  });

  it('sem PG não há razão', () => {
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

  it('os nomes da maquete: curto nos gráficos, por extenso na tabela, com o login e as siglas intactos', () => {
    expect(nomeCurto('ALFA MATIAS DE ARAUJO CRUZ')).toBe('Alfa M. Cruz');
    expect(nomeCurto('HAMILTON DE SOUZA LOPES')).toBe('Hamilton S. Lopes');
    expect(nomeCurto('GESTOR.SUL')).toBe('GESTOR.SUL');
    expect(nomeProprio('GESTOR FICTÍCIO DA REGIONAL NORTE PAULISTA')).toBe('Gestor Fictício da Regional Norte Paulista');
    expect(nomeProprio('IMPLEMENTOS CEN')).toBe('Implementos CEN');
  });
});
