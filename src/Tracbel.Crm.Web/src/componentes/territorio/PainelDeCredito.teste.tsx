/**
 * O CRÉDITO REAGE AO MUNICÍPIO, E DIZ DE ONDE VEM (issues 167 e 168) — na forma
 * da maquete `momento-credito.png` desde a fidelidade às maquetes (fase 3).
 *
 * O SICOR é a única das três fontes do bloco "Momento do mercado" que desce ao
 * município — por isso é aqui que o recorte muda o que se lê. E é aqui que a
 * regra da rastreabilidade se prova fora dos indicadores territoriais: um número
 * de crédito precisa informar a própria janela e origem sem depender de um selo
 * genérico do painel inteiro.
 *
 * O QUE A MAQUETE NÃO MOSTRA FOI PARA AS DICAS, E NÃO SUMIU: a comparação
 * Município · Região Tracbel · São Paulo, a janela com a carência e os produtos
 * financiados. Os testes abrem a dica pelo teclado e leem o que está lá.
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
const VIZINHO = 3599999;

const janelas = (linhas: number, valor: number): JanelasDeCredito => ({
  linhas,
  valor,
  linhasAnteriores: Math.round(linhas * 0.8),
  valorAnterior: Math.round(valor * 0.8),
});

/**
 * Dezesseis municípios da Região, com Cafelândia em último — ela tem de subir
 * mesmo assim —, e um VIZINHO de fora da Região, o maior de todos: ele não pode
 * entrar no top 5 da área de atuação.
 */
function municipios() {
  const outros = Array.from({ length: 15 }, (_, i) => ({
    codigoIbge: 3500000 + i,
    nome: `Município ${i + 1}`,
    pertenceAAdr: true,
    janelas: janelas(500 - i * 10, 5_000_000 - i * 100_000),
    indice: null,
  }));

  return [
    { codigoIbge: VIZINHO, nome: 'Vizinho de fora', pertenceAAdr: false, janelas: janelas(900, 90_000_000), indice: null },
    ...outros,
    { codigoIbge: CAFELANDIA, nome: 'Cafelândia', pertenceAAdr: true, janelas: janelas(9, 900_000), indice: null },
  ];
}

/**
 * A REGIÃO TRACBEL E O TRATOR TÊM NÚMEROS DIFERENTES DE PROPÓSITO (revisão de
 * 24/09/2026): eram iguais, e um cartão que lesse o produto no lugar da Região
 * passaria no teste. Região: 2.000 linhas, R$ 250 mi (R$ 125 mil por linha);
 * Trator: 4.000 linhas, R$ 400 mi (R$ 100 mil por linha).
 */
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
    regiao: { recorte: 'Região', municipios: 203, janelas: janelas(2_000, 250_000_000), indice: null },
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

function abrir(municipioSelecionado: number | null = null, ajustar: (p: PainelDeCreditoRural) => PainelDeCreditoRural = (p) => p) {
  obterCreditoRural.mockResolvedValue({ dados: ajustar(painel()), procedencia: null });
  render(
    <ProvedorDeContextoDeAcesso>
      <PainelDeCredito municipioSelecionado={municipioSelecionado} />
    </ProvedorDeContextoDeAcesso>,
  );
}

/** Abre uma dica pelo teclado, lê e fecha — o balão do Radix mora num portal. */
function lerDica(rotulo: string): string {
  const gatilho = screen.getByRole('button', { name: rotulo });
  fireEvent.focus(gatilho);
  const texto = screen.getByRole('tooltip').textContent ?? '';
  fireEvent.blur(gatilho);
  return texto;
}

/** Os municípios do detalhamento, pelo nome de cada linha. */
const nomesDoDetalhamento = () =>
  [...document.querySelectorAll<HTMLElement>('[data-bloco="credito-municipios"] tbody tr th')].map(
    (th) => th.textContent ?? '',
  );

const esperarOPainel = () => screen.findByRole('button', { name: 'De onde vem o crédito rural' });

