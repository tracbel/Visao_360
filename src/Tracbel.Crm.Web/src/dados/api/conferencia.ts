/**
 * A conferência com a Gestão de Negócios (28/09/2026) — `GET /api/v1/integracoes/conferencia-gn`. Pela filial do cabeçalho,
 * ou por todas, em "Todas as filiais".
 */

import type { ComProcedencia } from '../../tipos/api';
import type { ConferenciaComAGestao } from '../../tipos/conferencia';
import { ler, type ContextoDeAcesso } from './http';

export function obterConferenciaComAGestao(contexto: ContextoDeAcesso, sinal?: AbortSignal): Promise<ComProcedencia<ConferenciaComAGestao>> {
  return ler<ConferenciaComAGestao>('/v1/integracoes/conferencia-gn', contexto, { sinal });
}
