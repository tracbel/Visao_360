/**
 * A tabela de municípios (issue 170, parte A; maquete na fase 4).
 *
 * O QUE ESTÁ SOB PROVA É A CONFERÊNCIA: a soma das linhas tem de ser o total da
 * consulta — é o que o documento 32 compara com o SQL. Se não fechar, algo sumiu
 * ou foi contado duas vezes.
 *
 * O CONTRATO MUDOU NA FASE 4, de propósito: as linhas de total saíram do corpo
 * da tabela e foram para a dica do título (decisão 3 do usuário). Estes testes
 * abrem a dica pelo teclado e leem as somas lá — a conta, pura, está provada em
 * `totaisDaAdr.teste.ts`. E a tabela ganhou paginação, ordenação por coluna e a
 * escolha de colunas, que também estão aqui.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ARARAQUARA, CAFELANDIA, municipioDeTeste } from '../../testes/territorio';
import type { IndicadoresDoMunicipio } from '../../tipos/territorio';
import { CHAVE_DAS_COLUNAS } from './carteira/colunas';
import { TabelaDeMunicipios } from './TabelaDeMunicipios';
import { calcularTotais } from './totaisDaAdr';

// O `localStorage` do Node 25 não guarda nada sem `--localstorage-file`; a
// escolha de colunas lê e grava nele.
const guardado = new Map<string, string>();
beforeEach(() => {
  vi.stubGlobal('localStorage', {
    getItem: (chave: string) => guardado.get(chave) ?? null,
    setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
    removeItem: (chave: string) => void guardado.delete(chave),
    clear: () => guardado.clear(),
  });
});
afterEach(() => {
  guardado.clear();
  vi.unstubAllGlobals();
});

const DA_ADR = municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' });
const FORA_DA_ADR = municipioDeTeste({ codigoIbge: ARARAQUARA, nome: 'Araraquara', pertenceAAdr: false });

const FORA_DO_MAPA = [
  {
    grupo: 'MunicipioSemCodigoIbge',
    descricao: 'clientes cujo município não tem código IBGE',
    cobertura: {
      clientes: 1,
      vinculos: 1,
      vinculosComCadencia: 2,
      cobertos: 1,
      foraDaCadencia: 1,
      nuncaContatados: 0,
      semCadencia: 0,
      pendentes: 1,
      percentualPendente: 50,
    },
    vendas: {
      clientesQueCompraram: 1,
      valorLiquido: 10_000,
      maquina: 0,
      peca: 6_000,
      servico: 4_000,
      outros: 0,
      posVenda: 10_000,
    },
  },
];

function montar(parcial: Partial<Parameters<typeof TabelaDeMunicipios>[0]> = {}) {
  const aoSelecionar = vi.fn();
  const props = {
    municipios: [DA_ADR, FORA_DA_ADR],
    daAdr: [DA_ADR],
    foraDoMapa: FORA_DO_MAPA,
    totais: calcularTotais([DA_ADR]),
    selecionado: null,
    aoSelecionar,
    territorioNaoCarregado: false,
    semFiltro: true,
    ...parcial,
  };
  const resultado = render(<TabelaDeMunicipios {...props} />);
  return { aoSelecionar, ...resultado, props };
}

/**
 * Abre a dica dos totais pelo teclado, devolve o texto de cada linha dela e
 * FECHA — o balão vive num portal, e dois abertos confundiriam o `getByRole`.
 */
function linhasDaConferencia(): Record<string, string> {
  const gatilho = screen.getByRole('button', { name: 'Os totais da ADR e da consulta' });
  fireEvent.focus(gatilho);
  const dica = screen.getByRole('tooltip');
  const linhas = Object.fromEntries(
    [...dica.querySelectorAll<HTMLElement>('li')].map((li) => [li.dataset.conferencia ?? li.textContent!.split(':')[0], li.textContent ?? '']),
  );
  fireEvent.blur(gatilho);
  return linhas;
}

