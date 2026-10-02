/**
 * O que as peças do Dimensionamento da ADR (issue 259) compartilham: a formatação das grandezas da PAM, as faixas de cor do
 * mapa e o CSV da matriz municipal.
 *
 * O VALOR DA PRODUÇÃO CHEGA EM MIL REAIS, como o IBGE publica: R$ 3.350 mil é "R$ 3,4 mi". A conversão mora aqui, uma vez,
 * para nenhum bloco mostrar o número da PAM sem a escala.
 */

import type { DimensionamentoDaAdr, MedidaComAnterior } from '../../tipos/mercado';

export const n = (v: number, casas = 0) => v.toLocaleString('pt-BR', { maximumFractionDigits: casas });

/** Hectares: "12.345 ha", e acima de um milhão "1,23 mi ha". */
export function hectares(v: number | null): string {
  if (v === null) return '—';
  return Math.abs(v) >= 1_000_000 ? `${n(v / 1_000_000, 2)} mi ha` : `${n(v)} ha`;
}

/** Toneladas: "12.345 t", e acima de um milhão "1,2 mi t". */
export function toneladas(v: number | null): string {
  if (v === null) return '—';
  return Math.abs(v) >= 1_000_000 ? `${n(v / 1_000_000, 1)} mi t` : `${n(v)} t`;
}

/** O valor da PAM, que chega em MIL reais: 950 → "R$ 950 mil"; 3.350 → "R$ 3,4 mi"; 1.200.000 → "R$ 1,2 bi". */
export function reaisDeMil(v: number | null): string {
  if (v === null) return '—';
  const abs = Math.abs(v);
  if (abs >= 1_000_000) return `R$ ${n(v / 1_000_000, 1)} bi`;
  if (abs >= 1_000) return `R$ ${n(v / 1_000, 1)} mi`;
  return `R$ ${n(v)} mil`;
}

/** Reais inteiros, para a densidade: "R$ 9.306". */
export const reais = (v: number | null) => (v === null ? '—' : `R$ ${n(v)}`);

/** Um percentual já em pontos: 14,26 → "14,3%". */
export const pct = (v: number | null, casas = 1) => (v === null ? '—' : `${n(v, casas)}%`);

/** Uma variação já em pontos, com sinal: 3,2 → "+3,2%"; −5 → "−5%". */
export const variacao = (v: number | null) =>
  v === null ? '—' : `${v > 0 ? '+' : v < 0 ? '−' : ''}${n(Math.abs(v), 1)}%`;

/** O sentido da variação, para a seta: a cor só reforça o sinal escrito. */
export const sentidoDe = (v: number | null) => (v === null ? '' : v > 0.05 ? 'sobe' : v < -0.05 ? 'desce' : '');

/** Uma data da API (UTC) como o comercial lê: "02/10/2026". */
export function dataCurta(iso: string | null): string {
  if (!iso) return '—';
  const [ano, mes, dia] = iso.slice(0, 10).split('-');
  return `${dia}/${mes}/${ano}`;
}

/** "vs 2023 +3,2%", ou o traço com o ano quando não há base. */
export function contraOAnterior(medida: MedidaComAnterior, anoAnterior: number | null): string {
  const ano = anoAnterior ?? 'o ano anterior';
  return medida.variacaoPercentual === null ? `vs ${ano} —` : `vs ${ano} ${variacao(medida.variacaoPercentual)}`;
}

// ------------------------------------------------------------------------------------------------
// A ordenação das tabelas
// ------------------------------------------------------------------------------------------------

export type Ordem<C extends string> = { coluna: C; sentido: 1 | -1 };

/** Clicar na coluna já escolhida inverte; numa nova, texto começa de A a Z e número do maior para o menor. */
export function proximaOrdem<C extends string>(atual: Ordem<C>, coluna: C, texto: boolean): Ordem<C> {
  return atual.coluna === coluna ? { coluna, sentido: atual.sentido === 1 ? -1 : 1 } : { coluna, sentido: texto ? 1 : -1 };
}

/** Ordena sem mexer na lista recebida; o vazio vai sempre para o fim, nos dois sentidos. */
export function ordenar<T, C extends string>(linhas: readonly T[], ordem: Ordem<C>, valor: (linha: T, coluna: C) => number | string | null): T[] {
  return [...linhas].sort((a, b) => {
    const va = valor(a, ordem.coluna);
    const vb = valor(b, ordem.coluna);
    if (va === null && vb === null) return 0;
    if (va === null) return 1;
    if (vb === null) return -1;
    if (typeof va === 'string' && typeof vb === 'string') return ordem.sentido * va.localeCompare(vb, 'pt-BR');
    return ordem.sentido * ((va as number) - (vb as number));
  });
}

