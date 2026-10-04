/** O que as peças da Demanda e previsão (issue 258) compartilham: o nome dos meses, a formatação e o CSV da matriz. */

import type { DemandaEPrevisaoDaRegiao } from '../../tipos/mercado';
import { MESES_CURTOS, formatarNumeroComCasas } from '../../dados/formatadores';

/** O mês do calendário (1 a 12) pelo nome curto — a lista da API já vem na ordem do ano fiscal, de novembro a outubro. */
export const NOME_DO_MES = ['', ...MESES_CURTOS];
export const NOME_DO_MES_POR_EXTENSO = [
  '', 'janeiro', 'fevereiro', 'março', 'abril', 'maio', 'junho', 'julho', 'agosto', 'setembro', 'outubro', 'novembro', 'dezembro',
];

export const n = (v: number, casas = 1) => formatarNumeroComCasas(v, casas);

/** 1,12 vira "+12%"; 0,95 vira "−5%". */
export const variacaoDoFator = (fator: number) => {
  const d = (fator - 1) * 100;
  return `${d > 0 ? '+' : d < 0 ? '−' : ''}${n(Math.abs(d), 0)}%`;
};

/** Uma variação já em pontos percentuais: 8 vira "+8%". O sinal é o do número ARREDONDADO: −0,3 vira "0%", e não "−0%". */
export const variacaoPercentual = (v: number) => {
  const r = Math.round(v);
  return `${r > 0 ? '+' : r < 0 ? '−' : ''}${n(Math.abs(r), 0)}%`;
};

/** O CSV da matriz: uma linha por município, com todas as culturas — e não só a página. */
export function cabecalhoDoCsv(dados: DemandaEPrevisaoDaRegiao): string[] {
  return [
    'Município', 'Região', 'Loja', 'Área (ha)', 'Parque', ...dados.culturas.map((c) => `Demanda ${c.nome}`), 'Demanda/ano',
    'Demanda ajustada', 'A entregar', 'A entregar ajustada', 'Fator preço', 'Fator crédito', 'Efeito líquido (%)', 'Cultura predominante',
  ];
}

export function linhasDoCsv(dados: DemandaEPrevisaoDaRegiao): unknown[][] {
  const num = (v: number | null, casas = 2) =>
    v === null ? '' : v.toLocaleString('pt-BR', { maximumFractionDigits: casas, useGrouping: false });
  return dados.municipios.map((m) => [
    m.nome,
    m.regiao,
    m.loja ?? '',
    num(m.areaUtilHectares, 0),
    num(m.parque),
    ...dados.culturas.map((c) => num(m.porCultura.find((p) => p.culturaCodigo === c.codigo)?.demanda ?? null)),
    num(m.demandaEstrutural),
    num(m.demandaAjustada),
    num(m.aEntregar),
    num(m.aEntregarAjustada),
    num(m.fatorDePreco, 3),
    num(m.fatorDeCredito, 3),
    num(m.variacaoPercentual, 1),
    m.culturaPredominante ?? '',
  ]);
}
