/**
 * OS CENÁRIOS DE MERCADO NA TELA (issue 263): os quatro números, a tabela com o momento, o realizado, a meta e a
 * recomendação, a escolha gravada na hora (C / M / O e o número manual), o erro da gravação na linha, quem não grava, os
 * filtros que vão ao servidor e o CSV.
 *
 * O caminho é o de produção: as rotas são lidas e gravadas por um `fetch` de mentira, que anota o que a tela pediu.
 */

import { cleanup, fireEvent, render, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cabecalhoDoCsv, enquadramento, linhasDoCsv } from '../componentes/cenarios/cenarios';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { CenarioDoMunicipio, CenariosDeMercado as Cenarios, EscolhaDoCenario } from '../tipos/mercado';
import { CenariosDeMercado } from './CenariosDeMercado';

const pedidos: string[] = [];
const gravacoes: { url: string; corpo: Record<string, string> }[] = [];
let respostaDaGravacao: () => Response;

const linha = (parcial: Partial<CenarioDoMunicipio>): CenarioDoMunicipio => ({
  codigoIbge: 1,
  nome: 'Município',
  regiao: 'Norte',
  lojaCodigo: '010101',
  loja: 'TRACBEL AGRO — RIBEIRÃO PRETO',
  culturaPrincipal: 'Café',
  efeitoDoPreco: 3.2,
  efeitoDoCredito: -4.1,
  realizadoNoAno: 1,
  realizadoNoIntervalo: 1,
  realizadoNoAnoAnterior: 1,
  mediaDosAnosAnteriores: 0.5,
  clientes: 1,
  potencial: 9,
  mercadoAjustado: 9.5,
  metaEstrutural: 2.79,
  shareEstrutural: 11.1,
  shareAjustado: 10.5,
  conservador: 2.8,
  moderado: 2.95,
  otimista: 3.1,
  baseAjustada: true,
  escolha: null,
  aEntregar: 2.95,
  enquadramento: null,
  diferencaParaOModerado: null,
  recomendacao: 'Gradual',
  acimaDoRealizado: 195,
  metaGradual: 1.3,
  gravavel: true,
  ...parcial,
});

const MANUAL: EscolhaDoCenario = { cenario: 'Manual', valorManual: 5, metaCombinada: 5, metaDoCenarioHoje: null, gravadaPor: 'gerente', gravadaEm: '2026-10-02T15:00:00Z' };

const CEN: Cenarios = {
  categoria: 'TRATOR',
  categoriaNome: 'Trator',
  categorias: [
    { codigo: 'TRATOR', nome: 'Trator', ordem: 1 },
    { codigo: 'COLHEDORA_DE_CANA', nome: 'Colhedora de cana', ordem: 7 },
  ],
  anoFiscal: 2026,
  anosFiscais: [2027, 2026, 2025, 2024, 2023],
  anosDaMedia: [2022, 2023, 2024, 2025],
  situacaoDoAno: 'Corrente',
  intervaloDe: '2025-11',
  intervaloAte: '2026-10',
  shareAlvo: 31,
  shareDoPrototipo: true,
  podeGravar: true,
  porQueNaoGrava: null,
  alcanceDaGravacao: 'Todos os municípios',
  totais: {
    realizadoNoAno: 1,
    realizadoNoAnoAnterior: 1,
    mediaDosAnosAnteriores: 0.5,
    realizadoNoIntervalo: 1,
    clientes: 1,
    potencial: 18,
    mercadoAjustado: 19,
    metaEstrutural: 5.58,
    realizacaoSobrePotencial: 5.6,
    aEntregar: 9.85,
    municipiosComMeta: 1,
    municipios: 3,
    entregasAte: '2026-10-02',
  },
  municipios: [
    linha({ codigoIbge: 3, nome: 'Município C3' }),
    linha({
      codigoIbge: 2, nome: 'Município C2', realizadoNoAno: 0, realizadoNoAnoAnterior: 0, potencial: 6, metaEstrutural: 1.86,
      conservador: 1.77, moderado: 1.86, otimista: 1.95, escolha: MANUAL, aEntregar: 5, recomendacao: 'Atingivel', acimaDoRealizado: null, metaGradual: null,
    }),
    linha({
      codigoIbge: 1, nome: 'Município C1', loja: 'TRACBEL AGRO — BARRETOS', realizadoNoAno: 0, realizadoNoAnoAnterior: 0, potencial: 3,
      metaEstrutural: 0.93, conservador: 0.86, moderado: 0.9, otimista: 0.95, aEntregar: 0.9, recomendacao: 'Atingivel', acimaDoRealizado: null,
      metaGradual: null, gravavel: false,
    }),
  ],
  lacunas: [{ metrica: 'pecas', motivo: 'Peças não entram no planejamento.' }],
};

