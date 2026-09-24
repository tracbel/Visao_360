/**
 * AS POUCAS CONTAS QUE O MOMENTO FAZ NA TELA — e por que cada uma é permitida.
 *
 * A regra do bloco continua: o número vem pronto da API. O que mora aqui é
 * ORDENAR, ESCOLHER e PONDERAR números que já vieram — nunca criar um índice
 * novo. Cada função diz a regra que a sustenta, e todas têm teste, porque é
 * justamente numa conta de tela que um número plausível e errado nasce sem
 * ninguém ver.
 */

import type { CreditoDeMaquinasNoMunicipio, JanelasDeCredito, RentabilidadeDaCultura } from '../../../tipos/mercado';
import type { ResponsavelPelaCarteira } from '../../../tipos/territorio';

/* ---------------------------------------------------------------------------
   RENTABILIDADE
   --------------------------------------------------------------------------- */

/** O que o "Ordenar por" do ranking oferece — as quatro colunas da maquete. */
export type CriterioDaRentabilidade = 'margem' | 'receita' | 'custo' | 'margemPercentual';

export const CRITERIOS_DA_RENTABILIDADE: readonly { id: CriterioDaRentabilidade; rotulo: string }[] = [
  { id: 'margem', rotulo: 'Margem/ha' },
  { id: 'receita', rotulo: 'Receita/ha' },
  { id: 'custo', rotulo: 'Custo/ha' },
  { id: 'margemPercentual', rotulo: 'Margem %' },
];

/**
 * A margem como fração da receita — "52%" na maquete.
 *
 * Não é número novo: é a razão entre dois que a rota já devolve. Sem receita
 * positiva não há o que dividir, e a resposta é ausência, não zero.
 */
export function margemPercentual(l: RentabilidadeDaCultura): number | null {
  if (l.margemPorHectare === null || l.receitaPorHectare === null || l.receitaPorHectare <= 0) return null;
  return l.margemPorHectare / l.receitaPorHectare;
}

export function valorDoCriterio(l: RentabilidadeDaCultura, criterio: CriterioDaRentabilidade): number | null {
  switch (criterio) {
    case 'margem':
      return l.margemPorHectare;
    case 'receita':
      return l.receitaPorHectare;
    case 'custo':
      return l.custoPorHectare;
    case 'margemPercentual':
      return margemPercentual(l);
  }
}

/**
 * O ranking na ordem pedida, do maior para o menor.
 *
 * QUEM NÃO TEM O NÚMERO VAI PARA O FIM, e não some: uma cultura sem custo
 * apurado continua na lista, com o traço e o motivo — escondê-la faria o
 * ranking parecer completo.
 */
export function ordenarRentabilidade(
  linhas: readonly RentabilidadeDaCultura[],
  criterio: CriterioDaRentabilidade,
): RentabilidadeDaCultura[] {
  return [...linhas].sort((a, b) => {
    const va = valorDoCriterio(a, criterio);
    const vb = valorDoCriterio(b, criterio);
    if (va === null && vb === null) return a.culturaNome.localeCompare(b.culturaNome, 'pt-BR');
    if (va === null) return 1;
    if (vb === null) return -1;
    return vb - va;
  });
}

/**
 * A MARGEM MÉDIA PONDERADA PELA ÁREA COLHIDA — Σ (margem × área) ÷ Σ área.
 *
 * POR QUE PONDERADA, e não a média simples das culturas: a média simples dá à
 * laranja de 12 mil hectares o mesmo peso da cana de 280 mil, e responde a uma
 * pergunta que ninguém fez. Ponderada pela área, ela responde "quanto sobra, em
 * média, por hectare colhido" — que é o que o cartão diz.
 *
 * SÓ ENTRAM AS CULTURAS COM OS DOIS NÚMEROS. Sem margem não há o que pesar, e
 * sem área não há peso; somar uma delas como zero puxaria a média para um lado
 * que ninguém mediu. Sem nenhuma, a resposta é ausência.
 */
export function margemMediaPonderada(
  linhas: readonly { margem: number | null; area: number | null }[],
): { media: number | null; areaTotal: number; culturas: number } {
  const validas = linhas.filter(
    (l): l is { margem: number; area: number } => l.margem !== null && l.area !== null && l.area > 0,
  );
  const areaTotal = validas.reduce((s, l) => s + l.area, 0);
  if (areaTotal <= 0) return { media: null, areaTotal: 0, culturas: 0 };
  return {
    media: validas.reduce((s, l) => s + l.margem * l.area, 0) / areaTotal,
    areaTotal,
    culturas: validas.length,
  };
}

/**
 * A CULTURA DE MAIOR ÁREA — o critério da "Cultura destaque".
 *
 * É CONTEXTO, dito com o critério: "destaque" aqui é a que mais ocupa a terra
 * do recorte, e não a que mais paga (essa é a "Melhor margem/ha", ao lado).
 * Empate fica com a primeira, na ordem em que vieram.
 */
export function daMaiorArea<T>(itens: readonly T[], area: (item: T) => number | null): T | null {
  let melhor: T | null = null;
  let maior = 0;
  for (const item of itens) {
    const a = area(item);
    if (a !== null && a > maior) {
      maior = a;
      melhor = item;
    }
  }
  return melhor;
}

/* ---------------------------------------------------------------------------
   CRÉDITO
   --------------------------------------------------------------------------- */

/** A variação da janela recente sobre a anterior; nula quando a anterior é zero — não há base. */
export function variacao(atual: number, anterior: number): number | null {
  return anterior > 0 ? atual / anterior - 1 : null;
}

