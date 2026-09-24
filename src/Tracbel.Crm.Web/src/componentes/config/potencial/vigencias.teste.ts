/**
 * As regras das vigências na tela (issue 77) — as mesmas do servidor, para a tela não oferecer o que a API vai
 * recusar: hoje é o dia de São Paulo, só se revoga o que não passou de hoje, e vale a de início mais recente.
 */

import { describe, expect, it } from 'vitest';
import type { HistoricoDosParametrosDoPotencial, VigenciaDoParametro } from '../../../tipos/potencial';
import { ehRevogavel, estadoDaVigencia, formularioDosGerais, hojeEmSaoPaulo, iniciosVigentes, montarHistorico } from './vigencias';

function vigencia(vigenteDesde: string, extra: Partial<VigenciaDoParametro> = {}): VigenciaDoParametro {
  return {
    vigenteDesde,
    justificativa: 'teste',
    informadoPor: 'Fulano',
    informadoEm: `${vigenteDesde}T12:00:00`,
    revogadoEm: null,
    revogadoPor: null,
    motivoDaRevogacao: null,
    ...extra,
  };
}

const HOJE = '2026-09-21';

describe('hojeEmSaoPaulo', () => {
  it('às 22h de 30/09 em São Paulo ainda é 30/09, embora já seja 01/10 em UTC', () => {
    expect(hojeEmSaoPaulo(new Date('2026-10-01T01:00:00Z'))).toBe('2026-09-30');
    expect(hojeEmSaoPaulo(new Date('2026-10-01T03:00:00Z'))).toBe('2026-10-01');
  });
});

describe('estado e revogação de uma vigência', () => {
  it('vigente, futura, substituída e revogada', () => {
    expect(estadoDaVigencia(vigencia('2026-09-13'), '2026-09-13', HOJE)).toBe('vigente');
    expect(estadoDaVigencia(vigencia('2026-09-01'), '2026-09-13', HOJE)).toBe('substituida');
    expect(estadoDaVigencia(vigencia('2026-10-01'), '2026-09-13', HOJE)).toBe('futura');
    expect(estadoDaVigencia(vigencia('2026-10-01', { revogadoEm: '2026-09-21T10:00:00' }), '2026-09-13', HOJE)).toBe('revogada');
  });

  it('só se revoga o que ainda não passou de hoje — a de hoje ainda pode', () => {
    expect(ehRevogavel(vigencia('2026-09-20'), HOJE)).toBe(false);
    expect(ehRevogavel(vigencia(HOJE), HOJE)).toBe(true);
    expect(ehRevogavel(vigencia('2026-12-01'), HOJE)).toBe(true);
    expect(ehRevogavel(vigencia('2026-12-01', { revogadoEm: '2026-09-21T10:00:00' }), HOJE)).toBe(false);
  });

  it('vale numa data a de início mais recente até ela, ignorando as revogadas', () => {
    const itens = [
      { vigencia: vigencia('2026-09-13') },
      { vigencia: vigencia('2026-11-01') },
      { vigencia: vigencia('2026-10-01', { revogadoEm: '2026-09-21T10:00:00' }) },
    ];
    expect(iniciosVigentes(itens, () => 'cafe', '2026-10-15').get('cafe')).toBe('2026-09-13');
    expect(iniciosVigentes(itens, () => 'cafe', '2026-11-01').get('cafe')).toBe('2026-11-01');
    expect(iniciosVigentes(itens, () => 'cafe', '2026-09-12').get('cafe')).toBeUndefined();
  });
});

