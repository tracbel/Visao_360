/**
 * A ABA PERCEPÇÃO COMERCIAL (fidelidade às maquetes, fase 3).
 *
 * A REGRA QUE ESTE TESTE GUARDA É A DA ISSUE 191: nenhum nome de pessoa
 * inventado. A coluna "Gestor" mostra o responsável pela carteira do CRM que já
 * vem na leitura da tela — e, sem ele, o traço. E nenhum adjetivo sem regra:
 * a leitura é positiva, neutra ou negativa pelo sinal, e o índice de 0 a 100
 * da maquete não aparece, porque não existe.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../../dados/api/contexto';
import { municipioDeTeste } from '../../../testes/territorio';
import type { PercepcaoDoGestorDetalhe } from '../../../tipos/potencial';
import type { MomentoDoRecorte } from '../../../tipos/territorio';
import { recorteFiltrado, type RecorteFiltrado } from '../../territorio/indicadoresDaAdr';
import { AbaPercepcaoComercial } from './AbaPercepcaoComercial';

const obterParametrosDoPotencial = vi.hoisted(() => vi.fn());
const baixarCsv = vi.hoisted(() => vi.fn());

vi.mock('../../../dados/api/potencial', async (original) => ({
  ...(await original<typeof import('../../../dados/api/potencial')>()),
  obterParametrosDoPotencial,
}));
vi.mock('../../../dados/exportarCsv', async (original) => ({
  ...(await original<typeof import('../../../dados/exportarCsv')>()),
  baixarCsv,
}));

const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (c: string) => guardado.get(c) ?? null,
  setItem: (c: string, v: string) => void guardado.set(c, v),
  removeItem: (c: string) => void guardado.delete(c),
  clear: () => guardado.clear(),
});

function leitura(codigo: number, nome: string, percentual: number): PercepcaoDoGestorDetalhe {
  return {
    municipioCodigoIbge: codigo,
    municipioNome: nome,
    uf: 'SP',
    percentual,
    vigencia: {
      vigenteDesde: '2026-08-01',
      justificativa: 'leitura do teste',
      informadoPor: null,
      informadoEm: '2026-08-01T12:00:00Z',
      revogadoEm: null,
      revogadoPor: null,
      motivoDaRevogacao: null,
    },
  };
}

// CINCO MUNICÍPIOS NA REGIÃO; Borborema tem responsável de carteira, os outros não.
const MUNICIPIOS = [
  municipioDeTeste({
    codigoIbge: 1,
    nome: 'Borborema',
    responsaveisPelasCarteiras: [
      { nome: 'Responsável Cadastrado', natureza: 'CEN', vinculos: 12, carteiras: 1 },
      { nome: 'Outro Responsável', natureza: 'CEN', vinculos: 2, carteiras: 1 },
    ],
  }),
  municipioDeTeste({ codigoIbge: 2, nome: 'Tupã' }),
  municipioDeTeste({ codigoIbge: 3, nome: 'Guariba' }),
  municipioDeTeste({ codigoIbge: 4, nome: 'Ibitinga' }),
  municipioDeTeste({ codigoIbge: 5, nome: 'Matão' }),
  // FORA DA REGIÃO: a leitura dele não entra.
  municipioDeTeste({ codigoIbge: 9, nome: 'Vizinho', pertenceAAdr: false }),
];

const LEITURAS = [
  leitura(2, 'Tupã', -2),
  leitura(1, 'Borborema', 3),
  leitura(3, 'Guariba', 0),
  leitura(9, 'Vizinho', 5),
];

const MOMENTO = { percepcaoPercentual: 2 } as MomentoDoRecorte;

function abrir(leituras = LEITURAS, recorte: RecorteFiltrado | null = null) {
  obterParametrosDoPotencial.mockResolvedValue({
    dados: { em: '2026-09-23', geral: null, culturas: [], percepcoes: leituras, pendencias: [] },
    procedencia: null,
  });
  render(
    <ProvedorDeContextoDeAcesso>
      <AbaPercepcaoComercial momento={MOMENTO} municipios={MUNICIPIOS} recorte={recorte} />
    </ProvedorDeContextoDeAcesso>,
  );
}

/** Abre uma dica pelo teclado, lê e fecha. */
function lerDica(rotulo: string): string {
  const gatilho = screen.getByRole('button', { name: rotulo });
  fireEvent.focus(gatilho);
  const texto = screen.getByRole('tooltip').textContent ?? '';
  fireEvent.blur(gatilho);
  return texto;
}

const linhas = () => [...document.querySelectorAll<HTMLElement>('[data-bloco="percepcao-municipios"] tbody tr')];
const cartao = (rotulo: string) => document.querySelector<HTMLElement>(`[data-cartao="${rotulo}"]`)!;

