/**
 * AS PERMISSÕES AGRUPADAS POR ÁREA, como a pessoa entende — usado em "Minha conta" e no editor de perfis
 * (issues 134 e 113). A área sai do começo do código (`Cliente.Ler` → Clientes) e a ordem das ações é a natural
 * (Ler, Cadastrar, Alterar, Excluir), e não a alfabética.
 */

/** A profundidade como a pessoa entende — o mesmo texto que o servidor usa nas recusas. */
export const ALCANCE: Record<string, string> = {
  Organizacao: 'toda a organização',
  EmpresaEAbaixo: 'a filial escolhida e as que estão abaixo dela',
  Empresa: 'só a filial escolhida',
  Equipe: 'você e a sua equipe',
  Proprios: 'só os seus registros',
};

/** As profundidades que o editor oferece, da menor para a maior. */
export const PROFUNDIDADES = ['Proprios', 'Equipe', 'Empresa', 'EmpresaEAbaixo', 'Organizacao'] as const;

const AREAS: [prefixo: string, nome: string][] = [
  ['Cliente', 'Clientes'],
  ['Equipamento', 'Equipamentos'],
  ['Processo', 'Oportunidades'],
  ['Tarefa', 'Agenda'],
  ['Interacao', 'Interações'],
  ['Cobertura', 'Cobertura de carteira'],
  ['Relatorio', 'Relatórios'],
  ['Faturamento', 'Faturamento'],
  ['Territorio', 'Território'],
  ['ParametroDoPotencial', 'Potencial de mercado'],
  ['PercepcaoDoGestor', 'Potencial de mercado'],
  ['Catalogo', 'Catálogos'],
  ['Empresa', 'Filiais'],
  ['Usuario', 'Usuários'],
  ['Perfil', 'Perfis'],
  ['Integracao', 'Integrações'],
  ['Auditoria', 'Auditoria'],
  ['Legado', 'Sistema legado'],
];

const ORDEM_DAS_ACOES = ['Ler', 'Criar', 'Editar', 'Excluir', 'Informar', 'AlcanceEntreFiliais', 'Administrar'];

export type Area<T extends { codigo: string }> = { nome: string; itens: T[] };

/** Agrupa por área, na ordem das áreas e com as ações na ordem natural dentro de cada uma. */
export function agruparPorArea<T extends { codigo: string }>(itens: T[]): Area<T>[] {
  const areas = new Map<string, T[]>();
  for (const item of itens) {
    const prefixo = item.codigo.split('.')[0];
    const nome = AREAS.find(([p]) => p === prefixo)?.[1] ?? 'Outras';
    areas.set(nome, [...(areas.get(nome) ?? []), item]);
  }
  const posicao = (item: T) => {
    const i = ORDEM_DAS_ACOES.indexOf(item.codigo.split('.')[1] ?? '');
    return i < 0 ? ORDEM_DAS_ACOES.length : i;
  };
  const ordem = [...new Set(AREAS.map(([, nome]) => nome)), 'Outras'];
  return ordem
    .filter((nome) => areas.has(nome))
    .map((nome) => ({ nome, itens: [...areas.get(nome)!].sort((a, b) => posicao(a) - posicao(b)) }));
}
