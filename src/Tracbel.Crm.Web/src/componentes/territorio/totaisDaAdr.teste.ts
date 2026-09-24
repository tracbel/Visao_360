/**
 * As contas da ADR (issue 170, parte A).
 *
 * Elas saíram do corpo da tela para poderem ser provadas sem montar a página —
 * e o que se prova aqui são exatamente as duas decisões que um leitor desatento
 * inverteria: sigilo não é zero, e o denominador da fatia é o PUBLICADO.
 */

import { describe, expect, it } from 'vitest';
import { CAFELANDIA, ARARAQUARA, municipioDeTeste } from '../../testes/territorio';
import type { IndicadoresForaDoMapa } from '../../tipos/territorio';
import { calcularFatiaNoEstado, calcularTotais, conferenciaDaConsulta, diferencaParaASoma } from './totaisDaAdr';

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

/**
 * A CONFERÊNCIA DO DOCUMENTO 32, SEM LINHAS NA TABELA (fase 4).
 *
 * As linhas de total saíram do fim da tabela e foram para a dica do título; a
 * afirmação que elas carregavam — a soma das linhas é o total da consulta —
 * continua provada, agora sobre o dado.
 */
describe('a conferência da consulta', () => {
  const daAdr = [
    municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' }),
    municipioDeTeste({ codigoIbge: 3500105, nome: 'Adamantina', vendas: { ...municipioDeTeste({ codigoIbge: 1, nome: 'x' }).vendas, valorLiquido: 750_000, posVenda: 50_000 } }),
  ];
  const foraDaAdr = municipioDeTeste({ codigoIbge: ARARAQUARA, nome: 'Araraquara', pertenceAAdr: false });
  const foraDoMapa: IndicadoresForaDoMapa[] = [
    {
      grupo: 'MunicipioSemCodigoIbge',
      descricao: 'clientes cujo município não tem código IBGE',
      cobertura: { clientes: 1, vinculos: 1, vinculosComCadencia: 2, cobertos: 1, foraDaCadencia: 1, nuncaContatados: 0, semCadencia: 0, pendentes: 1, percentualPendente: 50 },
      vendas: { clientesQueCompraram: 1, valorLiquido: 10_000, maquina: 0, peca: 6_000, servico: 4_000, outros: 0, posVenda: 10_000 },
    },
  ];
  const municipios = [...daAdr, foraDaAdr];
  const totais = calcularTotais(daAdr);
  const somar = (linhas: { elegiveis: number; cobertos: number; pendentes: number; vendas: number; posVenda: number }[]) => ({
    elegiveis: linhas.reduce((s, l) => s + l.elegiveis, 0),
    cobertos: linhas.reduce((s, l) => s + l.cobertos, 0),
    pendentes: linhas.reduce((s, l) => s + l.pendentes, 0),
    vendas: linhas.reduce((s, l) => s + l.vendas, 0),
    posVenda: linhas.reduce((s, l) => s + l.posVenda, 0),
  });

  it('a soma das linhas da ADR é o Total da ADR', () => {
    const c = conferenciaDaConsulta({ municipios, daAdr, foraDoMapa, totais, semFiltro: true, territorioNaoCarregado: false });
    const linhas = somar(
      daAdr.map((m) => ({
        elegiveis: m.cobertura.vinculosComCadencia,
        cobertos: m.cobertura.cobertos,
        pendentes: m.cobertura.pendentes,
        vendas: m.vendas.valorLiquido,
        posVenda: m.vendas.posVenda,
      })),
    );
    expect(c.adr).toMatchObject({ rotulo: 'Total da ADR', ...linhas, maquinas: 2_480 });
  });

  it('ADR + São Paulo fora da ADR + fora do mapa = Total da consulta', () => {
    const c = conferenciaDaConsulta({ municipios, daAdr, foraDoMapa, totais, semFiltro: true, territorioNaoCarregado: false });

    expect(c.parcelas.map((p) => p.rotulo)).toEqual(['São Paulo fora da ADR', 'MunicipioSemCodigoIbge']);
    const soma = somar([c.adr!, ...c.parcelas]);
    expect(c.consulta).toMatchObject({ rotulo: 'Total da consulta', ...soma });
    // 18 + 18 (ADR) + 18 (fora da ADR) + 2 (fora do mapa).
    expect(c.consulta!.elegiveis).toBe(56);
    // Os grupos de fora não têm parque somado — e não aparecem com zero.
    expect(c.parcelas.every((p) => p.maquinas === null)).toBe(true);
  });

  it('com filtro, os grupos de fora da ADR e o total da consulta não entram — seriam de outro recorte', () => {
    const c = conferenciaDaConsulta({ municipios, daAdr, foraDoMapa, totais, semFiltro: false, territorioNaoCarregado: false });

    expect(c.adr!.rotulo).toBe('Total da ADR (filtro)');
    expect(c.parcelas.map((p) => p.rotulo)).toEqual(['MunicipioSemCodigoIbge']);
    expect(c.consulta).toBeNull();
  });

  it('sem nenhum município com parque, a ADR fica SEM máquinas teóricas — e não com "0 máquinas"', () => {
    // O DEFEITO (revisão de 24/09/2026): a soma vazia saía 0, que afirma um
    // parque zerado onde o que falta é o número (sem regra, sem área).
    const semParque = daAdr.map((m) => ({
      ...m,
      potencialEstrutural: { ...m.potencialEstrutural!, parqueDeMaquinas: null, motivoSemParque: 'SemRegra' as const },
    }));
    const c = conferenciaDaConsulta({
      municipios: [...semParque, foraDaAdr],
      daAdr: semParque,
      foraDoMapa,
      totais: calcularTotais(semParque),
      semFiltro: true,
      territorioNaoCarregado: false,
    });
    expect(c.adr!.maquinas).toBeNull();
  });

  it('território não carregado: sem linha da ADR (e não zeros), mas o total da consulta continua', () => {
    const c = conferenciaDaConsulta({
      municipios: [foraDaAdr],
      daAdr: [],
      foraDoMapa,
      totais: calcularTotais([]),
      semFiltro: true,
      territorioNaoCarregado: true,
    });

    expect(c.adr).toBeNull();
    expect(c.parcelas.map((p) => p.rotulo)).toEqual(['MunicipioSemCodigoIbge']);
    expect(c.consulta!.elegiveis).toBe(20);
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
