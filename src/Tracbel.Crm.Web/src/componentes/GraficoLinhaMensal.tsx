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
 */
import {
  CategoryScale,
  Chart as ChartJS,
  Filler,
  LineElement,
  LinearScale,
  PointElement,
  Tooltip,
  type ChartOptions,
  type TooltipItem,
} from 'chart.js';
import { Line } from 'react-chartjs-2';
import { COR_DA_GRADE, FONTE_DOS_GRAFICOS } from './padraoDosGraficos';
import { useFontesProntas } from './useFontesProntas';

ChartJS.register(CategoryScale, Filler, LineElement, LinearScale, PointElement, Tooltip);

const COR_LINHA = '#367C2B';
const COR_AREA = 'rgba(54, 124, 43, 0.10)';
const COR_PARCIAL = '#9CA3AF';

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
};

export function GraficoLinhaMensal({
  rotulos,
  valores,
  largura,
  altura,
  formatar,
  ultimoParcial = false,
}: GraficoLinhaMensalProps) {
  const fontesProntas = useFontesProntas();
  const ultimo = valores.length - 1;

  const data = {
    labels: rotulos,
    datasets: [
      {
        data: valores,
        borderColor: COR_LINHA,
        backgroundColor: COR_AREA,
        borderWidth: 2,
        fill: true,
        tension: 0.3,
        pointRadius: valores.map((_, i) => (i === ultimo && ultimoParcial ? 4 : 3)),
        pointBackgroundColor: valores.map((_, i) =>
          i === ultimo && ultimoParcial ? '#FFFFFF' : COR_LINHA,
        ),
        pointBorderColor: valores.map((_, i) =>
          i === ultimo && ultimoParcial ? COR_PARCIAL : COR_LINHA,
        ),
        pointBorderWidth: 2,
        // O TRECHO FINAL TRACEJADO quando o mês ainda corre. `segment` recebe o par de pontos e
        // decide por trecho — é o único jeito de ter um pedaço da mesma linha com outro traço.
        segment: ultimoParcial
          ? {
              borderDash: (ctx: { p1DataIndex: number }) =>
                ctx.p1DataIndex === ultimo ? [5, 4] : undefined,
              borderColor: (ctx: { p1DataIndex: number }) =>
                ctx.p1DataIndex === ultimo ? COR_PARCIAL : undefined,
            }
          : undefined,
      },
    ],
  };

  const options: ChartOptions<'line'> = {
    responsive: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (item: TooltipItem<'line'>) =>
            item.dataIndex === ultimo && ultimoParcial
              ? `${formatar(item.parsed.y)} — mês em curso`
              : formatar(item.parsed.y),
        },
      },
    },
    scales: {
      x: {
        grid: { display: false },
        ticks: { font: { size: 10, family: FONTE_DOS_GRAFICOS } },
      },
      y: {
        beginAtZero: true,
        grid: { color: COR_DA_GRADE },
        ticks: {
          font: { size: 10, family: FONTE_DOS_GRAFICOS },
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