function montar(dados: Cenarios = CEN) {
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string, init?: RequestInit) => {
      pedidos.push(entrada);
      if (init?.method === 'PUT' && entrada.includes('/v1/mercado/cenarios/')) {
        gravacoes.push({ url: entrada, corpo: JSON.parse(String(init.body)) as Record<string, string> });
        return respostaDaGravacao();
      }
      return entrada.includes('/v1/mercado/cenarios')
        ? new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 })
        : new Response('{"title":"não simulado"}', { status: 404 });
    }),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <CenariosDeMercado />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
}

beforeEach(() => {
  pedidos.length = 0;
  gravacoes.length = 0;
  respostaDaGravacao = () =>
    new Response(JSON.stringify({ cenario: 'Otimista', valorManual: null, metaCombinada: 3.1, metaDoCenarioHoje: 3.1, gravadaPor: 'cen.ribeiraopreto', gravadaEm: '2026-10-02T18:00:00Z' }), {
      status: 200,
    });
});
afterEach(() => vi.unstubAllGlobals());

const aTabela = () => waitFor(() => expect(document.querySelector('[data-bloco="planejamento"] table')).not.toBeNull());
const kpi = (rotulo: RegExp) => within(document.querySelector('[data-bloco="kpis"]') as HTMLElement).getByText(rotulo).closest('[data-kpi]');
const bloco = (nome: string) => document.querySelector(`[data-bloco="${nome}"]`) as HTMLElement;
const linhaDe = (nome: string) => within(bloco('planejamento')).getByText(nome).closest('tr') as HTMLElement;

