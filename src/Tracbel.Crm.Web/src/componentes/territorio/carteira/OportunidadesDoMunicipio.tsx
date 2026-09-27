/**
 * A ABA OPORTUNIDADES DA FICHA (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * NO TOPO, OS QUATRO NÚMEROS DE DECISÃO do município — Demanda anual, Mercado
 * anual, Captura Tracbel e Oportunidade. Eram a "camada executiva" da ficha, a
 * primeira coisa dela; a maquete da ficha não os tem, e eles vieram para cá
 * inteiros, com os mesmos motivos (decisão 3: nada de substância é apagado).
 * Esta é a aba do "quanto dá para vender aqui", e é a pergunta que eles
 * respondem.
 *
 * CAPTURA E OPORTUNIDADE SÃO DO MUNICÍPIO (27/09/2026): o servidor passou a
 * montá-las para cada um, pela mesma conta dos cartões do topo da aba Mercado —
 * a demanda das categorias que têm regra aqui, as máquinas vendidas daqui
 * nessas categorias e o fator de ciclo do recorte. Antes a ficha mostrava "—"
 * e "só no recorte": a rota calculava os dois ingredientes e jogava fora.
 *
 * O MOTIVO DE CADA AUSÊNCIA É A FRASE DO SERVIDOR (issue 69, parte A) — a do
 * município, e na falta dela a do recorte. A ficha NUNCA herda o NÚMERO do
 * recorte, que seria de outro lugar; `SO_NO_RECORTE` só aparece quando o
 * servidor não mandou a conta do município.
 *
 * EMBAIXO, A LISTA DE OPORTUNIDADES COM CONFIANÇA E ORIGEM (issue 162), com o
 * desenho pronto e vazia. As vendas em unidades por município já existem (issue
 * 69, pelo ART); o que falta é a classificação de confiança da 162, que diz de
 * onde vem cada oportunidade. Uma lista com linhas de exemplo seria dado
 * inventado.
 */

import { Inbox } from 'lucide-react';
import type { IndicadoresDoMunicipio, NumerosDeDecisao } from '../../../tipos/territorio';
import { InfoTooltip } from '../../InfoTooltip';
import { CartaoDeIndicador, GradeDeIndicadores } from '../../dashboard/Dashboard';
import { fraseDosPrecos, marcaDeParcial } from '../../mercado/precosDeReferencia';
import { reaisCompactos } from '../escalas';

const nº = (v: number) => v.toLocaleString('pt-BR');
/** Máquinas com uma casa: a demanda do município é estimativa, e 0,2 máquina é informação. */
const maquinas = (v: number) => v.toLocaleString('pt-BR', { maximumFractionDigits: 1 });
const porcento = (v: number) => `${v.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`;

/**
 * Por que o número que o RECORTE tem não aparece no município: a conta do
 * município pede as vendas em unidades separadas pelas categorias que têm
 * demanda aqui, e a leitura do município traz as unidades somadas.
 */
/**
 * O MERCADO ANUAL NÃO USA VENDAS (revisão de 27/09/2026): ele é a demanda de cada
 * categoria vezes o preço de referência dela. Desde a issue 70 o município traz a
 * própria conta; esta frase só aparece quando nem o município nem o recorte
 * mandaram a deles.
 */
const MERCADO_SO_NO_RECORTE =
  'O mercado anual do município é a demanda de cada categoria vezes o preço de referência dela, com as categorias daqui — e ele não saiu nesta leitura: o município não tem demanda estimada por categoria.';

const SO_NO_RECORTE = (oQue: string) =>
  `${oQue} existe para o recorte inteiro — está no topo da aba Mercado — e ainda não por município: a conta do município pede as vendas em unidades separadas pelas categorias que têm demanda aqui, e a leitura do município traz as unidades somadas. O número do recorte não é repetido aqui porque é de outro lugar.`;

/** As três confianças da issue 162, com a origem de cada uma — a legenda da lista. */
const CONFIANCAS = [
  { nivel: 'alta', rotulo: 'Alta', origem: 'venda perdida registrada no CRM' },
  { nivel: 'media', rotulo: 'Média', origem: 'cliente da carteira com chassi acima do ciclo e sem negociação aberta' },
  { nivel: 'baixa', rotulo: 'Baixa', origem: 'crédito de máquina no município sem venda Tracbel correspondente' },
] as const;

