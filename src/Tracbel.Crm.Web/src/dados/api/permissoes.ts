/**
 * O que o perfil de quem está usando permite, lido do escopo efetivo (`/api/v1/acesso/escopo`).
 *
 * SERVE PARA ESCONDER O BOTÃO, NÃO PARA PROTEGER. Quem protege é a API: toda rota confere a permissão e
 * responde 403. Aqui a tela só evita oferecer um formulário que vai ser recusado — e diz por quê.
 */

import { obterEscopo } from './acesso';
import { useContextoDeAcesso } from './contexto';
import { useRecurso } from './useRecurso';

/** Os códigos que as telas do potencial conferem — os mesmos de `Permissoes` no servidor. */
export const PERMISSAO = {
  parametroDoPotencialLer: 'ParametroDoPotencial.Ler',
  parametroDoPotencialAdministrar: 'ParametroDoPotencial.Administrar',
  percepcaoDoGestorInformar: 'PercepcaoDoGestor.Informar',
} as const;

export type PermissoesDoUsuario = {
  /** Enquanto o escopo não chegou, nenhuma permissão é presumida. */
  carregando: boolean;
  tem: (codigo: string) => boolean;
};

/** As permissões da filial atual. */
export function usePermissoes(): PermissoesDoUsuario {
  const { contexto } = useContextoDeAcesso();
  const escopo = useRecurso((sinal) => obterEscopo(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const codigos = new Set(escopo.dados?.permissoes.map((p) => p.codigo) ?? []);

  return { carregando: escopo.carregando, tem: (codigo) => codigos.has(codigo) };
}
