/**
 * O TERMO DE TROCA — quantas unidades da safra compram um trator (issue 70, D-P06 e D-P12, decididas pelo Ricardo em
 * 27/09/2026).
 *
 * AS DUAS METADES: o preço da safra é a série da CONAB (ou da Socicana), na unidade em que o mercado negocia — a saca,
 * a caixa, a tonelada; o preço do trator é a MEDIANA DOS TRATORES VENDIDOS NO MÊS, pela nota do Protheus, o mesmo
 * trator para as seis culturas (D-P06). O termo de um mês é a divisão das duas no MESMO mês — nunca o trator de um mês
 * contra a saca de outro.
 *
 * CONTA PURA, SEM TELA: a aba desenha o que sai daqui, e o teste confere a conta sem montar a aba.
 */

import type { SerieDePreco, SerieDePrecoDeMaquina } from '../../../tipos/mercado';

/** A categoria do trator base (D-P06): a mediana dos tratores vendidos no mês. */
export const CATEGORIA_DO_TRATOR_BASE = 'TRATOR';

/** O termo de um mês: o preço das duas metades e a divisão delas. */
export type TermoNoMes = {
  /** `aaaa-mm-01`. */
  mes: string;
  /** Quantas unidades comerciais da safra compram o trator. */
  unidades: number;
  precoDoTrator: number;
  /** O preço da unidade comercial (saca, caixa, tonelada) no mês. */
  precoDaUnidade: number;
  /** Quantas notas de trator o mês teve — a base da mediana. */
  notasDoTrator: number;
};

/** O trator base na lista de categorias; nulo quando a rotina não trouxe trator nenhum. */
export function tratorBase(maquinas: readonly SerieDePrecoDeMaquina[] | undefined): SerieDePrecoDeMaquina | null {
  return maquinas?.find((m) => m.categoriaCodigo === CATEGORIA_DO_TRATOR_BASE && m.meses.length > 0) ?? null;
}

/**
 * A SÉRIE DO TERMO DE UMA CULTURA: um ponto por mês que as DUAS séries têm. O mês em que só uma tem preço fica de
 * fora — sem venda de trator no mês não há mediana, e usar a do mês vizinho seria comparar meses diferentes.
 */
export function serieDoTermo(safra: SerieDePreco, trator: SerieDePrecoDeMaquina): TermoNoMes[] {
  const doTrator = new Map(trator.meses.map((m) => [m.mes, m]));

  return safra.meses.flatMap((m) => {
    const t = doTrator.get(m.mes);
    const precoDaUnidade = m.valorEmReais * safra.fatorComercial;
    if (!t || precoDaUnidade <= 0) return [];
    return [{ mes: m.mes, unidades: t.mediana / precoDaUnidade, precoDoTrator: t.mediana, precoDaUnidade, notasDoTrator: t.notas }];
  });
}

/** O mês `meses` antes de `aaaa-mm-01`. */
export function mesesAntes(mes: string, meses: number): string {
  const [ano, m] = mes.split('-').map(Number);
  const total = ano * 12 + (m - 1) - meses;
  return `${Math.floor(total / 12)}-${String((total % 12) + 1).padStart(2, '0')}-01`;
}

/**
 * QUANTO O TERMO MUDOU entre o último mês e o mesmo mês `meses` antes, em fração: `+0,10` é "10% mais unidades para o
 * mesmo trator". Nulo quando a série não tem os dois meses — e não a comparação com o mês mais próximo.
 */
export function variacaoDoTermo(termo: readonly TermoNoMes[], meses: number): number | null {
  const ultimo = termo.at(-1);
  if (!ultimo) return null;
  const antes = termo.find((t) => t.mes === mesesAntes(ultimo.mes, meses));
  return antes && antes.unidades > 0 ? ultimo.unidades / antes.unidades - 1 : null;
}

/**
 * O PODER DE COMPRA mudou o inverso do termo: precisar de 10% MAIS sacas é comprar 1/1,10 − 1 ≈ 9% MENOS trator com a
 * mesma safra. A tendência da maquete fala em poder de compra, e o sinal dela tem de ser o do produtor.
 */
export function poderDeCompra(variacaoDoTermo: number | null): number | null {
  return variacaoDoTermo === null ? null : 1 / (1 + variacaoDoTermo) - 1;
}

/** A média das unidades da série — a linha de referência do gráfico. */
export function mediaDoTermo(termo: readonly TermoNoMes[]): number | null {
  return termo.length === 0 ? null : termo.reduce((s, t) => s + t.unidades, 0) / termo.length;
}
