/**
 * O registro das sincronizações, para a administração (Configurações › TI e
 * Integrações › Integrações). Só leitura: a sincronização roda no serviço do
 * Windows `TracbelCrmSincronizacaoArt`, no servidor — documento 35, seção 11.
 */

import type { ComProcedencia, SituacaoDaSincronizacao } from '../../tipos/api';
import { ler, type ContextoDeAcesso } from './http';

/** Cada fluxo com a última execução, o último sucesso e as execuções recentes. */
export function listarSincronizacoes(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<ComProcedencia<SituacaoDaSincronizacao[]>> {
  return ler<SituacaoDaSincronizacao[]>('/v1/integracoes/sincronizacoes', contexto, {
    sinal,
    parametros: { execucoes: 10 },
  });
}
