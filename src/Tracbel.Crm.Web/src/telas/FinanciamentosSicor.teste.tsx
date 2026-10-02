/**
 * A GESTÃO DE FINANCIAMENTOS NA TELA (issue 261): os quatro números, os municípios por situação, o momento R3/R6/R12 com o
 * peso em São Paulo, a tabela por loja e a analítica com o ano anterior na célula, os filtros que vão ao servidor, a
 * granularidade da série e o CSV com as colunas separadas.
 *
 * O caminho é o de produção: a rota é lida por um `fetch` de mentira, que anota o que a tela pediu.
 */

import { fireEvent, render, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { agrupar, cabecalhoDaAnalitica, linhasDaAnalitica } from '../componentes/financiamentos/financiamentos';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { FinanciamentosDoSicor as Financiamentos, IndiceDeCredito, JanelasDeCredito } from '../tipos/mercado';
import { FinanciamentosSicor } from './FinanciamentosSicor';

// O GRÁFICO TEM TAMANHO MEDIDO (ResizeObserver) E DESENHA EM CANVAS — o jsdom não tem nenhum dos dois. Aqui o que está sob
// prova são os números; a moldura diz só que o gráfico existe.
vi.mock('../componentes/MolduraDeGrafico', () => ({ MolduraDeGrafico: () => <div data-grafico="credito" /> }));

const pedidos: string[] = [];

const indice = (j: JanelasDeCredito, faixa: IndiceDeCredito['faixa']): IndiceDeCredito => ({
  indice: 0.7 * (j.linhas / j.linhasAnteriores) + 0.3 * (j.valor / j.valorAnterior),
  faixa,
  indiceDeLinhas: j.linhas / j.linhasAnteriores,
  indiceDeValor: j.valor / j.valorAnterior,
  linhas: j.linhas,
  linhasAnteriores: j.linhasAnteriores,
  valorMedioPorLinha: j.valor / j.linhas,
  valorMedioAnterior: j.valorAnterior / j.linhasAnteriores,
  indiceDoValorMedio: null,
  basePequena: false,
  motivo: 'Nenhum',
});

const F1: JanelasDeCredito = { linhas: 4, valor: 650_000, linhasAnteriores: 2, valorAnterior: 150_000 };
const F2: JanelasDeCredito = { linhas: 1, valor: 300_000, linhasAnteriores: 1, valorAnterior: 200_000 };
const TOTAL: JanelasDeCredito = { linhas: 5, valor: 950_000, linhasAnteriores: 3, valorAnterior: 350_000 };

const FIN: Financiamentos = {
  primeiroMesDoSicor: '2024-01-01',
  ultimoMesDoSicor: '2026-08-01',
  mesesDeCarencia: 0,
  carenciaDecidida: false,
  de: '2025-09-01',
  ate: '2026-08-01',
  meses: 12,
  anteriorDe: '2024-09-01',
  anteriorAte: '2025-08-01',
  produto: null,
  programa: null,
  produtos: [{ codigo: 7080, nome: 'TRATORES' }, { codigo: 2700, nome: 'COLHEITADEIRAS' }],
  programas: [{ codigo: 155, nome: 'PRONAF' }],
  recorte: 'adr',
  recorteNome: 'Região Tracbel',
  municipiosNoRecorte: 2,
  lojas: [{ codigo: '010101', nome: 'TRACBEL AGRO — RIBEIRÃO PRETO', municipios: 1 }],
  totais: { linhas: 5, valor: 950_000, linhasAnteriores: 3, valorAnterior: 350_000, indice: indice(TOTAL, 'Superaquecido') },
  momento: [
    { meses: 3, janelas: { linhas: 3, valor: 500_000, linhasAnteriores: 2, valorAnterior: 300_000 }, indice: indice({ linhas: 3, valor: 500_000, linhasAnteriores: 2, valorAnterior: 300_000 }, 'Superaquecido') },
    { meses: 6, janelas: null, indice: null },
    { meses: 12, janelas: TOTAL, indice: indice(TOTAL, 'Superaquecido') },
  ],
  serie: [
    { mes: '2026-06-01', linhas: 0, valor: 0 },
    { mes: '2026-07-01', linhas: 1, valor: 300_000 },
    { mes: '2026-08-01', linhas: 2, valor: 200_000 },
  ],
  noEstado: { linhas: 5, linhasDoEstado: 6, fatiaDasLinhas: 83.33, valor: 950_000, valorDoEstado: 1_850_000, fatiaDoValor: 51.4 },
  situacoes: { retraidos: 0, intermediarios: 1, aquecidos: 0, superaquecidos: 1, semBase: 0, basePequena: 0 },
  porLoja: [{ lojaCodigo: '010101', loja: 'TRACBEL AGRO — RIBEIRÃO PRETO', municipios: 1, janelas: F1, indice: indice(F1, 'Superaquecido') }],
  municipios: [
    { codigoIbge: 1, nome: 'Município Fin 1', pertenceAAdr: true, lojaCodigo: '010101', loja: 'TRACBEL AGRO — RIBEIRÃO PRETO', janelas: F1, fatiaNoEstado: 35.14, variacaoDaFatia: 1.2, indice: indice(F1, 'Superaquecido') },
    { codigoIbge: 2, nome: 'Município Fin 2', pertenceAAdr: true, lojaCodigo: null, loja: null, janelas: F2, fatiaNoEstado: 16.216, variacaoDaFatia: -0.5, indice: indice(F2, 'Intermediaria') },
  ],
  lacunas: [{ metrica: 'linhas', motivo: 'Linha do SICOR não é contrato.' }],
};

function montar() {
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string) => {
      pedidos.push(entrada);
      return entrada.includes('/v1/mercado/financiamentos')
        ? new Response(JSON.stringify({ dados: FIN, procedencia: null }), { status: 200 })
        : new Response('{"title":"não simulado"}', { status: 404 });
    }),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <FinanciamentosSicor />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
}

