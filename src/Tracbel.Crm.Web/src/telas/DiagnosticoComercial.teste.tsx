/**
 * O DIAGNÓSTICO COMERCIAL NA TELA (issue 257; no desenho da maquete do Ricardo em 02/10/2026): o IOC ordena os
 * municípios, a classe vem escrita com a cor, o que não tem dado mostra o traço com o motivo, a prioridade filtra a
 * tela, a ficha abre ao escolher um município, os filtros da maquete (regional / loja, cultura principal e CEN / gestor)
 * recortam a tela e o CSV leva exatamente o que está na tabela.
 *
 * O caminho é o de produção: a rota é lida por um `fetch` de mentira, que anota o que a tela pediu.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { DiagnosticoComercialDaRegiao, MunicipioNoDiagnostico } from '../tipos/mercado';
import { linhasDoCsv, ordenar, resumir } from '../componentes/diagnostico/diagnostico';
import { DiagnosticoComercial } from './DiagnosticoComercial';

const guardado = new Map<string, string>();
const pedidos: string[] = [];

function municipio(parcial: Partial<MunicipioNoDiagnostico> & { codigoIbge: number; nome: string }): MunicipioNoDiagnostico {
  return {
    regiao: 'Norte',
    lojaCodigo: '010110',
    loja: 'Araraquara',
    culturaPrincipal: 'Cana-de-açúcar',
    indiceDePreco: 1.1,
    indiceDeCredito: 1.2,
    creditoBasePequena: false,
    demandaEstrutural: 10,
    demandaAjustada: 11,
    metaDePlanejamento: 3.41,
    vendidasNoPeriodo: null,
    vendidasNoAno: null,
    clientes: 12,
    clientesPorClasse: { a: 2, b: 3, c: 4, d: 1, semClasse: 2 },
    clientesEmCarteira: 9,
    clientesQueCompraram: 5,
    vinculosComCadencia: 20,
    cobertos: 5,
    cobertura: 0.25,
    penetracao: null,
    componentes: { potencial: 1, cobertura: 0.75, credito: 0.75, rentabilidade: 0.625, clientes: 0, realizacao: null, penetracao: null },
    ioc: 70,
    classe: 'Alta',
    situacao: 'elevado potencial, baixa cobertura comercial',
    planoDeAcao: 'Expandir cobertura e visitas presenciais · Explorar financiamento (Moderfrota, Finame)',
    componentesAusentes: ['realização: o ART não trouxe as vendas de máquina', 'penetração: o ART não trouxe as vendas de máquina'],
    estimativa: true,
    responsavel: 'ALFA MATIAS DE ARAUJO CRUZ',
    ...parcial,
  };
}

const DIAGNOSTICO: DiagnosticoComercialDaRegiao = {
  competenciaInicial: '2025-09-01',
  competenciaFinal: '2026-08-01',
  fracaoDoAnoNoPeriodo: 1,
  categoria: 'TRATOR',
  categoriaNome: 'Trator',
  categorias: [{ codigo: 'TRATOR', nome: 'Trator', ordem: 1 }],
  pesos: { potencial: 25, cobertura: 20, credito: 15, rentabilidade: 15, clientes: 10, realizacao: 5, penetracao: 10 },
  pesosVigentesDesde: '2026-09-27',
  pesosDoPrototipo: true,
  shares: [{ categoriaCodigo: 'TRATOR', categoriaNome: 'Trator', percentual: 31, doPrototipo: true }],
  percentil90: 10,
  resumo: {
    maxima: 1,
    alta: 1,
    moderada: 0,
    baixa: 0,
    manutencao: 1,
    semIndice: 1,
    total: 4,
    iocMedio: 60,
    demandaEstrutural: 30,
    demandaAjustada: 33,
    municipiosComDemanda: 3,
    metaDePlanejamento: 10.2,
    vendidasNoPeriodo: null,
    vendidasNoAno: null,
    penetracao: null,
    clientes: 48,
    clientesQueCompraram: 20,
  },
  municipios: [
    municipio({ codigoIbge: 1, nome: 'Barretos', ioc: 70, classe: 'Alta', lojaCodigo: '010103', loja: 'Barretos' }),
    municipio({ codigoIbge: 2, nome: 'Araraquara', ioc: 91.5, classe: 'Maxima', culturaPrincipal: 'Soja' }),
    municipio({
      codigoIbge: 3,
      nome: 'Colina',
      ioc: 18.5,
      classe: 'Manutencao',
      vinculosComCadencia: 0,
      cobertos: 0,
      cobertura: null,
      responsavel: null,
    }),
    municipio({ codigoIbge: 4, nome: 'Dumont', ioc: null, classe: null, demandaEstrutural: null, componentes: null }),
  ],
  lacunas: [
    { metrica: 'vendasEmUnidades', motivo: 'O ART não trouxe venda de máquina ao alcance desta consulta.' },
    { metrica: 'pesosDoIoc', motivo: 'Os pesos do IOC ainda são os do protótipo, a confirmar.' },
  ],
  responsaveis: [{ id: 7, nome: 'ALFA MATIAS DE ARAUJO CRUZ', natureza: 'Pessoa', carteiras: 2, gestor: null }],
};

function montar() {
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
      return entrada.includes('/v1/mercado/diagnostico')
        ? new Response(JSON.stringify({ dados: DIAGNOSTICO, procedencia: null }), { status: 200 })
        : new Response('{"title":"não simulado"}', { status: 404 });
    }),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <DiagnosticoComercial />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
}

beforeEach(() => {
  guardado.clear();
  pedidos.length = 0;
});
afterEach(() => vi.unstubAllGlobals());

const nomesNaTabela = () =>
  within(screen.getByRole('table'))
    .getAllByRole('row')
    .slice(1)
    .map((l) => l.querySelector('button.diag-municipio')?.textContent);

const aTabela = () => waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());
const campo = (rotulo: string) =>
  within(document.querySelector('[data-bloco="filtros"]') as HTMLElement).getByText(rotulo).closest('label')!.querySelector('select')!;
const kpi = (rotulo: string) => within(document.querySelector('[data-bloco="kpis"]') as HTMLElement).getByText(rotulo).closest('[data-kpi]');

describe('Diagnóstico Comercial (issue 257, maquete de 02/10/2026)', () => {
  it('ordena pelo IOC, do maior para o menor, com o sem índice por último, e numera a posição', async () => {
    montar();
    await aTabela();

    expect(nomesNaTabela()).toEqual(['Araraquara', 'Barretos', 'Colina', 'Dumont']);
    const primeira = within(screen.getByRole('table')).getAllByRole('row')[1];
    expect(primeira.querySelector('td')).toHaveTextContent('1');
    expect(pedidos.some((p) => p.includes('/v1/mercado/diagnostico'))).toBe(true);
  });

  it('a classe vem com o número, e os pesos do protótipo e o share-alvo moram em "Entenda os indicadores"', async () => {
    montar();
    await aTabela();

    const tabela = screen.getByRole('table');
    expect(within(tabela).getByTitle('Prioridade máxima')).toHaveTextContent('Prioridade máxima');
    expect(within(tabela).getByTitle('Manutenção')).toHaveTextContent('18,5');

    fireEvent.click(screen.getByRole('button', { name: /Entenda os indicadores/ }));
    expect(await screen.findByText('protótipo, a confirmar')).toBeInTheDocument();
    expect(screen.getByText(/Trator 31%/)).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /Ajustar em Configurações/ })).toHaveAttribute('href', '/config');
  });

  it('os quatro números de decisão: prioritários, demanda, meta com a rosca e vendas — e sem ART as vendas dizem por quê', async () => {
    montar();
    await aTabela();

    expect(kpi('Municípios prioritários')).toHaveTextContent('2de 4');
    expect(kpi('Municípios prioritários')).toHaveTextContent('(IOC médio: 60)');
    expect(kpi('Demanda anual')).toHaveTextContent('33unidades');
    expect(kpi('Meta de planejamento')).toHaveTextContent('share-alvo de 31%');
    expect(screen.getByRole('img', { name: 'Share-alvo de 31%' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Por que vendas no período não aparece' })).toBeInTheDocument();
  });

  it('sem dado mostra o traço com o motivo, e não zero', async () => {
    montar();
    await aTabela();

    expect(screen.getAllByRole('button', { name: 'Por que as vendas não aparece' }).length).toBeGreaterThan(0);
    expect(screen.getByRole('button', { name: 'Por que a cobertura não aparece' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Por que o IOC de Dumont não aparece' })).toBeInTheDocument();
  });

  it('a prioridade filtra a tela pela classe, sem pedir de novo, e clicar de novo mostra todas', async () => {
    montar();
    await aTabela();
    const antes = pedidos.length;

    const maxima = screen.getByRole('button', { name: /Prioridade máxima/ });
    fireEvent.click(maxima);
    expect(maxima).toHaveAttribute('aria-pressed', 'true');
    expect(nomesNaTabela()).toEqual(['Araraquara']);

    fireEvent.click(maxima);
    expect(nomesNaTabela()).toEqual(['Araraquara', 'Barretos', 'Colina', 'Dumont']);
    expect(pedidos.length).toBe(antes);
  });

  it('a cultura principal recorta a tela sem pedir de novo, e os cartões passam a ser do recorte', async () => {
    montar();
    await aTabela();
    const antes = pedidos.length;

    fireEvent.change(campo('Cultura principal'), { target: { value: 'Soja' } });

    expect(nomesNaTabela()).toEqual(['Araraquara']);
    expect(kpi('Municípios prioritários')).toHaveTextContent('1de 1');
    expect(screen.getByText(/só a cultura principal Soja/)).toBeInTheDocument();
    expect(pedidos.length).toBe(antes);
  });

  it('o CEN / Gestor vai ao servidor e deixa só os municípios com carteira dele', async () => {
    montar();
    await aTabela();

    expect(within(campo('CEN / Gestor')).getByRole('option', { name: 'Alfa Matias de Araujo Cruz' })).toBeInTheDocument();
    fireEvent.change(campo('CEN / Gestor'), { target: { value: '7' } });

    await waitFor(() => expect(pedidos.some((p) => p.includes('responsavel=7'))).toBe(true));
    await waitFor(() => expect(nomesNaTabela()).toEqual(['Araraquara', 'Barretos', 'Dumont']));
  });

  it('Regional / Loja é um campo só: a região ou a loja', async () => {
    montar();
    await aTabela();

    fireEvent.change(campo('Regional / Loja'), { target: { value: 'loja:010103' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('lojaCodigo=010103') && !p.includes('regiao='))).toBe(true));

    fireEvent.change(campo('Regional / Loja'), { target: { value: 'regiao:Noroeste' } });
    await waitFor(() => expect(pedidos.some((p) => p.includes('regiao=Noroeste') && !p.includes('lojaCodigo='))).toBe(true));
  });

  it('o tipo de máquina mora em "Mais filtros" e pede o diagnóstico de novo', async () => {
    montar();
    await aTabela();

    fireEvent.click(screen.getByRole('button', { name: /Mais filtros/ }));
    const tipo = (await screen.findByText('Tipo de máquina')).closest('label')!.querySelector('select')!;
    fireEvent.change(tipo, { target: { value: 'TODAS' } });

    await waitFor(() => expect(pedidos.some((p) => p.includes('categoria=TODAS'))).toBe(true));
  });

  it('"Onde agir primeiro" lista os de maior IOC com a região e o CEN; escolher abre a ficha, e fechar volta', async () => {
    montar();
    await aTabela();

    const ficha = () => document.querySelector('[data-bloco="ficha"]') as HTMLElement;
    expect(within(ficha()).getByText('Onde agir primeiro')).toBeInTheDocument();
    expect(within(ficha()).getAllByText('Região Norte • Alfa M. Cruz').length).toBeGreaterThan(0);

    fireEvent.click(within(screen.getByRole('table')).getByRole('button', { name: 'Barretos' }));

    expect(within(ficha()).getByText('Barretos')).toBeInTheDocument();
    expect(within(ficha()).getAllByText('fora da conta')).toHaveLength(2);
    expect(within(ficha()).getByText(/Fora da conta:/).closest('p')).toHaveTextContent('realização: o ART não trouxe as vendas de máquina');
    expect(within(ficha()).getByRole('list', { name: 'Plano de ação' }).querySelectorAll('li')).toHaveLength(2);
    expect(within(ficha()).getByText(/9 em carteira/)).toBeInTheDocument();

    fireEvent.click(within(ficha()).getByRole('button', { name: 'Fechar a ficha de Barretos' }));
    expect(within(ficha()).getByText('Onde agir primeiro')).toBeInTheDocument();
  });

  it('a tabela mostra a primeira ação do plano e guarda o plano inteiro na dica; o menu leva à ficha e aos Indicadores', async () => {
    montar();
    await aTabela();

    const linha = within(screen.getByRole('table')).getByRole('button', { name: 'Barretos' }).closest('tr') as HTMLElement;
    const acao = within(linha).getByText('Expandir cobertura e visitas presenciais');
    expect(acao).toHaveAttribute('title', expect.stringContaining('Explorar financiamento'));
    expect(within(linha).getByTitle('Ajustada pelo momento: 11')).toHaveTextContent('10');

    fireEvent.click(within(linha).getByRole('button', { name: 'Ações de Barretos' }));
    expect(within(linha).getByRole('menuitem', { name: 'Ver nos Indicadores Geográficos' })).toHaveAttribute('href', '/relatorios/territorio?municipio=1');
  });

  it('ordenar por nome põe em ordem alfabética, e clicar de novo inverte', async () => {
    montar();
    await aTabela();

    const cabecalho = within(screen.getByRole('table')).getAllByRole('columnheader')[1];
    fireEvent.click(within(cabecalho).getByRole('button'));
    expect(nomesNaTabela()).toEqual(['Araraquara', 'Barretos', 'Colina', 'Dumont']);
    fireEvent.click(within(cabecalho).getByRole('button'));
    expect(nomesNaTabela()).toEqual(['Dumont', 'Colina', 'Barretos', 'Araraquara']);
  });
});

describe('funções do diagnóstico', () => {
  it('o vazio vai para o fim nos dois sentidos', () => {
    const linhas = DIAGNOSTICO.municipios;
    expect(ordenar(linhas, 'ioc', 1).map((l) => l.nome)).toEqual(['Colina', 'Barretos', 'Araraquara', 'Dumont']);
    expect(ordenar(linhas, 'ioc', -1).map((l) => l.nome)).toEqual(['Araraquara', 'Barretos', 'Colina', 'Dumont']);
  });

  it('o resumo do recorte faz a mesma conta do servidor: classes, IOC médio e somas só de quem tem o número', () => {
    const r = resumir(DIAGNOSTICO.municipios, false);
    expect([r.maxima, r.alta, r.manutencao, r.semIndice, r.total]).toEqual([1, 1, 1, 1, 4]);
    expect(r.iocMedio).toBe(60);
    expect(r.demandaEstrutural).toBe(30);
    expect(r.vendidasNoPeriodo).toBeNull();
    expect(r.clientes).toBe(48);
  });

  it('o CSV sai com vírgula decimal, sem milhar, com a classe por extenso e os clientes em partes no fim', () => {
    const [primeira] = linhasDoCsv([DIAGNOSTICO.municipios[1]]);
    expect(primeira[0]).toBe('Araraquara');
    expect(primeira[4]).toBe('91,5');
    expect(primeira[5]).toBe('Prioridade máxima');
    expect(primeira[12]).toBe('25');
    expect(primeira[18]).toContain('realização');
    expect(primeira.slice(19)).toEqual([2, 3, 4, 1, 2, 9, 5]);
  });
});