// ------------------------------------------------------------------------------------------------
// O mapa estratégico
// ------------------------------------------------------------------------------------------------

export type VariavelDoMapa = 'valor' | 'area' | 'quantidade' | 'clientes';

export const VARIAVEIS_DO_MAPA: readonly { id: VariavelDoMapa; rotulo: string }[] = [
  { id: 'valor', rotulo: 'Produção' },
  { id: 'area', rotulo: 'Área' },
  { id: 'quantidade', rotulo: 'Qtd. produzida' },
  { id: 'clientes', rotulo: 'Clientes' },
];

/** O valor de um município na variável do mapa — já formatado, para a dica e a legenda. */
export function formatarNoMapa(variavel: VariavelDoMapa, v: number): string {
  if (variavel === 'valor') return reaisDeMil(v);
  if (variavel === 'area') return hectares(v);
  if (variavel === 'quantidade') return toneladas(v);
  return `${n(v)} clientes`;
}

/** Os tons do recorte, do claro ao escuro — o verde da marca, como o mapa do protótipo. */
export const TONS_DO_FOCO = ['#D7EBDA', '#A9D2B0', '#6FAF7B', '#3B8A4B', '#1B5E2A'];

/** Os tons do resto do estado — cinza, para o recorte saltar à vista sem esconder o tamanho dos vizinhos. */
export const TONS_DOS_DEMAIS = ['#EEF0EC', '#DADDD6', '#C4C9BF'];

/**
 * OS CORTES DAS FAIXAS PELOS QUANTIS dos valores positivos: com a PAM, a cana de um município pesa cem vezes o café de
 * outro, e uma escala linear deixaria o mapa inteiro claro com dois pontos escuros.
 */
export function cortes(valores: number[], faixas: number): number[] {
  const positivos = valores.filter((v) => v > 0).sort((a, b) => a - b);
  if (positivos.length === 0) return [];
  return Array.from({ length: faixas - 1 }, (_, i) => positivos[Math.floor(((i + 1) * positivos.length) / faixas)] ?? positivos[positivos.length - 1]);
}

/** A faixa de um valor pelos cortes: 0 é a mais clara. */
export function faixaDe(v: number, limites: number[]): number {
  let i = 0;
  while (i < limites.length && v > limites[i]) i++;
  return i;
}

// ------------------------------------------------------------------------------------------------
// O CSV da matriz municipal
// ------------------------------------------------------------------------------------------------

/** O cabeçalho do CSV: as 21 colunas do protótipo, mais a região e as usinas. */
export function cabecalhoDaMatriz(dados: DimensionamentoDaAdr): string[] {
  const atual = dados.anoBase ?? 'ano-base';
  const anterior = dados.anoAnterior ?? 'ano anterior';
  return [
    'Município', 'Região', 'Loja', 'Vendedor', 'Cultura principal',
    `Área ${anterior} (ha)`, `Área ${atual} (ha)`, 'Δ Área (%)',
    `Qtd ${anterior} (t)`, `Qtd ${atual} (t)`, 'Δ Qtd (%)',
    `Valor ${anterior} (mil R$)`, `Valor ${atual} (mil R$)`, 'Δ Valor (%)',
    'Part. no recorte (%)', 'Part. em SP (%)',
    'Clientes', 'Cli. A', 'Cli. B', 'Cli. C', 'Cli. D', 'Cli. sem classe', 'Último contato', 'Usinas',
  ];
}

export function linhasDaMatriz(dados: DimensionamentoDaAdr): unknown[][] {
  const num = (v: number | null, casas = 2) =>
    v === null ? '' : v.toLocaleString('pt-BR', { maximumFractionDigits: casas, useGrouping: false });
  return dados.matriz.map((m) => [
    m.nome,
    m.regiao ?? '',
    m.loja ?? '',
    m.vendedor ?? '',
    m.culturaPrincipal ?? '',
    num(m.area.anterior, 0), num(m.area.atual, 0), num(m.area.variacaoPercentual, 1),
    num(m.quantidade.anterior, 0), num(m.quantidade.atual, 0), num(m.quantidade.variacaoPercentual, 1),
    num(m.valor.anterior, 0), num(m.valor.atual, 0), num(m.valor.variacaoPercentual, 1),
    num(m.fatiaNaRegiao), num(m.fatiaNoEstado),
    m.clientes.clientes, m.clientes.a, m.clientes.b, m.clientes.c, m.clientes.d, m.clientes.semClasse,
    m.clientes.ultimoContatoEm ? dataCurta(m.clientes.ultimoContatoEm) : '',
    m.usinas,
  ]);
}
