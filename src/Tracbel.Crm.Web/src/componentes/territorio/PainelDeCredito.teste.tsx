/**
 * O CRÉDITO REAGE AO MUNICÍPIO, E DIZ DE ONDE VEM (issues 167 e 168).
 *
 * O SICOR é a única das três fontes do bloco "Momento do mercado" que desce ao
 * município — por isso é aqui que o recorte muda o que se lê. E é aqui que a
 * regra da rastreabilidade se prova fora dos indicadores territoriais: um número
 * de crédito precisa informar a própria janela e origem sem depender de um selo
 * genérico do painel inteiro.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import type { JanelasDeCredito, PainelDeCreditoRural } from '../../tipos/mercado';
import { PainelDeCredito } from './PainelDeCredito';

const obterCreditoRural = vi.hoisted(() => vi.fn());

vi.mock('../../dados/api/territorio', async (original) => ({
  ...(await original<typeof import('../../dados/api/territorio')>()),
  obterCreditoRural,
}));

// O Chart.js toca no canvas ao ser registrado, e o jsdom não tem canvas. O
// gráfico tem teste próprio; aqui o que está sob prova é o crédito.
vi.mock('../GraficoLinhaMensal', () => ({ GraficoLinhaMensal: () => <div data-grafico /> }));

const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (c: string) => guardado.get(c) ?? null,
  setItem: (c: string, v: string) => void guardado.set(c, v),
  removeItem: (c: string) => void guardado.delete(c),
  clear: () => guardado.clear(),
});

const CAFELANDIA = 3509502;

const janelas = (linhas: number, valor: number): JanelasDeCredito => ({
  linhas,
  valor,
  linhasAnteriores: Math.round(linhas * 0.8),
  valorAnterior: Math.round(valor * 0.8),
});

/** Dezesseis municípios, com Cafelândia em último — ela tem de subir mesmo assim. */
function municipios() {
  const outros = Array.from({ length: 15 }, (_, i) => ({
    codigoIbge: 3500000 + i,
    nome: `Município ${i + 1}`,
    pertenceAAdr: true,
    janelas: janelas(500 - i, 5_000_000 - i * 1000),
    indice: null,
  }));

  return [...outros, { codigoIbge: CAFELANDIA, nome: 'Cafelândia', pertenceAAdr: true, janelas: janelas(9, 900_000), indice: null }];
}

function painel(): PainelDeCreditoRural {
  return {
    ultimoMes: '2026-06-01',
    janela: {
      ultimoMesComDado: '2026-06-01',
      mesesDeCarencia: 3,
      carenciaDecidida: true,
      inicio: '2025-04-01',
      fim: '2026-03-01',
      inicioAnterior: '2024-04-01',
      fimAnterior: '2025-03-01',
      mesesPorJanela: 12,
    },
    porAno: [],
    porProduto: [
      { codigo: 7080, nome: 'Trator', ehMaquina: true, janelas: janelas(4_000, 400_000_000) },
      { codigo: 100, nome: 'Soja', ehMaquina: false, janelas: janelas(9_000, 900_000_000) },
    ],
    porMunicipio: municipios(),
    regiao: { recorte: 'Região', municipios: 203, janelas: janelas(4_000, 400_000_000), indice: null },
    saoPaulo: { recorte: 'São Paulo', municipios: 645, janelas: janelas(12_000, 1_200_000_000), indice: null },
    procedencia: {
      fonte: 'BCB/SICOR',
      pesquisa: 'Crédito rural — operações contratadas',
      tabela: 'InvestMunicipioProduto',
      variavel: 'Valor contratado e número de linhas',
      competencia: '04/2025 a 03/2026, contra 04/2024 a 03/2025',
      ultimaCargaUtc: '2026-09-22T03:00:00Z',
      ressalva:
        'Uma LINHA do SICOR não é um contrato: ela já é a soma dos contratos daquela combinação de município e ' +
        'produto, e não traz quantidade.',
    },
  };
}

function abrir(municipioSelecionado: number | null = null) {
  obterCreditoRural.mockResolvedValue({ dados: painel(), procedencia: null });
  render(
    <ProvedorDeContextoDeAcesso>
      <PainelDeCredito municipioSelecionado={municipioSelecionado} />
    </ProvedorDeContextoDeAcesso>,
  );
}

/** A tabela "Máquinas por município", pela primeira coluna das linhas. */
const nomesDosMunicipios = () =>
  [...document.querySelectorAll<HTMLElement>('table')]
    .flatMap((t) => [...t.querySelectorAll('tbody tr')])
    .map((tr) => tr.querySelector('td')?.textContent ?? '');

describe('o painel de crédito', () => {
  afterEach(() => {
    obterCreditoRural.mockReset();
    guardado.clear();
  });

  it('a procedência do SICOR abre pelo teclado e traz a janela — não um selo genérico', async () => {
    abrir();

    const gatilho = await screen.findByRole('button', { name: 'De onde vem o crédito rural' });
    fireEvent.focus(gatilho);

    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent('Fonte: BCB/SICOR');
    expect(dica).toHaveTextContent('Tabela: InvestMunicipioProduto');
    expect(dica).toHaveTextContent('Competência: 04/2025 a 03/2026, contra 04/2024 a 03/2025');
    expect(dica).toHaveTextContent('Última carga: 22/09/2026');
    expect(dica).toHaveTextContent('Uma LINHA do SICOR não é um contrato');
  });

  it('sem município, a comparação é Região Tracbel × São Paulo', async () => {
    abrir();
    await screen.findByRole('button', { name: 'De onde vem o crédito rural' });

    expect(screen.getByText('Região Tracbel')).toBeInTheDocument();
    expect(screen.queryByText('município escolhido')).not.toBeInTheDocument();
  });

  it('com município, ele entra ao lado da Região Tracbel e de São Paulo', async () => {
    abrir(CAFELANDIA);
    await screen.findByRole('button', { name: 'De onde vem o crédito rural' });

    const linha = screen.getByText('município escolhido').closest('tr')!;
    expect(within(linha).getByText('Cafelândia')).toBeInTheDocument();
  });

  it('o município escolhido sobe para o topo da lista, mesmo fora dos quinze primeiros', async () => {
    abrir(CAFELANDIA);
    await screen.findByRole('button', { name: 'De onde vem o crédito rural' });

    // Sem o recorte, Cafelândia é a 16ª e nem apareceria.
    const nomes = nomesDosMunicipios();
    const daTabelaDeMunicipios = nomes.filter((n) => n.startsWith('Município ') || n.startsWith('Cafelândia'));
    expect(daTabelaDeMunicipios[0]).toContain('Cafelândia');
  });

  it('a linha do SICOR não se chama ticket — é valor médio por linha', () => {
    abrir();
    const rotulos = [...document.querySelectorAll('th, .cad-kpi-rotulo')].map((n) => n.textContent ?? '');
    expect(rotulos.some((r) => /ticket/i.test(r))).toBe(false);
  });
});
