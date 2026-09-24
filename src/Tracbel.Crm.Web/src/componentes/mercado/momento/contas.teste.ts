/**
 * AS CONTAS DE TELA DO MOMENTO (fidelidade às maquetes, fase 3).
 *
 * São poucas e cada uma tem regra: ordenar o ranking, ponderar a margem pela
 * área, escolher a cultura de maior área, montar o top 5 do crédito e dizer de
 * quem é o nome da coluna "Gestor". É justamente numa conta de tela que um
 * número plausível e errado nasce sem ninguém ver — por isso cada uma tem prova.
 */

import { describe, expect, it } from 'vitest';
import type { CreditoDeMaquinasNoMunicipio, RentabilidadeDaCultura } from '../../../tipos/mercado';
import {
  daMaiorArea,
  distribuicaoDaPercepcao,
  margemMediaPonderada,
  margemPercentual,
  ordenarMunicipios,
  ordenarRentabilidade,
  participacao,
  responsavelPrincipal,
  topDaRegiao,
  valorMedioPorLinha,
  variacao,
} from './contas';
import { numero, percentualComSinal, pontosPercentuais, sentido } from './formatos';

function cultura(
  codigo: string,
  receita: number | null,
  custo: number | null,
  area: number | null = null,
): RentabilidadeDaCultura {
  const margem = receita !== null && custo !== null ? receita - custo : null;
  return {
    culturaCodigo: codigo,
    culturaNome: codigo,
    unidadeComercial: 'saca de 60 kg',
    anoDaProdutividade: 2024,
    produtividadeKgPorHa: 1_000,
    precoMedioPorKg: 1,
    mesesDePrecoNaMedia: 12,
    receitaPorHectare: receita,
    localDoCusto: 'Local',
    camadaDoCusto: 'Total',
    safraDoCusto: 2025,
    custoPorHectare: custo,
    margemPorHectare: margem,
    margemPorUnidade: null,
    areaColhidaHectares: area,
    margemTotal: null,
    motivo: margem === null ? 'SemCusto' : 'Nenhum',
    fraseDoMotivo: margem === null ? 'sem custo' : '',
  };
}

// CAFÉ: margem 30.000 (58%); CANA: 2.580 (22%); LARANJA: −1.800; MILHO: sem custo.
const CAFE = cultura('CAFE', 51_450, 21_450);
const CANA = cultura('CANA', 11_480, 8_900);
const LARANJA = cultura('LARANJA', 53_200, 55_000);
const MILHO = cultura('MILHO', 7_360, null);
const TODAS = [CANA, MILHO, LARANJA, CAFE];

describe('o ranking de culturas da Rentabilidade', () => {
  it('ordena pela margem por hectare, do maior para o menor, com a negativa embaixo', () => {
    expect(ordenarRentabilidade(TODAS, 'margem').map((l) => l.culturaCodigo)).toEqual([
      'CAFE',
      'CANA',
      'LARANJA',
      'MILHO',
    ]);
  });

  it('cada critério do "Ordenar por" muda a ordem — receita, custo e margem %', () => {
    expect(ordenarRentabilidade(TODAS, 'receita').map((l) => l.culturaCodigo)).toEqual(['LARANJA', 'CAFE', 'CANA', 'MILHO']);
    expect(ordenarRentabilidade(TODAS, 'custo').map((l) => l.culturaCodigo)).toEqual(['LARANJA', 'CAFE', 'CANA', 'MILHO']);
    expect(ordenarRentabilidade(TODAS, 'margemPercentual').map((l) => l.culturaCodigo)).toEqual([
      'CAFE',
      'CANA',
      'LARANJA',
      'MILHO',
    ]);
  });

  it('quem não tem o número vai para o fim, e não some', () => {
    const ordenado = ordenarRentabilidade(TODAS, 'custo');
    expect(ordenado).toHaveLength(TODAS.length);
    expect(ordenado.at(-1)!.culturaCodigo).toBe('MILHO');
  });

  it('a margem % é margem ÷ receita, e sem receita positiva é ausência', () => {
    expect(margemPercentual(CANA)).toBeCloseTo(2_580 / 11_480, 6);
    expect(margemPercentual(MILHO)).toBeNull();
    expect(margemPercentual(cultura('X', 0, 10))).toBeNull();
  });
});