describe('Cenários de Mercado (issue 263)', () => {
  it('os quatro números: realizado com a média e o intervalo, meta estrutural, realização e o que entregar', async () => {
    montar();
    await aTabela();

    expect(kpi(/Realizado FY2026/)).toHaveTextContent('média de 0,5 por ano em FY2022–FY2025');
    expect(kpi(/Realizado FY2026/)).toHaveTextContent('1 no intervalo (nov/25 a out/26)');
    expect(kpi(/Meta estrutural \(31%\)/)).toHaveTextContent('5,6');
    expect(kpi(/Realização vs potencial/)).toHaveTextContent('6%');
    expect(kpi(/A entregar/)).toHaveTextContent('8,9');
    expect(kpi(/A entregar/)).toHaveTextContent('1 de 3 municípios com meta gravada');
  });

  it('a linha traz o momento, o realizado, a meta, o share e a recomendação; o manual mostra o enquadramento', async () => {
    montar();
    await aTabela();

    const c3 = linhaDe('Município C3');
    expect(c3).toHaveTextContent('Preço ↑ +3,2%');
    expect(c3).toHaveTextContent('Crédito ↓ −4,1%');
    expect(c3).toHaveTextContent('FY2025: 1');
    expect(c3).toHaveTextContent('Sugerido 195% acima do realizado');
    expect(c3).toHaveTextContent('gradual: 1,3');
    expect(within(c3).getByRole('button', { name: 'M' })).toHaveClass('padrao');

    const c2 = linhaDe('Município C2');
    expect(c2).toHaveTextContent('Otimista (+169% do moderado)');
    expect(c2).toHaveTextContent('por gerente');
    expect(within(c2).getByRole('textbox', { name: /Meta manual de Município C2/ })).toHaveValue('5');
  });

  it('clicar num cenário grava na hora, e a linha e o total mudam sem reler a tela', async () => {
    montar();
    await aTabela();

    fireEvent.click(within(linhaDe('Município C3')).getByRole('button', { name: 'O' }));

    await waitFor(() => expect(gravacoes).toHaveLength(1));
    expect(gravacoes[0].url).toContain('/v1/mercado/cenarios/3');
    expect(gravacoes[0].corpo).toEqual({ categoria: 'TRATOR', anoFiscal: '2026', cenario: 'Otimista' });
    await waitFor(() => expect(within(linhaDe('Município C3')).getByRole('button', { name: 'O' })).toHaveAttribute('aria-pressed', 'true'));
    expect(kpi(/A entregar/)).toHaveTextContent('2 de 3 municípios com meta gravada');
    expect(kpi(/A entregar/)).toHaveTextContent('9');
    expect(pedidos.filter((p) => p.includes('/v1/mercado/cenarios?') || p.endsWith('/v1/mercado/cenarios'))).toHaveLength(1);
  });

  it('o número manual grava ao teclar Enter, uma vez só', async () => {
    montar();
    await aTabela();
    respostaDaGravacao = () =>
      new Response(JSON.stringify({ ...MANUAL, valorManual: 4, metaCombinada: 4, gravadaPor: 'cen.ribeiraopreto' }), { status: 200 });

    const campo = within(linhaDe('Município C3')).getByRole('textbox', { name: /Meta manual de Município C3/ });
    campo.focus();
    fireEvent.change(campo, { target: { value: '4' } });
    fireEvent.keyDown(campo, { key: 'Enter' });
    fireEvent.blur(campo);

    await waitFor(() => expect(gravacoes).toHaveLength(1));
    expect(gravacoes[0].corpo).toEqual({ categoria: 'TRATOR', anoFiscal: '2026', cenario: 'Manual', valorManual: '4' });
  });

  it('a recusa da gravação aparece na linha, com a mensagem do campo', async () => {
    montar();
    await aTabela();
    respostaDaGravacao = () =>
      new Response(JSON.stringify({ title: 'A escolha tem campos a corrigir.', erros: [{ campo: 'valorManual', mensagem: 'A meta vai de 0 a 100 mil máquinas.' }] }), {
        status: 422,
      });

    fireEvent.click(within(linhaDe('Município C3')).getByRole('button', { name: 'C' }));

    await waitFor(() => expect(within(linhaDe('Município C3')).getByRole('alert')).toHaveTextContent('A meta vai de 0 a 100 mil máquinas.'));
  });

  it('o município de filial fora do alcance fica com os botões desligados, e quem não grava vê o aviso', async () => {
    montar();
    await aTabela();
    expect(within(linhaDe('Município C1')).getByRole('button', { name: 'O' })).toBeDisabled();

    vi.unstubAllGlobals();
    cleanup();
    montar({ ...CEN, podeGravar: false, porQueNaoGrava: 'Gravar a meta pede a permissão Planejamento.Gravar.' });
    await aTabela();
    expect(bloco('so-leitura')).toHaveTextContent('Planejamento.Gravar');
    expect(within(linhaDe('Município C3')).getByRole('button', { name: 'M' })).toBeDisabled();
  });

  it('o produto e o ano fiscal vão ao servidor', async () => {
    montar();
    await aTabela();

    fireEvent.change(bloco('produto').querySelector('select')!, { target: { value: 'COLHEDORA_DE_CANA' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('categoria=COLHEDORA_DE_CANA'))).toBe(true));

    fireEvent.change(bloco('ano').querySelector('select')!, { target: { value: '2027' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('anoFiscal=2027'))).toBe(true));
  });
});

describe('as peças dos Cenários', () => {
  it('o enquadramento do manual é o cenário mais perto, como no protótipo', () => {
    const c3 = CEN.municipios[0];
    expect(enquadramento(c3, { ...MANUAL, metaCombinada: 2.82 })?.nome).toBe('Conservador');
    expect(enquadramento(c3, { ...MANUAL, metaCombinada: 2.96 })?.nome).toBe('Moderado');
    expect(enquadramento(c3, { ...MANUAL, metaCombinada: 9 })?.nome).toBe('Otimista');
    expect(enquadramento(c3, null)).toBeNull();
  });

  it('o CSV leva todas as colunas e a escolha gravada na visita', () => {
    const cabecalho = cabecalhoDoCsv(CEN);
    expect(cabecalho).toContain('Meta estrutural (31%)');
    expect(cabecalho).toContain('Média FY2022–FY2025');
    const otimista: EscolhaDoCenario = { cenario: 'Otimista', valorManual: null, metaCombinada: 3.1, metaDoCenarioHoje: 3.1, gravadaPor: 'x', gravadaEm: '' };
    const linhas = linhasDoCsv(CEN, new Map([[3, otimista]]));
    expect(linhas).toHaveLength(3);
    expect(linhas[0][cabecalho.indexOf('Cenário gravado')]).toBe('Otimista');
    expect(linhas[0][cabecalho.indexOf('A entregar')]).toBe('3,1');
    expect(linhas[1][cabecalho.indexOf('Cenário gravado')]).toBe('Manual');
  });
});
