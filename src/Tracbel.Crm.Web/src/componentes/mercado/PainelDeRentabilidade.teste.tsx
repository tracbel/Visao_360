/**
 * A ABA RENTABILIDADE DO MOMENTO (fidelidade às maquetes, fase 3).
 *
 * O que está sob prova é o que a maquete pede e a regra não deixa errar: o
 * ranking ordena pelas quatro colunas; a "Média da Região Tracbel" é ponderada
 * pela área colhida DA REGIÃO (e não pela de São Paulo, que é a que a rota traz);
 * a "Cultura destaque" é a de maior área, com o critério dito; a tendência sai
 * vazia com o motivo; a tabela está aberta; e os painéis antigos de preço e
 * custo continuam a um clique.
 *
 * AS AMOSTRAS TÊM A FORMA REAL DA LEITURA (revisão de 24/09/2026):
 * `potencial[]` traz UMA LINHA POR REGRA DE POTENCIAL vigente, em TODO
 * município, com área nula onde a PAM não divulgou; cultura sem regra não tem
 * linha nenhuma. As amostras de antes davam linha só a quem tinha área, e a
 * cultura sem regra parecia "sem área divulgada" — a tela passava aqui e
 * errava com o banco, onde hoje há UMA regra só.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import { municipioDeTeste } from '../../testes/territorio';
import type { RentabilidadeDaCultura } from '../../tipos/mercado';
import type { CulturaNoCatalogo } from '../../tipos/potencial';
import type { IndicadoresDoMunicipio, PotencialTerritorial } from '../../tipos/territorio';
import { recorteFiltrado, type RecorteFiltrado } from '../territorio/indicadoresDaAdr';
import { PainelDeRentabilidade } from './PainelDeRentabilidade';

const obterRentabilidadeDasCulturas = vi.hoisted(() => vi.fn());
const obterCatalogoDoMercado = vi.hoisted(() => vi.fn());

vi.mock('../../dados/api/territorio', async (original) => ({
  ...(await original<typeof import('../../dados/api/territorio')>()),
  obterRentabilidadeDasCulturas,
}));
vi.mock('../../dados/api/potencial', async (original) => ({
  ...(await original<typeof import('../../dados/api/potencial')>()),
  obterCatalogoDoMercado,
}));

// O GRÁFICO TEM TAMANHO MEDIDO (ResizeObserver) E DESENHA EM CANVAS — o jsdom
// não tem nenhum dos dois. Aqui o que está sob prova são os números e a ordem.
vi.mock('../MolduraDeGrafico', () => ({ MolduraDeGrafico: () => <div data-grafico /> }));
vi.mock('./momento/GraficoReceitaCustoMargem', () => ({ GraficoReceitaCustoMargem: () => null }));
vi.mock('../territorio/PainelDePrecos', () => ({ PainelDePrecos: () => <div data-bloco="precos" /> }));
vi.mock('../territorio/PainelDeCustos', () => ({ PainelDeCustos: () => <div data-bloco="custos" /> }));

const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (c: string) => guardado.get(c) ?? null,
  setItem: (c: string, v: string) => void guardado.set(c, v),
  removeItem: (c: string) => void guardado.delete(c),
  clear: () => guardado.clear(),
});

const PAM_CAFE = 2502;
const PAM_CANA = 2503;
const PAM_LARANJA = 2504;

function linha(codigo: string, nome: string, receita: number, custo: number): RentabilidadeDaCultura {
  return {
    culturaCodigo: codigo,
    culturaNome: nome,
    unidadeComercial: 'saca de 60 kg',
    anoDaProdutividade: 2024,
    produtividadeKgPorHa: 2_100,
    precoMedioPorKg: 24.5,
    mesesDePrecoNaMedia: 12,
    receitaPorHectare: receita,
    localDoCusto: 'Franca',
    camadaDoCusto: 'Total',
    safraDoCusto: 2025,
    custoPorHectare: custo,
    margemPorHectare: receita - custo,
    margemPorUnidade: 100,
    // A ÁREA DE SÃO PAULO — de propósito enorme e invertida: se a tela pesasse
    // por ela, a média e o destaque sairiam outros.
    areaColhidaHectares: codigo === 'CAFE' ? 5_000_000 : 1,
    margemTotal: 1,
    motivo: 'Nenhum',
    fraseDoMotivo: '',
  };
}

// Café: margem 30.000; Cana: 2.580; Laranja: −1.800.
const LINHAS = [
  linha('CANA', 'Cana-de-açúcar', 11_480, 8_900),
  linha('CAFE', 'Café (Total)', 51_450, 21_450),
  linha('LARANJA', 'Laranja', 53_200, 55_000),
];

function cultura(codigo: string, nome: string, pam: number): CulturaNoCatalogo {
  return {
    codigo,
    nome,
    segmento: 'lavoura',
    unidadeComercial: 'saca',
    quilosPorUnidade: 60,
    fonteDoPreco: 'CONAB',
    produtoDoPreco: codigo,
    serieDeCusto: nome,
    estaAtiva: true,
    produtos: [{ codigoIbge: pam, nome, entraNaSomaDaLavoura: true }],
  };
}

/** A linha de cada regra no município — nula onde a PAM não divulgou área. */
function porRegra(regras: readonly number[], areas: Partial<Record<number, number>>): PotencialTerritorial[] {
  return regras.map((produtoCodigoIbge) => {
    const area = areas[produtoCodigoIbge] ?? null;
    return {
      produtoCodigoIbge,
      areaPlantadaHectares: area,
      maquinasTeoricas: null,
      areaColhidaHectares: area,
      valorDaProducaoMilReais: null,
      ano: 2024,
      quantidadeProduzida: null,
      unidadeDaQuantidade: null,
      produtividade: null,
      unidadeDaProdutividade: null,
    };
  });
}

