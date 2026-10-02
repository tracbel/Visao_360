/**
 * A série mensal — uma linha por mês, com área sob ela.
 *
 * POR QUE LINHA E NÃO BARRA: doze meses de faturamento são uma sequência, e a pergunta que a
 * diretoria faz sobre eles é de tendência, não de comparação item a item. Barra convida a
 * comparar março com julho; linha mostra o caminho entre os dois.
 *
 * O ÚLTIMO PONTO PODE SER PARCIAL. Quando o mês mais recente ainda está correndo — dia 6 de
 * setembro tem seis dias de faturamento contra trinta e um de agosto —, ele é desenhado
 * TRACEJADO e com o ponto vazado. Sem isso a linha despenca no fim e parece queda de vendas,
 * quando é só o calendário.
 *
 * A SÉRIE ANTERIOR É OPCIONAL (27/09/2026): o mesmo mês, doze meses antes, como uma linha cinza
 * tracejada atrás da principal — "a janela recente sobre a anterior", que é como a maquete
 * desenha a evolução do crédito. Sem ela, o gráfico é o de sempre.
 */
// O REGISTRO VEM ANTES DO PADRÃO DOS GRÁFICOS: ver o comentário de registroDaLinhaMensal.ts.
import './registroDaLinhaMensal';
import type { ChartOptions, TooltipItem } from 'chart.js';
import { Line } from 'react-chartjs-2';
import { COR_DA_GRADE, FONTE_DOS_GRAFICOS } from './padraoDosGraficos';
import { useFontesProntas } from './useFontesProntas';

const COR_LINHA = '#367C2B';
const COR_AREA = 'rgba(54, 124, 43, 0.10)';
const COR_PARCIAL = '#9CA3AF';
const COR_ANTERIOR = '#9CA3AF';

/**
 * O DESENHO DE UMA MAQUETE (29/09/2026, Visão 360): a linha, a área, o ponto e a grade. Sem ele, o gráfico é o de sempre.
 *
 * O MÊS PARCIAL CONTINUA DIFERENTE DOS OUTROS em qualquer aparência — tracejado e na cor dele —: cheio ou vazado é só o
 * desenho do ponto.
 */
export type AparenciaDaLinhaMensal = {
  corDaLinha?: string;
  corDaArea?: string;
  corDoParcial?: string;
  /** O ponto do mês parcial pintado na cor dele, em vez de vazado. */
  parcialCheio?: boolean;
  larguraDaLinha?: number;
  raioDoPonto?: number;
  /** Borda branca em volta de cada ponto. */
  pontoComBorda?: boolean;
  /** As linhas verticais da grade, uma por mês. */
  gradeVertical?: boolean;
  tamanhoDaFonte?: number;
  corDaFonte?: string;
};

export type GraficoLinhaMensalProps = {
  /** O rótulo de cada mês, já formatado. Ex.: `set/26`. */
  rotulos: string[];
  valores: number[];
  largura: number;
  altura: number;
  /** Como o valor aparece no balão e no eixo. */
  formatar: (valor: number) => string;
  /** Quando verdadeiro, o último ponto é parcial e sai tracejado. */
  ultimoParcial?: boolean;
  /** O mesmo mês doze meses antes, alinhado a `valores`; nulo onde não há dado. */
  anteriores?: (number | null)[];
  /** Os nomes das duas séries no balão — só quando há a anterior. */
  nomeDaSerie?: string;
  nomeDaAnterior?: string;
  aparencia?: AparenciaDaLinhaMensal;
  /**
   * Linhas a mais no balão de cada mês, embaixo do valor (29/09/2026): na Visão 360, as máquinas entregues e a nota do
   * Protheus do mesmo mês, como conferência. Recebe o índice do mês.
   */
  linhasDoBalao?: (indice: number) => string[];
};