describe('a margem média da Região Tracbel', () => {
  it('é PONDERADA pela área: Σ(margem × área) ÷ Σ área', () => {
    // 30.000 × 10.000 + 2.580 × 90.000 = 532.200.000 ÷ 100.000 = 5.322
    const { media, areaTotal, culturas } = margemMediaPonderada([
      { margem: 30_000, area: 10_000 },
      { margem: 2_580, area: 90_000 },
    ]);
    expect(media).toBeCloseTo(5_322, 6);
    expect(areaTotal).toBe(100_000);
    expect(culturas).toBe(2);

    // A MÉDIA SIMPLES daria 16.290 — três vezes mais, e errado.
    expect(media).not.toBeCloseTo((30_000 + 2_580) / 2, 0);
  });

  it('cultura sem margem ou sem área não pesa — nem como zero', () => {
    const { media, culturas } = margemMediaPonderada([
      { margem: 1_000, area: 50 },
      { margem: null, area: 1_000_000 },
      { margem: 999_999, area: null },
      { margem: 999_999, area: 0 },
    ]);
    expect(media).toBe(1_000);
    expect(culturas).toBe(1);
  });

  it('sem nenhuma cultura com os dois números, a média é ausência', () => {
    expect(margemMediaPonderada([{ margem: 10, area: null }]).media).toBeNull();
    expect(margemMediaPonderada([]).media).toBeNull();
  });
});

describe('a cultura destaque', () => {
  it('é a de maior área colhida, e não a de maior margem', () => {
    const areas: Record<string, number | null> = { CAFE: 12_000, CANA: 280_000, LARANJA: null, MILHO: 70_000 };
    expect(daMaiorArea(TODAS, (l) => areas[l.culturaCodigo])?.culturaCodigo).toBe('CANA');
  });

  it('sem área nenhuma, não há destaque', () => {
    expect(daMaiorArea(TODAS, () => null)).toBeNull();
  });
});

function municipio(codigo: number, nome: string, valor: number, linhas: number, adr = true): CreditoDeMaquinasNoMunicipio {
  return {
    codigoIbge: codigo,
    nome,
    pertenceAAdr: adr,
    janelas: { linhas, valor, linhasAnteriores: Math.round(linhas * 0.8), valorAnterior: Math.round(valor * 0.9) },
    indice: null,
  };
}

