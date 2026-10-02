/**
 * O DIMENSIONAMENTO DA ADR NA TELA (issue 259): os quatro números com o ano anterior, a fatia da Tracbel no estado, a
 * carteira com as faixas de dias, os municípios prioritários, a matriz com o ano anterior na célula, os filtros que vão ao
 * servidor e o CSV com as colunas separadas.
 *
 * O caminho é o de produção: a rota é lida por um `fetch` de mentira, que anota o que a tela pediu. A malha do mapa não é
 * simulada — o mapa diz que está carregando, e o resto da tela funciona.
 */

import { fireEvent, render, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cabecalhoDaMatriz, linhasDaMatriz } from '../componentes/dimensionamento/dimensionamento';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { ClientesDaCarteira, DimensionamentoDaAdr as Dimensionamento } from '../tipos/mercado';
import { DimensionamentoAdr } from './DimensionamentoAdr';

const pedidos: string[] = [];

const clientes = (total: number, a: number, ultimo: string | null): ClientesDaCarteira => ({
  clientes: total,
  a,
  b: 1,
  c: 1,
  d: 0,
  semClasse: total - a - 2,
  faixas: { ate30: 1, ate60: 2, ate90: 2, ate120: 3, sem120: total - 3 },
  abSemContato: 1,
  ultimoContatoEm: ultimo,
});

const DIM: Dimensionamento = {
  anoBase: 2024,
  anoAnterior: 2023,
  anosDisponiveis: [2024, 2023],
  cultura: null,
  culturaNome: 'Todas as culturas',
  culturas: [
    { codigo: 'CAFE', nome: 'Café' },
    { codigo: 'SOJA', nome: 'Soja' },
    { codigo: 'OUTRAS', nome: 'Outras' },
  ],
  lojas: [
    { codigo: '010101', nome: 'TRACBEL AGRO — RIBEIRÃO PRETO', municipios: 1 },
    { codigo: '010103', nome: 'TRACBEL AGRO — BARRETOS', municipios: 1 },
  ],
  responsaveis: [{ id: 100, nome: 'MARIA SOUZA', natureza: 'Pessoa', carteiras: 1, gestor: null }],
  recorte: 'ADR (2 municípios)',
  municipiosNoRecorte: 2,
  tracbelNoEstado: {
    area: { parte: 360, todo: 2600, percentual: 13.85 },
    quantidade: { parte: 710, todo: 5500, percentual: 12.91 },
    valor: { parte: 3350, todo: 23500, percentual: 14.26 },
  },
  representatividade: null,
  totais: {
    area: { atual: 360, anterior: 80, variacaoPercentual: 350 },
    quantidade: { atual: 710, anterior: 120, variacaoPercentual: 491.7 },
    valor: { atual: 3350, anterior: 800, variacaoPercentual: 318.8 },
    densidade: { atual: 9305.56, anterior: 10000, variacaoPercentual: -6.9 },
    produtividade: null,
    municipiosComProducao: 2,
  },
  momento: null,
  mapa: [
    { codigoIbge: 3598101, nome: 'Município Dim 1', emFoco: true, area: 160, quantidade: 310, valor: 1350, clientes: 3 },
    { codigoIbge: 3598102, nome: 'Município Dim 2', emFoco: true, area: 200, quantidade: 400, valor: 2000, clientes: 1 },
    { codigoIbge: 3598103, nome: 'Município Dim 3', emFoco: false, area: 1000, quantidade: 2000, valor: 9000, clientes: 1 },
  ],
  porCultura: [
    { codigo: 'CAFE', nome: 'Café', area: { parte: 300, todo: 2000, percentual: 15 }, quantidade: { parte: 560, todo: 4000, percentual: 14 }, valor: { parte: 3000, todo: 20000, percentual: 15 }, fatiaDaLojaNaTracbel: null },
    { codigo: 'OUTRAS', nome: 'Outras', area: { parte: 10, todo: 100, percentual: 10 }, quantidade: { parte: null, todo: null, percentual: null }, valor: { parte: 50, todo: 500, percentual: 10 }, fatiaDaLojaNaTracbel: null },
  ],
  porLoja: [
    { lojaCodigo: '010103', loja: 'TRACBEL AGRO — BARRETOS', municipios: 1, area: 200, valor: 2000, reaisPorHectare: 10000, tecnificacao: null, culturaDominante: 'Café' },
    { lojaCodigo: '010101', loja: 'TRACBEL AGRO — RIBEIRÃO PRETO', municipios: 1, area: 160, valor: 1350, reaisPorHectare: 8437.5, tecnificacao: null, culturaDominante: 'Café' },
  ],
  carteira: { clientes: clientes(5, 2, '2026-09-22T12:00:00Z'), naRegiao: 4, fora: 1, potencialMedio: 3.25 },
  prioritarios: [
    { codigoIbge: 3598101, nome: 'Município Dim 1', valor: 1350, clientes: 3, coberturaAte90Percentual: 33.3, lacunaPercentual: 33.3, prioridade: 450 },
  ],
  matriz: [
    {
      codigoIbge: 3598101, nome: 'Município Dim 1', regiao: 'Norte', lojaCodigo: '010101', loja: 'TRACBEL AGRO — RIBEIRÃO PRETO',
      vendedor: 'MARIA SOUZA', culturaPrincipal: 'Café',
      area: { atual: 160, anterior: 80, variacaoPercentual: 100 }, quantidade: { atual: 310, anterior: 120, variacaoPercentual: 158.3 },
      valor: { atual: 1350, anterior: 800, variacaoPercentual: 68.8 }, fatiaNaRegiao: 40.3, fatiaNoEstado: 5.74,
      clientes: clientes(3, 2, '2026-09-22T12:00:00Z'), usinas: 1,
    },
    {
      codigoIbge: 3598102, nome: 'Município Dim 2', regiao: 'Noroeste', lojaCodigo: '010103', loja: 'TRACBEL AGRO — BARRETOS',
      vendedor: null, culturaPrincipal: 'Café',
      area: { atual: 200, anterior: null, variacaoPercentual: null }, quantidade: { atual: 400, anterior: null, variacaoPercentual: null },
      valor: { atual: 2000, anterior: null, variacaoPercentual: null }, fatiaNaRegiao: 59.7, fatiaNoEstado: 8.51,
      clientes: clientes(3, 0, null), usinas: 0,
    },
  ],
  porVendedor: [{ responsavelId: 100, nome: 'MARIA SOUZA', natureza: 'Pessoa', naRegiao: 4, fora: 1, clientes: clientes(5, 2, '2026-09-22T12:00:00Z') }],
  lacunas: [{ metrica: 'pesoDaFilial', motivo: 'O peso 0–100 da filial do protótipo usa pesos que ninguém decidiu.' }],
};

