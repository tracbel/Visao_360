/**
 * A LINHA "vs. ano anterior" SEM NÚMERO (fidelidade às maquetes).
 *
 * A maquete põe, embaixo de cada número de decisão, de cada venda e no resumo do
 * mapa de vendas, uma variação contra o ano anterior: "↑ +8% vs. ano anterior
 * (10.182)". DESDE 27/09/2026 A LEITURA TRAZ O MESMO TRECHO DO ANO ANTERIOR, e a
 * variação de verdade mora em `VariacaoContraOAnoAnterior`. Esta linha fica para
 * os números que NÃO têm ano anterior — e cada um diz por quê: o número é
 * estrutural (demanda, parque), é medido no instante (cobertura), ou a carga não
 * cobre a janela anterior.
 *
 * A LINHA EXISTE MESMO ASSIM, no lugar e no tamanho da maquete (decisão do
 * usuário de 23/09/2026): some-la faria o cartão encolher e a tela parecer
 * completa; inventar um "+8%" seria número fictício. Fica o traço, o texto da
 * linha e a dica com o motivo. Sem seta: a seta afirma uma direção, e direção sem
 * número é palpite.
 *
 * O MOTIVO É OBRIGATÓRIO (27/09/2026). Havia um padrão — "a leitura devolve UMA
 * janela de competência (issue 69)" —, e ele deixou de ser verdade no dia em que a
 * leitura passou a devolver as duas. Um motivo padrão é justamente o texto que
 * envelhece sem ninguém notar.
 *
 * O NOME DA DICA segue o padrão da tela ("Por que … não aparece"), com o nome do
 * número dentro — assim cada cartão tem a sua, e o leitor de tela sabe de qual
 * variação se trata.
 */

import { InfoTooltip } from '../InfoTooltip';

export function VariacaoAusente({
  deQue,
  compacta = false,
  motivo,
}: {
  deQue: string;
  compacta?: boolean;
  /** Por que esta variação não tem número — verdadeiro para ESTE número. */
  motivo: string;
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
