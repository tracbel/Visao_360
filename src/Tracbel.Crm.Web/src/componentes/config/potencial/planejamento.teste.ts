/**
 * As regras do planejamento comercial na tela (issue 256): a soma lê vírgula, os pesos viram fatia do índice, o
 * formulário parte do que vale hoje e a trilha identifica cada vigência para revogar.
 */

import { describe, expect, it } from 'vitest';
import type { HistoricoDoPlanejamento, ParametroDoPlanejamentoDetalhe, VigenciaDoParametro } from '../../../tipos/potencial';
import { fatiasDoIoc, formularioDoPlanejamento, montarHistoricoDoPlanejamento, somaDaSazonalidade } from './planejamento';

const vigencia = (vigenteDesde: string, parcial: Partial<VigenciaDoParametro> = {}): VigenciaDoParametro => ({
  vigenteDesde,
  justificativa: 'protótipo',
  informadoPor: null,
  informadoEm: `${vigenteDesde}T00:00:00`,
  revogadoEm: null,
  revogadoPor: null,
  motivoDaRevogacao: null,
  ...parcial,
});

const DO_PROTOTIPO: ParametroDoPlanejamentoDetalhe = {
  sazonalidade: [6.52, 6.89, 8.56, 8.59, 8.87, 8.58, 7.84, 8.75, 9.15, 10.51, 7.44, 8.3],
  pesos: { potencial: 25, cobertura: 20, credito: 15, rentabilidade: 15, clientes: 10, realizacao: 5, penetracao: 10 },
  vigencia: vigencia('2026-09-27'),
};

describe('planejamento comercial', () => {
  it('a soma lê vírgula e ignora o que ainda não é número', () => {
    expect(somaDaSazonalidade(['6,52', '6.89', '', 'abc'])).toBeCloseTo(13.41);
  });

  it('os pesos do protótipo somam 100 e cada um é a própria fatia', () => {
    const fatias = fatiasDoIoc(DO_PROTOTIPO.pesos);
    expect(fatias.potencial).toBeCloseTo(25);
    expect(fatias.realizacao).toBeCloseTo(5);
  });

  it('pesos que não somam 100 são normalizados — 50 e 50 valem metade cada', () => {
    const fatias = fatiasDoIoc({ potencial: 50, cobertura: 50, credito: 0, rentabilidade: 0, clientes: 0, realizacao: 0, penetracao: 0 });
    expect(fatias.potencial).toBe(50);
    expect(fatias.credito).toBe(0);
  });

  it('o formulário parte da vigência de hoje, com vírgula decimal e a data de hoje', () => {
    const formulario = formularioDoPlanejamento(DO_PROTOTIPO, '2026-10-01');
    expect(formulario.sazonalidade[0]).toBe('6,52');
    expect(formulario.sazonalidade[11]).toBe('8,3');
    expect(formulario.pesoDoPotencial).toBe('25');
    expect(formulario.vigenteDesde).toBe('2026-10-01');
    expect(formulario.justificativa).toBe('');
  });

  it('sem vigência, o formulário abre com os doze meses vazios', () => {
    expect(formularioDoPlanejamento(null, '2026-10-01').sazonalidade).toEqual(Array(12).fill(''));
  });

  it('a trilha marca vigente, futura e revogada, e sabe revogar cada uma', () => {
    const historico: HistoricoDoPlanejamento = {
      planejamentos: [DO_PROTOTIPO, { ...DO_PROTOTIPO, vigencia: vigencia('2026-11-01', { informadoPor: 'Ricardo' }) }],
      shares: [
        { categoriaDeMaquinaCodigo: 'TRATOR', categoriaDeMaquinaNome: 'Trator', percentual: 31, vigencia: vigencia('2026-09-27') },
        {
          categoriaDeMaquinaCodigo: 'TRATOR', categoriaDeMaquinaNome: 'Trator', percentual: 40,
          vigencia: vigencia('2026-10-05', { revogadoEm: '2026-09-28T10:00:00', revogadoPor: 'Ricardo', motivoDaRevogacao: 'erro' }),
        },
      ],
    };

    const linhas = montarHistoricoDoPlanejamento(historico, '2026-09-28');

    const estados = Object.fromEntries(linhas.map((l) => [`${l.tipo}|${l.vigencia.vigenteDesde}`, l.estado]));
    expect(estados).toEqual({
      'Sazonalidade e pesos do IOC|2026-09-27': 'vigente',
      'Sazonalidade e pesos do IOC|2026-11-01': 'futura',
      'Share-alvo|2026-09-27': 'vigente',
      'Share-alvo|2026-10-05': 'revogada',
    });

    const share = linhas.find((l) => l.tipo === 'Share-alvo' && l.estado === 'vigente')!;
    expect(share.chave).toBe('Trator');
    expect(share.resumo).toBe('31% da demanda');
    expect(share.alvo).toEqual({ tipo: 'share', categoriaDeMaquinaCodigo: 'TRATOR', vigenteDesde: '2026-09-27' });

    const planejamento = linhas.find((l) => l.tipo === 'Sazonalidade e pesos do IOC' && l.estado === 'vigente')!;
    expect(planejamento.resumo).toContain('Pico em Out (10,51%)');
    expect(planejamento.alvo).toEqual({ tipo: 'planejamento', vigenteDesde: '2026-09-27' });
  });
});
