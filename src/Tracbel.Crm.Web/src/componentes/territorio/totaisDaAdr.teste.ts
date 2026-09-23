/**
 * As contas da ADR (issue 170, parte A).
 *
 * Elas saíram do corpo da tela para poderem ser provadas sem montar a página —
 * e o que se prova aqui são exatamente as duas decisões que um leitor desatento
 * inverteria: sigilo não é zero, e o denominador da fatia é o PUBLICADO.
 */

import { describe, expect, it } from 'vitest';
import { CAFELANDIA, ARARAQUARA, municipioDeTeste } from '../../testes/territorio';
import { calcularFatiaNoEstado, calcularTotais, diferencaParaASoma } from './totaisDaAdr';

const ESTADO = {
  ano: 2024,
  areaPlantadaHectares: 8_000_000,
  valorDaProducaoMilReais: 90_000_000,
  areaColhidaHectares: 7_900_000,
  tratores: { publicado: 120_000, somaDosMunicipios: 118_000 },
  estabelecimentos: { publicado: 180_000, somaDosMunicipios: 179_000 },
  anoDoCenso: 2017,
  rebanho: { publicado: 10_000_000, somaDosMunicipios: 9_900_000 },
  anoDoRebanho: 2024,
};

describe('os totais da ADR', () => {
  it('soma as medidas dos municípios da ADR', () => {
    const totais = calcularTotais([
      municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' }),
      municipioDeTeste({ codigoIbge: ARARAQUARA, nome: 'Araraquara' }),
    ]);

    expect(totais.elegiveis).toBe(36);
    expect(totais.vendas).toBe(2_500_000);
    expect(totais.tratores).toBe(844);
    expect(totais.municipiosComArea).toBe(2);
  });

  it('o município sob sigilo não conta como zero: ele fica fora da contagem de divulgados', () => {
    const totais = calcularTotais([
      municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' }),
      municipioDeTeste({
        codigoIbge: ARARAQUARA,
        nome: 'Araraquara',
        estrutura: {
          ...municipioDeTeste({ codigoIbge: ARARAQUARA, nome: 'Araraquara' }).estrutura,
          tratores: null,
        },
      }),
    ]);

    // A soma é o que o IBGE DIVULGOU, e o número de municípios que entraram fica
    // ao lado justamente para que a diferença seja visível.
    expect(totais.tratores).toBe(422);
    expect(totais.municipiosComTratores).toBe(1);
  });

  it('o selo de estimativa acende quando alguma regra usada aqui ainda não foi confirmada', () => {
    const confirmado = municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' });
    expect(calcularTotais([confirmado]).potencialEstimado).toBe(true);

    const semEstimativa = {
      ...confirmado,
      potencialEstrutural: { ...confirmado.potencialEstrutural!, estimativa: false },
    };
    expect(calcularTotais([semEstimativa]).potencialEstimado).toBe(false);
  });

  it('o parque só soma quem tem parque — município sem regra não entra como zero', () => {
    const semParque = municipioDeTeste({
      codigoIbge: ARARAQUARA,
      nome: 'Araraquara',
      potencialEstrutural: {
        parqueDeMaquinas: null,
        demandaAnualDeMaquinas: null,
        areaUtilHectares: null,
        estimativa: false,
        motivoSemParque: 'SemRegra',
        motivoSemDemanda: 'SemRegra',
      },
    });

    const totais = calcularTotais([municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' }), semParque]);
    expect(totais.maquinasTeoricas).toBe(1_240);
    expect(totais.municipiosComArea).toBe(1);
  });
});

describe('a fatia em São Paulo', () => {
  it('divide pelo total PUBLICADO, e não pela soma dos municípios', () => {
    const totais = calcularTotais([municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' })]);
    const fatia = calcularFatiaNoEstado(ESTADO, totais);

    // 422 / 120.000 publicados, e não 422 / 118.000 somados.
    expect(fatia!.tratores).toBeCloseTo((100 * 422) / 120_000, 6);
    expect(fatia!.ano).toBe(2024);
  });

  it('sem os totais do estado não há fatia — e não sai zero', () => {
    const totais = calcularTotais([municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' })]);
    expect(calcularFatiaNoEstado(null, totais)).toBeNull();
  });

  it('denominador zerado não vira divisão por zero: fica sem dado', () => {
    const totais = calcularTotais([municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' })]);
    const fatia = calcularFatiaNoEstado({ ...ESTADO, tratores: { publicado: 0, somaDosMunicipios: 0 } }, totais);
    expect(fatia!.tratores).toBeNull();
  });
});

describe('o quanto o sigilo esconde', () => {
  it('diz de quanto a soma dos municípios fica abaixo do publicado', () => {
    expect(diferencaParaASoma(ESTADO.tratores)).toContain('2.000 abaixo do publicado');
  });

  it('cala quando não há diferença, em vez de escrever "0 abaixo"', () => {
    expect(diferencaParaASoma({ publicado: 100, somaDosMunicipios: 100 })).toBe('');
  });

  it('cala quando a medida não foi carregada', () => {
    expect(diferencaParaASoma(undefined)).toBe('');
    expect(diferencaParaASoma({ publicado: 100, somaDosMunicipios: null })).toBe('');
  });
});
