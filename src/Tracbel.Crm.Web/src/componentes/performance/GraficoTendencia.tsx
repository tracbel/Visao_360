/**
 * Gráfico de evolução mensal FYTD (linha por CEN) — porte de
 * `renderPerfChartTrend` (prototipo/referencia/assets/app.js linha 6273 /
 * perf-block.js linha 351). `chart.js` fica pinado em 4.4.1 (package.json),
 * mesma versão do protótipo.
 */
import {
  CategoryScale,
  Chart as ChartJS,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  Tooltip,
  type TooltipItem,
} from 'chart.js';
import { Line } from 'react-chartjs-2';
import type { FytdMes, PerfCen, PerfSeries } from '../../tipos/performanceCen';
import { fmtBRLcompact } from './formatoPerformance';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Tooltip, Legend);

/** "João Ribeiro" -> "João R." — mesma regra do original. */
function rotuloCurto(nome: string): string {
  const partes = nome.split(' ');
  return `${partes[0]} ${(partes[1] ?? '').charAt(0)}.`;
}

export function GraficoTendencia({
  ids,
  cens,
  series,
  fytdMeses,
}: {
  ids: string[];
  cens: PerfCen[];
  series: PerfSeries;
  fytdMeses: FytdMes[];
}) {
  const labels = fytdMeses.map((m) => m.label);
  const datasets = ids.map((id) => {
    const cen = cens.find((c) => c.id === id)!;
    const serie = series[id];
    return {
      label: rotuloCurto(cen.nome),
      data: serie.map((m) => m.vendas),
      borderColor: cen.avatar,
      backgroundColor: `${cen.avatar}20`,
      tension: 0.3,
      borderWidth: 2,
      pointRadius: 3,
      pointHoverRadius: 5,
    };
  });

  return (
    <div className="perf-chart-container">
      <Line
        data={{ labels, datasets }}
        options={{
          responsive: true,
          maintainAspectRatio: false,
          // Sem animação de entrada: o estado final é idêntico, mas evita que a
          // captura de tela (comparar-telas.mjs) pegue um frame intermediário
          // da curva — fonte de diffs não determinísticos no funil de QA visual.
          animation: false,
          plugins: {
            legend: {
              display: ids.length > 1,
              position: 'bottom',
              labels: { boxWidth: 10, boxHeight: 10, padding: 10, font: { size: 11 } },
            },
            tooltip: {
              callbacks: {
                // `parsed.y` é `number | null` desde o chart.js 4.5: mês sem dado não é R$ 0,00.
                label: (ctx: TooltipItem<'line'>) =>
                  `${ctx.dataset.label}: ${ctx.parsed.y === null ? 'sem dado' : fmtBRLcompact(ctx.parsed.y)}`,
              },
            },
          },
          scales: {
            y: {
              ticks: { callback: (v) => fmtBRLcompact(Number(v)), font: { size: 10 } },
              grid: { color: '#F1F5F9' },
            },
            x: {
              ticks: { font: { size: 10 } },
              grid: { display: false },
            },
          },
        }}
      />
    </div>
  );
}