export function GraficoLinhaMensal({
  rotulos,
  valores,
  largura,
  altura,
  formatar,
  ultimoParcial = false,
  anteriores,
  nomeDaSerie = 'Recente',
  nomeDaAnterior = 'Um ano antes',
  aparencia = {},
  linhasDoBalao,
}: GraficoLinhaMensalProps) {
  const fontesProntas = useFontesProntas();
  const ultimo = valores.length - 1;
  const corDaLinha = aparencia.corDaLinha ?? COR_LINHA;
  const corDoParcial = aparencia.corDoParcial ?? COR_PARCIAL;
  const raio = aparencia.raioDoPonto ?? 3;
  const ehParcial = (i: number) => i === ultimo && ultimoParcial;
  const fonte = { size: aparencia.tamanhoDaFonte ?? 10, family: FONTE_DOS_GRAFICOS };

  const data = {
    labels: rotulos,
    datasets: [
      {
        label: nomeDaSerie,
        data: valores,
        borderColor: corDaLinha,
        backgroundColor: aparencia.corDaArea ?? COR_AREA,
        borderWidth: aparencia.larguraDaLinha ?? 2,
        fill: true,
        tension: 0.3,
        pointRadius: valores.map((_, i) => (ehParcial(i) ? raio + 1 : raio)),
        pointBackgroundColor: valores.map((_, i) =>
          ehParcial(i) ? (aparencia.parcialCheio ? corDoParcial : '#FFFFFF') : corDaLinha,
        ),
        pointBorderColor: valores.map((_, i) =>
          aparencia.pontoComBorda ? '#FFFFFF' : ehParcial(i) ? corDoParcial : corDaLinha,
        ),
        pointBorderWidth: aparencia.pontoComBorda ? 1.5 : 2,
        // O TRECHO FINAL TRACEJADO quando o mês ainda corre. `segment` recebe o par de pontos e
        // decide por trecho — é o único jeito de ter um pedaço da mesma linha com outro traço.
        segment: ultimoParcial
          ? {
              borderDash: (ctx: { p1DataIndex: number }) =>
                ctx.p1DataIndex === ultimo ? [5, 4] : undefined,
              borderColor: (ctx: { p1DataIndex: number }) =>
                ctx.p1DataIndex === ultimo ? corDoParcial : undefined,
            }
          : undefined,
      },
      // A ANTERIOR VEM DEPOIS NA LISTA E ATRÁS NO DESENHO (`order` maior é desenhado antes): a
      // leitura é a da série recente, e a de antes é a referência.
      ...(anteriores
        ? [
            {
              label: nomeDaAnterior,
              data: anteriores,
              borderColor: COR_ANTERIOR,
              backgroundColor: 'transparent',
              borderWidth: 1.5,
              borderDash: [5, 4],
              fill: false,
              tension: 0.3,
              pointRadius: 2,
              pointBackgroundColor: COR_ANTERIOR,
              pointBorderColor: COR_ANTERIOR,
              pointBorderWidth: 1,
              order: 2,
            },
          ]
        : []),
    ],
  };

  const options: ChartOptions<'line'> = {
    responsive: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          // O chart.js 4.5 passou a tipar o ponto como `number | null`, e está certo: ponto sem
          // dado existe no eixo e não tem valor. Formatá-lo viraria "R$ 0,00", que é outra coisa —
          // zero é uma medida, ausência não.
          label: (item: TooltipItem<'line'>) => {
            const nome = anteriores ? `${item.dataset.label ?? ''}: ` : '';
            if (item.parsed.y === null) return `${nome}sem dado`;

            return item.datasetIndex === 0 && item.dataIndex === ultimo && ultimoParcial
              ? `${nome}${formatar(item.parsed.y)} — mês em curso`
              : `${nome}${formatar(item.parsed.y)}`;
          },
          ...(linhasDoBalao
            ? { afterLabel: (item: TooltipItem<'line'>) => (item.datasetIndex === 0 ? linhasDoBalao(item.dataIndex) : []) }
            : {}),
        },
      },
    },
    scales: {
      x: {
        grid: aparencia.gradeVertical ? { color: COR_DA_GRADE } : { display: false },
        ticks: { font: fonte, color: aparencia.corDaFonte },
      },
      y: {
        beginAtZero: true,
        grid: { color: COR_DA_GRADE },
        ticks: {
          font: fonte,
          color: aparencia.corDaFonte,
          callback: (valor) => formatar(Number(valor)),
          maxTicksLimit: 5,
        },
      },
    },
  };

  return (
    <Line
      key={fontesProntas ? 'fontes-prontas' : 'fontes-carregando'}
      data={data}
      options={options}
      width={largura}
      height={altura}
    />
  );
}