beforeEach(() => {
  pedidos.length = 0;
});
afterEach(() => vi.unstubAllGlobals());

const aAnalitica = () => waitFor(() => expect(document.querySelector('[data-bloco="analitica"] table')).not.toBeNull());
const kpi = (rotulo: RegExp) => within(document.querySelector('[data-bloco="kpis"]') as HTMLElement).getByText(rotulo).closest('[data-kpi]');
const bloco = (nome: string) => document.querySelector(`[data-bloco="${nome}"]`) as HTMLElement;

describe('Gestão de Financiamentos (issue 261)', () => {
  it('os quatro números: linhas, valor com o ano anterior, e o R12 das duas parcelas', async () => {
    montar();
    await aAnalitica();

    expect(kpi(/Linhas do SICOR/)).toHaveTextContent('5');
    expect(kpi(/Linhas do SICOR/)).toHaveTextContent('Região Tracbel · set/25 a ago/26');
    expect(kpi(/Valor financiado/)).toHaveTextContent('vs set/24 a ago/25 +171,4%');
    expect(kpi(/R12 linhas/)).toHaveTextContent('+66,7%');
    expect(kpi(/R12 valor/)).toHaveTextContent('superaquecido');
  });

  it('os municípios por situação e o momento, com o horizonte sem base dito e o peso em São Paulo', async () => {
    montar();
    await aAnalitica();

    expect(within(bloco('situacoes')).getByText('Intermediários').closest('.mom-cartao')).toHaveTextContent('1');
    expect(within(bloco('situacoes')).getByText('Aquecidos').closest('.mom-cartao')).toHaveTextContent('1 superaquecidos');

    const momento = bloco('momento');
    expect(within(momento).getByText('R3 — valor').closest('.mom-cartao')).toHaveTextContent('+66,7%');
    expect(within(momento).getByText('R6 — valor').closest('.mom-cartao')).toHaveTextContent('sem o mesmo período do ano anterior');
    expect(within(momento).getByText('Peso em São Paulo').closest('.mom-cartao')).toHaveTextContent('51,4%');
  });

  it('a tabela analítica traz o ano anterior na célula, a fatia, a situação, e a busca filtra', async () => {
    montar();
    await aAnalitica();

    const tabela = bloco('analitica');
    const f1 = within(tabela).getByText('Município Fin 1').closest('tr') as HTMLElement;
    expect(f1).toHaveTextContent('ano ant.: 2');
    expect(f1).toHaveTextContent('+100%');
    expect(f1).toHaveTextContent('35,14%');
    expect(f1).toHaveTextContent('Superaquecido');

    fireEvent.change(within(tabela).getByRole('searchbox', { name: 'Buscar município na tabela analítica' }), { target: { value: 'fin 2' } });
    expect(within(tabela).queryByText('Município Fin 1')).toBeNull();
    expect(within(tabela).getByText('Município Fin 2').closest('tr')).toHaveTextContent('Intermediária');
  });

  it('o produto, o programa e o recorte vão ao servidor', async () => {
    montar();
    await aAnalitica();

    fireEvent.change(bloco('produto').querySelector('select')!, { target: { value: '2700' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('produto=2700'))).toBe(true));

    fireEvent.change(bloco('programa').querySelector('select')!, { target: { value: '155' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('programa=155'))).toBe(true));

    fireEvent.change(bloco('regional').querySelector('select')!, { target: { value: 'recorte:sp' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('recorte=sp'))).toBe(true));
  });

  it('o atalho de 6 meses termina no fim do período padrão', async () => {
    montar();
    await aAnalitica();

    fireEvent.change(bloco('periodo').querySelector('select')!, { target: { value: '6' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('de=2026-03') && p.includes('ate=2026-08'))).toBe(true));
  });
});

describe('as peças dos Financiamentos', () => {
  it('a granularidade agrupa no calendário e marca o último grupo incompleto', () => {
    const serie = [
      { mes: '2025-11-01', linhas: 1, valor: 10 },
      { mes: '2025-12-01', linhas: 2, valor: 20 },
      { mes: '2026-01-01', linhas: 3, valor: 30 },
    ];
    expect(agrupar(serie, 'tri')).toEqual([
      { rotulo: '4º tri/25', linhas: 3, valor: 30, meses: 2, completo: false },
      { rotulo: '1º tri/26', linhas: 3, valor: 30, meses: 1, completo: false },
    ]);
    expect(agrupar(serie, 'ano').map((p) => p.rotulo)).toEqual(['2025', '2026']);
  });

  it('o CSV leva o período e o ano anterior em colunas separadas', () => {
    const cabecalho = cabecalhoDaAnalitica(FIN);
    expect(cabecalho).toContain('Valor set/25 a ago/26 (R$)');
    expect(cabecalho).toContain('Linhas set/24 a ago/25');
    const linhas = linhasDaAnalitica(FIN);
    expect(linhas).toHaveLength(2);
    expect(linhas[0][cabecalho.indexOf('Δ valor (%)')]).toBe('333,3');
    expect(linhas[1][cabecalho.indexOf('Situação')]).toBe('Intermediária');
  });
});
