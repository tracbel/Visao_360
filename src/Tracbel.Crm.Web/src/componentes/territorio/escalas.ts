/**
 * As escalas de cor dos três mapas — e os três estados que nunca se confundem.
 *
 * ZERO, SEM DADO E FORA DA ADR SÃO COISAS DIFERENTES, e cada um tem aparência
 * própria em todos os mapas:
 *
 * - **zero** é um valor: entra na escala, na primeira faixa, com rótulo "0";
 * - **sem dado** é a ausência de base para medir (nenhum vínculo elegível, área
 *   não divulgada): hachurado cinza, fora da escala;
 * - **fora da ADR** é contexto geográfico: cinza muito claro, sem valor nenhum
 *   pintado — mesmo quando há cliente lá, o número vai para o detalhe e não
 *   para a cor.
 *
 * AS FAIXAS SÃO FIXAS, e não quantis da resposta: com quantis, trocar o filtro
 * de período mudaria o que "vermelho" significa, e duas capturas de tela da
 * mesma cidade não seriam comparáveis.
 */

export type Faixa = {
  /** Limite superior, inclusive. `Infinity` na última. */
  ate: number;
  cor: string;
  rotulo: string;
};

export const COR_FORA_DA_ADR = '#EEF1EC';
export const COR_BORDA_ADR = '#1B5E20';
export const ID_HACHURA_SEM_DADO = 'territorio-hachura-sem-dado';

/**
 * Cobertura em percentual — a parte dos vínculos elegíveis com contato no prazo da cadência.
 *
 * VERMELHO É POUCO COBERTO, VERDE É COBERTO: a escala divergente da referência. É o complemento da
 * pendência na mesma base (elegíveis), e por isso as duas nunca aparecem ao mesmo tempo no mapa.
 */
export const FAIXAS_COBERTURA_PERCENTUAL: Faixa[] = [
  { ate: 25, cor: '#B91C1C', rotulo: 'até 25%' },
  { ate: 50, cor: '#F97316', rotulo: '25 a 50%' },
  { ate: 75, cor: '#FACC15', rotulo: '50 a 75%' },
  { ate: 90, cor: '#84CC16', rotulo: '75 a 90%' },
  { ate: Infinity, cor: '#15803D', rotulo: 'acima de 90%' },
];

/** Pendência de visita em percentual: branco-avermelhado a vermelho escuro. */
export const FAIXAS_PENDENCIA_PERCENTUAL: Faixa[] = [
  { ate: 0, cor: '#FFF5F5', rotulo: '0%' },
  { ate: 10, cor: '#FEE2E2', rotulo: 'até 10%' },
  { ate: 25, cor: '#FCA5A5', rotulo: 'até 25%' },
  { ate: 50, cor: '#F87171', rotulo: 'até 50%' },
  { ate: 75, cor: '#DC2626', rotulo: 'até 75%' },
  { ate: Infinity, cor: '#7F1D1D', rotulo: 'acima de 75%' },
];

/** Pendência em quantidade de vínculos. */
export const FAIXAS_PENDENCIA_QUANTIDADE: Faixa[] = [
  { ate: 0, cor: '#FFF5F5', rotulo: '0' },
  { ate: 5, cor: '#FEE2E2', rotulo: '1 a 5' },
  { ate: 20, cor: '#FCA5A5', rotulo: '6 a 20' },
  { ate: 50, cor: '#F87171', rotulo: '21 a 50' },
  { ate: 150, cor: '#DC2626', rotulo: '51 a 150' },
  { ate: Infinity, cor: '#7F1D1D', rotulo: 'mais de 150' },
];

/** Vendas em reais. */
export const FAIXAS_VENDAS: Faixa[] = [
  { ate: 0, cor: '#F5F9FF', rotulo: 'R$ 0' },
  { ate: 100_000, cor: '#DBEAFE', rotulo: 'até 100 mil' },
  { ate: 500_000, cor: '#93C5FD', rotulo: 'até 500 mil' },
  { ate: 2_000_000, cor: '#60A5FA', rotulo: 'até 2 mi' },
  { ate: 5_000_000, cor: '#2563EB', rotulo: 'até 5 mi' },
  { ate: 15_000_000, cor: '#1D4ED8', rotulo: 'até 15 mi' },
  { ate: Infinity, cor: '#1E3A8A', rotulo: 'acima de 15 mi' },
];

