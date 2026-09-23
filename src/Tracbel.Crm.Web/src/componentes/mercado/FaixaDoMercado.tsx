/**
 * O QUE A REGIÃO TEM, numa faixa de uma linha (fase T4.6).
 *
 * Parque de tratores, propriedades, valor da lavoura, usinas e rebanho estavam
 * desenhados como cartão, do mesmo tamanho dos quatro números de decisão, logo
 * abaixo deles. Nove cartões iguais empilhados é uma lista, não uma hierarquia:
 * o olho não tinha como saber que os quatro de cima decidem e os cinco de baixo
 * contextualizam.
 *
 * NENHUM NÚMERO FOI ELIMINADO. Eles continuam inteiros, com a procedência e as
 * duas comparações — Região Tracbel e São Paulo —, só que a comparação saiu da
 * tela e foi para a dica, que é onde ela já mora para todo o resto (issue 33,
 * nível 2). O valor e o nome ficam; o método fica a um passo.
 *
 * É a mesma decisão da §4.2 do documento 50 aplicada a outro bloco: hierarquia se
 * faz com tamanho e espaço, não com moldura.
 */

import { FaixaDeEstrutura, ItemDaFaixa } from '../dashboard/Dashboard';
import type { Indicador } from '../cadastro/Indicadores';

export function FaixaDoMercado({ indicadores }: { indicadores: Indicador[] }) {
  if (indicadores.length === 0) return null;

  return (
    <FaixaDeEstrutura data-bloco="faixa-do-mercado" aria-label="O que a região tem">
      {indicadores.map((i) => (
        <ItemDaFaixa
          key={i.rotulo}
          rotulo={i.rotulo}
          valor={i.valor}
          // A COMPARAÇÃO CONTINUA SENDO DE CADA NÚMERO (documento 50, §7), só
          // que na dica dele em vez de numa linha permanente embaixo. Uma dica
          // única no fim da faixa, com as cinco fatias juntas, faria o leitor
          // procurar a dele no meio de um parágrafo.
          comparacao={i.deOnde === '—' ? undefined : i.deOnde}
          procedencia={i.procedencia}
          motivoSemDado={i.semDado}
        />
      ))}
    </FaixaDeEstrutura>
  );
}
