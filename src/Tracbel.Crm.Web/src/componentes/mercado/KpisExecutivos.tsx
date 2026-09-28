/**
 * OS QUATRO NÚMEROS DE DECISÃO (documento 50, §4.1; redesenhados na T4.6 e na
 * fidelidade às maquetes de 23/09/2026).
 *
 *   Demanda anual | Mercado anual | Captura Tracbel | Oportunidade
 *
 * TRÊS DELES NÃO TÊM DADO HOJE, e dizem o que falta e qual issue o destrava —
 * com a frase que o SERVIDOR devolve (issue 69, parte A), e não uma escrita
 * aqui. Isto não é um espaço reservado: é a resposta certa. Um número plausível
 * e errado leva a uma decisão; um espaço explicado leva a uma pergunta.
 *
 * O DESENHO É O DA MAQUETE, e não o `CartaoDeIndicador` de antes: cartão tingido,
 * o glifo colorido grande numa coluna à esquerda e, empilhados à direita, o nome
 * com a dica, o valor com a unidade pequena ao lado e a linha "vs. ano anterior".
 * O `CartaoDeIndicador` continua como era — a aba Oportunidades da ficha do
 * município o usa, e a maquete dela é outra. Por isso o cartão daqui é outro
 * componente, e não uma variação escondida atrás de uma prop.
 *
 * A AUSÊNCIA TEM O LAYOUT DA PRESENÇA (decisão do usuário de 23/09/2026): onde a
 * maquete mostra "R$ 4,2 bi valor de mercado", a tela mostra "— ⓘ valor de
 * mercado" — o traço no lugar do número, a unidade no lugar dela, o motivo na
 * dica. A linha de variação existe nos quatro (27/09/2026): na captura e na
 * oportunidade, contra o mesmo trecho do ano anterior; na demanda e no mercado
 * anual, o traço com o motivo — eles são estruturais e não têm ano anterior.
 *
 * A LINHA DE BAIXO É A DA VARIAÇÃO (maquete), e por isso o contexto de cada
 * número mora em dois lugares: o que QUALIFICA O VALOR fica ao lado dele, no
 * lugar da unidade — "parcial — sem preço de …" quando o mercado anual soma só as
 * categorias que têm preço, "máquinas não capturadas" na oportunidade —, e o que
 * EXPLICA A CONTA vai para a dica do nome.
 *
 * CAPTURA, E NÃO MARKET SHARE (issue 162): enquanto o denominador for a demanda
 * ESTIMADA pelo motor, o nome é captura — e a unidade da maquete, "participação
 * no mercado", é a mesma palavra em português. Ela vira "da demanda estimada",
 * que é o que o número divide. A frase "das máquinas que a região renova por
 * ano" diria o mesmo, mas com "região" sozinha — que se lê como a sub-região — e
 * sem dizer que o denominador é estimativa, que é justamente o que o separa de
 * participação.
 *
 * A DEMANDA DA CAPTURA E DA OPORTUNIDADE É A DO PERÍODO (decisão do Ricardo de
 * 27/09/2026): a anual proporcional aos meses — com o ano fiscal até agosto, dez
 * doze avos. Com o ano inteiro, o número é o da planilha. O cartão "Demanda
 * anual" continua anual; é só a conta das duas que usa o período.
 */

import {
  ChartNoAxesColumnIncreasing,
  ChartPie,
  Lightbulb,
  Target,
} from 'lucide-react';
import { fatiasEmTexto, frasesDaProcedencia, montarSomavel } from '../comum/comparacoes';
import type {
  MomentoDoRecorte,
  NumeroNoAnoAnterior,
  NumerosDeDecisao,
  ProcedenciaDoIndicador,
  VendasDeMaquinaDoRecorte,
} from '../../tipos/territorio';
import { VariacaoContraOAnoAnterior } from '../territorio/comparacao';
import { useComparacao } from '../territorio/contextoDaComparacao';
import { reaisCompactos } from '../territorio/escalas';
import { nº, porcento } from '../territorio/indicadoresDaAdr';
import { fraseDosPrecos, marcaDeParcial } from './precosDeReferencia';
import { CartaoDeDecisao } from './CartaoDeDecisao';
import { VariacaoAusente } from './VariacaoAusente';

