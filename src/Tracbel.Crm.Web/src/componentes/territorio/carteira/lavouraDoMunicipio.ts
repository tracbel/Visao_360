/**
 * A LAVOURA DO MUNICÍPIO EM LINHAS — a tabelinha "Lavoura (área plantada)" da
 * visão geral da ficha (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * DE ONDE VÊM AS CULTURAS (issue 168, 27/09/2026): o histórico do município
 * (`GET …/municipios/{codigo}/historico`) traz TODAS as culturas da PAM de cada
 * ano, e a tabela põe CADA CULTURA NO ÚLTIMO ANO EM QUE A ÁREA DELA FOI
 * DIVULGADA (issue 152, a regra do resto da tela) — e não o ano mais recente da
 * lista, que numa PAM nova ainda incompleta faria a cultura que não chegou sumir
 * da lavoura em vez de aparecer com o ano anterior dela. Antes dele chegar — ou na ficha
 * sozinha, sem os filtros da página —, a tabela cai no que a leitura do painel
 * tem: `municipio.potencial[]`, que detalha SÓ as culturas com regra de
 * potencial, e `municipio.producao`, o total da PAM.
 *
 * "OUTROS" É O QUE FALTA PARA O TOTAL, nas duas formas: a área plantada da PAM
 * menos a soma das listadas. Com todas as culturas, "Outros" são as menores que
 * não couberam nas linhas; sem elas, são também as culturas sem regra. Nenhuma
 * cultura é inventada, e a soma da coluna fecha com o total. A dica do título
 * diz qual das duas formas está na tela.
 */

import type {
  CulturaNoEstado,
  IndicadoresDoMunicipio,
  LavouraNoAno,
  RegraDePotencialAplicada,
} from '../../../tipos/territorio';

export type LinhaDaLavoura = {
  nome: string;
  areaHectares: number;
  /** Percentual da base — o total da PAM quando existe. */
  participacao: number;
};

export type LavouraDoMunicipio = {
  linhas: LinhaDaLavoura[];
  /** O resto até o total; nulo quando não sobra nada (ou quando o total não fecha). */
  outros: LinhaDaLavoura | null;
  /** O ano da PAM do total, quando há total — com todas as culturas, o mais recente delas. */
  ano: number | null;
  /** Com todas as culturas, o ano mais antigo entre os de cada uma; igual a `ano` quando todas são do mesmo. */
  anoMaisAntigo: number | null;
  /** A área plantada total da PAM; nula quando a PAM não foi carregada aqui. */
  totalPlantado: number | null;
  /** Quantas culturas a PAM divulgou com área neste município. */
  culturasComArea: number | null;
  /** Quantas culturas o detalhe traz com área — as listadas mais as que foram para "Outros". */
  culturasDetalhadas: number;
  /**
   * Culturas com regra SEM ÁREA DIVULGADA no município — não entram na conta, e
   * não como zero.
   *
   * NÃO É "SOB SIGILO" (revisão de 24/09/2026): o repositório gera a linha da
   * regra em TODO município, e ela vem nula tanto onde o IBGE suprimiu o número
   * quanto onde a cultura simplesmente não é plantada. A leitura não separa os
   * dois casos, e a tela não afirma o que não sabe.
   */
  culturasSemAreaDivulgada: number;
  /**
   * `true` quando as linhas vêm de TODAS as culturas do ano (o histórico); `false`
   * quando vêm só das culturas com regra de potencial.
   */
  todasAsCulturas: boolean;
};

/** O nome da cultura: o da regra, o da mesma cultura em SP, ou o código — nunca um nome inventado. */
export function nomeDaCultura(
  produtoCodigoIbge: number,
  regras: RegraDePotencialAplicada[],
  culturasNoEstado: CulturaNoEstado[],
): string {
  return (
    regras.find((r) => r.produtoCodigoIbge === produtoCodigoIbge)?.produtoNome ??
    culturasNoEstado.find((c) => c.produtoCodigoIbge === produtoCodigoIbge)?.produtoNome ??
    `produto ${produtoCodigoIbge}`
  );
}

