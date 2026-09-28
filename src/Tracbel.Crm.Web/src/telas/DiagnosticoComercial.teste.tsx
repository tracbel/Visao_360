/**
 * O DIAGNÓSTICO COMERCIAL NA TELA (issue 257): o IOC ordena os municípios, a classe vem escrita ao lado da cor, o que
 * não tem dado mostra o traço com o motivo, e o CSV leva exatamente o que está na tela.
 *
 * O caminho é o de produção: a rota é lida por um `fetch` de mentira, que anota o que a tela pediu.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { DiagnosticoComercialDaRegiao, MunicipioNoDiagnostico } from '../tipos/mercado';
import { DiagnosticoComercial, linhasDoCsv, ordenar } from './DiagnosticoComercial';

const guardado = new Map<string, string>();
const pedidos: string[] = [];

function municipio(parcial: Partial<MunicipioNoDiagnostico> & { codigoIbge: number; nome: string }): MunicipioNoDiagnostico {
  return {
    regiao: 'Norte',
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
    vinculosComCadencia: 20,
    cobertos: 5,
    cobertura: 0.25,
    penetracao: null,
    componentes: { potencial: 1, cobertura: 0.75, credito: 0.75, rentabilidade: 0.625, clientes: 0, realizacao: null, penetracao: null },
    ioc: 70,
    classe: 'Alta',
    situacao: 'elevado potencial, baixa cobertura comercial',
    planoDeAcao: 'Expandir cobertura e visitas presenciais',
    componentesAusentes: ['realização: o ART não trouxe as vendas de máquina', 'penetração: o ART não trouxe as vendas de máquina'],
    estimativa: true,
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
  resumo: { maxima: 1, alta: 1, moderada: 0, baixa: 0, manutencao: 1, semIndice: 1, total: 4, iocMedio: 60 },
  municipios: [
    municipio({ codigoIbge: 1, nome: 'Barretos', ioc: 70, classe: 'Alta' }),
    municipio({ codigoIbge: 2, nome: 'Araraquara', ioc: 91.5, classe: 'Maxima' }),
    municipio({ codigoIbge: 3, nome: 'Colina', ioc: 18.5, classe: 'Manutencao', vinculosComCadencia: 0, cobertos: 0, cobertura: null }),
    municipio({ codigoIbge: 4, nome: 'Dumont', ioc: null, classe: null, demandaEstrutural: null, componentes: null }),
  ],
  lacunas: [
    { metrica: 'vendasEmUnidades', motivo: 'O ART não trouxe venda de máquina ao alcance desta consulta.' },
    { metrica: 'pesosDoIoc', motivo: 'Os pesos do IOC ainda são os do protótipo, a confirmar.' },
  ],
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
  within(screen.getByRole('table')).getAllByRole('row').slice(1).map((l) => l.querySelector('td')?.querySelector('button')?.textContent);

describe('Diagnóstico Comercial (issue 257)', () => {
  it('ordena pelo IOC, do maior para o menor, com o sem índice por último', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());

    expect(nomesNaTabela()).toEqual(['Araraquara', 'Barretos', 'Colina', 'Dumont']);
    expect(pedidos.some((p) => p.includes('/v1/mercado/diagnostico'))).toBe(true);
  });

  it('a classe vem escrita ao lado do número, e os pesos do protótipo aparecem como a confirmar', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());

    const tabela = screen.getByRole('table');
    expect(within(tabela).getByTitle('Prioridade máxima')).toHaveTextContent('Máxima');
    expect(within(tabela).getByText('Manutenção')).toBeInTheDocument();
    expect(screen.getByText('protótipo, a confirmar')).toBeInTheDocument();
    expect(screen.getByText(/Trator 31%/)).toBeInTheDocument();
  });

  it('sem dado mostra o traço com o motivo, e não zero', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());

    expect(screen.getAllByRole('button', { name: 'Por que as vendas não aparece' }).length).toBeGreaterThan(0);
    expect(screen.getByRole('button', { name: 'Por que a cobertura não aparece' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Por que o IOC de Dumont não aparece' })).toBeInTheDocument();
  });

  it('o filtro de classe e a busca agem só na tela, sem pedir de novo', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());
    const antes = pedidos.length;

    fireEvent.change(screen.getByLabelText('Classe'), { target: { value: 'Maxima' } });
    expect(nomesNaTabela()).toEqual(['Araraquara']);

    fireEvent.change(screen.getByLabelText('Classe'), { target: { value: '' } });
    fireEvent.change(screen.getByLabelText('Município'), { target: { value: 'col' } });
    expect(nomesNaTabela()).toEqual(['Colina']);

    expect(pedidos.length).toBe(antes);
  });

  it('trocar a categoria pede o diagnóstico de novo', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());

    fireEvent.change(screen.getByLabelText('Categoria'), { target: { value: 'TODAS' } });

    await waitFor(() => expect(pedidos.some((p) => p.includes('categoria=TODAS'))).toBe(true));
  });

  it('clicar no município abre os sete componentes e o que ficou fora da conta', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());

    fireEvent.click(screen.getByRole('button', { name: 'Barretos' }));

    expect(screen.getByRole('button', { name: 'Barretos' })).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getAllByText('fora da conta')).toHaveLength(2);
    expect(screen.getByText(/Fora da conta:/).closest('p')).toHaveTextContent('realização: o ART não trouxe as vendas de máquina');
  });

  it('ordenar por nome põe em ordem alfabética, e clicar de novo inverte', async () => {
    montar();
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument());

    fireEvent.click(screen.getByRole('button', { name: 'Município' }));
    expect(nomesNaTabela()).toEqual(['Araraquara', 'Barretos', 'Colina', 'Dumont']);
    fireEvent.click(screen.getByRole('button', { name: /Município/ }));
    expect(nomesNaTabela()).toEqual(['Dumont', 'Colina', 'Barretos', 'Araraquara']);
  });
});

describe('funções do diagnóstico', () => {
  it('o vazio vai para o fim nos dois sentidos', () => {
    const linhas = DIAGNOSTICO.municipios;
    expect(ordenar(linhas, 'ioc', 1).map((l) => l.nome)).toEqual(['Colina', 'Barretos', 'Araraquara', 'Dumont']);
    expect(ordenar(linhas, 'ioc', -1).map((l) => l.nome)).toEqual(['Araraquara', 'Barretos', 'Colina', 'Dumont']);
  });

  it('o CSV sai com vírgula decimal, sem milhar e com a classe por extenso', () => {
    const [primeira] = linhasDoCsv([DIAGNOSTICO.municipios[1]]);
    expect(primeira[0]).toBe('Araraquara');
    expect(primeira[4]).toBe('91,5');
    expect(primeira[5]).toBe('Prioridade máxima');
    expect(primeira[12]).toBe('25');
    expect(primeira[18]).toContain('realização');
  });
});
