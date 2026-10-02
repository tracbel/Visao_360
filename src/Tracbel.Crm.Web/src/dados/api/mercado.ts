/**
 * As rotas de Inteligência de Mercado que as telas do épico 264 (o protótipo da pasta 360 no CRM) consomem.
 */

import type { ComProcedencia } from '../../tipos/api';
import type {
  DemandaEPrevisaoDaRegiao,
  DiagnosticoComercialDaRegiao,
  DimensionamentoDaAdr,
  FiltrosDaDemanda,
  FiltrosDoDiagnostico,
  FiltrosDoDimensionamento,
  FiltrosDosFinanciamentos,
  FinanciamentosDoSicor,
  PrecosDasCulturas,
} from '../../tipos/mercado';
import { ler, type ContextoDeAcesso } from './http';

/** O IOC de cada município da ADR, com a situação e o plano de ação (issue 257). */
export function obterDiagnosticoComercial(
  contexto: ContextoDeAcesso,
  filtros: FiltrosDoDiagnostico,
  sinal?: AbortSignal,
): Promise<ComProcedencia<DiagnosticoComercialDaRegiao>> {
  return ler<DiagnosticoComercialDaRegiao>('/v1/mercado/diagnostico', contexto, {
    sinal,
    parametros: {
      competenciaInicial: filtros.competenciaInicial,
      competenciaFinal: filtros.competenciaFinal,
      regiao: filtros.regiao,
      lojaCodigo: filtros.lojaCodigo,
      visao: filtros.visao,
      categoria: filtros.categoria,
      responsavel: filtros.responsavel,
    },
  });
}

/** A demanda anual de máquinas, o que a Tracbel tem de entregar e a previsão por mês e por loja (issue 258). */
export function obterDemandaEPrevisao(
  contexto: ContextoDeAcesso,
  filtros: FiltrosDaDemanda,
  sinal?: AbortSignal,
): Promise<ComProcedencia<DemandaEPrevisaoDaRegiao>> {
  return ler<DemandaEPrevisaoDaRegiao>('/v1/mercado/demanda', contexto, {
    sinal,
    parametros: {
      regiao: filtros.regiao,
      lojaCodigo: filtros.lojaCodigo,
      categoria: filtros.categoria,
      anoFiscal: filtros.anoFiscal === undefined ? undefined : String(filtros.anoFiscal),
      cultura: filtros.cultura,
    },
  });
}

/** O tamanho da ADR em São Paulo, o perfil das lojas, a matriz municipal e a carteira (issue 259). */
export function obterDimensionamentoDaAdr(
  contexto: ContextoDeAcesso,
  filtros: FiltrosDoDimensionamento,
  sinal?: AbortSignal,
): Promise<ComProcedencia<DimensionamentoDaAdr>> {
  return ler<DimensionamentoDaAdr>('/v1/mercado/dimensionamento', contexto, {
    sinal,
    parametros: {
      anoBase: filtros.anoBase,
      cultura: filtros.cultura,
      regiao: filtros.regiao,
      lojaCodigo: filtros.lojaCodigo,
      visao: filtros.visao,
      responsavel: filtros.responsavel,
      classe: filtros.classe,
      usina: filtros.usina,
    },
  });
}

/** O crédito de mecanização do SICOR no período, produto e programa escolhidos, por município e loja (issue 261). */
export function obterFinanciamentosDoSicor(
  contexto: ContextoDeAcesso,
  filtros: FiltrosDosFinanciamentos,
  sinal?: AbortSignal,
): Promise<ComProcedencia<FinanciamentosDoSicor>> {
  return ler<FinanciamentosDoSicor>('/v1/mercado/financiamentos', contexto, {
    sinal,
    parametros: {
      de: filtros.de,
      ate: filtros.ate,
      produto: filtros.produto,
      programa: filtros.programa,
      recorte: filtros.recorte,
      lojaCodigo: filtros.lojaCodigo,
      usina: filtros.usina,
    },
  });
}

/** O momento de preço de cada cultura no mês, em R3, R6 e R12, e a série histórica (issue 260). */
export function obterPrecosDasCulturas(contexto: ContextoDeAcesso, sinal?: AbortSignal): Promise<ComProcedencia<PrecosDasCulturas>> {
  return ler<PrecosDasCulturas>('/v1/mercado/precos', contexto, { sinal });
}