describe('montarHistorico', () => {
  const geral = {
    mesesDaJanela: 12,
    pesoDosContratosNoCredito: 0.7,
    pesoDoValorNoCredito: 0.3,
    limiteDeRetracao: 1,
    limiteDeAquecimento: 1.2,
    limiteDeSuperaquecimento: 1.4,
    nomeDaFaixaIntermediaria: null,
    limiteDaPercepcao: 5,
    pesoDoIndicadorDePreco: null,
    pesoDoIndicadorDeCredito: null,
    pesoDoIndicadorComercial: null,
    fatorMinimo: null,
    fatorMaximo: null,
    mesesDeCarenciaDoSicor: null,
  };

  const historico: HistoricoDosParametrosDoPotencial = {
    gerais: [{ ...geral, vigencia: vigencia('2026-09-21', { informadoPor: null, informadoEm: '2026-09-21T00:00:00' }) }],
    culturas: [
      {
        produtoCodigoIbge: 40139, produtoNome: 'Café (em grão) Total', hectaresPorMaquina: 10, anosDeRenovacao: null,
        modeloDeReferencia: '3036N', situacao: 'AConfirmar', vigencia: vigencia('2026-09-13', { informadoEm: '2026-09-13T00:00:00' }),
        culturaCodigo: 'CAFE', culturaNome: 'Café', categoriaDeMaquinaCodigo: 'TRATOR', categoriaDeMaquinaNome: 'Trator',
      },
      {
        produtoCodigoIbge: 40139, produtoNome: 'Café (em grão) Total', hectaresPorMaquina: 20, anosDeRenovacao: 10,
        modeloDeReferencia: '3036N', situacao: 'Confirmada', vigencia: vigencia('2026-10-01', { informadoEm: '2026-09-21T15:00:00' }),
        culturaCodigo: 'CAFE', culturaNome: 'Café', categoriaDeMaquinaCodigo: 'TRATOR', categoriaDeMaquinaNome: 'Trator',
      },
    ],
    percepcoes: [
      { municipioCodigoIbge: 3516200, municipioNome: 'Franca', uf: 'SP', percentual: -2.5, vigencia: vigencia('2026-09-22', { informadoEm: '2026-09-21T16:00:00' }) },
    ],
  };

  it('junta as três vigências, da mais recente para a mais antiga, cada uma com o seu estado', () => {
    const linhas = montarHistorico(historico, HOJE);

    expect(linhas.map((l) => [l.tipo, l.estado])).toEqual([
      ['Percepção do gestor', 'futura'],
      ['Regra da cultura', 'futura'],
      ['Parâmetros gerais', 'vigente'],
      ['Regra da cultura', 'vigente'],
    ]);
    expect(linhas[0].resumo).toBe('-2,5%');
    expect(linhas[1].resumo).toContain('renovação a cada 10 anos');
    expect(linhas[3].resumo).toContain('renovação não informada');
    expect(linhas[1].alvo).toEqual({
      tipo: 'cultura',
      produtoCodigoIbge: 40139,
      categoriaDeMaquinaCodigo: 'TRATOR',
      vigenteDesde: '2026-10-01',
    });
  });

  // O DEFEITO QUE ISTO IMPEDE DE VOLTAR (D-P01, issue 63): a chave da regra era só o produto, e com o
  // trator e a colheitadeira do mesmo café registrados na mesma data, uma das duas era marcada como
  // SUBSTITUÍDA pela outra — sem erro e sem aviso. Elas são decisões diferentes e não se substituem.
  it('duas categorias do mesmo produto não disputam a mesma chave', () => {
    const colheitadeira = {
      produtoCodigoIbge: 40139,
      produtoNome: 'Café (em grão) Total',
      hectaresPorMaquina: 200,
      anosDeRenovacao: 12,
      modeloDeReferencia: 'C100',
      situacao: 'Confirmada' as const,
      vigencia: vigencia('2026-09-13', { informadoEm: '2026-09-13T10:00:00' }),
      culturaCodigo: 'CAFE',
      culturaNome: 'Café',
      categoriaDeMaquinaCodigo: 'COLHEITADEIRA',
      categoriaDeMaquinaNome: 'Colheitadeira',
    };

    const linhas = montarHistorico(
      { ...historico, culturas: [historico.culturas[0], colheitadeira] },
      HOJE,
    );

    const regras = linhas.filter((l) => l.tipo === 'Regra da cultura');
    expect(regras).toHaveLength(2);
    // As duas valem hoje: nenhuma substituiu a outra.
    expect(regras.map((r) => r.estado)).toEqual(['vigente', 'vigente']);
    // E a tela as distingue pelo nome, em vez de mostrar "Café" duas vezes.
    expect(regras.map((r) => r.chave).sort()).toEqual([
      'Café (em grão) Total · Colheitadeira',
      'Café (em grão) Total · Trator',
    ]);
  });

  it('o formulário dos gerais nasce com a vigência de hoje, em vírgula decimal, e sem justificativa', () => {
    const formulario = formularioDosGerais(historico.gerais[0], HOJE);

    expect(formulario.pesoDosContratosNoCredito).toBe('0,7');
    expect(formulario.limiteDeAquecimento).toBe('1,2');
    expect(formulario.pesoDoIndicadorDePreco).toBe('');
    expect(formulario.vigenteDesde).toBe(HOJE);
    expect(formulario.justificativa).toBe('');
  });
});