/**
 * VALOR ÷ LINHAS — e o nome é esse. Não é ticket médio nem valor por operação:
 * a linha do SICOR já é a soma dos contratos de uma combinação, e não traz
 * quantidade.
 */
export function valorMedioPorLinha(j: JanelasDeCredito): number | null {
  return j.linhas > 0 ? j.valor / j.linhas : null;
}

export function valorMedioAnterior(j: JanelasDeCredito): number | null {
  return j.linhasAnteriores > 0 ? j.valorAnterior / j.linhasAnteriores : null;
}

/** O que o "Ordenar por" do detalhamento de crédito oferece. */
export type CriterioDoCredito = 'valor' | 'linhas' | 'valorMedio' | 'variacao' | 'nome';

export const CRITERIOS_DO_CREDITO: readonly { id: CriterioDoCredito; rotulo: string }[] = [
  { id: 'valor', rotulo: 'Valor financiado' },
  { id: 'linhas', rotulo: 'Linhas do SICOR' },
  { id: 'valorMedio', rotulo: 'Valor médio' },
  { id: 'variacao', rotulo: 'Variação anual' },
  { id: 'nome', rotulo: 'Município (A–Z)' },
];

function valorDoMunicipio(m: CreditoDeMaquinasNoMunicipio, criterio: Exclude<CriterioDoCredito, 'nome'>): number | null {
  switch (criterio) {
    case 'valor':
      return m.janelas.valor;
    case 'linhas':
      return m.janelas.linhas;
    case 'valorMedio':
      return valorMedioPorLinha(m.janelas);
    case 'variacao':
      return variacao(m.janelas.valor, m.janelas.valorAnterior);
  }
}

/** Os municípios na ordem pedida; quem não tem o número vai para o fim. */
export function ordenarMunicipios(
  municipios: readonly CreditoDeMaquinasNoMunicipio[],
  criterio: CriterioDoCredito,
): CreditoDeMaquinasNoMunicipio[] {
  if (criterio === 'nome') return [...municipios].sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR'));
  return [...municipios].sort((a, b) => {
    const va = valorDoMunicipio(a, criterio);
    const vb = valorDoMunicipio(b, criterio);
    if (va === null && vb === null) return 0;
    if (va === null) return 1;
    if (vb === null) return -1;
    return vb - va;
  });
}

/**
 * O TOP 5 DA REGIÃO TRACBEL — só municípios da ADR.
 *
 * O SICOR publica São Paulo inteiro, e um município vizinho grande entraria no
 * topo de uma lista cujo título promete "na sua área de atuação".
 */
export function topDaRegiao(
  municipios: readonly CreditoDeMaquinasNoMunicipio[],
  criterio: 'valor' | 'linhas',
  quantos = 5,
): CreditoDeMaquinasNoMunicipio[] {
  return ordenarMunicipios(
    municipios.filter((m) => m.pertenceAAdr),
    criterio,
  ).slice(0, quantos);
}

/**
 * A PARTICIPAÇÃO de uma parte no total da Região Tracbel — fração, ou nula sem
 * denominador. É fatia do crédito da região, e não participação de mercado.
 */
export function participacao(parte: number, total: number): number | null {
  return total > 0 ? parte / total : null;
}

/* ---------------------------------------------------------------------------
   PERCEPÇÃO COMERCIAL
   --------------------------------------------------------------------------- */

/**
 * O SENTIDO DE UMA LEITURA DO GESTOR — positiva, neutra ou negativa, pelo SINAL.
 *
 * Não há faixa ("muito positiva", "aquecido") porque não há limite registrado
 * para ela; o sinal de um número em pontos percentuais é aritmética, não
 * decisão.
 */
export function sentidoDaLeitura(percentual: number): 'positiva' | 'neutra' | 'negativa' {
  return percentual > 0 ? 'positiva' : percentual < 0 ? 'negativa' : 'neutra';
}

/** Quantos municípios em cada sentido, e quantos da Região sem leitura registrada. */
export function distribuicaoDaPercepcao(
  leituras: readonly { percentual: number }[],
  municipiosDaRegiao: number,
): { positiva: number; neutra: number; negativa: number; semRegistro: number; total: number } {
  const conta = (s: 'positiva' | 'neutra' | 'negativa') => leituras.filter((l) => sentidoDaLeitura(l.percentual) === s).length;
  const registradas = leituras.length;
  const total = Math.max(municipiosDaRegiao, registradas);
  return {
    positiva: conta('positiva'),
    neutra: conta('neutra'),
    negativa: conta('negativa'),
    semRegistro: total - registradas,
    total,
  };
}

/**
 * QUEM RESPONDE PELO MUNICÍPIO NO CRM — o responsável da carteira com mais
 * vínculos ali (issue 107).
 *
 * É o único nome que a coluna "Gestor" pode mostrar: vem do cadastro das
 * carteiras, e não de uma amostra. Sem responsável cadastrado, a resposta é
 * nula e a tela mostra o traço — nunca um nome escrito para preencher.
 */
export function responsavelPrincipal(
  responsaveis: readonly ResponsavelPelaCarteira[],
): { nome: string; outros: number } | null {
  const comNome = responsaveis.filter((r) => r.nome.trim() !== '');
  if (comNome.length === 0) return null;
  const [primeiro] = [...comNome].sort((a, b) => b.vinculos - a.vinculos || a.nome.localeCompare(b.nome, 'pt-BR'));
  return { nome: primeiro.nome, outros: comNome.length - 1 };
}