/** Máquinas teóricas pela regra de potencial. */
export const FAIXAS_POTENCIAL: Faixa[] = [
  { ate: 0, cor: '#FAF5FF', rotulo: '0' },
  { ate: 10, cor: '#EDE9FE', rotulo: 'até 10' },
  { ate: 50, cor: '#C4B5FD', rotulo: 'até 50' },
  { ate: 200, cor: '#A78BFA', rotulo: 'até 200' },
  { ate: 500, cor: '#7C3AED', rotulo: 'até 500' },
  { ate: Infinity, cor: '#4C1D95', rotulo: 'acima de 500' },
];

/**
 * Tratores existentes (Censo Agropecuário). Faixas calibradas no dado real de São
 * Paulo: 175.433 tratores em 633 municípios, mediana perto de 180.
 */
export const FAIXAS_TRATORES: Faixa[] = [
  { ate: 0, cor: '#FFF7ED', rotulo: '0' },
  { ate: 50, cor: '#FFEDD5', rotulo: 'até 50' },
  { ate: 150, cor: '#FDBA74', rotulo: 'até 150' },
  { ate: 400, cor: '#FB923C', rotulo: 'até 400' },
  { ate: 1_000, cor: '#EA580C', rotulo: 'até 1.000' },
  { ate: Infinity, cor: '#7C2D12', rotulo: 'acima de 1.000' },
];

/**
 * Tratores por mil km² — a DENSIDADE do parque.
 *
 * Sem ela, o mapa de tratores é quase um mapa de tamanho do município: quem tem
 * mais terra tem mais máquina, e isso não diz nada sobre quem mecaniza mais.
 */
export const FAIXAS_DENSIDADE_DE_TRATORES: Faixa[] = [
  { ate: 0, cor: '#FFF7ED', rotulo: '0' },
  { ate: 200, cor: '#FFEDD5', rotulo: 'até 200' },
  { ate: 500, cor: '#FDBA74', rotulo: 'até 500' },
  { ate: 1_000, cor: '#FB923C', rotulo: 'até 1.000' },
  { ate: 2_000, cor: '#EA580C', rotulo: 'até 2.000' },
  { ate: Infinity, cor: '#7C2D12', rotulo: 'acima de 2.000' },
];

/** Estabelecimentos agropecuários (Censo). São Paulo tem 188.620 em 640 municípios. */
export const FAIXAS_ESTABELECIMENTOS: Faixa[] = [
  { ate: 0, cor: '#F0FDF4', rotulo: '0' },
  { ate: 100, cor: '#DCFCE7', rotulo: 'até 100' },
  { ate: 300, cor: '#86EFAC', rotulo: 'até 300' },
  { ate: 700, cor: '#4ADE80', rotulo: 'até 700' },
  { ate: 1_500, cor: '#16A34A', rotulo: 'até 1.500' },
  { ate: Infinity, cor: '#14532D', rotulo: 'acima de 1.500' },
];

/** Efetivo de bovinos (Pesquisa da Pecuária Municipal), em cabeças. */
export const FAIXAS_REBANHO: Faixa[] = [
  { ate: 0, cor: '#FEFCE8', rotulo: '0' },
  { ate: 5_000, cor: '#FEF9C3', rotulo: 'até 5 mil' },
  { ate: 20_000, cor: '#FDE047', rotulo: 'até 20 mil' },
  { ate: 50_000, cor: '#EAB308', rotulo: 'até 50 mil' },
  { ate: 100_000, cor: '#A16207', rotulo: 'até 100 mil' },
  { ate: Infinity, cor: '#713F12', rotulo: 'acima de 100 mil' },
];

