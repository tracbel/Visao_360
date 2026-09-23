/**
 * AUSÊNCIA NÃO É ZERO — o traço e o motivo, no lugar do número.
 *
 * As três ausências deste sistema são diferentes, e confundi-las muda uma
 * decisão:
 *
 * - **sigilo do IBGE** — o valor existe e foi ocultado porque poucos
 *   estabelecimentos o compõem;
 * - **fonte não integrada** — ninguém carregou ainda;
 * - **parâmetro não decidido** — a conta não pode ser feita porque falta uma
 *   escolha do negócio.
 *
 * Nenhuma das três é zero, e zero é uma afirmação: diz que a região não tem
 * nada disso.
 *
 * O MOTIVO VAI NA DICA, E NÃO DENTRO DO VALOR. Prosa dentro de um `<dd>` ou de
 * uma célula empurra a coluna, quebra o alinhamento dos números e faz o olho
 * parar onde não precisa (documento 50, §8).
 */

import { InfoTooltip } from '../InfoTooltip';

export function ValorAusente({
  motivo,
  oQue,
}: {
  /** Por que o número não saiu — a frase que o servidor devolveu, sempre que houver. */
  motivo: string;
  /** O nome do número, para o leitor de tela saber de qual dica se trata. */
  oQue: string;
}) {
  return (
    <span className="cad-ausente">
      <span aria-hidden="true">—</span>
      <span className="cad-so-leitor">sem dado</span>
      <InfoTooltip texto={motivo} rotulo={`Por que ${oQue} não aparece`} />
    </span>
  );
}
