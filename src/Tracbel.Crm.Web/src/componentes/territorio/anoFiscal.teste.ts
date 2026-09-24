/**
 * O CALENDÁRIO FISCAL DA TRACBEL — novembro a outubro (confirmado em 24/09/2026).
 *
 * O que está sob prova são as BORDAS, porque é só nelas que esta conta erra: em
 * novembro e dezembro o ano fiscal vai à frente do civil, e quem derivar o ano
 * fiscal do ano civil erra o rótulo inteiro. Nos outros dez meses os dois terminam
 * no mesmo lugar e a diferença é só onde começam.
 */

import { describe, expect, it } from 'vitest';
import { anoCivilFechado, anoFiscalFechado, nomeDoAnoFiscal } from './indicadoresDaAdr';

/** "Hoje" como data local, no dia 15 — longe das bordas do mês, que não são o assunto aqui. */
const em = (ano: number, mes: number) => new Date(ano, mes - 1, 15);

describe('o ano fiscal até o último mês fechado', () => {
  it('no meio do ano fiscal, começa no novembro anterior e vai até o último mês fechado', () => {
    // Setembro/2026: o último mês fechado é agosto, e o ano fiscal corrente abriu em nov/2025.
    expect(anoFiscalFechado(em(2026, 9))).toEqual({ competenciaInicial: '2025-11', competenciaFinal: '2026-08' });
  });

  it('EM NOVEMBRO o recorte é o ano fiscal que acabou de fechar, inteiro', () => {
    // O último mês fechado é outubro — a última competência do FY2026. São doze meses
    // cheios, e não um ano novo com um mês: novembro ainda não fechou.
    const recorte = anoFiscalFechado(em(2026, 11));
    expect(recorte).toEqual({ competenciaInicial: '2025-11', competenciaFinal: '2026-10' });
  });

  it('EM DEZEMBRO o ano fiscal já virou, e o civil não — é aqui que derivar um do outro erra', () => {
    // Último mês fechado: novembro/2026, que é o PRIMEIRO mês do FY2027.
    expect(anoFiscalFechado(em(2026, 12))).toEqual({ competenciaInicial: '2026-11', competenciaFinal: '2026-11' });
    // No mesmo dia, o ano civil ainda está em 2026, do começo.
    expect(anoCivilFechado(em(2026, 12))).toEqual({ competenciaInicial: '2026-01', competenciaFinal: '2026-11' });
  });

  it('EM JANEIRO o último mês fechado é dezembro, e o ano fiscal segue no do ano anterior', () => {
    expect(anoFiscalFechado(em(2027, 1))).toEqual({ competenciaInicial: '2026-11', competenciaFinal: '2026-12' });
  });

  it('os dois recortes NUNCA coincidem — é o que deixa a tela distinguir um do outro', () => {
    for (let mes = 1; mes <= 12; mes++) {
      const fiscal = anoFiscalFechado(em(2026, mes));
      const civil = anoCivilFechado(em(2026, mes));
      expect(fiscal.competenciaFinal, `mês ${mes}: terminam no mesmo mês fechado`).toBe(civil.competenciaFinal);
      expect(fiscal.competenciaInicial, `mês ${mes}: mas começam em meses diferentes`).not.toBe(civil.competenciaInicial);
    }
  });
});

describe('o nome do ano fiscal', () => {
  it('leva o nome do ano em que TERMINA, e não daquele em que começa', () => {
    expect(nomeDoAnoFiscal('2025-11')).toBe('FY2026');
    expect(nomeDoAnoFiscal('2025-12')).toBe('FY2026');
    expect(nomeDoAnoFiscal('2026-01')).toBe('FY2026');
    expect(nomeDoAnoFiscal('2026-10')).toBe('FY2026');
  });

  it('a virada é entre outubro e novembro, e não entre dezembro e janeiro', () => {
    expect(nomeDoAnoFiscal('2026-10')).toBe('FY2026');
    expect(nomeDoAnoFiscal('2026-11')).toBe('FY2027');
  });

  it('competência ilegível não vira um ano inventado', () => {
    expect(nomeDoAnoFiscal('')).toBeNull();
    expect(nomeDoAnoFiscal('2026-13')).toBeNull();
    expect(nomeDoAnoFiscal('sem-data')).toBeNull();
  });
});
