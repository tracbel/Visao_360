/**
 * O SHARE DA TRACBEL NO CRÉDITO DE MECANIZAÇÃO (issue 262): o número principal é
 * a Região e a filial; o município é indicativo e marca onde a Tracbel passa do
 * SICOR; recurso próprio e consórcio aparecem riscados, fora do share; e sem
 * financiamento carregado a tela diz isso, em vez de mostrar share zero.
 */

import { render, screen, within } from '@testing-library/react';
import type { ReactNode } from 'react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import type { ShareNoCreditoDeMecanizacao, ShareNoRecorte } from '../../tipos/mercado';
import { PainelDoShareNoCredito } from './PainelDoShareNoCredito';

const obterShareNoCredito = vi.hoisted(() => vi.fn());

vi.mock('../../dados/api/territorio', async (original) => ({
  ...(await original<typeof import('../../dados/api/territorio')>()),
  obterShareNoCredito,
}));

vi.mock('../GraficoLinhaMensal', () => ({
  GraficoLinhaMensal: (p: { valores: number[]; anteriores?: (number | null)[] }) => (
    <div data-grafico data-valores={p.valores.join(',')} data-anteriores={p.anteriores?.join(',')} />
  ),
}));

vi.mock('../MolduraDeGrafico', () => ({
  MolduraDeGrafico: ({ children }: { children: (l: number, a: number) => ReactNode }) => <>{children(600, 210)}</>,
}));

const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (c: string) => guardado.get(c) ?? null,
  setItem: (c: string, v: string) => void guardado.set(c, v),
  removeItem: (c: string) => void guardado.delete(c),
  clear: () => guardado.clear(),
});

const recorte = (valorTracbel: number, valorSicor: number, financiamentos = 1): ShareNoRecorte => ({
  valorTracbel,
  financiamentos,
  valorSicor,
  linhasSicor: 3,
  share: valorSicor > 0 ? valorTracbel / valorSicor : null,
  valorDaConcorrencia: Math.max(0, valorSicor - valorTracbel),
});

function resposta(): ShareNoCreditoDeMecanizacao {
  return {
    inicio: '2025-09-01',
    fim: '2026-08-01',
    ultimoPedido: '2026-09-20',
    regiao: recorte(650_000, 2_000_000, 3),
    porFilial: [
      { empresaId: 1, filial: 'Tracbel Agro — Ribeirão Preto', municipios: 2, share: recorte(650_000, 1_200_000, 3) },
      { empresaId: 2, filial: 'Tracbel Agro — Barretos', municipios: 1, share: recorte(0, 800_000, 0) },
    ],
    porMunicipio: [
      { codigoIbge: 3516200, nome: 'Franca', filial: 'Tracbel Agro — Ribeirão Preto', share: recorte(400_000, 1_000_000), acimaDoSicor: false },
      { codigoIbge: 3513108, nome: 'Cravinhos', filial: 'Tracbel Agro — Ribeirão Preto', share: recorte(250_000, 200_000), acimaDoSicor: true },
      { codigoIbge: 3599999, nome: 'Sem Sicor', filial: null, share: recorte(10_000, 0), acimaDoSicor: true },
    ],
    porLinha: [
      { linha: 'MODER FROTA', contaNoShare: true, financiamentos: 1, valor: 300_000 },
      { linha: 'RECURSO PRÓPRIO', contaNoShare: false, financiamentos: 1, valor: 999_000 },
    ],
    porMes: [
      { mes: '2026-07-01', valorTracbel: 250_000, valorSicor: 1_000_000 },
      { mes: '2026-08-01', valorTracbel: 300_000, valorSicor: 1_000_000 },
    ],
    foraDaRegiao: { semMunicipio: 1, valorSemMunicipio: 90_000, foraDaAdr: 1, valorForaDaAdr: 150_000 },
    procedencia: null,
  };
}

function abrir(dados: ShareNoCreditoDeMecanizacao) {
  obterShareNoCredito.mockResolvedValue({ dados, procedencia: null });
  render(
    <ProvedorDeContextoDeAcesso>
      <PainelDoShareNoCredito />
    </ProvedorDeContextoDeAcesso>,
  );
}

describe('o share no crédito de mecanização', () => {
  afterEach(() => {
    obterShareNoCredito.mockReset();
    guardado.clear();
  });

  it('mostra o share da Região e a janela', async () => {
    abrir(resposta());

    const cartao = await screen.findByText('Share Tracbel no crédito');
    const corpo = cartao.closest('.mom-cartao') as HTMLElement;
    expect(within(corpo).getByText('32,5%')).toBeTruthy();
    expect(corpo.textContent).toContain('Região Tracbel');
  });

  it('por filial, o share de cada uma; a que não financiou nada mostra 0%', async () => {
    abrir(resposta());
    await screen.findByText('Share por filial');

    const ribeirao = document.querySelector('[data-bloco="share-por-filial"] [data-filial="1"]') as HTMLElement;
    expect(ribeirao.textContent).toContain('54,2%');
    const barretos = document.querySelector('[data-bloco="share-por-filial"] [data-filial="2"]') as HTMLElement;
    expect(barretos.textContent).toContain('0,0%');
  });

  it('marca o município em que a Tracbel passa do SICOR, e sem SICOR não inventa share', async () => {
    abrir(resposta());
    await screen.findByText('Por município (indicativo)');

    const cravinhos = document.querySelector('[data-municipio="3513108"]') as HTMLElement;
    expect(cravinhos.dataset.acima).toBe('true');
    expect(cravinhos.textContent).toContain('Tracbel acima do SICOR');
    expect(cravinhos.textContent).toContain('125,0%');

    const semSicor = document.querySelector('[data-municipio="3599999"]') as HTMLElement;
    expect(semSicor.textContent).not.toContain('%');
  });

  it('as linhas fora do share aparecem riscadas', async () => {
    abrir(resposta());
    await screen.findByText('O que a Tracbel financiou, por linha');

    const proprio = document.querySelector('[data-bloco="share-por-linha"] tr[data-conta="false"]') as HTMLElement;
    expect(proprio.querySelector('s')?.textContent).toBe('RECURSO PRÓPRIO');
    expect(proprio.textContent).toContain('não entra no share');
  });

  it('desenha Tracbel contra SICOR mês a mês', async () => {
    abrir(resposta());
    await screen.findByText('Tracbel × SICOR, mês a mês');

    const grafico = document.querySelector('[data-grafico]') as HTMLElement;
    expect(grafico.dataset.valores).toBe('250000,300000');
    expect(grafico.dataset.anteriores).toBe('1000000,1000000');
  });

  it('sem financiamento carregado, diz isso — e não share zero', async () => {
    abrir({ ...resposta(), ultimoPedido: null });

    expect(await screen.findByText('O financiamento das vendas ainda não foi carregado neste banco')).toBeTruthy();
    expect(screen.queryByText('Share Tracbel no crédito')).toBeNull();
  });
});
