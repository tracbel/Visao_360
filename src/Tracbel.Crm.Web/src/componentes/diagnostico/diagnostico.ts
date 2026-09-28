/**
 * O que as peças do Diagnóstico Comercial compartilham (issue 257): as cinco classes com a cor de cada uma, os sete
 * componentes, a formatação dos números, a ordenação da tabela e as linhas do CSV.
 *
 * AS CORES MORAM AQUI, e não só no CSS, porque o mapa pinta o polígono pelo atributo `fill` do SVG, e a barra da
 * distribuição e o selo da tabela precisam dizer a mesma cor para a mesma classe.
 */

import type { ClasseDePrioridade, ComponentesDoIoc, MunicipioNoDiagnostico } from '../../tipos/mercado';

/**
 * As cinco classes: o nome inteiro nos cartões e no CSV, o curto no selo da tabela, a faixa do IOC e a cor do mapa.
 *
 * A COR DO MAPA É UMA ESCALA ORDENADA, do verde escuro (máxima) ao cinza (manutenção): o olho lê "mais escuro, mais a
 * ganhar" sem legenda. O nome da classe vai sempre escrito ao lado — a cor sozinha não diz nada a quem não a distingue.
 */
export const CLASSES: { chave: ClasseDePrioridade; rotulo: string; curto: string; faixa: string; cor: string }[] = [
  { chave: 'Maxima', rotulo: 'Prioridade máxima', curto: 'Máxima', faixa: 'IOC 80 ou mais', cor: '#1B5E20' },
  { chave: 'Alta', rotulo: 'Alta prioridade', curto: 'Alta', faixa: 'IOC 60 a 80', cor: '#4A8A4F' },
  { chave: 'Moderada', rotulo: 'Prioridade moderada', curto: 'Moderada', faixa: 'IOC 40 a 60', cor: '#D9A400' },
  { chave: 'Baixa', rotulo: 'Baixa prioridade', curto: 'Baixa', faixa: 'IOC 20 a 40', cor: '#E8743B' },
  { chave: 'Manutencao', rotulo: 'Manutenção', curto: 'Manutenção', faixa: 'IOC abaixo de 20', cor: '#B8C0B8' },
];

export const ROTULO_DA_CLASSE = Object.fromEntries(CLASSES.map((c) => [c.chave, c.rotulo])) as Record<ClasseDePrioridade, string>;
export const CURTO_DA_CLASSE = Object.fromEntries(CLASSES.map((c) => [c.chave, c.curto])) as Record<ClasseDePrioridade, string>;
export const COR_DA_CLASSE = Object.fromEntries(CLASSES.map((c) => [c.chave, c.cor])) as Record<ClasseDePrioridade, string>;

/** Os sete componentes, na ordem dos pesos do protótipo. */
export const COMPONENTES: { chave: keyof ComponentesDoIoc; rotulo: string; mede: string }[] = [
  { chave: 'potencial', rotulo: 'Potencial', mede: 'a demanda do município sobre o percentil 90 da ADR' },
  { chave: 'cobertura', rotulo: 'Cobertura', mede: 'os vínculos da carteira fora da cadência' },
  { chave: 'credito', rotulo: 'Crédito', mede: 'o crédito de mecanização (SICOR), de −40% a +40%' },
  { chave: 'rentabilidade', rotulo: 'Rentabilidade', mede: 'o preço da cultura principal, de −40% a +40%' },
  { chave: 'clientes', rotulo: 'Clientes', mede: 'poucos clientes frente à demanda' },
  { chave: 'realizacao', rotulo: 'Realização', mede: 'a distância até a meta de planejamento (share-alvo)' },
  { chave: 'penetracao', rotulo: 'Penetração', mede: 'a venda da Tracbel sobre a demanda estrutural' },
];

export const SEM_ART = 'O ART não trouxe as vendas de máquina deste recorte: ausência de carga não é venda zero.';

export const n = (v: number, casas = 1) => v.toLocaleString('pt-BR', { maximumFractionDigits: casas });
export const pct = (v: number) => `${n(v * 100, 0)}%`;

