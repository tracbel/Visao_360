/**
 * COMO INTERPRETAR OS INDICADORES — documentação, não painel (fase T2.1).
 *
 * Isto era um CARTÃO DE LARGURA INTEIRA, permanente, acima dos números: um
 * título e uma linha por selo, com o motivo de cada um. É informação boa e
 * necessária — e é nível 4 da hierarquia da issue 33 (documentação técnica),
 * competindo por espaço com nível 1 (dado operacional).
 *
 * A T2.1 o recolheu num `<details>` de uma linha entre os filtros e as abas. A
 * fidelidade às maquetes (23/09/2026) deu o passo seguinte: a maquete não tem
 * essa linha, e a decisão do usuário é que o que existe hoje e não está na
 * maquete vai para a dica ao lado do assunto. O assunto aqui são os números de
 * cada aba — então o texto mora na dica de Mercado (ao lado do "Comparar com
 * período anterior", na linha das abas) e na do título "A carteira na área de
 * atuação", em Território.
 *
 * NADA SE PERDEU: os mesmos selos, com os mesmos motivos, a um toque, a um Tab
 * e a um ponteiro de distância. O que sai do documento é o `<details>`.
 *
 * O SELO VIRA PALAVRA EM NEGRITO dentro da dica: o selo colorido é um botão com
 * a própria dica, e dica dentro de dica não abre — o balão não recebe ponteiro.
 */

import type { ClassificacaoDeIndicador } from '../../tipos/territorio';

export function ComoLerEstesNumeros({ classificacoes }: { classificacoes: ClassificacaoDeIndicador[] }) {
  if (classificacoes.length === 0) return null;

  return (
    <>
      <p>
        <strong>Como interpretar os indicadores</strong>
      </p>
      <ul>
        {classificacoes.map((c) => (
          <li key={c.indicador}>
            <strong>{c.selo}</strong>: {c.motivo}
          </li>
        ))}
      </ul>
    </>
  );
}
