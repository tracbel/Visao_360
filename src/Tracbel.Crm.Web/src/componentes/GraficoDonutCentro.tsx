/**
 * Donut/gauge genérico com texto central opcional — porte das três chamadas
 * de `new Chart(..., { type: 'doughnut' })` em `mountVisao360()`
 * (prototipo/referencia/assets/app.js:7855-8038):
 *  - Gauge "Conhecimento de mercado" (meia-lua: rotation=-90, circumference=180)
 *  - Donut "Status da cobertura" (círculo completo, texto central)
 *  - Donut "Mix por linha" (círculo completo, sem texto central)
 *
 * Reutilizável em qualquer card que precise de um donut com 1-N segmentos e,
 * opcionalmente, um número grande centralizado (ex.: outros dashboards).
 */
import { Chart as ChartJS, ArcElement, Tooltip, type ChartOptions, type Plugin, type TooltipItem } from 'chart.js';
import { Doughnut } from 'react-chartjs-2';

ChartJS.register(ArcElement, Tooltip);

export type SegmentoDonut = { valor: number; cor: string };

export type TextoCentroDonut = {
  linha1: string;
  linha2?: string;
  /** Cor da linha1 (a linha2 é sempre `#6B7280`, igual ao protótipo). */
  corLinha1?: string;
  tamanhoLinha1?: number;
  tamanhoLinha2?: number;
  /** Distância vertical (px) entre a linha1 e a linha2. */
  deslocamentoLinha2?: number;
  /**
   * Quando `true`, o texto é ancorado à base da área do gráfico (usado pelo
   * gauge em meia-lua). Quando `false` (padrão), fica centralizado
   * verticalmente (donuts de círculo completo).
   */
  naBase?: boolean;
};

export type GraficoDonutCentroProps = {
  segmentos: SegmentoDonut[];
  largura: number;
  altura: number;
  /** Ex.: `'72%'`. */
  cutout: string;
  /** Graus. `0` = círculo completo (padrão). `-90` = início da meia-lua no gauge. */
  rotation?: number;
  /** Graus. `360` = círculo completo (padrão). `180` = meia-lua. */
  circumference?: number;
  /** Borda branca de 2px entre fatias (usada pelos donuts, não pelo gauge). */
  bordaBranca?: boolean;
  /** Quando presente, desenha o texto central via plugin `afterDraw`. */
  centro?: TextoCentroDonut;
  /** Sufixo do tooltip padrão (ex.: `'%'` no Mix por linha). Sem isso, usa o tooltip padrão do Chart.js. */
  tooltipUnidade?: string;
};

export function GraficoDonutCentro({
  segmentos,
  largura,
  altura,
  cutout,
  rotation = 0,
  circumference = 360,
  bordaBranca = false,
  centro,
  tooltipUnidade,
}: GraficoDonutCentroProps) {
  const data = {
    labels: segmentos.map(() => ''),
    datasets: [
      {
        data: segmentos.map((s) => s.valor),
        backgroundColor: segmentos.map((s) => s.cor),
        borderWidth: bordaBranca ? 2 : 0,
        borderColor: bordaBranca ? '#FFFFFF' : undefined,
      },
    ],
  };

  const options: ChartOptions<'doughnut'> = {
    responsive: false,
    rotation,
    circumference,
    cutout,
    plugins: {
      legend: { display: false },
      tooltip: tooltipUnidade
        ? { callbacks: { label: (item: TooltipItem<'doughnut'>) => `${item.label}: ${item.parsed}${tooltipUnidade}` } }
        : { enabled: true },
    },
  };

  const plugins: Plugin<'doughnut'>[] = centro
    ? [
        {
          id: 'textoCentralDonut',
          afterDraw(chart) {
            const { ctx, chartArea } = chart;
            if (!chartArea) return;
            const cx = (chartArea.left + chartArea.right) / 2;
            const cy = centro.naBase ? chartArea.bottom - 10 : (chartArea.top + chartArea.bottom) / 2;
            const tamanho1 = centro.tamanhoLinha1 ?? 22;
            const tamanho2 = centro.tamanhoLinha2 ?? 10;
            ctx.save();
            ctx.textAlign = 'center';
            ctx.font = `700 ${tamanho1}px Inter, sans-serif`;
            ctx.fillStyle = centro.corLinha1 ?? '#111827';
            ctx.fillText(centro.linha1, cx, cy);
            if (centro.linha2) {
              ctx.font = `500 ${tamanho2}px Inter, sans-serif`;
              ctx.fillStyle = '#6B7280';
              ctx.fillText(centro.linha2, cx, cy + (centro.deslocamentoLinha2 ?? 14));
            }
            ctx.restore();
          },
        },
      ]
    : [];

  return <Doughnut data={data} options={options} plugins={plugins} width={largura} height={altura} />;
}
