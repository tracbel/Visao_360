/**
 * A LAVOURA DO MUNICÍPIO EM LINHAS — a tabelinha "Lavoura (área plantada)" da
 * visão geral da ficha (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * O QUE A LEITURA TRAZ, E O QUE NÃO TRAZ: `municipio.potencial[]` tem o detalhe
 * SÓ das culturas com regra de potencial (o repositório monta uma linha por
 * regra), e `municipio.producao` tem o TOTAL da PAM — todas as culturas somadas.
 * Não há, nesta leitura, a lista de todas as culturas do município.
 *
 * Então a tabela mostra as culturas que existem no detalhe, da maior para a
 * menor, e "Outros" é o que falta para o total: a área plantada da PAM menos a
 * soma das listadas. Nenhuma cultura é inventada, e a soma da coluna fecha com o
 * total publicado. A dica do título diz isso.
 */

import type { CulturaNoEstado, IndicadoresDoMunicipio, RegraDePotencialAplicada } from '../../../tipos/territorio';

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
): LavouraDoMunicipio | null {
  const comArea = municipio.potencial
    .filter((p) => p.areaPlantadaHectares != null && p.areaPlantadaHectares > 0)
    .map((p) => ({ nome: nomeDaCultura(p.produtoCodigoIbge, regras, culturasNoEstado), area: p.areaPlantadaHectares! }))
    .sort((a, b) => b.area - a.area);

  const total = municipio.producao?.areaPlantadaHectares ?? null;
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
    ano: municipio.producao?.ano ?? null,
    totalPlantado: temTotal ? total : null,
    culturasComArea: municipio.producao?.culturasComArea ?? null,
    culturasDetalhadas: comArea.length,
    culturasSemAreaDivulgada: municipio.potencial.filter((p) => p.areaPlantadaHectares == null).length,
  };
}