/** Vinte e três municípios da ADR, com vendas e parques diferentes — para paginar e ordenar. */
function muitos(): IndicadoresDoMunicipio[] {
  const nomes = [
    'Adamantina', 'Barretos', 'Batatais', 'Bebedouro', 'Cafelândia', 'Catanduva', 'Colina', 'Franca',
    'Guaíra', 'Ituverava', 'Jaboticabal', 'Morro Agudo', 'Nuporanga', 'Olímpia', 'Orlândia', 'Pedregulho',
    'Pitangueiras', 'Restinga', 'Sales Oliveira', 'São Joaquim da Barra', 'Sertãozinho', 'Taquaritinga', 'Viradouro',
  ];
  return nomes.map((nome, i) =>
    municipioDeTeste({
      codigoIbge: 3_500_000 + i,
      nome,
      vendas: { ...DA_ADR.vendas, valorLiquido: 100_000 * (i + 1) },
      cobertura: { ...DA_ADR.cobertura, vinculosComCadencia: 50 - i },
      // O PARQUE DO QUINTO É NULO: ausência não é o menor número, e vai para o
      // fim nas duas direções.
      potencialEstrutural:
        i === 4
          ? { ...DA_ADR.potencialEstrutural!, parqueDeMaquinas: null, motivoSemParque: 'SemArea' }
          : { ...DA_ADR.potencialEstrutural!, parqueDeMaquinas: 10 * (i + 1) },
    }),
  );
}

const nomesNaTela = () =>
  [...document.querySelectorAll<HTMLElement>('tbody td[data-coluna="municipio"]')].map((td) => td.textContent);

