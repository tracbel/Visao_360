/**
 * As rotas de Inteligência de Mercado que as telas do épico 264 (o protótipo da pasta 360 no CRM) consomem.
 */

import type { ComProcedencia } from '../../tipos/api';
import type { DiagnosticoComercialDaRegiao, FiltrosDoDiagnostico } from '../../tipos/mercado';
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
      categoria: filtros.categoria,
    },
  });
}