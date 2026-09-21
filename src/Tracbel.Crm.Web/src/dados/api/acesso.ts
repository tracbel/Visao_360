/**
 * O escopo efetivo de quem está usando — as filiais que pode escolher e o que
 * pode fazer na filial atual (fase 3 do documento 41; issue 46).
 *
 * A ROTA RESPONDE MESMO COM A FILIAL RECUSADA: se a filial guardada no
 * navegador deixou de ser permitida (P-20), toda outra chamada daria 403. Esta
 * responde pela filial de casa e diz qual foi recusada — e o seletor de filial
 * volta sozinho.
 */

import type { ComProcedencia } from '../../tipos/api';
import { ler, type ContextoDeAcesso } from './http';

/**
 * O código que pede TODAS AS FILIAIS no lugar de uma — o mesmo de
 * `ContextoAcesso.CodigoDeTodasAsFiliais` no servidor. Só quem administra o CRM
 * (visão entre filiais em profundidade Organização) consegue escolher.
 */
export const TODAS_AS_FILIAIS = 'TODAS';

/** Uma filial do escopo. */
export type FilialDoEscopo = { codigo: string; nome: string; ehCasa: boolean };

/** Uma permissão do escopo, com até onde alcança. */
export type PermissaoDoEscopo = { codigo: string; descricao: string; profundidade: string };

export type EscopoDoUsuario = {
  usuario: string;
  filialAtual: FilialDoEscopo;
  /** A filial do cabeçalho que não é permitida; nulo quando não houve recusa. */
  filialPedidaRecusada: string | null;
  filiaisPermitidas: FilialDoEscopo[];
  permissoes: PermissaoDoEscopo[];
  /** Se o seletor oferece "Todas as filiais" (`TODAS_AS_FILIAIS`). */
  podeVerTodasAsFiliais: boolean;
};

/** Lê o escopo efetivo. */
export function obterEscopo(contexto: ContextoDeAcesso, sinal?: AbortSignal): Promise<ComProcedencia<EscopoDoUsuario>> {
  return ler<EscopoDoUsuario>('/v1/acesso/escopo', contexto, { sinal });
}
