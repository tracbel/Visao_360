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
 * O QUE AS PARTES SEGUINTES ACRESCENTAM AQUI: Usuários e Perfis (#113),
 * Auditoria (#135) e Taxonomias (#45), cada uma com a permissão que a rota dela
 * exigir.
 */

import { PERMISSAO } from '../../dados/api/permissoes';

export type AbaConfig = 'conta' | 'comercial' | 'administracao';

export type SecaoConfig = 'conta' | 'potencial' | 'fontes' | 'integracoes';

export type DefinicaoDeSecao = {
  id: SecaoConfig;
  aba: AbaConfig;
  icone: string;
  rotulo: string;
  /** Recebe "a pessoa tem esta permissão?" e diz se a seção aparece. */
  visivel: (tem: (codigo: string) => boolean) => boolean;
};

export const ABAS: { id: AbaConfig; rotulo: string }[] = [
  { id: 'conta', rotulo: 'Minha conta' },
  { id: 'comercial', rotulo: 'Comercial' },
  { id: 'administracao', rotulo: 'Administração' },
];

export const SECOES: DefinicaoDeSecao[] = [
  // Todo mundo tem uma conta.
  { id: 'conta', aba: 'conta', icone: '👤', rotulo: 'Minha conta', visivel: () => true },

  // Ler os parâmetros é de todo mundo (issue 71: quem vê o número vê o parâmetro), e isso já aparece nas
  // telas de indicadores. A seção de Configurações é de quem ALTERA alguma coisa neles.
  {
    id: 'potencial',
    aba: 'comercial',
    icone: '📈',
    rotulo: 'Potencial de mercado',
    visivel: (tem) => tem(PERMISSAO.parametroDoPotencialAdministrar) || tem(PERMISSAO.percepcaoDoGestorInformar),
  },

  { id: 'integracoes', aba: 'administracao', icone: '🔌', rotulo: 'Integrações', visivel: (tem) => tem(PERMISSAO.integracaoLer) },
  { id: 'fontes', aba: 'administracao', icone: '🌎', rotulo: 'Fontes públicas', visivel: (tem) => tem(PERMISSAO.integracaoLer) },
];

/** As seções que a pessoa vê, na ordem da lista. */
export function secoesVisiveis(tem: (codigo: string) => boolean): DefinicaoDeSecao[] {
  return SECOES.filter((secao) => secao.visivel(tem));
}

/** As abas com pelo menos uma seção visível, na ordem das abas. */
export function abasVisiveis(tem: (codigo: string) => boolean): { id: AbaConfig; rotulo: string }[] {
  const comSecao = new Set(secoesVisiveis(tem).map((secao) => secao.aba));
  return ABAS.filter((aba) => comSecao.has(aba.id));
}
