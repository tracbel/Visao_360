/**
 * DE ONDE VEIO ESTE NÚMERO — a procedência de UM indicador (issue 167).
 *
 * A TELA NÃO ESCREVE FONTE. Tudo o que aparece aqui vem do contrato da API:
 * pesquisa, tabela, variável, competência e a data da última carga. Escrever
 * "Fonte: IBGE" à mão no front foi o que produziu os parágrafos cinza de oito
 * linhas embaixo de cada cartão — e eles envelhecem sem ninguém notar, porque
 * ninguém lembra de mudá-los quando a carga muda.
 *
 * O CONTEÚDO NÃO SE PERDE, MUDA DE CAMADA: o que era parágrafo fixo vira o texto
 * desta dica, alcançável pelo ponteiro, pelo teclado e pelo toque.
 */

import { InfoTooltip } from '../InfoTooltip';
import type { ProcedenciaDoIndicador } from '../../tipos/territorio';
import { frasesDaProcedencia } from './comparacoes';

export function Procedencia({
  procedencia,
  oQue,
}: {
  procedencia: ProcedenciaDoIndicador | null | undefined;
  /** O nome do número, para o leitor de tela saber de qual dica se trata. */
  oQue: string;
}) {
  if (!procedencia) return null;
  return <InfoTooltip texto={frasesDaProcedencia(procedencia)} rotulo={`De onde vem ${oQue}`} />;
}
