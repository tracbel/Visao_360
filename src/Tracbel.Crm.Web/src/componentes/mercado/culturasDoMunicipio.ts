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
 * A área colhida de uma cultura no recorte — o número, ou POR QUE ele não sai.
 *
 * As três ausências são diferentes e levam a frases diferentes na tela:
 *
 * - `semMunicipio` — nenhum município da ADR veio na leitura;
 * - `foraDaLeitura` — a leitura não traz a área desta cultura (ela não tem
 *   regra de potencial vigente): a área existe na PAM e não chega aqui;
 * - `semAreaDivulgada` — a leitura traz a cultura, e nenhum município do
 *   recorte tem área divulgada (sigilo do IBGE, ou a cultura não é plantada ali).
 */
export type AreaNoRecorte =
  | { situacao: 'comArea'; hectares: number }
  | { situacao: 'semMunicipio' }
  | { situacao: 'foraDaLeitura' }
  | { situacao: 'semAreaDivulgada' };

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
 * O QUE A LEITURA TRAZ (revisão de 24/09/2026): `municipio.potencial[]` tem UMA
 * LINHA POR REGRA DE POTENCIAL vigente, em todo município — o repositório a
 * monta pelas regras, e não pelo catálogo. Cultura do catálogo sem regra nunca
 * tem linha ali, e a falta dela não diz nada sobre a lavoura: diz que a leitura
 * não a traz. A tela dizia "nenhum município tem área colhida divulgada" dessa
 * cultura — uma frase falsa sobre uma área que simplesmente não veio. Se só
 * PARTE dos produtos que somam na cultura tem regra, a soma sairia menor que a
 * lavoura, sem aviso: isso também é `foraDaLeitura`.
 *
 * NULO NÃO É ZERO: com a cultura na leitura e nenhum município com área
 * divulgada, a resposta é ausência — e ela não pesa na média, em vez de pesar
 * zero.
 */
export function areaColhidaNoRecorte(
  cultura: CulturaNoCatalogo,
  municipios: readonly IndicadoresDoMunicipio[],
): AreaNoRecorte {
  const queSomam = cultura.produtos.filter((p) => p.entraNaSomaDaLavoura);
  const codigos = new Set((queSomam.length > 0 ? queSomam : cultura.produtos).map((p) => p.codigoIbge));

  const daAdr = municipios.filter((m) => m.pertenceAAdr);
  if (daAdr.length === 0) return { situacao: 'semMunicipio' };

  // A LINHA DA REGRA EXISTE EM TODO MUNICÍPIO, com área nula onde a PAM não
  // divulgou: basta ela aparecer em algum para a leitura trazer o produto.
  const naLeitura = new Set(daAdr.flatMap((m) => m.potencial.map((p) => p.produtoCodigoIbge)));
  if ([...codigos].some((c) => !naLeitura.has(c))) return { situacao: 'foraDaLeitura' };

  let total = 0;
  let algum = false;
  for (const m of daAdr) {
    for (const p of m.potencial) {
      if (!codigos.has(p.produtoCodigoIbge) || p.areaColhidaHectares == null) continue;
      total += p.areaColhidaHectares;
      algum = true;
    }
  }

  return algum ? { situacao: 'comArea', hectares: total } : { situacao: 'semAreaDivulgada' };
}
