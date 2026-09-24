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
 * O `CartaoDeIndicador` continua como era — Rentabilidade, Crédito e a ficha do
 * município o usam, e a maquete deles é outra. Por isso o cartão daqui é outro
 * componente, e não uma variação escondida atrás de uma prop.
 *
 * A AUSÊNCIA TEM O LAYOUT DA PRESENÇA (decisão do usuário de 23/09/2026): onde a
 * maquete mostra "R$ 4,2 bi valor de mercado", a tela mostra "— ⓘ valor de
 * mercado" — o traço no lugar do número, a unidade no lugar dela, o motivo na
 * dica. A linha de variação existe nos quatro, com traço, porque não há ano
 * anterior na leitura (issue 69).
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
 */

import {
  ChartNoAxesColumnIncreasing,
  ChartPie,
  Lightbulb,
  Target,
  type LucideIcon,
} from 'lucide-react';
import type { ReactNode } from 'react';
import { InfoTooltip } from '../InfoTooltip';
import { fatiasEmTexto, frasesDaProcedencia, montarSomavel } from '../comum/comparacoes';
import { ValorAusente } from '../comum/ValorAusente';
import type { MomentoDoRecorte, NumerosDeDecisao, ProcedenciaDoIndicador } from '../../tipos/territorio';
import { reaisCompactos } from '../territorio/escalas';
import { nº, porcento } from '../territorio/indicadoresDaAdr';
import { VariacaoAusente } from './VariacaoAusente';

/* AS QUATRO CONSTANTES DE MOTIVO SAÍRAM DAQUI (issue 69, parte A).
 *
 * Elas diziam, em texto escrito no TypeScript, por que cada número faltava — e as mesmas frases estavam
 * repetidas na ficha do município e no bloco Performance, com redações que já tinham começado a divergir.
 * Agora o motivo vem da API, de `DecisaoDoMercado.Frase`, que tem teste. A tela não escreve mais por que um
 * número falta: ela mostra o que o servidor afirma. */

/** Casas fixas: um fator neutro tem de sair `1,00`, e não `1`. */
const fator = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

type Tom = 'demanda' | 'mercado' | 'captura' | 'oportunidade';

/**
 * UM NÚMERO DE DECISÃO, no desenho da maquete.
 *
 * AS TRÊS LINHAS EXISTEM SEMPRE — nome, valor, variação —, com ou sem dado: é o
 * que mantém os quatro cartões da mesma altura sem fixar altura em pixel. Quando
 * um cartão fica mais alto que o vizinho, o olho lê a diferença como diferença de
 * importância, e aqui os quatro pesam igual.
 */
function CartaoDeDecisao({
  rotulo,
  icone: Icone,
  tom,
  valor,
  unidade,
  sobre,
  motivoSemDado,
}: {
  rotulo: string;
  icone: LucideIcon;
  tom: Tom;
  /** O valor pronto para a tela. `null` mostra o traço com o motivo — nunca zero. */
  valor: string | null;
  /** A unidade da maquete, ao lado do número e menor que ele. */
  unidade: string;
  /**
   * O QUE O NÚMERO É, na dica ao lado do nome: definição, o que o momento faz
   * com ele e de onde vem. É o que a maquete não mostra e a tela não pode perder.
   */
  sobre: ReactNode;
  /**
   * Por que o número falta — a frase do servidor (issue 69, parte A).
   *
   * Ausente enquanto a leitura não respondeu: aí sai o traço SEM dica, que é o
   * certo — não se pode afirmar por que falta um número antes de saber se ele falta.
   */
  motivoSemDado: string | undefined;
}) {
  const nome = rotulo.toLowerCase();

  return (
    <div className="mv-kpi" data-kpi={rotulo} data-tom={tom}>
      {/* O GLIFO É ENDEREÇO, NÃO INFORMAÇÃO: ele não diz nada que o nome já não
          diga, e por isso some do leitor de tela. Serve para o olho achar "o de
          captura" de longe, e é colorido e grande como na maquete — sem
          ladrilho em volta. */}
      <span className="mv-kpi-glifo" aria-hidden="true">
        <Icone size={30} strokeWidth={2} />
      </span>

      <div className="mv-kpi-corpo">
        <div className="mv-kpi-rotulo">
          <span>{rotulo}</span>
          <InfoTooltip rotulo={`Fonte e método: ${rotulo}`} texto={sobre} />
        </div>

        <div className="mv-kpi-valor">
          {/* O TRAÇO OCUPA O LUGAR DO NÚMERO, e a unidade continua no dela: a
              linha tem a forma da maquete, e o motivo inteiro está na dica. */}
          {valor !== null ? (
            <strong>{valor}</strong>
          ) : motivoSemDado ? (
            <ValorAusente motivo={motivoSemDado} oQue={nome} />
          ) : (
            <span className="cad-ausente">
              <span aria-hidden="true">—</span>
              <span className="cad-so-leitor">sem dado</span>
            </span>
          )}
          <span className="mv-kpi-unidade">{unidade}</span>
        </div>

        <div className="mv-kpi-contexto">
          <VariacaoAusente deQue={nome} />
        </div>
      </div>
    </div>
  );
}