function montar() {
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string) => {
      pedidos.push(entrada);
      return entrada.includes('/v1/mercado/dimensionamento')
        ? new Response(JSON.stringify({ dados: DIM, procedencia: null }), { status: 200 })
        : new Response('{"title":"não simulado"}', { status: 404 });
    }),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <DimensionamentoAdr />
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
const bloco = (nome: string) => document.querySelector(`[data-bloco="${nome}"]`) as HTMLElement;

describe('Dimensionamento ADR (issue 259)', () => {
  it('os quatro números trazem a escala certa e a variação contra o ano anterior', async () => {
    montar();
    await aMatriz();

    expect(kpi(/Área plantada/)).toHaveTextContent('360 ha');
    expect(kpi(/Área plantada/)).toHaveTextContent('vs 2023 +350%');
    expect(kpi(/Quantidade produzida/)).toHaveTextContent('710 t');
    // O VALOR CHEGA EM MIL REAIS: 3.350 é R$ 3,4 milhões.
    expect(kpi(/Valor da produção/)).toHaveTextContent('R$ 3,4 mi');
    expect(kpi(/Densidade econômica/)).toHaveTextContent('R$ 9.306/ha');
    expect(kpi(/Densidade econômica/)).toHaveTextContent('vs 2023 −6,9%');
  });

  it('a fatia da Tracbel no estado mostra o percentual e a parte do todo', async () => {
    montar();
    await aMatriz();

    const valor = within(bloco('tracbel-no-estado')).getByText(/Valor da produção — no Estado/).closest('.mom-cartao') as HTMLElement;
    expect(valor).toHaveTextContent('14,3%');
    expect(valor).toHaveTextContent('R$ 3,4 mi de R$ 23,5 mi');
    expect(bloco('tracbel-no-estado')).toHaveTextContent('2 de 3');
  });

  it('a cultura Outras fica sem quantidade, e não com zero', async () => {
    montar();
    await aMatriz();

    const linha = within(bloco('por-cultura')).getByText('Outras').closest('tr') as HTMLElement;
    expect(within(linha).getAllByText('—').length).toBeGreaterThanOrEqual(3);
  });

  it('a carteira traz o total, a região, as faixas acumuladas e o A/B em risco', async () => {
    montar();
    await aMatriz();

    const carteira = bloco('carteira-do-recorte');
    expect(within(carteira).getByText('Total da carteira').closest('.dim-numero')).toHaveTextContent('5');
    expect(within(carteira).getByText('Fora da região').closest('.dim-numero')).toHaveTextContent('1');
    expect(within(carteira).getByText('Cobertos em 120 dias').closest('.dim-numero')).toHaveTextContent('60% da carteira');
    expect(within(carteira).getByText('A/B em risco').closest('.dim-numero')).toHaveTextContent('1');
    expect(within(carteira).getByText('Potencial médio').closest('.dim-numero')).toHaveTextContent('3,25');
  });

  it('os municípios prioritários mostram a cobertura de 90 dias e o valor sem cobertura', async () => {
    montar();
    await aMatriz();

    const linha = within(bloco('prioritarios')).getByText('Município Dim 1').closest('tr') as HTMLElement;
    expect(linha).toHaveTextContent('R$ 1,4 mi');
    expect(linha).toHaveTextContent('33%');
    expect(linha).toHaveTextContent('R$ 450 mil sem cobertura');
  });

  it('a matriz ordena pelo valor, mostra o ano anterior na célula, e a busca filtra', async () => {
    montar();
    await aMatriz();

    const matriz = bloco('matriz');
    const nomes = () => [...matriz.querySelectorAll('tbody .dim-municipio')].map((e) => e.textContent);
    expect(nomes()).toEqual(['Município Dim 2', 'Município Dim 1']);

    const dim1 = within(matriz).getByText('Município Dim 1').closest('tr') as HTMLElement;
    expect(dim1).toHaveTextContent('2023: 800');
    expect(dim1).toHaveTextContent('+68,8%');
    expect(dim1).toHaveTextContent('1 usina');

    fireEvent.change(within(matriz).getByRole('searchbox', { name: 'Buscar município na matriz' }), { target: { value: 'dim 1' } });
    expect(nomes()).toEqual(['Município Dim 1']);
  });

  it('a cultura e a loja vão ao servidor', async () => {
    montar();
    await aMatriz();

    fireEvent.change(bloco('cultura').querySelector('select')!, { target: { value: 'CAFE' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('cultura=CAFE'))).toBe(true));

    fireEvent.change(bloco('regional').querySelector('select')!, { target: { value: 'loja:010101' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('lojaCodigo=010101'))).toBe(true));
  });
});

describe('o CSV da matriz municipal', () => {
  it('leva o ano anterior em colunas separadas e todos os municípios, com vírgula decimal', () => {
    const cabecalho = cabecalhoDaMatriz(DIM);
    expect(cabecalho).toContain('Valor 2023 (mil R$)');
    expect(cabecalho).toContain('Valor 2024 (mil R$)');

    const linhas = linhasDaMatriz(DIM);
    expect(linhas).toHaveLength(2);
    expect(linhas[0][0]).toBe('Município Dim 1');
    expect(linhas[0][cabecalho.indexOf('Valor 2023 (mil R$)')]).toBe('800');
    expect(linhas[0][cabecalho.indexOf('Part. em SP (%)')]).toBe('5,74');
    expect(linhas[1][cabecalho.indexOf('Valor 2023 (mil R$)')]).toBe('');
  });
});
