/**
 * AS CULTURAS QUE IMPORTAM NO MUNICÍPIO ESCOLHIDO (issue 168).
 *
 * O MUNICÍPIO NÃO MUDA O NÚMERO — MUDA A ORDEM. Preço e custo são publicados
 * para São Paulo e para a localidade de referência da CONAB; não existe versão
 * municipal deles, e repartir um número estadual por município inventaria uma
 * precisão que a fonte não tem. O que o recorte faz é dizer **quais culturas
 * olhar primeiro**, que é a pergunta real de quem escolheu um município.
 *
 * A JUNÇÃO É O CATÁLOGO, e ela já existia: o município traz códigos de produto
 * do IBGE (`potencial[].produtoCodigoIbge`), e cada cultura do catálogo declara
 * os produtos da PAM que a compõem, a fonte de preço e a série de custo. Foi por
 * isso que nada precisou ser acrescentado ao contrato.
 *
 * A ORDEM É POR ÁREA PLANTADA, do maior para o menor: é a que responde "o que se
 * planta aqui?". Cultura sem área divulgada fica de fora da priorização — e não
 * entra como zero.
 */

import type { IndicadoresDoMunicipio } from '../../tipos/territorio';
import type { CulturaNoCatalogo } from '../../tipos/potencial';

/**
 * Os produtos da PAM do município, do maior para o menor em área plantada.
 *
 * Devolve vazio quando não há município escolhido ou quando ele não tem área
 * divulgada — e aí nada é priorizado, em vez de uma ordem inventada.
 */
export function produtosDoMunicipio(municipio: IndicadoresDoMunicipio | null): number[] {
  if (!municipio) return [];

  return municipio.potencial
    .filter((p) => p.areaPlantadaHectares != null && p.areaPlantadaHectares > 0)
    .sort((a, b) => (b.areaPlantadaHectares ?? 0) - (a.areaPlantadaHectares ?? 0))
    .map((p) => p.produtoCodigoIbge);
}

/**
 * O catálogo com as culturas do município à frente, na ordem de área plantada.
 *
 * O resto fica como estava — a ordem do catálogo é uma decisão do negócio
 * (issue 165) e continua valendo para quem não escolheu município.
 */
export function comAsCulturasDoMunicipioPrimeiro(
  culturas: CulturaNoCatalogo[],
  produtosDoIbge: readonly number[],
): CulturaNoCatalogo[] {
  if (produtosDoIbge.length === 0) return culturas;

  /** Em que posição da lista do município esta cultura aparece; -1 se não aparece. */
  const posicao = (c: CulturaNoCatalogo) => {
    const posicoes = c.produtos
      .map((p) => produtosDoIbge.indexOf(p.codigoIbge))
      .filter((i) => i >= 0);
    return posicoes.length === 0 ? -1 : Math.min(...posicoes);
  };

  const doMunicipio = culturas.filter((c) => posicao(c) >= 0).sort((a, b) => posicao(a) - posicao(b));
  const resto = culturas.filter((c) => posicao(c) < 0);

  return [...doMunicipio, ...resto];
}

/**
 * A ÁREA COLHIDA DE UMA CULTURA NA REGIÃO TRACBEL DO RECORTE (fidelidade às
 * maquetes, 23/09/2026) — a soma da PAM dos municípios da ADR.
 *
 * POR QUE NÃO A ÁREA QUE A ROTA DE RENTABILIDADE DEVOLVE: aquela é a área
 * colhida de SÃO PAULO (é a que multiplica a margem na `margemTotal`), e a
 * "Média da Região Tracbel" pesada por ela seria a média do estado com o nome
 * da região. A margem é referência estadual; o PESO é que precisa ser daqui.
 *
 * A JUNÇÃO É O CATÁLOGO, como no resto deste arquivo: a cultura declara os
 * produtos da PAM que a compõem. Só entram os que somam na lavoura — o café tem
 * "Total", "Arábica" e "Canephora", e somar os três contaria a mesma terra duas
 * vezes. Se a cultura não marca nenhum, entram todos os dela.
 *
 * NULO NÃO É ZERO: sem nenhum município com área divulgada para a cultura, a
 * resposta é ausência — e ela não pesa na média, em vez de pesar zero.
 */
export function areaColhidaNoRecorte(
  cultura: CulturaNoCatalogo,
  municipios: readonly IndicadoresDoMunicipio[],
): number | null {
  const queSomam = cultura.produtos.filter((p) => p.entraNaSomaDaLavoura);
  const codigos = new Set((queSomam.length > 0 ? queSomam : cultura.produtos).map((p) => p.codigoIbge));

  let total = 0;
  let algum = false;
  for (const m of municipios) {
    if (!m.pertenceAAdr) continue;
    for (const p of m.potencial) {
      if (!codigos.has(p.produtoCodigoIbge) || p.areaColhidaHectares == null) continue;
      total += p.areaColhidaHectares;
      algum = true;
    }
  }

  return algum ? total : null;
}
