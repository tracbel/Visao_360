/**
 * OS QUATRO NÚMEROS DE DECISÃO (documento 50, §4.1; fase T3).
 *
 *   Demanda anual | Mercado anual | Captura Tracbel | Oportunidade
 *
 * TRÊS DELES NÃO TÊM DADO HOJE, e nascem dizendo o que falta e qual issue o
 * destrava. Isto não é um espaço reservado: é a resposta certa. Um número
 * plausível e errado leva a uma decisão; um espaço explicado leva a uma pergunta.
 *
 * CAPTURA, E NÃO MARKET SHARE (issue 162): enquanto o denominador for a demanda
 * ESTIMADA pelo motor, o nome é captura. Share exigiria o total vendido por todos
 * os fabricantes, que nenhuma fonte aberta publica.
 */

import type { Indicador } from '../cadastro/Indicadores';
import { PainelDeIndicadores } from '../cadastro/Indicadores';
import { fatiasEmTexto, montarSomavel } from '../comum/comparacoes';
import type { MomentoDoRecorte, ProcedenciaDoIndicador } from '../../tipos/territorio';
import { nº } from '../territorio/indicadoresDaAdr';

/** O que falta, e a issue que destrava — nunca um número de exemplo. */
const MERCADO_ANUAL_SEM_DADO =
  'Demanda anual × preço de referência, agregada por categoria de máquina. Precisa do preço de máquina por ' +
  'modelo ao longo do tempo (issue 70), que não existe no CRM. Um preço genérico aplicado à demanda inteira ' +
  'misturaria colhedora com trator compacto.';

const CAPTURA_SEM_DADO =
  'Vendas da Tracbel em unidades ÷ demanda anual estimada. Precisa da issue 69, que traz as vendas por município ' +
  'em MÁQUINAS: o faturamento em reais que já existe não serve de numerador para uma demanda medida em máquinas. ' +
  'Não é market share — share exigiria o total vendido por todos os fabricantes.';

const OPORTUNIDADE_SEM_DADO =
  'A demanda ajustada menos as vendas, nunca abaixo de zero (issue 162). Depende da issue 69 para sair em ' +
  'unidades e da issue 70 para sair em reais.';

export function KpisExecutivos({
  momento,
  demandaDeSaoPaulo,
  carregando,
  procedenciaDaDemanda,
}: {
  momento: MomentoDoRecorte | null;
  /**
   * A demanda anual de São Paulo, quando houver.
   *
   * HOJE NÃO HÁ, e por isso a fatia simplesmente não aparece — em vez de "0% de
   * SP", que afirmaria que a região não demanda nada.
   */
  demandaDeSaoPaulo: number | null;
  carregando: boolean;
  procedenciaDaDemanda: ProcedenciaDoIndicador | null;
}) {
  const demanda = momento?.potencial.demandaEstrutural ?? null;

  // A DEMANDA É SOMÁVEL: ela compara por FATIA. Sem denominador, o selo some —
  // e não vira 0%, que afirmaria que a região não demanda nada.
  const comoFatia = montarSomavel(demanda, null, demandaDeSaoPaulo);
  const contexto = fatiasEmTexto(comoFatia);

  const indicadores: Indicador[] = [
    {
      rotulo: 'Demanda anual',
      valor: demanda === null ? null : `${nº(Math.round(demanda))} máq/ano`,
      deOnde:
        contexto ??
        (momento?.potencial.variacaoPercentual != null
          ? `${momento.potencial.variacaoPercentual > 0 ? '+' : ''}${nº(Math.round(momento.potencial.variacaoPercentual))}% sobre a estrutural`
          : 'o que o parque renova por ano'),
      procedencia: procedenciaDaDemanda,
      semDado:
        'falta o ciclo de renovação por cultura — a decisão D-P01 (issue 63) fixa cultura, categoria, hectares por ' +
        'máquina e anos de renovação, as quatro juntas',
    },
    { rotulo: 'Mercado anual', valor: null, deOnde: '—', semDado: MERCADO_ANUAL_SEM_DADO },
    { rotulo: 'Captura Tracbel', valor: null, deOnde: '—', semDado: CAPTURA_SEM_DADO },
    { rotulo: 'Oportunidade', valor: null, deOnde: '—', semDado: OPORTUNIDADE_SEM_DADO },
  ];

  return (
    <div data-bloco="kpis-executivos">
      <PainelDeIndicadores indicadores={indicadores} carregando={carregando} />
    </div>
  );
}
