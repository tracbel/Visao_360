/**
 * OS FORMATOS ÚNICOS (documento 54 §3.5): cada um era uma cópia idêntica em duas ou mais telas e passou a morar aqui. O
 * texto tem de ser o de antes, letra por letra — inclusive o espaço inseparável que o `toLocaleString` põe na moeda.
 */

import { describe, expect, it } from 'vitest';
import {
  DIAS_DA_SEMANA_CURTOS,
  MESES_CURTOS,
  formatarDiaDaSemanaEHora,
  formatarDiaEHora,
  formatarMesComAno,
  formatarMesCurto,
  formatarMesNumerico,
  formatarNumeroComCasas,
  formatarPercentualDaParte,
  formatarReaisCurtos,
} from './formatadores';

describe('formatadores únicos', () => {
  it('número com no máximo N casas', () => {
    expect(formatarNumeroComCasas(1234.567, 1)).toBe('1.234,6');
    expect(formatarNumeroComCasas(1234.567, 0)).toBe('1.235');
    expect(formatarNumeroComCasas(2, 1)).toBe('2');
  });

  it('o percentual inteiro de uma parte, e travessão sem denominador', () => {
    expect(formatarPercentualDaParte(1, 3)).toBe('33%');
    expect(formatarPercentualDaParte(2, 3)).toBe('67%');
    expect(formatarPercentualDaParte(5, 0)).toBe('—');
  });

  it('reais curtos: mi, mil e, abaixo de mil, a moeda inteira', () => {
    expect(formatarReaisCurtos(2_500_000)).toBe('R$ 2,5 mi');
    expect(formatarReaisCurtos(12_000)).toBe('R$ 12 mil');
    expect(formatarReaisCurtos(-12_000)).toBe('R$ -12 mil');
    expect(formatarReaisCurtos(950)).toBe('R$ 950');
  });

  it('o dia da semana, o dia, o mês curto e a hora, no horário de quem lê', () => {
    expect(formatarDiaDaSemanaEHora('2026-10-03T21:07:00')).toBe('sáb · 03/out · 21:07');
  });

  it('o instante da API sem fuso é UTC, como com o Z', () => {
    expect(formatarDiaEHora('2026-09-24T16:27:48')).toBe(formatarDiaEHora('2026-09-24T16:27:48Z'));
    expect(formatarDiaEHora('2026-09-24T16:27:48Z')).toMatch(/^\d{2}\/\d{2},? \d{2}:\d{2}$/);
  });

  it('a competência como mês curto, numérico e com o ano inteiro', () => {
    expect(formatarMesCurto('2026-09-01')).toBe('set/26');
    expect(formatarMesNumerico('2026-09-01')).toBe('09/2026');
    expect(formatarMesComAno('2026-09-01')).toBe('set/2026');
  });

  it('os nomes curtos dos meses e dos dias', () => {
    expect(MESES_CURTOS).toEqual(['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez']);
    expect(DIAS_DA_SEMANA_CURTOS).toEqual(['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb']);
  });
});
