/**
 * A administração de usuários e concessões (issue 113) — `/api/v1/admin/usuarios` e `/api/v1/admin/perfis`.
 *
 * Ver é `Usuario.Ler`; agir é `Usuario.Administrar`. A tela esconde o que a pessoa não pode, mas quem decide é a
 * API: ela recusa agir sobre a própria conta, dar um acesso maior do que o de quem concede e olhar fora da filial.
 */

import type { ComProcedencia, PaginaDe } from '../../tipos/api';
import type { PermissaoDoEscopo } from './acesso';
import { ler, pedir, type ContextoDeAcesso } from './http';

const BASE = '/v1/admin/usuarios';

export type SituacaoDaConta = 'Ativa' | 'AguardandoLiberacao' | 'Desativada';

export type UsuarioNaAdministracao = {
  chave: string;
  nome: string;
  nomePrincipal: string;
  natureza: string;
  filialCodigo: string;
  filialNome: string;
  estaAtivo: boolean;
  aguardandoLiberacaoDesde: string | null;
  ultimoLoginEm: string | null;
  perfis: string[];
};

export type ConcessaoNaAdministracao = {
  id: number;
  perfilCodigo: string;
  perfilNome: string;
  filialCodigo: string | null;
  filialNome: string | null;
  justificativa: string;
  concedidaEm: string;
  concedidaPor: string;
  expiraEm: string | null;
  revogadaEm: string | null;
  revogadaPor: string | null;
  motivoDaRevogacao: string | null;
  vigente: boolean;
};

export type UsuarioDetalhado = { usuario: UsuarioNaAdministracao; concessoes: ConcessaoNaAdministracao[] };

export type PerfilParaConceder = {
  codigo: string;
  nome: string;
  descricao: string | null;
  ehPadrao: boolean;
  /** Se quem pergunta tem as permissões dele — só esses aparecem no formulário. */
  podeConceder: boolean;
  permissoes: PermissaoDoEscopo[];
};

export type NovaConcessao = { perfilCodigo: string; filialCodigo: string; validaAte: string; justificativa: string };

export function listarUsuarios(
  contexto: ContextoDeAcesso,
  consulta: { situacao: SituacaoDaConta; termo: string; pagina: number; tamanho: number },
  sinal?: AbortSignal,
): Promise<ComProcedencia<PaginaDe<UsuarioNaAdministracao>>> {
  return ler<PaginaDe<UsuarioNaAdministracao>>(BASE, contexto, {
    sinal,
    parametros: { situacao: consulta.situacao, termo: consulta.termo || undefined, pagina: consulta.pagina, tamanho: consulta.tamanho },
  });
}

export function obterUsuario(contexto: ContextoDeAcesso, chave: string, sinal?: AbortSignal): Promise<ComProcedencia<UsuarioDetalhado>> {
  return ler<UsuarioDetalhado>(`${BASE}/${chave}`, contexto, { sinal });
}

export function listarPerfisParaConceder(contexto: ContextoDeAcesso, sinal?: AbortSignal): Promise<ComProcedencia<PerfilParaConceder[]>> {
  return ler<PerfilParaConceder[]>('/v1/admin/perfis', contexto, { sinal });
}

export function liberarUsuario(contexto: ContextoDeAcesso, chave: string, filialCodigo: string) {
  return pedir<UsuarioDetalhado>(`${BASE}/${chave}/liberacao`, contexto, { metodo: 'POST', corpo: { filialCodigo } });
}

export function concederPerfil(contexto: ContextoDeAcesso, chave: string, corpo: NovaConcessao) {
  return pedir<UsuarioDetalhado>(`${BASE}/${chave}/concessoes`, contexto, {
    metodo: 'POST',
    corpo: { ...corpo, filialCodigo: corpo.filialCodigo || null, validaAte: corpo.validaAte || null },
  });
}

export function revogarConcessao(contexto: ContextoDeAcesso, chave: string, concessaoId: number, motivo: string) {
  return pedir<UsuarioDetalhado>(`${BASE}/${chave}/concessoes/${concessaoId}/revogacao`, contexto, { metodo: 'POST', corpo: { motivo } });
}

export function desativarUsuario(contexto: ContextoDeAcesso, chave: string) {
  return pedir<UsuarioDetalhado>(`${BASE}/${chave}/desativacao`, contexto, { metodo: 'POST' });
}

export function reativarUsuario(contexto: ContextoDeAcesso, chave: string) {
  return pedir<UsuarioDetalhado>(`${BASE}/${chave}/reativacao`, contexto, { metodo: 'POST' });
}
