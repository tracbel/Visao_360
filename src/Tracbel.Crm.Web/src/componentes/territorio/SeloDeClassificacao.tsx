import { InfoTooltip } from '../InfoTooltip';
import type { ClassificacaoDeIndicador } from '../../tipos/territorio';

/**
 * O selo de como ler um indicador — medido, regra provisória, estimativa.
 *
 * O MOTIVO SAIU DO `title=` E VIROU DICA (issue 167): o `title` do navegador só
 * aparece com o ponteiro parado, nunca pelo teclado e nunca no toque — e este
 * selo é justamente o aviso de que o número não é medido. Quem navega por Tab
 * lia "estimativa" sem nunca saber de quê.
 */
export function SeloDeClassificacao({ classificacao }: { classificacao: ClassificacaoDeIndicador | null }) {
  if (!classificacao) return null;
  return (
    <InfoTooltip texto={classificacao.motivo} rotulo={`Por que este número está marcado como ${classificacao.selo}`}>
      <span className={`terr-selo terr-selo-${classificacao.situacao}`}>{classificacao.selo}</span>
    </InfoTooltip>
  );
}
