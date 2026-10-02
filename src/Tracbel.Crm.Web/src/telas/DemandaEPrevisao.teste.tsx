/**
 * A DEMANDA E PREVISÃO NA TELA (issue 258; no desenho da maquete do Ricardo em 02/10/2026): os quatro números contra o ano
 * anterior, a previsão mês a mês com a entrega realizada e o atendimento, a entrega por loja com as cinco maiores, os
 * parâmetros por cultura, os maiores municípios, os filtros da maquete e o CSV com todas as linhas.
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

function municipio(i: number, nome: string, demanda: number) {
  return {
    codigoIbge: i, nome, regiao: 'Norte', lojaCodigo: '010110', loja: 'Araraquara', areaUtilHectares: demanda * 50, parque: demanda * 5,
    porCultura: [{ culturaCodigo: 'CAFE', demanda }], demandaEstrutural: demanda, demandaAjustada: demanda + 1, aEntregar: demanda * 0.31,
    aEntregarAjustada: (demanda + 1) * 0.31, fatorDePreco: 1.12, fatorDeCredito: 0.95, culturaPredominante: 'Café', variacaoPercentual: 16.7,
    demandaEstruturalAnoAnterior: demanda * 0.8, variacaoAnoAnterior: 25,
  };
}

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
    culturasComRegra: ['Café', 'Soja'],
    municipios: 12,
    municipiosComDemanda: 12,
    estimativa: true,
    parqueAnoAnterior: 72,
    demandaEstruturalAnoAnterior: 14.4,
    culturasComAumentoDeArea: ['Café'],
    entreguesNoPeriodo: 5,
    entreguesNoPeriodoAnterior: 4,
    entregasAte: '2026-10-02',
  },
  porCultura: [
    {
      culturaCodigo: 'CAFE', cultura: 'Café', areaUtilHectares: 900, hectaresPorMaquina: 10, anosDeRenovacao: 5, parque: 90, demandaEstrutural: 18,
      demandaAjustada: 20, variacaoPercentual: 11.1, areaAnoAnterior: 720, parqueAnoAnterior: 72, demandaAnoAnterior: 14.4,
    },
  ],
  previsaoMensal: MESES.map((mes) => ({
    mes,
    fracao: mes === 5 ? 0.12 : 0.08,
    demandaEstrutural: 1.5,
    demandaAjustada: mes === 5 ? 2.4 : 1.6,
    aEntregar: 0.47,
    aEntregarAjustada: mes === 5 ? 0.74 : 0.5,
    entregues: mes === 5 ? 1 : mes === 9 ? null : 0,
  })),
  porLoja: [
    { lojaCodigo: '010110', loja: 'Araraquara', aEntregarNoAno: 6.2, porMes: MESES.map(() => 0.52) },
    ...['B', 'C', 'D', 'E', 'F'].map((l, i) => ({ lojaCodigo: `01012${i}`, loja: `Loja ${l}`, aEntregarNoAno: 5 - i, porMes: MESES.map(() => 0.3) })),
  ],
  municipios: Array.from({ length: 12 }, (_, i) => municipio(i + 1, `Município ${String.fromCharCode(65 + i)}`, 12 - i)),
  culturas: [{ codigo: 'CAFE', nome: 'Café', demanda: 18 }],
  lacunas: [
    { metrica: 'porCliente', motivo: 'A demanda é por município, e não por cliente.' },
    { metrica: 'entregaRealizada', motivo: 'A entrega realizada é a do ART pela data da entrega.' },
  ],
  anoFiscal: 2026,
  anosFiscais: [2026, 2025, 2024, 2023],
  cultura: null,
  culturasDoFiltro: [
    { codigo: 'CAFE', nome: 'Café', demanda: null },
    { codigo: 'SOJA', nome: 'Soja', demanda: null },
  ],
  anoDaAreaAnterior: 2023,
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

const aTela = () => waitFor(() => expect(document.querySelector('[data-bloco="matriz"] table')).not.toBeNull());
const kpi = (rotulo: RegExp) => within(document.querySelector('[data-bloco="kpis"]') as HTMLElement).getByText(rotulo).closest('[data-kpi]');
const campo = (rotulo: string) =>
  within(document.querySelector('[data-bloco="filtros"]') as HTMLElement).getByText(rotulo).closest('label')!.querySelector('select')!;

describe('Demanda e Previsão (issue 258, maquete de 02/10/2026)', () => {
  it('os quatro números: parque e demanda contra o ano anterior, a entrega realizada e as culturas com aumento de área', async () => {
    montar();
    await aTela();

    // O PADRÃO É "TODOS OS CENÁRIOS", como a maquete: a estrutural no número e a ajustada ao lado.
    expect(campo('Meta / Previsão')).toHaveDisplayValue('Todos os cenários');
    expect(kpi(/Parque potencial/)).toHaveTextContent('90');
    expect(kpi(/Parque potencial/)).toHaveTextContent('+25%');
    expect(kpi(/Parque potencial/)).toHaveTextContent('Trator na área plantada comparável');
    expect(kpi(/Demanda anual/)).toHaveTextContent('18unidades');
    expect(kpi(/Demanda anual/)).toHaveTextContent('+25% em relação ao ano anterior');
    expect(kpi(/Demanda anual/)).toHaveTextContent('20 ajustada pelo momento atual');
    expect(kpi(/Entrega no período/)).toHaveTextContent('5unidades');
    expect(kpi(/Entrega no período/)).toHaveTextContent('28% da previsão anual');
    expect(kpi(/Entrega no período/)).toHaveTextContent('+25% vs. mesmo período ano anterior');
    expect(kpi(/Culturas com espaço/)).toHaveTextContent('Culturas com espaço1');
    expect(kpi(/Culturas com espaço/)).toHaveTextContent('Aumento em área, puxado por café');
    // OS GRÁFICOS PEQUENOS DOS CARTÕES SÃO SÉRIES DE VERDADE, e dizem qual.
    expect(within(kpi(/Demanda anual/) as HTMLElement).getByRole('img', { name: /A demanda prevista mês a mês/ })).toBeInTheDocument();
  });

  it('a base ajustada troca os números da tela inteira, sem pedir de novo', async () => {
    montar();
    await aTela();
    const antes = pedidos.length;

    fireEvent.change(campo('Meta / Previsão'), { target: { value: 'ajustada' } });

    expect(kpi(/Demanda anual/)).toHaveTextContent('20unidades');
    expect(kpi(/Demanda anual/)).toHaveTextContent('18 estrutural, sem o momento');
    expect(kpi(/Entrega no período/)).toHaveTextContent('25% da previsão anual');
    expect(pedidos.length).toBe(antes);
  });

  it('a previsão vai de novembro a outubro com a demanda, a entrega e o atendimento; escolher um mês vira o cartão', async () => {
    montar();
    await aTela();

    const previsao = document.querySelector('[data-bloco="previsao-mensal"]') as HTMLElement;
    const meses = within(previsao).getAllByRole('button', { pressed: false });
    expect(meses[0]).toHaveAccessibleName(/^novembro/);
    expect(meses[11]).toHaveAccessibleName(/^outubro/);
    expect(within(previsao).getByRole('button', { name: /^maio: 1,5 máquinas de demanda prevista, 1 entregues, 67% de atendimento/ })).toBeInTheDocument();
    // O MÊS SEM ENTREGA CONTADA (o que ainda não começou) NÃO TEM ATENDIMENTO: nem zero, nem ponto.
    expect(within(previsao).getByRole('button', { name: /^setembro/ })).not.toHaveAccessibleName(/entregues/);

    fireEvent.click(within(previsao).getByRole('button', { name: /^maio/ }));
    expect(kpi(/Entrega em maio/)).toHaveTextContent('1unidades');
    expect(kpi(/Entrega em maio/)).toHaveTextContent('67% da previsão do mês');
  });

  it('a entrega por loja mostra as cinco maiores, o total, e "Ver todas as lojas" abre o resto', async () => {
    montar();
    await aTela();

    const lojas = document.querySelector('[data-bloco="entrega-por-loja"]') as HTMLElement;
    const linha = within(lojas).getByText('Araraquara').closest('tr') as HTMLElement;
    expect(within(linha).getAllByText('0,5')).toHaveLength(12);
    expect(within(linha).getByText('6,2')).toBeInTheDocument();
    expect(within(lojas).queryByText('Loja F')).toBeNull();

    fireEvent.click(within(lojas).getByRole('button', { name: /Ver todas as lojas/ }));
    expect(within(lojas).getByText('Loja F')).toBeInTheDocument();
  });

  it('a cultura mostra os parâmetros, a renovação anual e o ano anterior na dica', async () => {
    montar();
    await aTela();

    const culturas = document.querySelector('[data-bloco="por-cultura"]') as HTMLElement;
    const linha = within(culturas).getByText('Café').closest('tr') as HTMLElement;
    expect(linha).toHaveTextContent('20%');
    expect(linha).toHaveTextContent('+11%');
    expect(within(linha).getByTitle('Área em 2023: 720 (+25%)')).toHaveTextContent('900');
  });

  it('os maiores municípios mostram os dez primeiros, a variação do ano anterior e "Ver todos" abre a busca', async () => {
    montar();
    await aTela();

    const tabela = document.querySelector('[data-bloco="matriz"]') as HTMLElement;
    const nomes = () => [...tabela.querySelectorAll('tbody .dem-municipio')].map((e) => e.textContent);
    expect(nomes()).toHaveLength(10);
    expect(nomes()[0]).toBe('Município A');
    expect(within(tabela).getAllByText('+25%').length).toBe(10);

    fireEvent.click(within(tabela).getByRole('button', { name: /Ver todos os municípios/ }));
    fireEvent.change(within(tabela).getByRole('searchbox', { name: 'Buscar município' }), { target: { value: 'município l' } });
    expect(nomes()).toEqual(['Município L']);
  });

  // UM FILTRO POR TESTE: os três pedidos num teste só passavam dos 5 s quando a suíte inteira disputava a máquina.
  it('o período pede a demanda de novo, do ano fiscal escolhido', async () => {
    montar();
    await aTela();

    fireEvent.change(campo('Período'), { target: { value: '2025' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('anoFiscal=2025'))).toBe(true));
  });

  it('a cultura pede a demanda de novo, recortada', async () => {
    montar();
    await aTela();

    fireEvent.change(campo('Cultura'), { target: { value: 'CAFE' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('cultura=CAFE'))).toBe(true));
  });

  it('o tipo de máquina mora em "Mais filtros" e pede a demanda de novo', async () => {
    montar();
    await aTela();

    fireEvent.click(screen.getByRole('button', { name: /Mais filtros/ }));
    const tipo = (await screen.findByText('Tipo de máquina')).closest('label')!.querySelector('select')!;
    fireEvent.change(tipo, { target: { value: 'TODAS' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('categoria=TODAS'))).toBe(true));
  });
});

describe('o CSV da demanda', () => {
  it('leva uma coluna por cultura e todos os municípios, com vírgula decimal', () => {
    expect(cabecalhoDoCsv(DEMANDA)).toContain('Demanda Café');
    const linhas = linhasDoCsv(DEMANDA);
    expect(linhas).toHaveLength(12);
    expect(linhas[0][0]).toBe('Município A');
    expect(linhas[0][5]).toBe('12');
    expect(linhas[0][10]).toBe('1,12');
  });
});
