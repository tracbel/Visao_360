/**
 * UM NÚMERO DE DECISÃO, no desenho da maquete dos Indicadores Geográficos — cartão tingido, o glifo colorido grande
 * numa coluna à esquerda e, empilhados à direita, o nome com a dica, o valor com a unidade pequena ao lado e a linha de
 * baixo (a variação, ou o contexto do número).
 *
 * MORAVA DENTRO DOS `KpisExecutivos` e saiu dali em 28/09/2026, quando a Visão 360 passou a seguir o mesmo desenho: os
 * cinco números do painel executivo são estes cartões, e não uma cópia deles.
 *
 * AS TRÊS LINHAS EXISTEM SEMPRE — nome, valor, linha de baixo —, com ou sem dado: é o que mantém os cartões da mesma
 * altura sem fixar altura em pixel. Quando um cartão fica mais alto que o vizinho, o olho lê a diferença como diferença
 * de importância.
 */

import type { LucideIcon } from 'lucide-react';
import type { ReactNode } from 'react';
import { InfoTooltip } from '../InfoTooltip';
import { ValorAusente } from '../comum/ValorAusente';

/** A tinta do cartão — endereço, não semáforo. `neutro` é o cartão branco. */
export type TomDoCartaoDeDecisao = 'demanda' | 'mercado' | 'captura' | 'oportunidade' | 'neutro';

export function CartaoDeDecisao({
  rotulo,
  icone: Icone,
  tom,
  valor,
  carregando = false,
  unidade,
  sobre,
  motivoSemDado,
  variacao,
  oQue,
}: {
  /**
   * A LINHA DE BAIXO: nos Indicadores, a variação contra o mesmo trecho do ano anterior (27/09/2026), ou o traço com o
   * motivo; na Visão 360, o contexto do número em uma linha.
   */
  variacao: ReactNode;
  rotulo: string;
  icone: LucideIcon;
  tom: TomDoCartaoDeDecisao;
  /** O valor pronto para a tela. `null` mostra o traço com o motivo — nunca zero. */
  valor: string | null;
  /**
   * A leitura ainda não voltou. Aí não há ausência a afirmar: o traço com o motivo antes da resposta é um motivo falso
   * por alguns segundos. A barra pulsante é a mesma dos cartões da carteira.
   */
  carregando?: boolean;
  /** A unidade da maquete, ao lado do número e menor que ele. */
  unidade?: ReactNode;
  /**
   * O QUE O NÚMERO É, na dica ao lado do nome: definição, composição e de onde vem. É o que a maquete não mostra e a
   * tela não pode perder.
   */
  sobre: ReactNode;
  /**
   * Por que o número falta.
   *
   * Ausente enquanto a leitura não respondeu: aí sai o traço SEM dica, que é o certo — não se pode afirmar por que
   * falta um número antes de saber se ele falta.
   */
  motivoSemDado: string | undefined;
  /** Como o leitor de tela chama o número ausente; o padrão é o rótulo em minúsculas. */
  oQue?: string;
}) {
  const nome = oQue ?? rotulo.toLowerCase();

  return (
    <div className="mv-kpi" data-kpi={rotulo} data-tom={tom}>
      {/* O GLIFO É ENDEREÇO, NÃO INFORMAÇÃO: ele não diz nada que o nome já não diga, e por isso some do leitor de tela.
          Serve para o olho achar "o de captura" de longe, e é colorido e grande como na maquete. */}
      <span className="mv-kpi-glifo" aria-hidden="true">
        <Icone size={30} strokeWidth={2} />
      </span>

      <div className="mv-kpi-corpo">
        <div className="mv-kpi-rotulo">
          <span>{rotulo}</span>
          <InfoTooltip rotulo={`Fonte e método: ${rotulo}`} texto={sobre} />
        </div>

        <div className="mv-kpi-valor">
          {/* O TRAÇO OCUPA O LUGAR DO NÚMERO, e a unidade continua no dela: a linha tem a forma da maquete, e o motivo
              inteiro está na dica. */}
          {carregando ? (
            <>
              <span className="cad-kpi-esqueleto" aria-hidden="true" />
              <span className="cad-so-leitor">carregando…</span>
            </>
          ) : valor !== null ? (
            <strong>{valor}</strong>
          ) : motivoSemDado ? (
            <ValorAusente motivo={motivoSemDado} oQue={nome} />
          ) : (
            <span className="cad-ausente">
              <span aria-hidden="true">—</span>
              <span className="cad-so-leitor">sem dado</span>
            </span>
          )}
          {unidade && <span className="mv-kpi-unidade">{unidade}</span>}
        </div>

        <div className="mv-kpi-contexto">{variacao}</div>
      </div>
    </div>
  );
}