/** O índice de momento (1,00 é estável) como variação: 1,12 vira "+12%". */
export const variacao = (indice: number) => {
  const d = (indice - 1) * 100;
  return `${d > 0 ? '+' : d < 0 ? '−' : ''}${n(Math.abs(d), 0)}%`;
};

/** "2026-08-01" vira "08/2026". */
export const mes = (aaaammdd: string) => `${aaaammdd.slice(5, 7)}/${aaaammdd.slice(0, 4)}`;

/** "2026-09-27" vira "27/09/2026". */
export function dataCurta(aaaammdd: string) {
  const [ano, mesDoAno, dia] = aaaammdd.split('-');
  return `${dia}/${mesDoAno}/${ano}`;
}

/** O plano de ação vem em frases separadas por " · " — cada uma vira uma etiqueta. */
export const acoesDoPlano = (plano: string) =>
  plano
    .split('·')
    .map((a) => a.trim())
    .filter(Boolean);

export type ColunaDoDiagnostico =
  | 'nome'
  | 'culturaPrincipal'
  | 'ioc'
  | 'demandaEstrutural'
  | 'metaDePlanejamento'
  | 'vendidasNoPeriodo'
  | 'clientes'
  | 'cobertura'
  | 'penetracao'
  | 'indiceDeCredito';

/** Ordena com os vazios no fim, nos dois sentidos — "sem dado" nunca é o maior nem o menor. */
export function ordenar(
  linhas: MunicipioNoDiagnostico[],
  coluna: ColunaDoDiagnostico,
  sentido: 1 | -1,
): MunicipioNoDiagnostico[] {
  return [...linhas].sort((a, b) => {
    const va = a[coluna];
    const vb = b[coluna];
    if (va === null && vb === null) return 0;
    if (va === null) return 1;
    if (vb === null) return -1;
    if (typeof va === 'string' && typeof vb === 'string') return sentido * va.localeCompare(vb, 'pt-BR');
    return sentido * ((va as number) - (vb as number));
  });
}

export const CABECALHO_DO_CSV = [
  'Município', 'Região', 'Loja', 'Cultura principal', 'IOC', 'Classe', 'Demanda estrutural (máq/ano)', 'Demanda ajustada',
  'Meta de planejamento', 'Vendidas no período', 'Vendidas no ano', 'Clientes', 'Cobertura (%)', 'Penetração (%)',
  'Índice de crédito', 'Índice de preço', 'Situação', 'Plano de ação', 'Fora da conta', 'Clientes A', 'Clientes B',
  'Clientes C', 'Clientes D', 'Clientes sem classe', 'Clientes em carteira', 'Compraram no período',
];

/**
 * As linhas da tela como o CSV as leva — a mesma ordem e os mesmos filtros.
 *
 * AS COLUNAS NOVAS VÃO NO FIM (28/09/2026): quem já abria o arquivo pela posição das colunas continua achando cada
 * uma onde estava.
 */
export function linhasDoCsv(linhas: MunicipioNoDiagnostico[]): unknown[][] {
  const num = (v: number | null, casas = 2) =>
    v === null ? '' : v.toLocaleString('pt-BR', { maximumFractionDigits: casas, useGrouping: false });
  return linhas.map((l) => [
    l.nome,
    l.regiao,
    l.loja ?? '',
    l.culturaPrincipal ?? '',
    num(l.ioc, 1),
    l.classe ? ROTULO_DA_CLASSE[l.classe] : '',
    num(l.demandaEstrutural),
    num(l.demandaAjustada),
    num(l.metaDePlanejamento),
    l.vendidasNoPeriodo ?? '',
    num(l.vendidasNoAno),
    l.clientes,
    l.cobertura === null ? '' : num(l.cobertura * 100, 1),
    l.penetracao === null ? '' : num(l.penetracao * 100, 1),
    num(l.indiceDeCredito, 3),
    num(l.indiceDePreco, 3),
    l.situacao,
    l.planoDeAcao,
    l.componentesAusentes.join(' | '),
    l.clientesPorClasse?.a ?? '',
    l.clientesPorClasse?.b ?? '',
    l.clientesPorClasse?.c ?? '',
    l.clientesPorClasse?.d ?? '',
    l.clientesPorClasse?.semClasse ?? '',
    l.clientesEmCarteira ?? '',
    l.clientesQueCompraram,
  ]);
}
