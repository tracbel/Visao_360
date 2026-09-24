/**
 * A LINHA "vs. ano anterior" QUE AINDA NÃO TEM NÚMERO (fidelidade às maquetes).
 *
 * A maquete põe, embaixo de cada número de decisão, de cada venda e no resumo do
 * mapa de vendas, uma variação contra o ano anterior: "↑ +8% vs. ano anterior
 * (10.182)". A leitura desta tela devolve UMA janela de competência, e não duas —
 * não há ano anterior para comparar.
 *
 * A LINHA EXISTE MESMO ASSIM, no lugar e no tamanho da maquete (decisão do
 * usuário de 23/09/2026): some-la faria o cartão encolher e a tela parecer
 * completa; inventar um "+8%" seria número fictício. Fica o traço, o texto da
 * linha e a dica com o motivo e a issue que destrava. Sem seta: a seta afirma
 * uma direção, e direção sem número é palpite.
 *
 * O NOME DA DICA segue o padrão da tela ("Por que … não aparece"), com o nome do
 * número dentro — assim cada cartão tem a sua, e o leitor de tela sabe de qual
 * variação se trata.
 */

import { InfoTooltip } from '../InfoTooltip';

/** O motivo, igual em todo lugar em que a variação falta. */
const MOTIVO_SEM_ANO_ANTERIOR =
  'A leitura desta tela devolve UMA janela de competência — a do filtro de período —, e não duas: não há ano ' +
  'anterior para comparar. A variação aparece quando a leitura passar a devolver a janela anterior junto (issue 69). ' +
  'Até lá, um "+8%" aqui seria número inventado.';

export function VariacaoAusente({
  deQue,
  compacta = false,
  motivo = MOTIVO_SEM_ANO_ANTERIOR,
}: {
  deQue: string;
  compacta?: boolean;
  /**
   * Outro motivo, quando o que falta não é a janela anterior da leitura — a
   * percepção comercial do Momento, por exemplo, não tem série guardada (issue
   * 71). A frase e o desenho da linha continuam os mesmos da tela inteira.
   */
  motivo?: string;
}) {
  return (
    <span className="mv-variacao" data-compacta={compacta || undefined}>
      <span className="mv-variacao-traco" aria-hidden="true">
        —
      </span>
      <span className="cad-so-leitor">sem dado</span>
      <span className="mv-variacao-texto">vs. ano anterior</span>
      <InfoTooltip texto={motivo} rotulo={`Por que a variação de ${deQue} não aparece`} />
    </span>
  );
}
