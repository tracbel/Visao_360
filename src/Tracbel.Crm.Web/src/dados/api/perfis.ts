/**
 * A administração de perfis (issue 113, parte 2b) — `Perfil.Administrar`, que só o Administrador tem.
 *
 * Os perfis do sistema são fixos: a API recusa editá-los (409), e a tela oferece duplicar. Ninguém cria nem edita
 * um perfil com permissão maior do que a que tem.
 */

import type { ComProcedencia } from '../../tipos/api';
import type { PermissaoDoEscopo } from './acesso';
import { ler, pedir, type ContextoDeAcesso } from './http';

export type PerfilNaAdministracao = {
  codigo: string;
  nome: string;
  descricao: string | null;
  ehPadrao: boolean;
  ehDoSistema: boolean;
  estaAtivo: boolean;
  pessoasComOPerfil: number;
  permissoes: PermissaoDoEscopo[];
};

export type PermissaoDoCatalogo = { codigo: string; descricao: string };

export type PermissaoDoEditor = { codigo: string; profundidade: string };

export function listarPerfisDaAdministracao(contexto: ContextoDeAcesso, sinal?: AbortSignal): Promise<ComProcedencia<PerfilNaAdministracao[]>> {
  return ler<PerfilNaAdministracao[]>('/v1/admin/perfis/todos', contexto, { sinal });
}

export function listarCatalogoDePermissoes(contexto: ContextoDeAcesso, sinal?: AbortSignal): Promise<ComProcedencia<PermissaoDoCatalogo[]>> {
  return ler<PermissaoDoCatalogo[]>('/v1/admin/permissoes', contexto, { sinal });
}

export function criarPerfil(
  contexto: ContextoDeAcesso,
  corpo: { codigo: string; nome: string; descricao: string; permissoes: PermissaoDoEditor[] },
) {
  return pedir<PerfilNaAdministracao>('/v1/admin/perfis', contexto, { metodo: 'POST', corpo });
}

export function editarPerfil(
  contexto: ContextoDeAcesso,
  codigo: string,
  corpo: { nome: string; descricao: string; permissoes: PermissaoDoEditor[] },
) {
  return pedir<PerfilNaAdministracao>(`/v1/admin/perfis/${encodeURIComponent(codigo)}`, contexto, { metodo: 'PUT', corpo });
}

export function desativarPerfil(contexto: ContextoDeAcesso, codigo: string) {
  return pedir<PerfilNaAdministracao>(`/v1/admin/perfis/${encodeURIComponent(codigo)}/desativacao`, contexto, { metodo: 'POST' });
}

export function reativarPerfil(contexto: ContextoDeAcesso, codigo: string) {
  return pedir<PerfilNaAdministracao>(`/v1/admin/perfis/${encodeURIComponent(codigo)}/reativacao`, contexto, { metodo: 'POST' });
}