describe('o crédito por município', () => {
  const MUNICIPIOS = [
    municipio(1, 'Borborema', 14_800_000, 48),
    municipio(2, 'Jaboticabal', 13_100_000, 60),
    municipio(3, 'Vizinho grande', 90_000_000, 400, false),
    municipio(4, 'Tupã', 12_900_000, 41),
    municipio(5, 'Guariba', 7_600_000, 28),
    municipio(6, 'Araraquara', 6_400_000, 24),
    municipio(7, 'Ibitinga', 1_000_000, 90),
  ];

  it('o top 5 é só da Região Tracbel — o vizinho grande de fora não entra', () => {
    const top = topDaRegiao(MUNICIPIOS, 'valor');
    expect(top.map((m) => m.nome)).toEqual(['Borborema', 'Jaboticabal', 'Tupã', 'Guariba', 'Araraquara']);
    expect(top.some((m) => m.nome === 'Vizinho grande')).toBe(false);
  });

  it('o top 5 por linhas do SICOR tem outra ordem', () => {
    expect(topDaRegiao(MUNICIPIOS, 'linhas').map((m) => m.nome)).toEqual([
      'Ibitinga',
      'Jaboticabal',
      'Borborema',
      'Tupã',
      'Guariba',
    ]);
  });

  it('a participação é a fatia no total da Região; sem total, ausência', () => {
    expect(participacao(14_800_000, 186_400_000)).toBeCloseTo(0.0794, 4);
    expect(participacao(10, 0)).toBeNull();
  });

  it('o detalhamento ordena por qualquer coluna, e o nome em ordem alfabética', () => {
    expect(ordenarMunicipios(MUNICIPIOS, 'nome').map((m) => m.nome)[0]).toBe('Araraquara');
    // Tupã: 12,9 mi ÷ 41 linhas ≈ 314 mil — o maior valor médio, mesmo não sendo o maior valor.
    expect(ordenarMunicipios(MUNICIPIOS, 'valorMedio')[0].nome).toBe('Tupã');
  });

  it('valor médio é valor ÷ LINHAS, e a variação sem base anterior é ausência', () => {
    expect(valorMedioPorLinha({ linhas: 4, valor: 1_000, linhasAnteriores: 0, valorAnterior: 0 })).toBe(250);
    expect(valorMedioPorLinha({ linhas: 0, valor: 0, linhasAnteriores: 0, valorAnterior: 0 })).toBeNull();
    expect(variacao(114, 100)).toBeCloseTo(0.14, 6);
    expect(variacao(10, 0)).toBeNull();
  });
});

describe('a percepção comercial', () => {
  it('conta os municípios pelo SINAL da leitura, e os da Região sem leitura', () => {
    const d = distribuicaoDaPercepcao([{ percentual: 3 }, { percentual: 0 }, { percentual: -1.5 }, { percentual: 2 }], 10);
    expect(d).toEqual({ positiva: 2, neutra: 1, negativa: 1, semRegistro: 6, total: 10 });
  });

  it('o gestor é o responsável da carteira com mais vínculos — e sem cadastro, ninguém', () => {
    expect(
      responsavelPrincipal([
        { nome: 'Responsável B', natureza: 'CEN', vinculos: 3, carteiras: 1 },
        { nome: 'Responsável A', natureza: 'CEN', vinculos: 9, carteiras: 2 },
      ]),
    ).toEqual({ nome: 'Responsável A', outros: 1 });

    // NENHUM NOME É INVENTADO: sem responsável cadastrado, a resposta é nula.
    expect(responsavelPrincipal([])).toBeNull();
    expect(responsavelPrincipal([{ nome: '  ', natureza: 'CEN', vinculos: 5, carteiras: 1 }])).toBeNull();
  });
});

describe('os formatos do Momento', () => {
  it('o sinal é escrito sempre, e zero não tem sinal', () => {
    expect(percentualComSinal(0.12)).toBe('+12%');
    expect(percentualComSinal(-0.08)).toBe('-8%');
    expect(percentualComSinal(-0.0004)).toBe('0%');
    expect(pontosPercentuais(2)).toBe('+2 p.p.');
  });

  it('"-0" é ruído de arredondamento: o que arredonda para zero sai sem sinal (revisão de 24/09/2026)', () => {
    // A margem % de uma cultura que quase empata (−0,3%) saía "-0%" e lia como prejuízo.
    expect(numero(-0.3, 0)).toBe('0');
    expect(numero(-0.004, 2)).toBe('0,00');
    expect(numero(-1.4, 0)).toBe('-1');
    expect(pontosPercentuais(-0.04)).toBe('0 p.p.');
    expect(pontosPercentuais(0.04)).toBe('0 p.p.');
    expect(pontosPercentuais(-0.5)).toBe('-0,5 p.p.');
  });

  it('o sentido segue a régua da composição: perto de zero é →', () => {
    expect(sentido(0.2)).toBe('↑');
    expect(sentido(-0.2)).toBe('↓');
    expect(sentido(0.001)).toBe('→');
    expect(sentido(null)).toBeNull();
  });
});