export function KpisExecutivos({
  momento,
  demandaEstrutural,
  demandaDeSaoPaulo,
  carregando,
  procedenciaDaDemanda,
  numeros,
}: {
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
  const variacao = fatorAgregado == null ? null : (fatorAgregado - 1) * 100;

  const mercado = numeros?.mercadoAnual ?? null;

  return (
    <div className="dash-kpis mv-kpis" data-bloco="kpis-executivos">
      <CartaoDeDecisao
        rotulo="Demanda anual"
        icone={ChartNoAxesColumnIncreasing}
        tom="demanda"
        valor={carregando || demandaEstrutural === null ? null : nº(Math.round(demandaEstrutural))}
        unidade="máquinas"
        motivoSemDado={numeros?.demandaAnual.frase}
        sobre={
          <>
            <p>
              Máquinas por ano: o que o parque do recorte renova — o parque dividido pelo ciclo de renovação de
              cada cultura (issue 72).
            </p>
            {variacao != null && fatorAgregado != null && (
              <p>
                {`${variacao > 0 ? '+' : ''}${nº(Math.round(variacao))}% com o momento do mercado`} — o fator
                agregado {fator(fatorAgregado)} aplicado a esta demanda.
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
        unidade={
          mercado?.parcial ? `parcial — sem preço de ${mercado.categoriasSemPreco.join(', ')}` : 'valor de mercado'
        }
        motivoSemDado={mercado?.frase}
        sobre={
          'Quanto vale, em reais, a demanda anual de máquinas do recorte: a demanda de cada categoria vezes o ' +
          'preço de referência dela — nunca a demanda inteira vezes um preço genérico. Categoria sem preço não ' +
          'entra como zero: fica fora da soma, e o número sai marcado como parcial, com o nome dela.'
        }
      />
      <CartaoDeDecisao
        rotulo="Captura Tracbel"
        icone={Target}
        tom="captura"
        valor={numeros?.capturaPercentual.valor == null ? null : porcento(numeros.capturaPercentual.valor)}
        unidade="da demanda estimada"
        motivoSemDado={numeros?.capturaPercentual.frase}
        sobre={
          'A parte da demanda anual estimada que a Tracbel vendeu, em máquinas. Chama-se captura, e não ' +
          'participação de mercado: o denominador é a demanda que o motor estima, e participação exigiria o ' +
          'total vendido por todos os fabricantes, que nenhuma fonte aberta publica (issue 162).'
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
        sobre={
          'Em máquinas: a demanda ajustada pelo momento menos o que a Tracbel já vendeu, nunca abaixo de zero ' +
          '(issue 162). É o que o mercado de hoje comporta e ainda não foi capturado — e não o de um ano médio.'
        }
      />
    </div>
  );
}
