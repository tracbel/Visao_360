/**
 * AS SEÇÕES DE CONFIGURAÇÕES E QUEM AS VÊ (issue 134, 22/09/2026).
 *
 * UMA SEÇÃO APARECE PARA QUEM TEM A PERMISSÃO DO QUE ELA DEIXA FAZER — nunca
 * pelo nome do perfil. Os perfis (Padrão, Gerência, Diretoria, Administrador)
 * são pacotes de permissões que o administrador edita; se a regra fosse "se for
 * DIRETORIA, mostra", mexer num perfil deixaria a tela mentindo. Quem protege de
 * verdade é a API, que responde 403: esconder aqui só evita oferecer o que vai
 * ser recusado.
 *
 * A ABA SEM NENHUMA SEÇÃO VISÍVEL NÃO APARECE. Os cadeados do protótipo saíram:
 * mostravam a aba a todos e não conferiam nada.
 *
 * O QUE AS PARTES SEGUINTES ACRESCENTAM AQUI: Taxonomias (#45), com a
 * permissão que a rota dela exigir. A Auditoria (#135) já entrou.
 *
 * OS ÍCONES SÃO OS DO RESTO DO CRM (29/09/2026, #293 bloco 5): eram emojis, que cada sistema desenha de um jeito e
 * que nenhuma outra tela usa. Agora são os mesmos traços do menu e das abas dos Indicadores.
 */

import {
  ClipboardList,
  Globe,
  Plug,
  Settings,
  ShieldCheck,
  TrendingUp,
  UserRound,
  Users,
  type LucideIcon,
} from 'lucide-react';
import { PERMISSAO } from '../../dados/api/permissoes';

export type AbaConfig = 'conta' | 'comercial' | 'administracao';

export type SecaoConfig = 'conta' | 'potencial' | 'usuarios' | 'perfis' | 'auditoria' | 'fontes' | 'integracoes';

export type DefinicaoDeSecao = {
  id: SecaoConfig;
  aba: AbaConfig;
  icone: LucideIcon;
  rotulo: string;
  /** Recebe "a pessoa tem esta permissão?" e diz se a seção aparece. */
  visivel: (tem: (codigo: string) => boolean) => boolean;
};

export type DefinicaoDeAba = { id: AbaConfig; rotulo: string; icone: LucideIcon };

export const ABAS: DefinicaoDeAba[] = [
  { id: 'conta', rotulo: 'Minha conta', icone: UserRound },
  { id: 'comercial', rotulo: 'Comercial', icone: TrendingUp },
  { id: 'administracao', rotulo: 'Administração', icone: Settings },
];

export const SECOES: DefinicaoDeSecao[] = [
  // Todo mundo tem uma conta.
  { id: 'conta', aba: 'conta', icone: UserRound, rotulo: 'Minha conta', visivel: () => true },

  // Ler os parâmetros é de todo mundo (issue 71: quem vê o número vê o parâmetro), e isso já aparece nas
  // telas de indicadores. A seção de Configurações é de quem ALTERA alguma coisa neles.
  {
    id: 'potencial',
    aba: 'comercial',
    icone: TrendingUp,
    rotulo: 'Potencial de mercado',
    visivel: (tem) => tem(PERMISSAO.parametroDoPotencialAdministrar) || tem(PERMISSAO.percepcaoDoGestorInformar),
  },

  // Issue 113: ver é Usuario.Ler (a gerência vê a filial, a diretoria todos); agir é Usuario.Administrar.
  {
    id: 'usuarios',
    aba: 'administracao',
    icone: Users,
    rotulo: 'Usuários',
    visivel: (tem) => tem(PERMISSAO.usuarioLer) || tem(PERMISSAO.usuarioAdministrar),
  },

  // Issue 113 (2b): os perfis próprios e a matriz de permissões — só o Administrador.
  { id: 'perfis', aba: 'administracao', icone: ShieldCheck, rotulo: 'Perfis', visivel: (tem) => tem(PERMISSAO.perfilAdministrar) },

  // Issue 135: a trilha de auditoria — Diretoria e Administrador.
  { id: 'auditoria', aba: 'administracao', icone: ClipboardList, rotulo: 'Auditoria', visivel: (tem) => tem(PERMISSAO.auditoriaLer) },

  { id: 'integracoes', aba: 'administracao', icone: Plug, rotulo: 'Integrações', visivel: (tem) => tem(PERMISSAO.integracaoLer) },
  { id: 'fontes', aba: 'administracao', icone: Globe, rotulo: 'Fontes públicas', visivel: (tem) => tem(PERMISSAO.integracaoLer) },
];

/** As seções que a pessoa vê, na ordem da lista. */
export function secoesVisiveis(tem: (codigo: string) => boolean): DefinicaoDeSecao[] {
  return SECOES.filter((secao) => secao.visivel(tem));
}

/** As abas com pelo menos uma seção visível, na ordem das abas. */
export function abasVisiveis(tem: (codigo: string) => boolean): DefinicaoDeAba[] {
  const comSecao = new Set(secoesVisiveis(tem).map((secao) => secao.aba));
  return ABAS.filter((aba) => comSecao.has(aba.id));
}
