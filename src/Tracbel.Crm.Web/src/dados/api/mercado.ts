/**
 * As rotas de Inteligência de Mercado que as telas do épico 264 (o protótipo da pasta 360 no CRM) consomem.
 */

import type { ComProcedencia } from '../../tipos/api';
import type {
  DemandaEPrevisaoDaRegiao,
  DiagnosticoComercialDaRegiao,
  FiltrosDaDemanda,
  FiltrosDoDiagnostico,
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
    parametros: { regiao: filtros.regiao, lojaCodigo: filtros.lojaCodigo, categoria: filtros.categoria },
  });
}
