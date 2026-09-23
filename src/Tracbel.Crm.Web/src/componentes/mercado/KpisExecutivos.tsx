/**
 * OS QUATRO NÚMEROS DE DECISÃO (documento 50, §4.1; redesenhados na T4.6).
 *
 *   Demanda anual | Mercado anual | Captura Tracbel | Oportunidade
 *
 * TRÊS DELES NÃO TÊM DADO HOJE, e nascem dizendo o que falta e qual issue o
 * destrava. Isto não é um espaço reservado: é a resposta certa. Um número
 * plausível e errado leva a uma decisão; um espaço explicado leva a uma pergunta.
 *
 * O QUE MUDOU NA T4.6 — nada de conteúdo, tudo de peso visual:
 *
 * - **O motivo saiu de dentro do cartão.** Ele era um parágrafo embaixo do
 *   travessão, e empurrava o número para baixo: os quatro cartões ficavam com
 *   alturas diferentes, e o olho lê diferença de altura como diferença de
 *   importância. Agora o cartão mostra `—` com um ⓘ, e o motivo inteiro, com a
 *   issue que o destrava, está na dica.
 * - **A grade tem quatro colunas declaradas.** Era `auto-fit`, que dava cinco
 *   colunas num monitor largo e três em outro — "os quatro do topo" deixava de
 *   ser uma coisa reconhecível.
 *
 * CAPTURA, E NÃO MARKET SHARE (issue 162): enquanto o denominador for a demanda
 * ESTIMADA pelo motor, o nome é captura. Share exigiria o total vendido por todos
 * os fabricantes, que nenhuma fonte aberta publica.
 */

import { BarChart3, Lightbulb, PieChart, Target } from 'lucide-react';
import { CartaoDeIndicador, GradeDeIndicadores } from '../dashboard/Dashboard';
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

const DEMANDA_SEM_DADO =
  'Falta o ciclo de renovação por cultura. A decisão D-P01 (issue 63) fixa cultura, categoria, hectares por ' +
  'máquina e anos de renovação — as quatro juntas. Sem elas não há demanda anual, e zero aqui afirmaria que a ' +
  'região não renova máquina nenhuma.';

export function KpisExecutivos({
  momento,
  demandaEstrutural,
  demandaDeSaoPaulo,
  carregando,
  procedenciaDaDemanda,
}: {
  momento: MomentoDoRecorte | null;
  /**
   * A DEMANDA ANUAL DO RECORTE INTEIRO, pelo motor (issue 72).
   *
   * Ela vem do potencial, e NÃO da soma que o fator agregado usa: aquela soma
   * conta só as culturas que entraram na razão, e mostrá-la aqui encolheria o
   * mercado em silêncio toda vez que uma cultura ficasse sem preço.
   */
  demandaEstrutural: number | null;
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
  // A DEMANDA É SOMÁVEL: ela compara por FATIA. Sem denominador, o selo some —
  // e não vira 0%, que afirmaria que a região não demanda nada.
  const comoFatia = montarSomavel(demandaEstrutural, null, demandaDeSaoPaulo);
  const contexto = fatiasEmTexto(comoFatia);

  // O QUE O MOMENTO FAZ COM ELA, direto do fator agregado — nenhum número novo:
  // fator 0,88 é a mesma coisa que "12% abaixo da estrutural".
  const variacao = momento?.fatorAgregado == null ? null : (momento.fatorAgregado - 1) * 100;

  return (
    <GradeDeIndicadores data-bloco="kpis-executivos">
      <CartaoDeIndicador
        rotulo="Demanda anual"
        destaque
        icone={BarChart3}
        tom="demanda"
        valor={carregando || demandaEstrutural === null ? null : nº(Math.round(demandaEstrutural))}
        unidade="máquinas/ano"
        contexto={
          contexto ??
          (variacao != null
            ? `${variacao > 0 ? '+' : ''}${nº(Math.round(variacao))}% com o momento do mercado`
            : 'o que o parque renova por ano')
        }
        procedencia={procedenciaDaDemanda}
        motivoSemDado={DEMANDA_SEM_DADO}
      />
      <CartaoDeIndicador
        rotulo="Mercado anual"
        icone={PieChart}
        tom="mercado"
        valor={null}
        motivoSemDado={MERCADO_ANUAL_SEM_DADO}
      />
      <CartaoDeIndicador
        rotulo="Captura Tracbel"
        icone={Target}
        tom="captura"
        valor={null}
        motivoSemDado={CAPTURA_SEM_DADO}
      />
      <CartaoDeIndicador
        rotulo="Oportunidade"
        icone={Lightbulb}
        tom="oportunidade"
        valor={null}
        motivoSemDado={OPORTUNIDADE_SEM_DADO}
      />
    </GradeDeIndicadores>
  );
}