describe('o painel de crédito', () => {
  afterEach(() => {
    obterCreditoRural.mockReset();
    guardado.clear();
  });

  it('a procedência do SICOR abre pelo teclado e traz a janela — não um selo genérico', async () => {
    abrir();

    const gatilho = await esperarOPainel();
    fireEvent.focus(gatilho);

    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent('Fonte: BCB/SICOR');
    expect(dica).toHaveTextContent('Tabela: InvestMunicipioProduto');
    expect(dica).toHaveTextContent('Competência: 04/2025 a 03/2026, contra 04/2024 a 03/2025');
    expect(dica).toHaveTextContent('Última carga: 22/09/2026');
    expect(dica).toHaveTextContent('Uma LINHA do SICOR não é um contrato');
  });

  it('os quatro cartões da maquete, com o vocabulário decidido', async () => {
    abrir();
    await esperarOPainel();

    const rotulos = [...document.querySelectorAll<HTMLElement>('.mom-cartao-rotulo')].map((n) => n.textContent ?? '');
    expect(rotulos).toEqual(['Valor total financiado', 'Linhas do SICOR', 'Valor médio por linha', 'Variação anual']);

    // A Região Tracbel: 250 mi, contra 200 mi nos 12 meses anteriores.
    const valor = document.querySelector<HTMLElement>('[data-cartao="Valor total financiado"]')!;
    expect(valor).toHaveTextContent('R$ 250 mi');
    expect(valor).toHaveTextContent('+25%');
    expect(valor).toHaveTextContent('vs. 12 meses anteriores');
  });

  it('os cartões leem a REGIÃO TRACBEL — e não o produto Trator de São Paulo', async () => {
    abrir();
    await esperarOPainel();

    const cartao = (rotulo: string) =>
      document.querySelector<HTMLElement>(`[data-cartao="${rotulo}"] .mom-cartao-valor`)!.textContent;
    expect(cartao('Valor total financiado')).toBe('R$ 250 mi');
    expect(cartao('Linhas do SICOR')).toBe('2.000');
    expect(cartao('Valor médio por linha')).toBe('R$ 125 mil');
    // Os números do trator — 400 mi, 4.000 linhas, 100 mil por linha — não estão em cartão nenhum.
    const cartoes = document.querySelector<HTMLElement>('.mom-cartoes')!;
    expect(cartoes).not.toHaveTextContent('R$ 400 mi');
    expect(cartoes).not.toHaveTextContent('4.000');
    expect(cartoes).not.toHaveTextContent('R$ 100 mil');
  });

  it('LINHA NÃO É CONTRATO: nenhum rótulo diz "contrato", "operação", "ticket" ou "share"', async () => {
    abrir();
    await esperarOPainel();

    // O motivo e a ressalva PODEM dizer "não é um contrato" — é ali que a
    // diferença se explica. O que não pode é um rótulo nomear o número assim.
    const rotulos = [
      ...document.querySelectorAll<HTMLElement>('th, .mom-cartao-rotulo, .mom-painel-titulo, .mom-ranking-cabecalho, option'),
    ].map((n) => n.textContent ?? '');
    expect(rotulos.filter((r) => /contrat|opera[cç][aã]o|ticket|share/i.test(r))).toEqual([]);
    expect(rotulos.some((r) => /Linhas do SICOR/.test(r))).toBe(true);
  });

  it('sem município, a comparação da dica é Região Tracbel × São Paulo', async () => {
    abrir();
    await esperarOPainel();

    const dica = lerDica('De onde vem o crédito rural');
    expect(dica).toContain('Região Tracbel');
    expect(dica).toContain('São Paulo');
    expect(dica).not.toContain('município escolhido');
  });

  it('com município, ele entra ao lado da Região Tracbel e de São Paulo', async () => {
    abrir(CAFELANDIA);
    await esperarOPainel();

    expect(lerDica('De onde vem o crédito rural')).toContain('Cafelândia (município escolhido)');
  });

  it('a janela e a carência continuam ditas, na dica da variação', async () => {
    abrir();
    await esperarOPainel();

    const dica = lerDica('O que é a variação anual do valor financiado');
    expect(dica).toContain('abr/25 a mar/26 contra abr/24 a mar/25');
    expect(dica).toContain('os 3 meses mais recentes ficaram de fora');
  });

  it('os produtos financiados continuam na tela, na dica das linhas do SICOR', async () => {
    abrir();
    await esperarOPainel();

    const dica = lerDica('O que é a linha do SICOR');
    expect(dica).toContain('Produtos financiados');
    expect(dica).toContain('Trator (máquina)');
    expect(dica).toContain('Soja');
  });

  it('o top 5 é só da Região Tracbel — o vizinho maior de fora não entra', async () => {
    abrir();
    await esperarOPainel();

    const top = document.querySelector<HTMLElement>('[data-bloco="credito-top5"]')!;
    const nomes = [...top.querySelectorAll<HTMLElement>('.mom-ranking-nome')].map((n) => n.textContent);
    expect(nomes).toEqual(['Município 1', 'Município 2', 'Município 3', 'Município 4', 'Município 5']);
    expect(top).not.toHaveTextContent('Vizinho de fora');

    // A PARTICIPAÇÃO É A FATIA NA REGIÃO: 5 mi de 250 mi = 2%.
    expect(top.querySelector('.mom-ranking-participacao')).toHaveTextContent('2%');
  });

  it('fatia real abaixo de 1% sai "<1%", e não "0%" — zero diria que o município não pegou crédito', async () => {
    // 5 mi de 2 bi = 0,25%: arredondada, virava "0%".
    abrir(null, (p) => ({ ...p, regiao: { ...p.regiao!, janelas: janelas(2_000, 2_000_000_000) } }));
    await esperarOPainel();

    const participacoes = [...document.querySelectorAll<HTMLElement>('[data-bloco="credito-top5"] .mom-ranking-participacao')];
    expect(participacoes[0]).toHaveTextContent('<1%');
    expect(participacoes.filter((p) => /(^|\D)0%/.test(p.textContent ?? ''))).toEqual([]);
  });

  it('"Por linhas do SICOR" reordena o top 5', async () => {
    abrir();
    await esperarOPainel();

    fireEvent.change(screen.getByRole('combobox', { name: 'Ordenar o top 5' }), { target: { value: 'linhas' } });
    expect(screen.getByRole('heading', { name: /Top 5 municípios em linhas do SICOR/ })).toBeInTheDocument();
  });

  it('o município escolhido sobe para o topo do detalhamento, mesmo fora dos primeiros', async () => {
    abrir(CAFELANDIA);
    await esperarOPainel();

    // Sem o recorte, Cafelândia é a última da Região e nem apareceria.
    expect(nomesDoDetalhamento()[0]).toContain('Cafelândia');
  });

  it('"Ver todos" abre a lista inteira da Região, e o vizinho só entra se pedido', async () => {
    abrir();
    await esperarOPainel();

    expect(nomesDoDetalhamento()).toHaveLength(5);
    fireEvent.click(screen.getByRole('button', { name: 'Ver todos (16)' }));
    expect(nomesDoDetalhamento()).toHaveLength(16);
    expect(nomesDoDetalhamento().some((n) => n.includes('Vizinho de fora'))).toBe(false);

    fireEvent.click(screen.getByRole('checkbox', { name: 'incluir municípios fora da Região Tracbel' }));
    expect(nomesDoDetalhamento()[0]).toContain('Vizinho de fora');
  });

  it('RECOLHER desmarca o "fora da Região": os cinco primeiros voltam a ser só da Região, e o contador bate', async () => {
    // O DEFEITO (revisão de 24/09/2026): recolher com a caixa marcada escondia
    // a caixa e deixava o vizinho de fora no topo dos cinco, sem aviso.
    abrir();
    await esperarOPainel();

    fireEvent.click(screen.getByRole('button', { name: 'Ver todos (16)' }));
    fireEvent.click(screen.getByRole('checkbox', { name: 'incluir municípios fora da Região Tracbel' }));
    expect(nomesDoDetalhamento()[0]).toContain('Vizinho de fora');

    fireEvent.click(screen.getByRole('button', { name: 'Ver só os 5 primeiros' }));
    expect(nomesDoDetalhamento()).toHaveLength(5);
    expect(nomesDoDetalhamento().some((n) => n.includes('Vizinho de fora'))).toBe(false);

    // Reabrir começa como da primeira vez: a caixa desmarcada, e a lista do tamanho que o botão prometeu.
    fireEvent.click(screen.getByRole('button', { name: 'Ver todos (16)' }));
    expect(screen.getByRole('checkbox', { name: 'incluir municípios fora da Região Tracbel' })).not.toBeChecked();
    expect(nomesDoDetalhamento()).toHaveLength(16);
  });

  it('o "Ver todos (N)" conta a lista que vai abrir — inclusive o escolhido de fora da Região', async () => {
    abrir(VIZINHO);
    await esperarOPainel();

    // Os 16 da Região e o vizinho escolhido no topo: 17 linhas, e o botão diz 17.
    fireEvent.click(screen.getByRole('button', { name: 'Ver todos (17)' }));
    expect(nomesDoDetalhamento()).toHaveLength(17);
    expect(nomesDoDetalhamento()[0]).toContain('Vizinho de fora');
  });

  it('cada traço do detalhamento tem o ⓘ com o motivo — valor médio sem linha e variação sem base', async () => {
    abrir(null, (p) => ({
      ...p,
      porMunicipio: [
        ...p.porMunicipio,
        {
          codigoIbge: 3588888,
          nome: 'Sem linha na janela',
          pertenceAAdr: true,
          janelas: { linhas: 0, valor: 0, linhasAnteriores: 0, valorAnterior: 0 },
          indice: null,
        },
      ],
    }));
    await esperarOPainel();
    fireEvent.click(screen.getByRole('button', { name: 'Ver todos (17)' }));

    const linha = [...document.querySelectorAll<HTMLElement>('[data-bloco="credito-municipios"] tbody tr')].find((tr) =>
      tr.textContent?.includes('Sem linha na janela'),
    )!;
    for (const oQue of ['o valor médio de Sem linha na janela', 'a variação de Sem linha na janela']) {
      const gatilho = within(linha).getByRole('button', { name: `Por que ${oQue} não aparece` });
      expect(gatilho.closest('td')).toHaveTextContent('—');
      fireEvent.focus(gatilho);
      expect(screen.getByRole('tooltip').textContent!.length).toBeGreaterThan(20);
      fireEvent.blur(gatilho);
    }
  });

  it('as dicas e os aria-label não dizem "região" sozinha nem citam a maquete', async () => {
    abrir(CAFELANDIA);
    await esperarOPainel();

    const bloco = document.querySelector<HTMLElement>('[data-bloco="credito"]')!;
    const lidos = [
      ...[...bloco.querySelectorAll('[aria-label]')].map((n) => n.getAttribute('aria-label') ?? ''),
      ...[...bloco.querySelectorAll<HTMLButtonElement>('.dica-gatilho')].map((g) => {
        fireEvent.focus(g);
        const texto = screen.getByRole('tooltip').textContent ?? '';
        fireEvent.blur(g);
        return texto;
      }),
    ];
    expect(lidos.length).toBeGreaterThan(10);
    expect(lidos.filter((t) => /(?<!sub-)\bregião\b(?! Tracbel)/iu.test(t))).toEqual([]);
    expect(lidos.filter((t) => /maquete/i.test(t))).toEqual([]);
  });

  it('"Ordenar por" muda a ordem do detalhamento', async () => {
    abrir();
    await esperarOPainel();

    fireEvent.change(screen.getByRole('combobox', { name: 'Ordenar por' }), { target: { value: 'nome' } });
    // Em ordem alfabética, "Cafelândia" vem antes de "Município 1".
    expect(nomesDoDetalhamento()[0]).toContain('Cafelândia');
  });

  it('a evolução é ANUAL e diz por que não é mensal — não finge meses', async () => {
    abrir();
    await esperarOPainel();

    const dica = lerDica('Como ler evolução do valor financiado');
    expect(dica).toMatch(/somado por ANO/);
    expect(dica).toMatch(/fingir um detalhe/);
  });
});

describe('o detalhamento dentro do painel', () => {
  afterEach(() => {
    obterCreditoRural.mockReset();
    guardado.clear();
  });

  it('cada linha abre o detalhe pelo ⋮, com as duas janelas', async () => {
    abrir();
    await esperarOPainel();

    const linha = document.querySelector<HTMLElement>('[data-bloco="credito-municipios"] tbody tr')!;
    fireEvent.focus(within(linha).getByRole('button', { name: 'O crédito de Município 1' }));
    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent('500 na janela, contra 400 nos 12 meses anteriores');
  });
});