/* AS QUATRO CONSTANTES DE MOTIVO SAÍRAM DAQUI (issue 69, parte A).
 *
 * Elas diziam, em texto escrito no TypeScript, por que cada número faltava — e as mesmas frases estavam
 * repetidas na ficha do município e no bloco Performance, com redações que já tinham começado a divergir.
 * Agora o motivo vem da API, de `DecisaoDoMercado.Frase`, que tem teste. A tela não escreve mais por que um
 * número falta: ela mostra o que o servidor afirma. */

/** Casas fixas: um fator neutro tem de sair `1,00`, e não `1`. */
const fator = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });


export function KpisExecutivos({
  momento,
  demandaEstrutural,
  demandaDeSaoPaulo,
  carregando,
  procedenciaDaDemanda,
  numeros,
  maquinasVendidas,
  procedenciaDasVendas,
  tipoDeProduto = null,
}: {
  /**
   * O nome da categoria do filtro "Tipo de produto", quando ele está ligado. A demanda aqui passa a ser a da
   * categoria, e o fator agregado do momento continua sendo o do recorte inteiro — a dica diz isso.
   */
  tipoDeProduto?: string | null;
  /**
   * As vendas de máquina do recorte em UNIDADES — o numerador da captura (issue 69, D-P08).
   *
   * Ela está aqui por uma razão só: dizer QUANTAS máquinas e POR QUAL DATA. Uma captura de 12% sem o
   * numerador escrito é um número que ninguém confere, e o critério de data é decisão registrada
   * (D-P08.1) — deixá-lo implícito seria escondê-lo.
   */
  maquinasVendidas: VendasDeMaquinaDoRecorte | null;
  /** O carimbo da captura, do servidor; nulo quando o ART não trouxe venda nenhuma. */
  procedenciaDasVendas: ProcedenciaDoIndicador | null;
  /**
   * Os quatro números com o motivo de cada ausência — da API (issue 69, parte A).
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
  // A DEMANDA É SOMÁVEL: ela compara por FATIA. Sem denominador, a fatia some —
  // e não vira 0%, que afirmaria que a região não demanda nada.
  const fatias = fatiasEmTexto(montarSomavel(demandaEstrutural, null, demandaDeSaoPaulo));

  // O QUE O MOMENTO FAZ COM ELA, direto do fator agregado — nenhum número novo:
  // fator 0,88 é a mesma coisa que "12% abaixo da estrutural". Era a linha de
  // baixo do cartão; a maquete põe ali a variação contra o ano anterior, então a
  // leitura do momento foi para a dica do nome, inteira.
  const fatorAgregado = momento?.fatorAgregado ?? null;
  // ARREDONDADA ANTES DO SINAL, e o `|| 0` tira o "-0": um fator de 0,998 é
  // −0,2%, que arredonda para zero — e "-0%" leria como queda.
  const variacao = fatorAgregado == null ? null : Math.round((fatorAgregado - 1) * 100) || 0;

  const mercado = numeros?.mercadoAnual ?? null;
  const frasePrecos = fraseDosPrecos(mercado);

  // O MESMO TRECHO DO ANO ANTERIOR (27/09/2026), com a frase do servidor para cada
  // ausência: a demanda e o mercado são estruturais; a captura e a oportunidade
  // comparam as vendas de antes contra a MESMA demanda.
  const { numeros: doAnoAnterior } = useComparacao();
  const lendo = carregando || numeros === null;
  const semAnterior = (n: NumeroNoAnoAnterior | undefined, deQue: string) => (
    <VariacaoAusente deQue={deQue} motivo={lendo ? 'A leitura ainda não voltou.' : (n?.frase ?? 'A leitura não trouxe o ano anterior.')} />
  );

  return (
    <div className="dash-kpis mv-kpis" data-bloco="kpis-executivos">
      <CartaoDeDecisao
        rotulo="Demanda anual"
        icone={ChartNoAxesColumnIncreasing}
        tom="demanda"
        valor={demandaEstrutural === null ? null : nº(Math.round(demandaEstrutural))}
        carregando={carregando}
        unidade="máquinas"
        motivoSemDado={numeros?.demandaAnual.frase}
        variacao={semAnterior(doAnoAnterior?.demandaAnual, 'demanda anual')}
        sobre={
          <>
            <p>
              Máquinas por ano: o que o parque do recorte renova — o parque dividido pelo ciclo de renovação de
              cada cultura (issue 72).
            </p>
            {variacao != null && fatorAgregado != null && !tipoDeProduto && (
              <p>
                {`${variacao > 0 ? '+' : ''}${nº(variacao)}% com o momento do mercado`} — o fator
                agregado {fator(fatorAgregado)} aplicado a esta demanda.
              </p>
            )}
            {/* COM O FILTRO DE TIPO DE PRODUTO, O FATOR AGREGADO NÃO É O DESTA DEMANDA: ele pesa todas as
                categorias do recorte, e a demanda ajustada da categoria aplica o fator de cada cultura dela. */}
            {variacao != null && fatorAgregado != null && tipoDeProduto && (
              <p>
                {`O momento do mercado do recorte inteiro, com todas as categorias, é ${variacao > 0 ? '+' : ''}${nº(variacao)}%`}{' '}
                (fator agregado {fator(fatorAgregado)}). Esta demanda é só a de {tipoDeProduto}, e a ajustada dela
                aplica o fator de cada cultura da categoria — ela pode andar diferente do agregado.
              </p>
            )}
            {fatias && <p>{fatias}</p>}
            {procedenciaDaDemanda && <p>{frasesDaProcedencia(procedenciaDaDemanda)}</p>}
          </>
        }
      />
      {/* OS TRÊS QUE AINDA NÃO TÊM DADO VÊM DA API (issue 69, parte A).
          Eram `valor={null}` e três constantes escritas aqui. Nenhum número mudou — o que mudou é que a
          ausência virou afirmação do servidor, com teste, e com a mesma redação da ficha do município. */}
      <CartaoDeDecisao
        rotulo="Mercado anual"
        icone={ChartPie}
        tom="mercado"
        valor={mercado?.valor == null ? null : reaisCompactos(mercado.valor)}
        // PARCIAL SE DIZ AO LADO DO NÚMERO, e não só na dica: a soma das
        // categorias com preço, lida sem a marca, afirmaria que a categoria sem
        // preço não vale nada.
        unidade={marcaDeParcial(mercado) ?? 'valor de mercado'}
        motivoSemDado={mercado?.frase}
        variacao={semAnterior(doAnoAnterior?.mercadoAnual, 'mercado anual')}
        sobre={
          <>
            <p>
              Quanto vale, em reais, a demanda anual de máquinas do recorte: a demanda de cada categoria vezes o
              preço de referência dela — nunca a demanda inteira vezes um preço genérico. Categoria sem preço não
              entra como zero: fica fora da soma, e o número sai marcado como parcial, com o nome dela.
            </p>
            {/* O PREÇO FICA ESCRITO (issue 70): um valor em reais sem o preço que o fez é um número que
                ninguém confere. */}
            {frasePrecos && <p>{frasePrecos}</p>}
          </>
        }
      />
      <CartaoDeDecisao
        rotulo="Captura Tracbel"
        icone={Target}
        tom="captura"
        valor={numeros?.capturaPercentual.valor == null ? null : porcento(numeros.capturaPercentual.valor)}
        unidade="da demanda estimada"
        motivoSemDado={numeros?.capturaPercentual.frase}
        // A CAPTURA JÁ É UM PERCENTUAL: a variação é em pontos percentuais, e o
        // número de antes divide as vendas de antes pela MESMA demanda.
        variacao={
          <VariacaoContraOAnoAnterior
            deQue="captura tracbel"
            atual={lendo ? null : (numeros?.capturaPercentual.valor ?? null)}
            anterior={lendo ? null : (doAnoAnterior?.capturaPercentual.valor ?? null)}
            motivoSemAnterior={
              lendo ? null : numeros?.capturaPercentual.valor == null ? numeros?.capturaPercentual.frase : doAnoAnterior?.capturaPercentual.frase
            }
            formatar={porcento}
            emPontos
          />
        }
        sobre={
          <>
            <p>
              A parte da demanda estimada DO PERÍODO que a Tracbel vendeu, em máquinas. A demanda do período é a
              anual proporcional aos meses — com dez meses, dez doze avos dela —, para as vendas de uma parte do ano
              não serem lidas contra o ano inteiro; com o ano fiscal inteiro, o número é o da planilha. Chama-se
              captura, e não participação de mercado: o denominador é a demanda que o motor estima, e participação
              exigiria o total vendido por todos os fabricantes, que nenhuma fonte aberta publica (issue 162).
            </p>
            {/* O NUMERADOR FICA ESCRITO, e com o critério de data junto (issue 69, D-P08.1).
                Uma captura de 12% sem o numerador é um número que ninguém confere; e o
                critério de data é DECISÃO — decidido não é o mesmo que implícito, e a frase
                vem do servidor, que é quem contou.

                O NUMERADOR NÃO É O TOTAL DO ART (D-P01, 27/09/2026): são as máquinas das
                categorias que têm demanda, e o que ficou de fora vem contado na mesma frase. */}
            {maquinasVendidas && numeros?.baseDaCaptura && (
              <p>{`${numeros.baseDaCaptura.frase} ${maquinasVendidas.fraseDoCriterio}`}</p>
            )}
            {procedenciaDasVendas && <p>{frasesDaProcedencia(procedenciaDasVendas)}</p>}
          </>
        }
      />
      <CartaoDeDecisao
        rotulo="Oportunidade"
        icone={Lightbulb}
        tom="oportunidade"
        valor={numeros?.oportunidade.valor == null ? null : nº(Math.round(numeros.oportunidade.valor))}
        // "MÁQUINAS NÃO CAPTURADAS", e não o "mercado não capturado" da maquete:
        // o número é contado em máquinas, e "mercado" ao lado dele se leria como
        // reais — que é o mercado anual, o cartão ao lado.
        unidade="máquinas não capturadas"
        motivoSemDado={numeros?.oportunidade.frase}
        variacao={
          <VariacaoContraOAnoAnterior
            deQue="oportunidade"
            atual={lendo ? null : (numeros?.oportunidade.valor ?? null)}
            anterior={lendo ? null : (doAnoAnterior?.oportunidade.valor ?? null)}
            motivoSemAnterior={
              lendo ? null : numeros?.oportunidade.valor == null ? numeros?.oportunidade.frase : doAnoAnterior?.oportunidade.frase
            }
            formatar={(v) => `${nº(Math.round(v))} máquinas`}
          />
        }
        sobre={
          <>
            <p>
              Em máquinas: a demanda ajustada pelo momento, proporcional aos meses do período, menos o que a Tracbel
              já vendeu nele, nunca abaixo de zero (issue 162). É o que o mercado de hoje comporta e ainda não foi
              capturado — e não o de um ano médio.
            </p>
            {/* EM REAIS, O POTENCIAL INCREMENTAL (issue 70): a oportunidade de cada categoria vezes o preço dela. */}
            {numeros?.potencialIncremental?.valor != null ? (
              <p>
                {`Em reais, o potencial incremental é ${reaisCompactos(numeros.potencialIncremental.valor)}`}
                {marcaDeParcial(numeros.potencialIncremental) ? ` (${marcaDeParcial(numeros.potencialIncremental)})` : ''}
                {' '}— a oportunidade de cada categoria vezes o preço dela. O que uma categoria vendeu além da demanda
                não abate a falta de outra, e por isso ele pode passar destas máquinas vezes um preço médio.
              </p>
            ) : (
              numeros?.potencialIncremental?.frase && <p>{numeros.potencialIncremental.frase}</p>
            )}
          </>
        }
      />
    </div>
  );
}
