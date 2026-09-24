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
import type { MomentoDoRecorte, NumerosDeDecisao, ProcedenciaDoIndicador } from '../../tipos/territorio';
import { reaisCompactos } from '../territorio/escalas';
import { nº, porcento } from '../territorio/indicadoresDaAdr';

/* AS QUATRO CONSTANTES DE MOTIVO SAÍRAM DAQUI (issue 69, parte A).
 *
 * Elas diziam, em texto escrito no TypeScript, por que cada número faltava — e as mesmas frases estavam
 * repetidas na ficha do município e no bloco Performance, com redações que já tinham começado a divergir.
 * Agora o motivo vem da API, de `DecisaoDoMercado.Frase`, que tem teste. A tela não escreve mais por que um
 * número falta: ela mostra o que o servidor afirma. */

export function KpisExecutivos({
  momento,
  demandaEstrutural,
  demandaDeSaoPaulo,
  carregando,
  procedenciaDaDemanda,
  numeros,
}: {
  /**
   * Os três números que ainda não têm dado, com o motivo — da API (issue 69, parte A).
   *
   * Nulo enquanto a leitura não respondeu; aí os cartões mostram o traço sem frase, que é o certo: não se
   * pode afirmar por que falta um número antes de saber se ele falta.
   */
  numeros: NumerosDeDecisao | null;
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
        motivoSemDado={numeros?.demandaAnual.frase}
      />
      {/* OS TRÊS QUE AINDA NÃO TÊM DADO VÊM DA API (issue 69, parte A).
          Eram `valor={null}` e três constantes escritas aqui. Nenhum número mudou — o que mudou é que a
          ausência virou afirmação do servidor, com teste, e com a mesma redação da ficha do município. */}
      <CartaoDeIndicador
        rotulo="Mercado anual"
        icone={PieChart}
        tom="mercado"
        valor={numeros?.mercadoAnual.valor == null ? null : reaisCompactos(numeros.mercadoAnual.valor)}
        contexto={
          numeros?.mercadoAnual.parcial
            ? `parcial — sem preço de ${numeros.mercadoAnual.categoriasSemPreco.join(', ')}`
            : 'demanda de cada categoria × o preço dela'
        }
        motivoSemDado={numeros?.mercadoAnual.frase}
      />
      <CartaoDeIndicador
        rotulo="Captura Tracbel"
        icone={Target}
        tom="captura"
        valor={numeros?.capturaPercentual.valor == null ? null : porcento(numeros.capturaPercentual.valor)}
        contexto="das máquinas que a região renova por ano"
        motivoSemDado={numeros?.capturaPercentual.frase}
      />
      <CartaoDeIndicador
        rotulo="Oportunidade"
        icone={Lightbulb}
        tom="oportunidade"
        valor={numeros?.oportunidade.valor == null ? null : nº(Math.round(numeros.oportunidade.valor))}
        unidade="máquinas"
        contexto="a demanda ajustada menos o que já vendemos"
        motivoSemDado={numeros?.oportunidade.frase}
      />
    </GradeDeIndicadores>
  );
}