describe('a tabela de municípios — a conferência', () => {
  it('lista os municípios da ADR e deixa clicar num deles', () => {
    const { aoSelecionar } = montar();

    fireEvent.click(screen.getByRole('button', { name: 'Cafelândia' }));
    expect(aoSelecionar).toHaveBeenCalledWith(CAFELANDIA);

    fireEvent.click(screen.getByRole('button', { name: 'Abrir a ficha de Cafelândia' }));
    expect(aoSelecionar).toHaveBeenCalledTimes(2);
  });

  it('as linhas de total saíram do corpo da tabela', () => {
    montar();
    const corpo = document.querySelector('tbody')!;
    expect(corpo).not.toHaveTextContent('Total da ADR');
    expect(corpo).not.toHaveTextContent('Total da consulta');
    expect(corpo).not.toHaveTextContent('São Paulo fora da ADR');
  });

  it('o total da ADR, na dica do título, soma só os municípios da ADR', () => {
    montar();
    const linhas = linhasDaConferencia();
    // 18 elegíveis, 11 no prazo, 7 pendentes — os do único município da ADR.
    expect(linhas['Total da ADR']).toContain('18 elegíveis');
    expect(linhas['Total da ADR']).toContain('11 no prazo');
    expect(linhas['Total da ADR']).toContain('1.240 máquinas teóricas');
  });

  it('o total da consulta, na dica, soma o mapa e o que ficou fora dele', () => {
    montar();
    // 18 (ADR) + 18 (fora da ADR, mas no mapa) + 2 (fora do mapa) = 38 elegíveis.
    const linhas = linhasDaConferencia();
    expect(linhas['São Paulo fora da ADR']).toContain('18 elegíveis');
    expect(linhas.MunicipioSemCodigoIbge).toContain('2 elegíveis');
    expect(linhas['Total da consulta']).toContain('38 elegíveis');
  });

  it('com filtro, os grupos de fora não aparecem — o total seria de outro recorte', () => {
    montar({ semFiltro: false });
    const linhas = linhasDaConferencia();

    expect(linhas['São Paulo fora da ADR']).toBeUndefined();
    expect(linhas['Total da consulta']).toBeUndefined();
    expect(linhas['Total da ADR (filtro)']).toContain('18 elegíveis');
  });

  it('território não carregado troca o total da ADR por uma explicação, e não por zeros', () => {
    montar({ territorioNaoCarregado: true, daAdr: [], totais: calcularTotais([]) });

    // O corpo diz o que aconteceu, e não fica vazio sem explicação.
    expect(document.querySelector('tbody')).toHaveTextContent(/Território não carregado neste banco/);

    // A linha da ADR vira explicação; o TOTAL DA CONSULTA fica, e é o que prova
    // que as vendas continuam no banco — 18 + 18 do mapa mais os 2 de fora.
    const linhas = linhasDaConferencia();
    expect(linhas['Total da ADR']).toMatch(/território não carregado neste banco/);
    expect(linhas['Total da consulta']).toContain('38 elegíveis');
  });

  // O CONTRATO MUDOU NA FASE T2: o motivo saía num `title=`, que não abre pelo
  // teclado nem no toque. Agora é `ValorAusente` — traço no lugar do número e o
  // motivo numa dica alcançável (issue 167).
  it('município sem parque mostra traço, e o motivo abre numa dica pelo teclado', () => {
    const semParque = municipioDeTeste({
      codigoIbge: CAFELANDIA,
      nome: 'Cafelândia',
      potencialEstrutural: {
        parqueDeMaquinas: null,
        demandaAnualDeMaquinas: null,
        areaUtilHectares: null,
        estimativa: false,
        motivoSemParque: 'SemArea',
        motivoSemDemanda: 'SemArea',
      },
    });
    montar({ daAdr: [semParque], municipios: [semParque], totais: calcularTotais([semParque]) });

    const gatilho = screen.getByRole('button', { name: 'Por que o parque de Cafelândia não aparece' });
    expect(gatilho.closest('td')).toHaveTextContent('—');

    fireEvent.focus(gatilho);
    expect(screen.getByRole('tooltip')).toHaveTextContent('área plantada não divulgada aqui (sigilo do IBGE)');
  });

  it('os cabeçalhos são visíveis — inclusive "Ação" — e o % pendente saiu da linha para a dica', () => {
    montar();
    const cabecalho = document.querySelector('thead')!;
    expect(cabecalho.querySelector('.cad-so-leitor')).toBeNull();
    expect(within(cabecalho).getByText('Ação')).toBeInTheDocument();
    expect(within(cabecalho).getByText('Sub-região · Loja')).toBeInTheDocument();

    expect(document.querySelector('tbody td[data-coluna="pendentes"]')).toHaveTextContent(/^7$/);
    fireEvent.focus(screen.getByRole('button', { name: 'O que conta como pendente' }));
    expect(screen.getByRole('tooltip')).toHaveTextContent('percentual pendente de cada município está na ficha');
  });
});