/**
 * NA REGIÃO TRACBEL: cana 90.000 ha e café 10.000 ha; a laranja nenhuma área
 * divulgada. FORA DA REGIÃO: um vizinho cheio de café, que não pode virar o
 * destaque daqui. Quais culturas vêm na leitura depende das regras.
 */
function municipiosCom(regras: readonly number[]): IndicadoresDoMunicipio[] {
  return [
    municipioDeTeste({ codigoIbge: 1, nome: 'A', potencial: porRegra(regras, { [PAM_CANA]: 60_000, [PAM_CAFE]: 10_000 }) }),
    municipioDeTeste({ codigoIbge: 2, nome: 'B', potencial: porRegra(regras, { [PAM_CANA]: 30_000 }) }),
    municipioDeTeste({ codigoIbge: 3, nome: 'C', pertenceAAdr: false, potencial: porRegra(regras, { [PAM_CAFE]: 900_000 }) }),
  ];
}

/** Todas as culturas com regra — o dia em que a área por cultura chegar. */
const COM_TODAS_AS_REGRAS = municipiosCom([PAM_CAFE, PAM_CANA, PAM_LARANJA]);
/** O BANCO DE HOJE: uma regra só, a da cana. Café e laranja não vêm na leitura. */
const COM_UMA_REGRA = municipiosCom([PAM_CANA]);

function abrir({
  catalogoFalha = false,
  nomeDoMunicipio = null as string | null,
  municipios = COM_TODAS_AS_REGRAS,
  carregando = false,
  recorte = null as RecorteFiltrado | null,
  linhas = LINHAS,
} = {}) {
  obterRentabilidadeDasCulturas.mockResolvedValue({ dados: linhas, procedencia: null });
  if (catalogoFalha) obterCatalogoDoMercado.mockRejectedValue(new Error('403'));
  else
    obterCatalogoDoMercado.mockResolvedValue({
      dados: {
        culturas: [
          cultura('CANA', 'Cana-de-açúcar', PAM_CANA),
          cultura('CAFE', 'Café (Total)', PAM_CAFE),
          cultura('LARANJA', 'Laranja', PAM_LARANJA),
        ],
        categorias: [],
      },
      procedencia: null,
    });

  render(
    <ProvedorDeContextoDeAcesso>
      <PainelDeRentabilidade
        municipios={municipios}
        nomeDoMunicipio={nomeDoMunicipio}
        carregando={carregando}
        recorte={recorte}
      />
    </ProvedorDeContextoDeAcesso>,
  );
}

const cartao = (rotulo: string) => document.querySelector<HTMLElement>(`[data-cartao="${rotulo}"]`)!;
const ranking = () =>
  [...document.querySelectorAll<HTMLElement>('.mom-ranking .mom-ranking-linha')].map((l) => l.dataset.cultura);
