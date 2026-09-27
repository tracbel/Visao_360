/**
 * A LAVOURA DO MUNICÍPIO EM LINHAS — a tabelinha "Lavoura (área plantada)" da
 * visão geral da ficha (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * DE ONDE VÊM AS CULTURAS (issue 168, 27/09/2026): o histórico do município
 * (`GET …/municipios/{codigo}/historico`) traz TODAS as culturas da PAM de cada
 * ano, e a tabela usa o ano mais recente dele. Antes dele chegar — ou na ficha
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
  /** O ano da PAM do total, quando há total. */
  ano: number | null;
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

export function lavouraDoMunicipio(
  municipio: IndicadoresDoMunicipio,
  regras: RegraDePotencialAplicada[],
  culturasNoEstado: CulturaNoEstado[],
  maximo = 4,
  /** O ano mais recente da PAM no histórico do município — todas as culturas; nulo antes dele chegar. */
  doHistorico: LavouraNoAno | null = null,
): LavouraDoMunicipio | null {
  const todasAsCulturas = doHistorico !== null && doHistorico.culturas.length > 0;

  const comArea = todasAsCulturas
    ? doHistorico.culturas
        .filter((c) => c.areaPlantadaHectares != null && c.areaPlantadaHectares > 0)
        .map((c) => ({ nome: c.produtoNome, area: c.areaPlantadaHectares! }))
        .sort((a, b) => b.area - a.area)
    : municipio.potencial
        .filter((p) => p.areaPlantadaHectares != null && p.areaPlantadaHectares > 0)
        .map((p) => ({ nome: nomeDaCultura(p.produtoCodigoIbge, regras, culturasNoEstado), area: p.areaPlantadaHectares! }))
        .sort((a, b) => b.area - a.area);

  const total = todasAsCulturas ? doHistorico.areaPlantadaHectares : (municipio.producao?.areaPlantadaHectares ?? null);
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
    ano: todasAsCulturas ? doHistorico.ano : (municipio.producao?.ano ?? null),
    totalPlantado: temTotal ? total : null,
    culturasComArea: todasAsCulturas ? comArea.length : (municipio.producao?.culturasComArea ?? null),
    culturasDetalhadas: comArea.length,
    // COM TODAS AS CULTURAS, a sem área divulgada nem chega na lista (o servidor só manda as com área):
    // não há o que contar, e a tela não afirma um número que não recebeu.
    culturasSemAreaDivulgada: todasAsCulturas ? 0 : municipio.potencial.filter((p) => p.areaPlantadaHectares == null).length,
    todasAsCulturas,
  };
}