describe('a tabela de municípios — paginação', () => {
  it('mostra dez por página, e a faixa diz onde se está', () => {
    montar({ daAdr: muitos() });

    expect(nomesNaTela()).toHaveLength(10);
    expect(screen.getByText('Mostrando 1–10 de 23 municípios')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Página 1' })).toHaveAttribute('aria-current', 'page');
    expect(screen.getByRole('button', { name: 'Página anterior' })).toBeDisabled();

    fireEvent.click(screen.getByRole('button', { name: 'Página 3' }));
    expect(nomesNaTela()).toHaveLength(3);
    expect(screen.getByText('Mostrando 21–23 de 23 municípios')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Próxima página' })).toBeDisabled();

    fireEvent.click(screen.getByRole('button', { name: 'Página anterior' }));
    expect(screen.getByText('Mostrando 11–20 de 23 municípios')).toBeInTheDocument();
  });

  it('o tamanho da página muda para 25 e 50, e volta para a primeira página', () => {
    montar({ daAdr: muitos() });
    fireEvent.click(screen.getByRole('button', { name: 'Página 2' }));

    const tamanho = screen.getByRole('combobox', { name: 'Municípios por página' }) as HTMLSelectElement;
    expect([...tamanho.options].map((o) => o.textContent)).toEqual(['10 por página', '25 por página', '50 por página']);

    fireEvent.change(tamanho, { target: { value: '25' } });
    expect(nomesNaTela()).toHaveLength(23);
    expect(screen.getByText('Mostrando 1–23 de 23 municípios')).toBeInTheDocument();
  });

  it('com muitas páginas, as do meio viram reticências — "1 2 3 4 5 … 21"', () => {
    const duzentos = Array.from({ length: 205 }, (_, i) =>
      municipioDeTeste({ codigoIbge: 3_600_000 + i, nome: `Município ${String(i).padStart(3, '0')}` }),
    );
    montar({ daAdr: duzentos });

    const paginas = screen.getByRole('navigation', { name: 'Páginas da tabela de municípios' });
    expect(paginas.textContent).toBe('12345…21');
  });

  it('a busca volta para a primeira página e diz de quantos filtrou', () => {
    montar({ daAdr: muitos() });
    fireEvent.click(screen.getByRole('button', { name: 'Página 3' }));

    fireEvent.change(screen.getByRole('searchbox', { name: /Buscar município/ }), { target: { value: 'sao joaquim' } });
    expect(nomesNaTela()).toEqual(['São Joaquim da Barra']);
    expect(screen.getByText(/Mostrando 1–1 de 1 municípios encontrados, de 23/)).toBeInTheDocument();
  });

  it('o município escolhido fora da tabela aparece nela: a página vai até a linha dele', () => {
    const lista = muitos();
    // A ordem padrão é por venda, do maior para o menor: Adamantina (a menor) é a última.
    montar({ daAdr: lista, selecionado: lista[0].codigoIbge });

    expect(screen.getByText('Mostrando 21–23 de 23 municípios')).toBeInTheDocument();
    const linha = screen.getByRole('button', { name: 'Adamantina' }).closest('tr')!;
    expect(linha).toHaveAttribute('aria-current', 'true');
  });
});

describe('a tabela de municípios — ordenação', () => {
  const cabecalho = (coluna: string) => document.querySelector<HTMLElement>(`thead th[data-coluna="${coluna}"]`)!;
  const ordenarPor = (nome: string) => fireEvent.click(within(document.querySelector('thead')!).getByRole('button', { name: nome }));

  it('abre por vendas, do maior para o menor — e o cabeçalho diz isso no aria-sort', () => {
    montar({ daAdr: muitos() });

    expect(cabecalho('vendas')).toHaveAttribute('aria-sort', 'descending');
    expect(cabecalho('municipio')).toHaveAttribute('aria-sort', 'none');
    expect(nomesNaTela()[0]).toBe('Viradouro');
  });

  it('clicar no nome ordena de A a Z; clicar de novo inverte', () => {
    montar({ daAdr: muitos(), selecionado: null });

    ordenarPor('Município');
    expect(cabecalho('municipio')).toHaveAttribute('aria-sort', 'ascending');
    expect(cabecalho('vendas')).toHaveAttribute('aria-sort', 'none');
    expect(nomesNaTela().slice(0, 3)).toEqual(['Adamantina', 'Barretos', 'Batatais']);

    ordenarPor('Município');
    expect(cabecalho('municipio')).toHaveAttribute('aria-sort', 'descending');
    expect(nomesNaTela()[0]).toBe('Viradouro');
  });

  it('toda coluna numérica ordena pelo clique no cabeçalho', () => {
    montar({ daAdr: muitos() });

    for (const [nome, coluna] of [
      ['Elegíveis', 'elegiveis'],
      ['No prazo', 'noPrazo'],
      ['Pendentes', 'pendentes'],
      ['Vendas', 'vendas'],
      ['Pós-venda', 'posVenda'],
      ['Máquinas', 'maquinas'],
    ] as const) {
      ordenarPor(nome);
      expect(cabecalho(coluna).getAttribute('aria-sort'), nome).toMatch(/ascending|descending/);
    }

    // Elegíveis do maior para o menor: 50, 49, 48…
    ordenarPor('Elegíveis');
    expect(cabecalho('elegiveis')).toHaveAttribute('aria-sort', 'descending');
    expect(nomesNaTela()[0]).toBe('Adamantina');
  });

  it('parque ausente vai para o fim nas duas direções — ausência não é o menor número', () => {
    montar({ daAdr: muitos() });
    fireEvent.change(screen.getByRole('combobox', { name: 'Municípios por página' }), { target: { value: '25' } });

    ordenarPor('Máquinas');
    expect(cabecalho('maquinas')).toHaveAttribute('aria-sort', 'descending');
    expect(nomesNaTela().at(-1)).toBe('Cafelândia');

    ordenarPor('Máquinas');
    expect(cabecalho('maquinas')).toHaveAttribute('aria-sort', 'ascending');
    expect(nomesNaTela()[0]).toBe('Adamantina');
    expect(nomesNaTela().at(-1)).toBe('Cafelândia');
  });
});

describe('a tabela de municípios — colunas escolhidas', () => {
  const colunasNaTela = () => [...document.querySelectorAll<HTMLElement>('thead th')].map((th) => th.dataset.coluna);

  it('a engrenagem esconde uma coluna, e a escolha fica guardada no navegador', () => {
    const { unmount } = montar();
    expect(colunasNaTela()).toEqual(['municipio', 'hierarquia', 'elegiveis', 'noPrazo', 'pendentes', 'vendas', 'posVenda', 'maquinas', 'acao']);

    fireEvent.click(screen.getByRole('button', { name: 'Escolher as colunas da tabela' }));
    fireEvent.click(screen.getByRole('checkbox', { name: 'Elegíveis' }));
    fireEvent.click(screen.getByRole('checkbox', { name: 'Pós-venda (provisório)' }));

    expect(colunasNaTela()).toEqual(['municipio', 'hierarquia', 'noPrazo', 'pendentes', 'vendas', 'maquinas', 'acao']);
    expect(document.querySelector('tbody td[data-coluna="elegiveis"]')).toBeNull();
    expect(JSON.parse(guardado.get(CHAVE_DAS_COLUNAS)!)).toEqual(['elegiveis', 'posVenda']);
    // O botão diz quantas estão escondidas.
    expect(screen.getByRole('button', { name: 'Escolher as colunas da tabela' })).toHaveTextContent('2');

    // Remontar é abrir a página de novo: a escolha continua.
    unmount();
    montar();
    expect(colunasNaTela()).not.toContain('elegiveis');

    fireEvent.click(screen.getByRole('button', { name: 'Escolher as colunas da tabela' }));
    fireEvent.click(screen.getByRole('button', { name: 'Mostrar todas' }));
    expect(colunasNaTela()).toContain('elegiveis');
    expect(colunasNaTela()).toContain('posVenda');
  });

  it('um navegador que recusa guardar não quebra a tabela — ela abre com todas as colunas', () => {
    vi.stubGlobal('localStorage', {
      getItem: () => {
        throw new Error('SecurityError');
      },
      setItem: () => {
        throw new Error('QuotaExceededError');
      },
    });
    montar();
    expect(colunasNaTela()).toHaveLength(9);

    fireEvent.click(screen.getByRole('button', { name: 'Escolher as colunas da tabela' }));
    fireEvent.click(screen.getByRole('checkbox', { name: 'No prazo' }));
    expect(colunasNaTela()).not.toContain('noPrazo');
  });

  it('o que veio do navegador é conferido: coluna desconhecida não esconde nada', () => {
    guardado.set(CHAVE_DAS_COLUNAS, JSON.stringify(['municipio', 'coisa', 'vendas']));
    montar();
    // "municipio" não se esconde; "coisa" não existe; "vendas" vale.
    expect(colunasNaTela()).toEqual(['municipio', 'hierarquia', 'elegiveis', 'noPrazo', 'pendentes', 'posVenda', 'maquinas', 'acao']);
  });
});