/**
 * Capacidade instalada de etanol, em m³/dia (ANP).
 *
 * Zero tem cor própria e significa "tem usina, e ela está parada ou não informou"
 * — município SEM usina cai em "sem dado", hachurado, porque ausência na lista da
 * ANP não é a mesma coisa que capacidade nula.
 */
export const FAIXAS_USINAS: Faixa[] = [
  { ate: 0, cor: '#F5F3FF', rotulo: 'parada' },
  { ate: 500, cor: '#DDD6FE', rotulo: 'até 500' },
  { ate: 1_500, cor: '#C4B5FD', rotulo: 'até 1.500' },
  { ate: 3_000, cor: '#8B5CF6', rotulo: 'até 3.000' },
  { ate: Infinity, cor: '#5B21B6', rotulo: 'acima de 3.000' },
];

/**
 * Valor da produção agrícola, em MIL reais — como o IBGE publica.
 *
 * É o que o município COLHE, não o que a Tracbel vende: as faixas são outras e a
 * cor é outra, para que ninguém confunda este mapa com o de vendas.
 */
export const FAIXAS_VALOR_DA_PRODUCAO: Faixa[] = [
  { ate: 0, cor: '#F0FDFA', rotulo: 'R$ 0' },
  { ate: 20_000, cor: '#CCFBF1', rotulo: 'até 20 mi' },
  { ate: 100_000, cor: '#5EEAD4', rotulo: 'até 100 mi' },
  { ate: 300_000, cor: '#14B8A6', rotulo: 'até 300 mi' },
  { ate: 800_000, cor: '#0F766E', rotulo: 'até 800 mi' },
  { ate: Infinity, cor: '#134E4A', rotulo: 'acima de 800 mi' },
];

/** Área plantada, em hectares. */
export const FAIXAS_AREA_PLANTADA: Faixa[] = [
  { ate: 0, cor: '#F7FEE7', rotulo: '0' },
  { ate: 2_000, cor: '#ECFCCB', rotulo: 'até 2 mil' },
  { ate: 10_000, cor: '#BEF264', rotulo: 'até 10 mil' },
  { ate: 30_000, cor: '#84CC16', rotulo: 'até 30 mil' },
  { ate: 60_000, cor: '#4D7C0F', rotulo: 'até 60 mil' },
  { ate: Infinity, cor: '#1A2E05', rotulo: 'acima de 60 mil' },
];

/** A faixa de um valor. Valor negativo (estorno maior que a venda) cai na primeira. */
export function faixaDe(faixas: Faixa[], valor: number): Faixa {
  return faixas.find((f) => valor <= f.ate) ?? faixas[faixas.length - 1];
}

/** "R$ 6,3 mi", "R$ 480 mil", "R$ 950" — o valor como a diretoria o lê. */
export function reaisCompactos(valor: number): string {
  const abs = Math.abs(valor);
  if (abs >= 1_000_000) return `R$ ${(valor / 1_000_000).toLocaleString('pt-BR', { maximumFractionDigits: 1 })} mi`;
  if (abs >= 1_000) return `R$ ${(valor / 1_000).toLocaleString('pt-BR', { maximumFractionDigits: 0 })} mil`;
  return `R$ ${valor.toLocaleString('pt-BR', { maximumFractionDigits: 0 })}`;
}

/**
 * O valor da produção do IBGE, que vem em MIL reais, escrito em reais de verdade.
 *
 * Esquecer a multiplicação por mil erraria o número em três ordens de grandeza —
 * a lavoura da região viraria R$ 50 mil em vez de R$ 50 bilhões. Por isso a
 * conversão mora aqui, num lugar só, e não em cada tela.
 */
export function reaisDaProducao(milReais: number): string {
  const reais = milReais * 1_000;
  if (Math.abs(reais) >= 1_000_000_000)
    return `R$ ${(reais / 1_000_000_000).toLocaleString('pt-BR', { maximumFractionDigits: 1 })} bi`;
  return reaisCompactos(reais);
}
