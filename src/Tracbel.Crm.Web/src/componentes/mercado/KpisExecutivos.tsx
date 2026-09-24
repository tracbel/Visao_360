/**
 * OS QUATRO NÚMEROS DE DECISÃO (documento 50, §4.1; redesenhados na T4.6 e na
 * fidelidade às maquetes de 23/09/2026).
 *
 *   Demanda anual | Mercado anual | Captura Tracbel | Oportunidade
 *
 * TRÊS DELES NÃO TÊM DADO HOJE, e nascem dizendo o que falta e qual issue o
 * destrava. Isto não é um espaço reservado: é a resposta certa. Um número
 * plausível e errado leva a uma decisão; um espaço explicado leva a uma pergunta.
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
 * CAPTURA, E NÃO MARKET SHARE (issue 162): enquanto o denominador for a demanda
 * ESTIMADA pelo motor, o nome é captura — e a unidade da maquete, "participação
 * no mercado", é a mesma palavra em português. Ela vira "da demanda estimada",
 * que é o que o número divide.
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
import type { MomentoDoRecorte, ProcedenciaDoIndicador } from '../../tipos/territorio';
import { nº } from '../territorio/indicadoresDaAdr';
import { VariacaoAusente } from './VariacaoAusente';

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
  motivoSemDado: string;
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
          {valor === null ? <ValorAusente motivo={motivoSemDado} oQue={nome} /> : <strong>{valor}</strong>}
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
  // A DEMANDA É SOMÁVEL: ela compara por FATIA. Sem denominador, a fatia some —
  // e não vira 0%, que afirmaria que a região não demanda nada.
  const fatias = fatiasEmTexto(montarSomavel(demandaEstrutural, null, demandaDeSaoPaulo));

  // O QUE O MOMENTO FAZ COM ELA, direto do fator agregado — nenhum número novo:
  // fator 0,88 é a mesma coisa que "12% abaixo da estrutural". Era a linha de
  // baixo do cartão; a maquete põe ali a variação contra o ano anterior, então a
  // leitura do momento foi para a dica do nome, inteira.
  const fatorAgregado = momento?.fatorAgregado ?? null;
  const variacao = fatorAgregado == null ? null : (fatorAgregado - 1) * 100;

  return (
    <div className="dash-kpis mv-kpis" data-bloco="kpis-executivos">
      <CartaoDeDecisao
        rotulo="Demanda anual"
        icone={ChartNoAxesColumnIncreasing}
        tom="demanda"
        valor={carregando || demandaEstrutural === null ? null : nº(Math.round(demandaEstrutural))}
        unidade="máquinas"
        motivoSemDado={DEMANDA_SEM_DADO}
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
      <CartaoDeDecisao
        rotulo="Mercado anual"
        icone={ChartPie}
        tom="mercado"
        valor={null}
        unidade="valor de mercado"
        motivoSemDado={MERCADO_ANUAL_SEM_DADO}
        sobre="Quanto vale, em reais, a demanda anual de máquinas do recorte: a demanda de cada categoria vezes o preço de referência dela."
      />
      <CartaoDeDecisao
        rotulo="Captura Tracbel"
        icone={Target}
        tom="captura"
        valor={null}
        unidade="da demanda estimada"
        motivoSemDado={CAPTURA_SEM_DADO}
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
        valor={null}
        unidade="mercado não capturado"
        motivoSemDado={OPORTUNIDADE_SEM_DADO}
        sobre="O que a demanda ajustada comporta e a Tracbel ainda não vendeu — a demanda menos as vendas, nunca abaixo de zero (issue 162)."
      />
    </div>
  );
}