export function OportunidadesDoMunicipio({
  municipio,
  numerosDeDecisao,
}: {
  municipio: IndicadoresDoMunicipio;
  /** Os números de decisão do recorte — daqui só sai a frase de cada ausência. */
  numerosDeDecisao: NumerosDeDecisao | null;
}) {
  const motor = municipio.potencialEstrutural;
  const doMunicipio = municipio.numerosDeDecisao ?? null;
  const captura = doMunicipio?.capturaPercentual.valor ?? null;
  const oportunidade = doMunicipio?.oportunidade.valor ?? null;

  /** A frase da ausência: a do município, a do recorte, e só então a constante. */
  const motivo = (
    doMunicipioFrase: string | undefined,
    doRecorteFrase: string | undefined,
    constante: string,
  ): string | undefined =>
    doMunicipioFrase || (numerosDeDecisao ? doRecorteFrase || constante : undefined);

  return (
    <div className="terr-ficha-oportunidades">
      <section className="terr-ficha-bloco" data-camada="decisao" aria-labelledby="ficha-decide-titulo">
        <h3 id="ficha-decide-titulo" className="terr-ficha-bloco-titulo">
          O que este município decide
        </h3>
        <GradeDeIndicadores>
          <CartaoDeIndicador
            rotulo="Demanda anual"
            // COM ARTIGO, porque é assim que esta dica já se chamava aqui — e
            // mudar o nome acessível de um controle que as pessoas conhecem é
            // uma regressão silenciosa para quem usa leitor de tela.
            oQue="a demanda anual"
            destaque
            valor={motor?.demandaAnualDeMaquinas == null ? null : `${nº(motor.demandaAnualDeMaquinas)} máq/ano`}
            contexto="o que o parque daqui renova por ano"
            motivoSemDado={
              motor?.motivoSemDemanda === 'SemCicloDeRenovacao'
                ? 'A regra não informou de quantos em quantos anos a máquina é trocada. A decisão D-P01 (issue 63) fixa cultura, categoria, hectares por máquina e anos de renovação — as quatro juntas.'
                : 'Sem parque, não há o que renovar.'
            }
          />
          {/* O MERCADO ANUAL DO MUNICÍPIO (issue 70, 27/09/2026): a demanda de cada categoria daqui vezes o
              preço dela. Parcial se diz ao lado do número, e o preço usado vai no contexto. */}
          <CartaoDeIndicador
            rotulo="Mercado anual"
            valor={doMunicipio?.mercadoAnual.valor == null ? null : reaisCompactos(doMunicipio.mercadoAnual.valor)}
            contexto={
              [marcaDeParcial(doMunicipio?.mercadoAnual), fraseDosPrecos(doMunicipio?.mercadoAnual)]
                .filter(Boolean)
                .join(' · ') || undefined
            }
            motivoSemDado={motivo(doMunicipio?.mercadoAnual.frase, numerosDeDecisao?.mercadoAnual.frase, MERCADO_SO_NO_RECORTE)}
          />
          <CartaoDeIndicador
            rotulo="Captura Tracbel"
            valor={captura == null ? null : porcento(captura)}
            contexto={doMunicipio?.baseDaCaptura?.frase}
            motivoSemDado={motivo(doMunicipio?.capturaPercentual.frase, numerosDeDecisao?.capturaPercentual.frase, SO_NO_RECORTE('A captura'))}
          />
          <CartaoDeIndicador
            rotulo="Oportunidade"
            valor={oportunidade == null ? null : `${maquinas(oportunidade)} máq`}
            contexto="a demanda ajustada do período menos as vendas daqui"
            motivoSemDado={motivo(doMunicipio?.oportunidade.frase, numerosDeDecisao?.oportunidade.frase, SO_NO_RECORTE('A oportunidade'))}
          />
        </GradeDeIndicadores>
      </section>

      <section className="terr-ficha-bloco" data-bloco-da-ficha="lista-de-oportunidades" aria-labelledby="ficha-lista-titulo">
        <h3 id="ficha-lista-titulo" className="terr-ficha-bloco-titulo">
          Oportunidades
          <InfoTooltip
            rotulo="Como uma oportunidade é classificada"
            texto='Toda oportunidade traz a confiança e a origem — a legenda embaixo da lista —, e nenhuma vira "venda perdida" sozinha (issue 162, D-IM-09).'
          />
        </h3>
        <table className="terr-ficha-oport-lista">
          <caption className="cad-so-leitor">Oportunidades no município, com confiança e origem</caption>
          <thead>
            <tr>
              <th scope="col">Oportunidade</th>
              <th scope="col">Confiança</th>
              <th scope="col">Origem</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td colSpan={3}>
                <span className="terr-ficha-vazio">
                  <Inbox size={16} strokeWidth={2} aria-hidden="true" />
                  <span aria-hidden="true">—</span>
                  <span>aguarda a confiança da #162</span>
                  <InfoTooltip
                    rotulo="Por que não há oportunidades listadas"
                    texto="A oportunidade é a demanda ajustada menos as vendas da Tracbel em unidades, e a confiança dela vem da origem — venda perdida, chassi acima do ciclo ou crédito sem venda. As vendas em unidades por município já existem (issue 69, pelo ART, na aba Estrutura); o que falta é a classificação de confiança (issue 162), que diz de onde vem cada oportunidade. Até lá a lista fica vazia, e não com exemplos."
                  />
                </span>
              </td>
            </tr>
          </tbody>
        </table>
        <ul className="terr-ficha-confiancas" aria-label="Níveis de confiança">
          {CONFIANCAS.map((c) => (
            <li key={c.nivel}>
              <span className="terr-ficha-confianca" data-nivel={c.nivel}>
                {c.rotulo}
              </span>
              {c.origem}
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}