/**
 * CADA CULTURA NO ÚLTIMO ANO EM QUE A ÁREA DELA FOI DIVULGADA (issue 152). O
 * servidor só manda, em cada ano, as culturas com área; o ano mais recente de
 * cada uma sobrescreve os anteriores.
 */
function culturasNoUltimoAnoDeCada(lavoura: readonly LavouraNoAno[]): { nome: string; area: number; ano: number }[] {
  const porProduto = new Map<number, { nome: string; area: number; ano: number }>();
  for (const doAno of [...lavoura].sort((a, b) => a.ano - b.ano))
    for (const c of doAno.culturas)
      if (c.areaPlantadaHectares != null && c.areaPlantadaHectares > 0)
        porProduto.set(c.produtoCodigoIbge, { nome: c.produtoNome, area: c.areaPlantadaHectares, ano: doAno.ano });
  return [...porProduto.values()];
}

export function lavouraDoMunicipio(
  municipio: IndicadoresDoMunicipio,
  regras: RegraDePotencialAplicada[],
  culturasNoEstado: CulturaNoEstado[],
  maximo = 4,
  /** A lavoura de cada ano da PAM no histórico do município — todas as culturas; nula antes dele chegar. */
  doHistorico: readonly LavouraNoAno[] | null = null,
): LavouraDoMunicipio | null {
  const noUltimoAnoDeCada = doHistorico === null ? [] : culturasNoUltimoAnoDeCada(doHistorico);
  const todasAsCulturas = noUltimoAnoDeCada.length > 0;

  const comArea = todasAsCulturas
    ? [...noUltimoAnoDeCada].sort((a, b) => b.area - a.area)
    : municipio.potencial
        .filter((p) => p.areaPlantadaHectares != null && p.areaPlantadaHectares > 0)
        .map((p) => ({ nome: nomeDaCultura(p.produtoCodigoIbge, regras, culturasNoEstado), area: p.areaPlantadaHectares! }))
        .sort((a, b) => b.area - a.area);

  // COM TODAS AS CULTURAS, O TOTAL É A SOMA DELAS, cada uma no ano dela — o "Outros" são as que não couberam.
  const total = todasAsCulturas
    ? noUltimoAnoDeCada.reduce((s, c) => s + c.area, 0)
    : (municipio.producao?.areaPlantadaHectares ?? null);
  const temTotal = total !== null && total > 0;
  if (!temTotal && comArea.length === 0) return null;

  const base = temTotal ? total : comArea.reduce((s, c) => s + c.area, 0);
  const listadas = comArea.slice(0, maximo);
  const somaListadas = listadas.reduce((s, c) => s + c.area, 0);
  const areaDosOutros = temTotal ? total - somaListadas : comArea.slice(maximo).reduce((s, c) => s + c.area, 0);

  const linha = (nome: string, area: number): LinhaDaLavoura => ({ nome, areaHectares: area, participacao: (100 * area) / base });

  return {
    linhas: listadas.map((c) => linha(c.nome, c.area)),
    // MENOS DE UM HECTARE NÃO É "OUTROS": é arredondamento da PAM, e uma linha
    // "Outros 0 ha 0%" só ocuparia espaço.
    outros: areaDosOutros >= 1 ? linha('Outros', areaDosOutros) : null,
    ano: todasAsCulturas ? Math.max(...noUltimoAnoDeCada.map((c) => c.ano)) : (municipio.producao?.ano ?? null),
    anoMaisAntigo: todasAsCulturas ? Math.min(...noUltimoAnoDeCada.map((c) => c.ano)) : (municipio.producao?.ano ?? null),
    totalPlantado: temTotal ? total : null,
    culturasComArea: todasAsCulturas ? comArea.length : (municipio.producao?.culturasComArea ?? null),
    culturasDetalhadas: comArea.length,
    // COM TODAS AS CULTURAS, a sem área divulgada nem chega na lista (o servidor só manda as com área):
    // não há o que contar, e a tela não afirma um número que não recebeu.
    culturasSemAreaDivulgada: todasAsCulturas ? 0 : municipio.potencial.filter((p) => p.areaPlantadaHectares == null).length,
    todasAsCulturas,
  };
}
