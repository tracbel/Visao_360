/**
 * A SOMA DA META DE VENDA ENTRE AS FILIAIS (#138): a filial que falha fica fora da soma e nomeada — nunca vira zero —; a
 * que diz 403 está fora do alcance, e não é falha; todas em 403 é falta da permissão; por consultor conta a pessoa, uma vez
 * só; e a pendente do ART que nenhuma filial mediu fica nula, e não zero.
 */

import { describe, expect, it } from 'vitest';
import type { MetaERealizadoDaFilial } from '../../tipos/metas';
import { ErroDaApi } from './http';
import { somarMetas, type MetaDaFilial } from './metas';

const periodo = {
  inicial: '2025-11-01', final: '2026-08-01', meses: 10, anoFiscal: 2026, texto: 'nov/2025 a ago/2026', ehOPadrao: true,
  inicialDoAnterior: '2024-11-01', finalDoAnterior: '2025-08-01',
};

function meta(parcial: Partial<MetaERealizadoDaFilial> & { meta: number; realizado: number; pendentes?: number | null }): MetaERealizadoDaFilial {
  return {
    periodo,
    alcance: 'Filial',
    totais: { metaMaquinas: parcial.meta, realizadoMaquinas: parcial.realizado, pendentesNoArt: parcial.pendentes === undefined ? 2 : parcial.pendentes, metaConsorcio: 3 },
    porMes: [],
    porLinha: [{ codigo: 'TRATOR_MEDIO', nome: 'TRATOR MÉDIO', meta: parcial.meta, realizado: parcial.realizado }],
    porConsultor: parcial.porConsultor ?? [],
    mesEmCurso: { competencia: '2026-09-01', metaMaquinas: 1, realizadoMaquinas: 1, metaConsorcio: 0 },
    mesmoTrechoDoFyAnterior: { realizadoMaquinas: 4 },
    origem: parcial.origem === undefined ? { sistema: 'API Gestão de Negócios', rota: '/api/v1/cadastros/metas', lidaEm: '2026-09-27T09:00:00Z', geradaNaOrigemEm: null } : parcial.origem,
    metricasSemDado: [{ metrica: 'consorcio', motivo: 'Meta de consórcio à parte.' }],
  };
}

const filial = (codigo: string) => ({ codigo, nome: `Filial ${codigo}` });
const respondeu = (codigo: string, m: MetaERealizadoDaFilial): MetaDaFilial => ({ filial: filial(codigo), meta: m, erro: null, foraDoAlcance: false });
const proibida = (codigo: string): MetaDaFilial => ({
  filial: filial(codigo), meta: null, foraDoAlcance: true, erro: new ErroDaApi(403, { title: 'Sem permissão' }, 'recusada'),
});
const falhou = (codigo: string): MetaDaFilial => ({ filial: filial(codigo), meta: null, foraDoAlcance: false, erro: new Error('caiu') });

describe('somarMetas', () => {
  it('soma as filiais que responderam, e a que falhou fica fora — nomeada, e não zero', () => {
    const soma = somarMetas([respondeu('010101', meta({ meta: 10, realizado: 7 })), respondeu('010103', meta({ meta: 5, realizado: 6 })), falhou('010105')]);

    expect(soma.respondidas).toBe(2);
    expect(soma.metaMaquinas).toBe(15);
    expect(soma.realizadoMaquinas).toBe(13);
    expect(soma.pendentesNoArt).toBe(4);
    expect(soma.metaConsorcio).toBe(6);
    expect(soma.realizadoNoAnterior).toBe(8);
    expect(soma.mesEmCurso).toEqual({ competencia: '2026-09-01', metaMaquinas: 2, realizadoMaquinas: 2 });
    expect(soma.porLinha).toEqual([{ codigo: 'TRATOR_MEDIO', nome: 'TRATOR MÉDIO', meta: 15, realizado: 13 }]);
    expect(soma.filiais.find((f) => f.filial.codigo === '010105')?.erro?.message).toBe('caiu');
    expect(soma.semPermissao).toBe(false);
    expect(soma.periodo?.anoFiscal).toBe(2026);
  });

  it('403 é fora do alcance, e todas em 403 é falta da permissão', () => {
    const parcial = somarMetas([respondeu('010101', meta({ meta: 3, realizado: 1 })), proibida('010103')]);
    expect(parcial.foraDoAlcance).toBe(1);
    expect(parcial.semPermissao).toBe(false);
    expect(parcial.metaMaquinas).toBe(3);

    const nenhuma = somarMetas([proibida('010101'), proibida('010103')]);
    expect(nenhuma.semPermissao).toBe(true);
    expect(nenhuma.respondidas).toBe(0);
  });

  it('por consultor conta a pessoa: quem tem meta em duas filiais aparece uma vez', () => {
    const soma = somarMetas([
      respondeu('010101', meta({ meta: 3, realizado: 1, porConsultor: [{ consultor: 'FULANO.DE.TAL', temConta: false, meta: 3, realizado: 1 }] })),
      respondeu('010103', meta({ meta: 2, realizado: 2, porConsultor: [{ consultor: 'FULANO.DE.TAL', temConta: true, meta: 2, realizado: 2 }] })),
    ]);

    expect(soma.porConsultor).toEqual([{ consultor: 'FULANO.DE.TAL', temConta: true, meta: 5, realizado: 3 }]);
  });

  it('no alcance Próprios a pendente não é medida: fica nula, e não zero', () => {
    const soma = somarMetas([respondeu('010101', { ...meta({ meta: 3, realizado: 1, pendentes: null }), alcance: 'Proprios' })]);

    expect(soma.pendentesNoArt).toBeNull();
    expect(soma.alcance).toBe('Proprios');
  });
});