describe('a aba Percepção comercial', () => {
  afterEach(() => {
    obterParametrosDoPotencial.mockReset();
    baixarCsv.mockReset();
    guardado.clear();
  });

  it('a tabela traz as leituras da Região, da maior para a menor — o vizinho de fora não entra', async () => {
    abrir();
    await screen.findByText('Borborema', { selector: 'th' });

    expect(linhas().map((l) => l.querySelector('th')!.textContent)).toEqual(['Borborema', 'Guariba', 'Tupã']);
    expect(linhas()[0]).toHaveTextContent('+3 p.p.');
    expect(linhas()[0]).toHaveTextContent('Positiva');
    expect(linhas()[2]).toHaveTextContent('Negativa');
  });

  it('NENHUM NOME INVENTADO: o gestor é o responsável da carteira, e sem ele é o traço', async () => {
    abrir();
    await screen.findByText('Borborema', { selector: 'th' });

    const gestores = linhas().map((l) => l.querySelector<HTMLElement>('[data-gestor]')!);
    expect(gestores[0]).toHaveTextContent('Responsável Cadastrado');
    expect(gestores[0]).toHaveTextContent('+1');
    // Guariba e Tupã não têm responsável cadastrado: nada além do traço.
    for (const g of gestores.slice(1)) {
      expect(g.textContent).toContain('—');
      expect(g.textContent?.replace('—', '').replace('sem responsável cadastrado', '').trim()).toBe('');
    }

    // E o único nome da tela inteira é o que veio do cadastro das carteiras.
    const nomes = new Set(['Responsável Cadastrado']);
    for (const g of gestores) {
      const texto = (g.textContent ?? '').replace(/\+\d+/, '').replace('—', '').replace('sem responsável cadastrado', '').trim();
      if (texto) expect(nomes.has(texto)).toBe(true);
    }
  });

  it('os cartões contam pelo SINAL — sem "aquecido" nem "em alerta" nos rótulos', async () => {
    abrir();
    await screen.findByText('Borborema', { selector: 'th' });

    expect(cartao('Percepção positiva')).toHaveTextContent('1 município');
    expect(cartao('Percepção negativa')).toHaveTextContent('1 município');
    expect(cartao('Percepção no recorte')).toHaveTextContent('+2 p.p.');

    const rotulos = [...document.querySelectorAll('.mom-cartao-rotulo, th, .mom-painel-titulo')].map((n) => n.textContent ?? '');
    expect(rotulos.filter((r) => /aquecid|alerta|muito positiva|0 ?[–-] ?100/i.test(r))).toEqual([]);
  });

  it('a distribuição conta os municípios da Região sem leitura', async () => {
    abrir();
    await screen.findByText('Borborema', { selector: 'th' });

    const sem = document.querySelector<HTMLElement>('[data-sentido-da-leitura="semRegistro"]')!;
    // Cinco da Região, três com leitura: dois sem.
    expect(sem).toHaveTextContent('2');
    expect(sem).toHaveTextContent('40%');
  });

  it('tendência, evolução e impacto saem vazios com o motivo — nunca um número', async () => {
    abrir();
    await screen.findByText('Borborema', { selector: 'th' });

    expect(cartao('Tendência dos gestores').querySelector('.mom-cartao-valor')!.textContent).not.toMatch(/\d/);
    const vazio = document.querySelector<HTMLElement>('[data-grafico-vazio]')!;
    fireEvent.focus(within(vazio).getByRole('button'));
    expect(screen.getByRole('tooltip')).toHaveTextContent(/issue 71/);
  });

  it('"Exportar" baixa o CSV das linhas da tela, com o gestor do cadastro', async () => {
    abrir();
    await screen.findByText('Borborema', { selector: 'th' });

    fireEvent.click(screen.getByRole('button', { name: 'Exportar' }));
    expect(baixarCsv).toHaveBeenCalledTimes(1);
    const [, cabecalho, linhasDoCsv] = baixarCsv.mock.calls[0];
    expect(cabecalho).toContain('Responsável pela carteira');
    expect(linhasDoCsv[0]).toEqual(['Borborema', '3', 'Positiva', 'Responsável Cadastrado', '01/08/2026']);
    expect(linhasDoCsv[1][3]).toBe('');
  });

  it('sem filtro, os municípios são "da Região Tracbel"', async () => {
    abrir();
    await screen.findByText('Borborema', { selector: 'th' });

    expect(lerDica('O que é os municípios com percepção positiva')).toMatch(/^Municípios da Região Tracbel com leitura/);
    expect(screen.getByText('Municípios da Região Tracbel por sentido da leitura registrada.')).toBeInTheDocument();
  });

  it('COM FILTRO, o texto diz o recorte: os municípios já vêm filtrados, e "Região Tracbel" é a ADR inteira', async () => {
    abrir(LEITURAS, recorteFiltrado('Norte', null));
    await screen.findByText('Borborema', { selector: 'th' });

    for (const rotulo of [
      'O que é os municípios com percepção positiva',
      'O que é os municípios com percepção negativa',
      'Como ler distribuição da percepção comercial',
      'Como ler top 5 municípios por percepção comercial',
      'Como ler percepção comercial por município',
    ]) {
      const dica = lerDica(rotulo);
      expect(dica, rotulo).toContain('da Sub-região Norte');
      expect(dica, rotulo).not.toContain('Região Tracbel');
    }
    expect(screen.getByText('Municípios da Sub-região Norte por sentido da leitura registrada.')).toBeInTheDocument();
    expect(screen.getByText('Leitura média dos gestores no recorte (Sub-região Norte).')).toBeInTheDocument();
  });

  it('a escala é explicada a quem usa a tela — sem "a maquete mostra"', async () => {
    abrir();
    await screen.findByText('Borborema', { selector: 'th' });

    const escala = lerDica('Por que não há índice de 0 a 100');
    expect(escala).toMatch(/Não existe um índice de 0 a 100/);
    expect(escala).not.toMatch(/maquete/i);
  });

  it('sem leitura registrada, a tabela diz isso e o Exportar fica desligado', async () => {
    abrir([]);
    await screen.findByText('Nenhum município com percepção registrada.', { exact: false });

    expect(screen.getByRole('button', { name: 'Exportar' })).toBeDisabled();
  });
});
