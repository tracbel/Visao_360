/**
 * AS DUAS REGRAS DE COMPARAÇÃO, em funções puras (documento 50, §7; issue 163).
 *
 * SÃO DOIS CONCEITOS, E MISTURÁ-LOS PRODUZ UM NÚMERO SEM SIGNIFICADO:
 *
 * 1. **Somável** — área, quantidade, valor, parque, propriedades, rebanho,
 *    crédito contratado, demanda e vendas em unidades. Somar os municípios dá o
 *    total, e por isso "que fatia isto é?" tem resposta:
 *
 *        37.226 ha
 *        4,2% da Região Tracbel · 0,5% de SP
 *
 * 2. **Razão** — produtividade, preço, rentabilidade, captura, índices e o
 *    fator. Somar produtividades de municípios não dá a produtividade do estado,
 *    e uma captura de 14,8% não é fatia de coisa alguma. O que se mostra é a
 *    DISTÂNCIA até a referência:
 *
 *        1,63 t/ha
 *        6% acima da Região Tracbel · 10% acima de SP
 *
 *    E quando a própria grandeza já é um percentual, a distância é em PONTOS
 *    PERCENTUAIS, porque "3,2% acima de 14,8%" seria 15,3% — outra coisa:
 *
 *        Captura 14,8%
 *        +3,2 p.p. vs Região Tracbel
 *
 * DENOMINADOR AUSENTE NÃO VIRA 0%. "0% de São Paulo" afirma que a região não tem
 * nada disso; fonte não carregada é outra coisa, e a tela precisa poder dizer
 * qual das duas está olhando — então o selo simplesmente não aparece.
 *
 * REGIÃO TRACBEL NÃO É "REGIÃO": no código, `RegiaoDaAreaDeAtuacao` é Norte ou
 * Noroeste, que são SUB-REGIÕES. Escrever só "região" aqui faria a diretoria ler
 * a fatia da sub-região como fatia da ADR (issue 163).
 *
 * As funções moram neste arquivo, e não junto dos componentes, porque um módulo
 * que exporta componente e função quebra o recarregamento rápido do Vite.
 */

import type { MedidaDeRazao, MedidaSomavel } from '../../tipos/territorio';

const pt = (v: number, casas = 1) => v.toLocaleString('pt-BR', { maximumFractionDigits: casas });

/**
 * A fatia, com a mesma regra do servidor: denominador ausente ou zerado NÃO vira 0%.
 *
 * "0% de São Paulo" afirma que a região não tem nada disso; fonte não carregada é
 * outra coisa. As duas pontas — API e tela — usam esta mesma conta, e é por isso
 * que ela mora aqui e não espalhada nos painéis.
 */
export function fatia(parte: number | null, total: number | null): number | null {
  if (parte === null || total === null || total === 0) return null;
  return (100 * parte) / total;
}

/**
 * Monta uma grandeza somável a partir do valor e dos dois denominadores.
 *
 * Serve para os indicadores cujo numerador a tela já somou (os totais da ADR) e
 * cujos denominadores a API entrega prontos — Região Tracbel e São Paulo.
 */
export function montarSomavel(
  valor: number | null,
  totalRegiaoTracbel: number | null,
  totalSaoPaulo: number | null,
  extras: Partial<Pick<MedidaSomavel, 'motivoDaAusencia' | 'procedencia'>> = {},
): MedidaSomavel {
  return {
    valor,
    totalRegiaoTracbel,
    totalSaoPaulo,
    fatiaRegiaoTracbel: fatia(valor, totalRegiaoTracbel),
    fatiaSaoPaulo: fatia(valor, totalSaoPaulo),
    motivoDaAusencia: extras.motivoDaAusencia ?? null,
    procedencia: extras.procedencia ?? null,
  };
}

/** `4,2% da Região Tracbel · 0,5% de SP` — e nada quando não há denominador. */
export function fatiasEmTexto(m: MedidaSomavel): string | null {
  const partes = [
    m.fatiaRegiaoTracbel !== null && `${pt(m.fatiaRegiaoTracbel)}% da Região Tracbel`,
    m.fatiaSaoPaulo !== null && `${pt(m.fatiaSaoPaulo)}% de SP`,
  ].filter(Boolean) as string[];

  return partes.length === 0 ? null : partes.join(' · ');
}

/**
 * Como cada referência se escreve.
 *
 * A PREPOSIÇÃO ANDA COM O NOME porque ela muda: "acima **da** Região Tracbel" e
 * "acima **de** SP". Montar a frase com um `da ${nome}` fixo produzia "10% acima
 * da SP" — um teste pegou isso.
 */
type Referencia = { nome: string; comPreposicao: string };

const REGIAO: Referencia = { nome: 'Região Tracbel', comPreposicao: 'da Região Tracbel' };
const SAO_PAULO: Referencia = { nome: 'SP', comPreposicao: 'de SP' };

/**
 * A distância até uma referência, na unidade certa.
 *
 * Percentual → pontos percentuais, com sinal. Contínua → percentual relativo,
 * com a palavra "acima" ou "abaixo", que é como a diretoria lê.
 */
export function distanciaEmTexto(valor: number, referencia: number, ehPercentual: boolean, onde: Referencia): string | null {
  if (ehPercentual) {
    const pp = valor - referencia;
    if (Math.abs(pp) < 0.05) return `sem diferença vs ${onde.nome}`;
    return `${pp > 0 ? '+' : '−'}${pt(Math.abs(pp))} p.p. vs ${onde.nome}`;
  }

  if (referencia === 0) return null;
  const variacao = (100 * (valor - referencia)) / referencia;
  if (Math.abs(variacao) < 0.5) return `na média ${onde.comPreposicao}`;
  return `${pt(Math.abs(variacao), 0)}% ${variacao > 0 ? 'acima' : 'abaixo'} ${onde.comPreposicao}`;
}

/** `6% acima da Região Tracbel · 10% acima de SP` — e nada sem referência. */
export function distanciasEmTexto(m: MedidaDeRazao): string | null {
  if (m.valor === null) return null;

  const partes = [
    m.referenciaRegiaoTracbel !== null && distanciaEmTexto(m.valor, m.referenciaRegiaoTracbel, m.ehPercentual, REGIAO),
    m.referenciaSaoPaulo !== null && distanciaEmTexto(m.valor, m.referenciaSaoPaulo, m.ehPercentual, SAO_PAULO),
  ].filter(Boolean) as string[];

  return partes.length === 0 ? null : partes.join(' · ');
}

/** A procedência em uma frase, na ordem em que se confere numa fonte oficial. */
export function frasesDaProcedencia(p: {
  fonte: string;
  pesquisa: string | null;
  tabela: string | null;
  variavel: string | null;
  competencia: string | null;
  ultimaCargaUtc: string | null;
  ressalva: string | null;
}): string {
  const partes = [
    `Fonte: ${p.fonte}`,
    p.pesquisa && `Pesquisa: ${p.pesquisa}`,
    p.tabela && `Tabela: ${p.tabela}`,
    p.variavel && `Variável: ${p.variavel}`,
    p.competencia && `Competência: ${p.competencia}`,
    p.ultimaCargaUtc && `Última carga: ${new Date(p.ultimaCargaUtc).toLocaleDateString('pt-BR')}`,
  ].filter(Boolean);

  return p.ressalva ? `${partes.join(' · ')}. ${p.ressalva}` : `${partes.join(' · ')}.`;
}
