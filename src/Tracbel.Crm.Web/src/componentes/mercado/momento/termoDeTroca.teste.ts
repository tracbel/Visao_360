/**
 * A CONTA DO TERMO DE TROCA (issue 70): o mesmo mês nas duas séries, a variação só com os dois meses, e o poder de
 * compra com o sinal do produtor.
 */

import { describe, expect, it } from 'vitest';
import type { SerieDePreco, SerieDePrecoDeMaquina } from '../../../tipos/mercado';
import { mediaDoTermo, mesesAntes, poderDeCompra, serieDoTermo, tratorBase, variacaoDoTermo } from './termoDeTroca';

const safra: SerieDePreco = {
  fonte: 'CONAB',
  codigoNaFonte: 'SOJA',
  nivel: 'RECEBIDO PELO PRODUTOR',
  produto: 'SOJA',
  classificacao: 'EM GRÃOS',
  unidade: 'kg',
  unidadeComercial: 'saca de 60 kg',
  fatorComercial: 60,
  meses: [
    { mes: '2026-05-01', valorEmReais: 2, valorEmDolares: null },
    { mes: '2026-06-01', valorEmReais: 2, valorEmDolares: null },
    { mes: '2026-08-01', valorEmReais: 2.5, valorEmDolares: null },
  ],
  procedencia: null,
};

const trator: SerieDePrecoDeMaquina = {
  categoriaCodigo: 'TRATOR',
  categoriaNome: 'Trator',
  meses: [
    { mes: '2026-05-01', mediana: 480_000, menor: 480_000, maior: 480_000, notas: 1 },
    { mes: '2026-07-01', mediana: 500_000, menor: 500_000, maior: 500_000, notas: 1 },
    { mes: '2026-08-01', mediana: 450_000, menor: 450_000, maior: 450_000, notas: 2 },
  ],
  procedencia: null,
};

describe('o termo de troca', () => {
  it('só os meses que as duas séries têm: junho não tem trator, julho não tem safra', () => {
    const termo = serieDoTermo(safra, trator);
    expect(termo.map((t) => t.mes)).toEqual(['2026-05-01', '2026-08-01']);
    expect(termo[0].unidades).toBe(4_000);
    expect(termo[1].unidades).toBe(3_000);
    expect(mediaDoTermo(termo)).toBe(3_500);
  });

  it('a variação precisa do mesmo mês antes — maio é três meses antes de agosto', () => {
    const termo = serieDoTermo(safra, trator);
    expect(variacaoDoTermo(termo, 3)).toBeCloseTo(-0.25);
    expect(variacaoDoTermo(termo, 12)).toBeNull();
  });

  it('precisar de 25% menos sacas é comprar um terço a mais de trator', () => {
    expect(poderDeCompra(-0.25)).toBeCloseTo(1 / 3);
    expect(poderDeCompra(null)).toBeNull();
  });

  it('os meses antes atravessam o ano', () => {
    expect(mesesAntes('2026-02-01', 3)).toBe('2025-11-01');
    expect(mesesAntes('2026-08-01', 60)).toBe('2021-08-01');
  });

  it('o trator base é o TRATOR, e sem ele não há termo', () => {
    expect(tratorBase([{ ...trator, categoriaCodigo: 'COLHEITADEIRA' }])).toBeNull();
    expect(tratorBase([trator])?.categoriaCodigo).toBe('TRATOR');
    expect(tratorBase(undefined)).toBeNull();
  });
});
