/**
 * A ABA RENTABILIDADE DO MOMENTO (fidelidade às maquetes, fase 3).
 *
 * O que está sob prova é o que a maquete pede e a regra não deixa errar: o
 * ranking ordena pelas quatro colunas; a "Média da Região Tracbel" é ponderada
 * pela área colhida DA REGIÃO (e não pela de São Paulo, que é a que a rota traz);
 * a "Cultura destaque" é a de maior área, com o critério dito; a tendência sai
 * vazia com o motivo; a tabela está aberta; e os painéis antigos de preço e
 * custo continuam a um clique.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import { municipioDeTeste } from '../../testes/territorio';
import type { RentabilidadeDaCultura } from '../../tipos/mercado';
import type { CulturaNoCatalogo } from '../../tipos/potencial';
import type { IndicadoresDoMunicipio, PotencialTerritorial } from '../../tipos/territorio';
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
vi.mock('../territorio/PainelDoPrecoImplicito', () => ({
  PainelDoPrecoImplicito: ({ municipioCodigoIbge }: { municipioCodigoIbge?: number | null }) => (
    <div data-bloco="preco-implicito" data-municipio={municipioCodigoIbge ?? 'recorte'} />
  ),
}));

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

function colhida(areas: Array<[number, number]>): PotencialTerritorial[] {
  return areas.map(([produtoCodigoIbge, areaColhidaHectares]) => ({
    produtoCodigoIbge,
    areaPlantadaHectares: areaColhidaHectares,
    maquinasTeoricas: null,
    areaColhidaHectares,
    valorDaProducaoMilReais: null,
    ano: 2024,
    quantidadeProduzida: null,
    unidadeDaQuantidade: null,
    produtividade: null,
    unidadeDaProdutividade: null,
  }));
}

// NA REGIÃO TRACBEL: cana 90.000 ha, café 10.000 ha, laranja sem área divulgada.
const MUNICIPIOS: IndicadoresDoMunicipio[] = [
  municipioDeTeste({ codigoIbge: 1, nome: 'A', potencial: colhida([[PAM_CANA, 60_000], [PAM_CAFE, 10_000]]) }),
  municipioDeTeste({ codigoIbge: 2, nome: 'B', potencial: colhida([[PAM_CANA, 30_000]]) }),
  // FORA DA REGIÃO: um vizinho cheio de café não pode virar o destaque daqui.
  municipioDeTeste({ codigoIbge: 3, nome: 'C', pertenceAAdr: false, potencial: colhida([[PAM_CAFE, 900_000]]) }),
];

function abrir({
  catalogoFalha = false,
  nomeDoMunicipio = null as string | null,
  municipioCodigoIbge = null as number | null,
} = {}) {
  obterRentabilidadeDasCulturas.mockResolvedValue({ dados: LINHAS, procedencia: null });
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
        municipios={MUNICIPIOS}
        nomeDoMunicipio={nomeDoMunicipio}
        municipioCodigoIbge={municipioCodigoIbge}
      />
    </ProvedorDeContextoDeAcesso>,
  );
}

const cartao = (rotulo: string) => document.querySelector<HTMLElement>(`[data-cartao="${rotulo}"]`)!;
const ranking = () =>
  [...document.querySelectorAll<HTMLElement>('.mom-ranking .mom-ranking-linha')].map((l) => l.dataset.cultura);

function lerDica(rotulo: string, dentroDe: HTMLElement = document.body): string {
  const gatilho = within(dentroDe).getByRole('button', { name: rotulo });
  fireEvent.focus(gatilho);
  const texto = screen.getByRole('tooltip').textContent ?? '';
  fireEvent.blur(gatilho);
  return texto;
}

const esperar = () => screen.findByText('Melhor margem/ha');

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
    expect(lerDica('O que é a margem média da Região Tracbel')).toMatch(/2 culturas que têm margem e área/);
  });

  it('a cultura destaque é a de maior área colhida na Região — e o critério está dito', async () => {
    abrir();
    await screen.findByText('R$ 5.322,00');

    const destaque = cartao('Cultura destaque');
    expect(destaque).toHaveTextContent('Cana-de-açúcar');
    expect(destaque).toHaveTextContent('R$ 2.580,00 / ha');
    expect(lerDica('O que é a cultura destaque')).toMatch(/MAIOR ÁREA COLHIDA/);
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

    const cana = tabela.querySelector<HTMLElement>('tr[data-cultura="CANA"]')!;
    expect(cana).toHaveTextContent('90.000');
    expect(cana).toHaveTextContent('22%');
    // A laranja não tem área divulgada na Região: traço, e não zero.
    const laranja = tabela.querySelector<HTMLElement>('tr[data-cultura="LARANJA"]')!;
    expect(within(laranja).getByRole('button', { name: 'Por que a área colhida de Laranja não aparece' })).toBeInTheDocument();
  });

  it('sem o catálogo, área, destaque e média saem com o motivo — nada é estimado', async () => {
    abrir({ catalogoFalha: true });
    await esperar();
    await screen.findAllByRole('button', { name: /Por que a área colhida de/ });

    expect(cartao('Média da Região Tracbel').querySelector('.mom-cartao-valor')!.textContent).not.toMatch(/\d/);
    expect(lerDica('Por que a margem média da Região Tracbel não aparece')).toMatch(/catálogo de culturas/);
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

    const cafe = document.querySelector<HTMLElement>('tr[data-cultura="CAFE"]')!;
    const dica = lerDica('Como a margem de Café (Total) se compõe', cafe);
    expect(dica).toMatch(/PAM 2024/);
    expect(dica).toMatch(/Franca/);
    expect(dica).toMatch(/Área colhida em SP/);
  });

  it('os painéis de preço e custo continuam a um clique, em "Ver séries"', async () => {
    abrir();
    await esperar();

    expect(document.querySelector('[data-bloco="precos"]')).toBeNull();
    fireEvent.click(screen.getByRole('button', { name: 'Ver séries de preço e custo' }));
    expect(document.querySelector('[data-bloco="precos"]')).not.toBeNull();
    expect(document.querySelector('[data-bloco="custos"]')).not.toBeNull();
  });

  it('o preço recebido pelo produtor (PAM, issue 198) abre junto das séries, entre preço e custo, no município escolhido', async () => {
    abrir({ nomeDoMunicipio: 'Cafelândia', municipioCodigoIbge: 3508702 });
    await esperar();

    expect(document.querySelector('[data-bloco="preco-implicito"]')).toBeNull();
    fireEvent.click(screen.getByRole('button', { name: 'Ver séries de preço e custo' }));

    // DUAS SÉRIES DE PREÇO, DOIS PAINÉIS: a da CONAB e a do IBGE ficam lado a lado e nunca se emendam.
    const series = document.querySelector<HTMLElement>('[data-bloco="rentabilidade-fontes"]')!;
    expect([...series.children].map((n) => (n as HTMLElement).dataset.bloco)).toEqual(['precos', 'preco-implicito', 'custos']);
    // A DA PAM É MUNICIPAL: ela recebe o município escolhido, ao contrário das da CONAB.
    expect(series.querySelector('[data-bloco="preco-implicito"]')).toHaveAttribute('data-municipio', '3508702');
  });

  it('nenhum `title=` cru', async () => {
    abrir();
    await esperar();
    expect([...document.querySelectorAll('[title]')]).toEqual([]);
  });
});
