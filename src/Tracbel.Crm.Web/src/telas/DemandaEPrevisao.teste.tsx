/**
 * A DEMANDA E PREVISÃO NA TELA (issue 258): os quatro números, a previsão mês a mês (e o mês escolhido vira o cartão),
 * a entrega por loja, a tabela das culturas, a matriz com as culturas em coluna e o CSV com todas as linhas.
 *
 * O caminho é o de produção: a rota é lida por um `fetch` de mentira, que anota o que a tela pediu.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { DemandaEPrevisaoDaRegiao } from '../tipos/mercado';
import { cabecalhoDoCsv, linhasDoCsv } from '../componentes/demanda/demanda';
import { DemandaEPrevisao } from './DemandaEPrevisao';

const pedidos: string[] = [];
const MESES = [11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

const DEMANDA: DemandaEPrevisaoDaRegiao = {
  categoria: 'TRATOR',
  categoriaNome: 'Trator',
  categorias: [{ codigo: 'TRATOR', nome: 'Trator', ordem: 1 }],
  shareAlvo: 31,
  shareDoPrototipo: true,
  sazonalidadeVigenteDesde: '2026-09-27',
  sazonalidadeDoPrototipo: true,
  anoDaAreaPlantada: 2024,
  totais: {
    parque: 90,
    demandaEstrutural: 18,
    demandaAjustada: 20,
    aEntregar: 5.58,
    aEntregarAjustada: 6.2,
    culturasComRegra: ['Café'],
    municipios: 2,
    municipiosComDemanda: 2,
    estimativa: true,
  },
  porCultura: [
    { culturaCodigo: 'CAFE', cultura: 'Café', areaUtilHectares: 900, hectaresPorMaquina: 10, anosDeRenovacao: 5, parque: 90, demandaEstrutural: 18, demandaAjustada: 20, variacaoPercentual: 11.1 },
  ],
  previsaoMensal: MESES.map((mes) => ({
    mes,
    fracao: mes === 5 ? 0.12 : 0.08,
    demandaEstrutural: 1.5,
    demandaAjustada: 1.7,
    aEntregar: 0.47,
    aEntregarAjustada: mes === 5 ? 0.74 : 0.5,
  })),
  porLoja: [{ lojaCodigo: '010110', loja: 'Araraquara', aEntregarNoAno: 6.2, porMes: MESES.map(() => 0.52) }],
  municipios: [
    {
      codigoIbge: 1, nome: 'Garça', regiao: 'Norte', lojaCodigo: '010110', loja: 'Araraquara', areaUtilHectares: 300, parque: 30,
      porCultura: [{ culturaCodigo: 'CAFE', demanda: 6 }], demandaEstrutural: 6, demandaAjustada: 7, aEntregar: 1.86,
      aEntregarAjustada: 2.17, fatorDePreco: 1.12, fatorDeCredito: 0.95, culturaPredominante: 'Café', variacaoPercentual: 16.7,
    },
    {
      codigoIbge: 2, nome: 'Marília', regiao: 'Norte', lojaCodigo: '010110', loja: 'Araraquara', areaUtilHectares: 600, parque: 60,
      porCultura: [{ culturaCodigo: 'CAFE', demanda: 12 }], demandaEstrutural: 12, demandaAjustada: 13, aEntregar: 3.72,
      aEntregarAjustada: 4.03, fatorDePreco: 1.05, fatorDeCredito: 1.03, culturaPredominante: 'Café', variacaoPercentual: 8.3,
    },
  ],
  culturas: [{ codigo: 'CAFE', nome: 'Café', demanda: 18 }],
  lacunas: [{ metrica: 'porCliente', motivo: 'A demanda é por município, e não por cliente.' }],
};

function montar() {
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string) => {
      pedidos.push(entrada);
      return entrada.includes('/v1/mercado/demanda')
        ? new Response(JSON.stringify({ dados: DEMANDA, procedencia: null }), { status: 200 })
        : new Response('{"title":"não simulado"}', { status: 404 });
    }),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <DemandaEPrevisao />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
}

beforeEach(() => {
  pedidos.length = 0;
});
afterEach(() => vi.unstubAllGlobals());

const aMatriz = () => waitFor(() => expect(document.querySelector('[data-bloco="matriz"] table')).not.toBeNull());
const kpi = (rotulo: RegExp) => within(document.querySelector('[data-bloco="kpis"]') as HTMLElement).getByText(rotulo).closest('[data-kpi]');

describe('Demanda e Previsão (issue 258)', () => {
  it('os quatro números: parque, demanda ajustada, a entregar pelo share e as culturas com regra', async () => {
    montar();
    await aMatriz();

    expect(kpi(/Parque necessário/)).toHaveTextContent('90máquinas');
    expect(kpi(/Demanda anual/)).toHaveTextContent('20máquinas/ano');
    expect(kpi(/Demanda anual/)).toHaveTextContent('estrutural 18');
    expect(kpi(/A entregar no ano/)).toHaveTextContent('6,2');
    expect(kpi(/A entregar no ano/)).toHaveTextContent('share-alvo de 31%');
    expect(kpi(/Culturas com regra/)).toHaveTextContent('Café');
  });

  it('a previsão vai de novembro a outubro, e escolher um mês põe a entrega dele no cartão', async () => {
    montar();
    await aMatriz();

    const previsao = document.querySelector('[data-bloco="previsao-mensal"]') as HTMLElement;
    const botoes = within(previsao).getAllByRole('button', { pressed: false });
    expect(botoes[0]).toHaveAccessibleName(/^novembro/);
    expect(botoes[11]).toHaveAccessibleName(/^outubro/);

    fireEvent.click(within(previsao).getByRole('button', { name: /^maio/ }));
    // UMA CASA DECIMAL NA TELA: 0,74 máquina aparece como 0,7.
    expect(kpi(/A entregar em maio/)).toHaveTextContent('0,7');
    expect(kpi(/A entregar em maio/)).toHaveTextContent('12% do ano');
  });

  it('a entrega por loja mostra o número em cada mês e o total do ano', async () => {
    montar();
    await aMatriz();

    const lojas = document.querySelector('[data-bloco="entrega-por-loja"]') as HTMLElement;
    const linha = within(lojas).getByText('Araraquara').closest('tr') as HTMLElement;
    expect(within(linha).getAllByText('0,5')).toHaveLength(12);
    expect(within(linha).getByText('6,2')).toBeInTheDocument();
  });

  it('a cultura mostra os parâmetros e o efeito do momento', async () => {
    montar();
    await aMatriz();

    const culturas = document.querySelector('[data-bloco="por-cultura"]') as HTMLElement;
    const linha = within(culturas).getByText('Café').closest('tr') as HTMLElement;
    expect(linha).toHaveTextContent('10');
    expect(linha).toHaveTextContent('+11%');
  });

  it('a matriz ordena pela demanda, traz a cultura em coluna e os dois fatores, e a busca filtra', async () => {
    montar();
    await aMatriz();

    const matriz = document.querySelector('[data-bloco="matriz"]') as HTMLElement;
    const nomes = () => [...matriz.querySelectorAll('tbody .dem-municipio')].map((e) => e.textContent);
    expect(nomes()).toEqual(['Marília', 'Garça']);
    expect(within(matriz).getByRole('columnheader', { name: /Café/ })).toBeInTheDocument();
    expect(within(matriz).getByText('+12%')).toBeInTheDocument();
    expect(within(matriz).getByText('−5%')).toBeInTheDocument();

    fireEvent.change(within(matriz).getByRole('searchbox', { name: 'Buscar município na matriz' }), { target: { value: 'gar' } });
    expect(nomes()).toEqual(['Garça']);
  });

  it('trocar o tipo de máquina pede a demanda de novo', async () => {
    montar();
    await aMatriz();

    const campo = screen.getByText('Tipo de máquina').closest('label')!.querySelector('select')!;
    fireEvent.change(campo, { target: { value: 'TODAS' } });

    await waitFor(() => expect(pedidos.some((p) => p.includes('categoria=TODAS'))).toBe(true));
  });
});

describe('o CSV da matriz', () => {
  it('leva uma coluna por cultura e todos os municípios, com vírgula decimal', () => {
    expect(cabecalhoDoCsv(DEMANDA)).toContain('Demanda Café');
    const linhas = linhasDoCsv(DEMANDA);
    expect(linhas).toHaveLength(2);
    expect(linhas[0][0]).toBe('Garça');
    expect(linhas[0][5]).toBe('6');
    expect(linhas[0][10]).toBe('1,12');
  });
});
