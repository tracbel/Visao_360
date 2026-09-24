/**
 * A PRIORIZAÇÃO DAS CULTURAS PELO MUNICÍPIO (issue 168).
 *
 * O que está sob prova é a regra que separa "reagir ao recorte" de "inventar
 * dado municipal": o município muda a ORDEM das culturas, e nunca o número —
 * preço e custo continuam sendo os de São Paulo e os da localidade da CONAB.
 */

import { describe, expect, it } from 'vitest';
import { municipioDeTeste, CAFELANDIA } from '../../testes/territorio';
import type { CulturaNoCatalogo } from '../../tipos/potencial';
import type { PotencialTerritorial } from '../../tipos/territorio';
import { areaColhidaNoRecorte, comAsCulturasDoMunicipioPrimeiro, produtosDoMunicipio } from './culturasDoMunicipio';

const CAFE = 2502;
const CANA = 2503;
const LARANJA = 2504;

function cultura(codigo: string, produtos: number[]): CulturaNoCatalogo {
  return {
    codigo,
    nome: codigo,
    segmento: 'lavoura',
    unidadeComercial: 'sc',
    quilosPorUnidade: 60,
    fonteDoPreco: 'CONAB',
    produtoDoPreco: codigo,
    serieDeCusto: codigo,
    estaAtiva: true,
    produtos: produtos.map((codigoIbge) => ({ codigoIbge, nome: String(codigoIbge), entraNaSomaDaLavoura: true })),
  };
}

const CATALOGO = [cultura('CAFE', [CAFE]), cultura('CANA', [CANA]), cultura('LARANJA', [LARANJA])];

function comArea(areas: Array<[number, number | null]>): PotencialTerritorial[] {
  return areas.map(([produtoCodigoIbge, areaPlantadaHectares]) => ({
    produtoCodigoIbge,
    areaPlantadaHectares,
    maquinasTeoricas: null,
    areaColhidaHectares: null,
    valorDaProducaoMilReais: null,
    ano: 2024,
    quantidadeProduzida: null,
    unidadeDaQuantidade: null,
    produtividade: null,
    unidadeDaProdutividade: null,
  }));
}

describe('os produtos do município', () => {
  it('saem do maior para o menor em área plantada', () => {
    const m = municipioDeTeste({
      codigoIbge: CAFELANDIA,
      nome: 'Cafelândia',
      potencial: comArea([
        [CAFE, 1_000],
        [CANA, 30_000],
        [LARANJA, 5_000],
      ]),
    });

    expect(produtosDoMunicipio(m)).toEqual([CANA, LARANJA, CAFE]);
  });

  it('cultura sem área divulgada fica de fora — sigilo não entra como zero', () => {
    const m = municipioDeTeste({
      codigoIbge: CAFELANDIA,
      nome: 'Cafelândia',
      potencial: comArea([
        [CAFE, null],
        [CANA, 30_000],
        [LARANJA, 0],
      ]),
    });

    expect(produtosDoMunicipio(m)).toEqual([CANA]);
  });

  it('sem município escolhido, não há priorização', () => {
    expect(produtosDoMunicipio(null)).toEqual([]);
  });
});

describe('o catálogo com as culturas do município primeiro', () => {
  it('traz as do município na ordem da área, e o resto atrás', () => {
    const ordenado = comAsCulturasDoMunicipioPrimeiro(CATALOGO, [CANA, CAFE]);
    expect(ordenado.map((c) => c.codigo)).toEqual(['CANA', 'CAFE', 'LARANJA']);
  });

  it('sem município, o catálogo fica exatamente como o negócio decidiu (issue 165)', () => {
    expect(comAsCulturasDoMunicipioPrimeiro(CATALOGO, []).map((c) => c.codigo)).toEqual(['CAFE', 'CANA', 'LARANJA']);
  });

  it('não perde nem duplica cultura nenhuma', () => {
    const ordenado = comAsCulturasDoMunicipioPrimeiro(CATALOGO, [LARANJA]);
    expect(ordenado).toHaveLength(CATALOGO.length);
    expect(new Set(ordenado.map((c) => c.codigo)).size).toBe(CATALOGO.length);
  });

  it('cultura com vários produtos da PAM entra pela melhor posição de qualquer um deles', () => {
    // O café tem "Total", "Arábica" e "Canephora": se o município planta
    // qualquer um, a cultura é dele.
    const cafeComTres = cultura('CAFE', [CAFE, 9001, 9002]);
    const ordenado = comAsCulturasDoMunicipioPrimeiro([cultura('CANA', [CANA]), cafeComTres], [9002]);
    expect(ordenado.map((c) => c.codigo)).toEqual(['CAFE', 'CANA']);
  });
});

/**
 * A ÁREA COLHIDA DA REGIÃO TRACBEL (fidelidade às maquetes, fase 3) — o peso da
 * "Média da Região Tracbel" e o critério da "Cultura destaque".
 *
 * A ROTA DE RENTABILIDADE TRAZ A ÁREA DE SÃO PAULO, e é por isso que esta soma
 * existe: pesar a margem pela área do estado e chamar de "Região Tracbel" seria
 * o nome errado num número certo.
 */
describe('a área colhida de uma cultura na Região Tracbel', () => {
  function colhida(areas: Array<[number, number | null]>): PotencialTerritorial[] {
    return comArea(areas.map(([p]) => [p, 1])).map((p, i) => ({ ...p, areaColhidaHectares: areas[i][1] }));
  }

  const municipios = [
    municipioDeTeste({ codigoIbge: 1, nome: 'A', potencial: colhida([[CANA, 30_000], [CAFE, 1_000]]) }),
    municipioDeTeste({ codigoIbge: 2, nome: 'B', potencial: colhida([[CANA, 12_000]]) }),
    // FORA DA ADR: um vizinho grande não entra na área da Região Tracbel.
    municipioDeTeste({ codigoIbge: 3, nome: 'C', pertenceAAdr: false, potencial: colhida([[CANA, 900_000]]) }),
  ];

  it('soma os municípios da ADR, e só eles', () => {
    expect(areaColhidaNoRecorte(cultura('CANA', [CANA]), municipios)).toBe(42_000);
  });

  it('sem área divulgada em nenhum município, é ausência — e não zero', () => {
    expect(areaColhidaNoRecorte(cultura('LARANJA', [LARANJA]), municipios)).toBeNull();

    const soSigilo = [municipioDeTeste({ codigoIbge: 4, nome: 'D', potencial: colhida([[LARANJA, null]]) })];
    expect(areaColhidaNoRecorte(cultura('LARANJA', [LARANJA]), soSigilo)).toBeNull();
  });

  it('o café não conta a mesma terra duas vezes: só entram os produtos que somam na lavoura', () => {
    const cafe: CulturaNoCatalogo = {
      ...cultura('CAFE', []),
      produtos: [
        { codigoIbge: CAFE, nome: 'Café (Total)', entraNaSomaDaLavoura: true },
        { codigoIbge: 9001, nome: 'Café Arábica', entraNaSomaDaLavoura: false },
      ],
    };
    const comDetalhe = [
      municipioDeTeste({ codigoIbge: 5, nome: 'E', potencial: colhida([[CAFE, 1_000], [9001, 800]]) }),
    ];
    expect(areaColhidaNoRecorte(cafe, comDetalhe)).toBe(1_000);
  });
});
