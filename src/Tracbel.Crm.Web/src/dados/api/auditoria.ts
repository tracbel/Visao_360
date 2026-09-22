/**
 * A trilha de auditoria (issue 135) — `/api/v1/admin/auditoria`, com `Auditoria.Ler` (Diretoria e Administrador).
 *
 * Só leitura: a trilha nasce na gravação do dado, na mesma transação, e ninguém a altera. A fronteira é a de
 * filial — "Todas as filiais" no seletor mostra todas.
 */

import type { ComProcedencia, PaginaDe } from '../../tipos/api';
import { ler, type ContextoDeAcesso } from './http';

const BASE = '/v1/admin/auditoria';

export type OperacaoAuditada = 'Inclusao' | 'Alteracao' | 'Exclusao';
export type OrigemDaOperacao = 'Usuario' | 'Integracao' | 'Importacao' | 'Sistema' | 'Job';

export type CampoDaAuditoria = { campo: string; rotulo: string; antes: string | null; depois: string | null };

export type EventoDaAuditoria = {
  quando: string;
  entidade: string;
  entidadeRotulo: string;
  registroId: number;
  registro: string;
  operacao: OperacaoAuditada;
  origem: OrigemDaOperacao;
  sistema: string | null;
  autor: string;
  autorLogin: string;
  filialCodigo: string;
  filialNome: string;
  correlacao: string | null;
  campos: CampoDaAuditoria[];
};

export type EntidadeAuditada = { codigo: string; rotulo: string };

export type FiltroDaTrilha = {
  de: string;
  ate: string;
  entidade: string;
  /** Só este registro da entidade — o histórico de um registro. */
  registro: number | null;
  operacao: string;
  origem: string;
  autor: string;
  pagina: number;
  tamanho: number;
};

export function consultarTrilha(contexto: ContextoDeAcesso, filtro: FiltroDaTrilha, sinal?: AbortSignal): Promise<ComProcedencia<PaginaDe<EventoDaAuditoria>>> {
  return ler<PaginaDe<EventoDaAuditoria>>(BASE, contexto, {
    sinal,
    parametros: {
      de: filtro.de || undefined,
      ate: filtro.ate || undefined,
      entidade: filtro.entidade || undefined,
      registro: filtro.registro ?? undefined,
      operacao: filtro.operacao || undefined,
      origem: filtro.origem || undefined,
      autor: filtro.autor || undefined,
      pagina: filtro.pagina,
      tamanho: filtro.tamanho,
    },
  });
}

export function listarEntidadesAuditadas(contexto: ContextoDeAcesso, sinal?: AbortSignal): Promise<ComProcedencia<EntidadeAuditada[]>> {
  return ler<EntidadeAuditada[]>(`${BASE}/entidades`, contexto, { sinal });
}