const linhaDaTabela = (codigo: string) => document.querySelector<HTMLElement>(`table.mom-tabela tr[data-cultura="${codigo}"]`)!;

function lerDica(rotulo: string, dentroDe: HTMLElement = document.body): string {
  const gatilho = within(dentroDe).getByRole('button', { name: rotulo });
  fireEvent.focus(gatilho);
  const texto = screen.getByRole('tooltip').textContent ?? '';
  fireEvent.blur(gatilho);
  return texto;
}

const esperar = () => screen.findByText('Melhor margem/ha');
/** O catálogo chegou: a coluna de área já afirma alguma coisa. */
const esperarAArea = () => screen.findAllByRole('button', { name: /Por que a área colhida de/ });

describe('a aba Rentabilidade', () => {
  afterEach(() => {
    obterRentabilidadeDasCulturas.mockReset();
    obterCatalogoDoMercado.mockReset();
    guardado.clear();
  });

  it('a melhor margem por hectare vem com centavos e o nome da cultura — não de um município', async () => {
    abrir();
    await esperar();

    expect(cartao('Melhor margem/ha')).toHaveTextContent('R$ 30.000,00');
    expect(cartao('Melhor margem/ha')).toHaveTextContent('Café (Total)');
    expect(lerDica('O que é a melhor margem por hectare')).toMatch(/Não existe versão municipal/);
  });

  it('a média da Região Tracbel é PONDERADA pela área colhida DA REGIÃO', async () => {
    abrir();
    await screen.findByText('R$ 5.322,00');

    // (30.000 × 10.000 + 2.580 × 90.000) ÷ 100.000 = 5.322. A média simples
    // daria 10.260; pesada pela área de São Paulo, quase 30.000.
    const media = cartao('Média da Região Tracbel');
    expect(media).toHaveTextContent('R$ 5.322,00');
    expect(media).toHaveTextContent('margem média ponderada');
    const dica = lerDica('O que é a margem média da Região Tracbel');
    expect(dica).toMatch(/2 culturas que têm margem e área/);
    // A LARANJA TEM REGRA E NENHUMA ÁREA DIVULGADA: ela não pesa, e a dica diz.
    expect(dica).toMatch(/Laranja não pesa: nenhum município da Região Tracbel tem área divulgada/);
  });

  it('a cultura destaque é a de maior área colhida na Região — e o critério está dito', async () => {
    abrir();
    await screen.findByText('R$ 5.322,00');

    const destaque = cartao('Cultura destaque');
    expect(destaque).toHaveTextContent('Cana-de-açúcar');
    expect(destaque).toHaveTextContent('R$ 2.580,00 / ha');
    expect(lerDica('O que é a cultura destaque')).toMatch(/MAIOR ÁREA COLHIDA/);
  });

  it('NO BANCO DE HOJE (uma regra só), média e destaque saem com o traço e o motivo verdadeiro', async () => {
    // O DEFEITO: com a área de uma cultura só, a "média ponderada" era a margem
    // da cana com o nome de média, e o destaque era sempre a cana.
    abrir({ municipios: COM_UMA_REGRA });
    await esperar();
    await esperarAArea();

    for (const [rotulo, oQue] of [
      ['Média da Região Tracbel', 'a margem média da Região Tracbel'],
      ['Cultura destaque', 'a cultura destaque'],
    ] as const) {
      const valor = cartao(rotulo).querySelector('.mom-cartao-valor')!;
      expect(valor.textContent, rotulo).not.toMatch(/\d|Cana/);
      const motivo = lerDica(`Por que ${oQue} não aparece`);
      expect(motivo).toMatch(/só traz a área colhida das culturas com regra de potencial/);
      expect(motivo).toMatch(/falta a de Café \(Total\) e Laranja/);
      expect(motivo).toMatch(/pedida ao backend/);
    }
    expect(lerDica('Por que a margem média da Região Tracbel não aparece')).toMatch(/média ponderada pela área de atuação/);

    // A COLUNA DE ÁREA DIZ A AUSÊNCIA CERTA DE CADA LINHA: a cana tem a área; o
    // café não tem regra — e não é "nenhum município tem área divulgada".
    expect(linhaDaTabela('CANA')).toHaveTextContent('90.000');
    const cafe = lerDica('Por que a área colhida de Café (Total) não aparece', linhaDaTabela('CAFE'));
    expect(cafe).toMatch(/Café \(Total\) não tem regra vigente/);
    expect(cafe).not.toMatch(/Nenhum município/);
  });

  it('cultura COM regra e sem área divulgada: o motivo é sigilo ou lavoura que não existe — e não "a leitura não traz"', async () => {
    abrir();
    await esperarAArea();

    const laranja = lerDica('Por que a área colhida de Laranja não aparece', linhaDaTabela('LARANJA'));
    expect(laranja).toMatch(/Nenhum município da Região Tracbel tem área colhida de Laranja divulgada na PAM/);
    expect(laranja).toMatch(/sigilo do IBGE, ou a cultura não é plantada ali/);
    expect(laranja).not.toMatch(/regra/);
  });

  it('enquanto os municípios não chegam, área, média e destaque dizem "carregando" — e não afirmam ausência', async () => {
    abrir({ carregando: true });
    await esperar();

    expect(screen.queryAllByRole('button', { name: /Por que a área colhida de/ })).toEqual([]);
    expect(linhaDaTabela('CANA')).toHaveTextContent('carregando…');
    for (const rotulo of ['Média da Região Tracbel', 'Cultura destaque']) {
      expect(cartao(rotulo)).toHaveTextContent('carregando…');
      expect(within(cartao(rotulo)).queryByRole('button', { name: /^Por que .* não aparece$/ })).toBeNull();
    }
  });

  it('COM FILTRO, o cartão diz o recorte — "Região Tracbel" é a área de atuação inteira', async () => {
    abrir({ recorte: recorteFiltrado('Norte', null) });
    await screen.findByText('R$ 5.322,00');

    expect(cartao('Média da Sub-região Norte')).toHaveTextContent('R$ 5.322,00');
    expect(document.querySelector('[data-cartao="Média da Região Tracbel"]')).toBeNull();
    const dica = lerDica('O que é a margem média da Sub-região Norte');
    expect(dica).toMatch(/O recorte dos filtros é Sub-região Norte/);
    expect(dica).toMatch(/municípios da Sub-região Norte/);
  });

  it('com sub-região E loja, o rótulo fica curto — "Média do recorte" —, e o nome inteiro vai na dica', async () => {
    abrir({ recorte: recorteFiltrado('Norte', 'Catanduva') });
    await screen.findByText('R$ 5.322,00');

    expect(cartao('Média do recorte')).toHaveTextContent('R$ 5.322,00');
    expect(lerDica('O que é a margem média do recorte')).toMatch(/Sub-região Norte · loja Catanduva/);
  });

  it('a tendência sai com o traço e o motivo, e nunca com um número', async () => {
    abrir();
    await esperar();

    const valor = cartao('Tendência').querySelector('.mom-cartao-valor')!;
    expect(valor.textContent).not.toMatch(/\d/);
    expect(lerDica('Por que a tendência da margem não aparece')).toMatch(/issue 159/);
  });

  it('"Ordenar por" reordena o ranking pelas quatro colunas', async () => {
    abrir();
    await esperar();

    expect(ranking()).toEqual(['CAFE', 'CANA', 'LARANJA']);

    const ordenar = screen.getByRole('combobox', { name: 'Ordenar por' });
    fireEvent.change(ordenar, { target: { value: 'custo' } });
    expect(ranking()).toEqual(['LARANJA', 'CAFE', 'CANA']);
    expect(screen.getByRole('heading', { name: /Ranking de culturas por custo\/ha/ })).toBeInTheDocument();

    fireEvent.change(ordenar, { target: { value: 'margemPercentual' } });
    expect(ranking()).toEqual(['CAFE', 'CANA', 'LARANJA']);
  });

  it('o detalhamento está ABERTO, com as colunas da maquete e a área da Região', async () => {
    abrir();
    await screen.findByText('R$ 5.322,00');

    const tabela = document.querySelector<HTMLElement>('table.mom-tabela')!;
    const colunas = [...tabela.querySelectorAll('thead th')].map((n) => (n.textContent ?? '').trim());
    expect(colunas).toEqual([
      'Cultura',
      'Receita / ha',
      'Custo / ha',
      'Margem / ha',
      'Margem %',
      'Produtividade',
      'Preço médio',
      'Área colhida (ha)',
      'Detalhes',
    ]);

    const cana = linhaDaTabela('CANA');
    expect(cana).toHaveTextContent('90.000');
    expect(cana).toHaveTextContent('22%');
    // A laranja não tem área divulgada na Região: traço, e não zero.
    expect(within(linhaDaTabela('LARANJA')).getByRole('button', { name: 'Por que a área colhida de Laranja não aparece' })).toBeInTheDocument();
  });

  it('cada traço do detalhamento tem o ⓘ com o motivo — inclusive sem a frase do servidor', async () => {
    const semNada: RentabilidadeDaCultura = {
      ...linha('MILHO', 'Milho', 0, 0),
      receitaPorHectare: null,
      custoPorHectare: null,
      margemPorHectare: null,
      produtividadeKgPorHa: null,
      precoMedioPorKg: null,
      motivo: 'SemPreco',
      fraseDoMotivo: '',
    };
    abrir({ linhas: [...LINHAS, semNada] });
    await esperarAArea();

    const milho = linhaDaTabela('MILHO');
    const celulas = [...milho.querySelectorAll<HTMLElement>('td.mom-num')];
    const comTraco = celulas.filter((td) => td.textContent?.includes('—'));
    // Receita, custo, margem, margem %, produtividade, preço e área.
    expect(comTraco).toHaveLength(7);
    for (const td of comTraco) {
      const gatilho = within(td).getByRole('button', { name: /^Por que .* não aparece$/ });
      fireEvent.focus(gatilho);
      expect((screen.getByRole('tooltip').textContent ?? '').length, gatilho.getAttribute('aria-label')!).toBeGreaterThan(20);
      fireEvent.blur(gatilho);
    }
  });

  it('sem o catálogo, área, destaque e média saem com o motivo — nada é estimado', async () => {
    abrir({ catalogoFalha: true });
    await esperar();
    await esperarAArea();

    expect(cartao('Média da Região Tracbel').querySelector('.mom-cartao-valor')!.textContent).not.toMatch(/\d/);
    expect(lerDica('Por que a margem média da Região Tracbel não aparece')).toMatch(/catálogo de culturas/);
    expect(lerDica('Por que a área colhida de Laranja não aparece', linhaDaTabela('LARANJA'))).toMatch(/catálogo de culturas/);
  });

  it('a referência estadual (issue 168) está na dica, com o município escolhido', async () => {
    abrir({ nomeDoMunicipio: 'Cafelândia' });
    await esperar();

    const dica = lerDica('Como ler detalhamento por cultura');
    expect(dica).toMatch(/Referência: São Paulo/);
    expect(dica).toMatch(/Escolher Cafelândia destaca as culturas dele/);
  });

  it('a competência de cada linha está no ⋮ — e a área de São Paulo também', async () => {
    abrir();
    await esperar();

    const dica = lerDica('Como a margem de Café (Total) se compõe', linhaDaTabela('CAFE'));
    expect(dica).toMatch(/PAM 2024/);
    expect(dica).toMatch(/Franca/);
    expect(dica).toMatch(/Área colhida em SP/);
  });

  it('os painéis de preço e custo continuam a um clique, em "Ver séries" — e o aria-controls só aponta para o que existe', async () => {
    abrir();
    await esperar();

    const botao = screen.getByRole('button', { name: 'Ver séries de preço e custo' });
    expect(document.querySelector('[data-bloco="precos"]')).toBeNull();
    // FECHADO, AS SÉRIES NÃO ESTÃO NO DOCUMENTO: nada de `aria-controls` para um id ausente.
    expect(botao).not.toHaveAttribute('aria-controls');

    fireEvent.click(botao);
    expect(document.querySelector('[data-bloco="precos"]')).not.toBeNull();
    expect(document.querySelector('[data-bloco="custos"]')).not.toBeNull();
    const controlado = screen.getByRole('button', { name: 'Fechar as séries' }).getAttribute('aria-controls')!;
    expect(document.getElementById(controlado)).toHaveAttribute('data-bloco', 'rentabilidade-fontes');
  });

  it('as dicas e os aria-label não dizem "região" sozinha nem citam a maquete', async () => {
    abrir({ municipios: COM_UMA_REGRA });
    await esperarAArea();

    const bloco = document.querySelector<HTMLElement>('[data-bloco="rentabilidade"]')!;
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

  it('nenhum `title=` cru', async () => {
    abrir();
    await esperar();
    expect([...document.querySelectorAll('[title]')]).toEqual([]);
  });
});
