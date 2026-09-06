/**
 * Ranking por métrica (barras horizontais) — porte de
 * `renderPerfChartBreakdown` (prototipo/referencia/assets/app.js linha 6329 /
 * perf-block.js linha 407).
 */
import { BarElement, CategoryScale, Chart as ChartJS, Legend, LinearScale, Tooltip, type TooltipItem } from 'chart.js';
import { Bar } from 'react-chartjs-2';
import type { MetricaBreakdown, PerfDetalheCen } from '../../tipos/performanceCen';
import { fmtBRLcompact } from './formatoPerformance';

ChartJS.register(CategoryScale, LinearScale, BarElement, Tooltip, Legend);

function valorMetrica(p: PerfDetalheCen, metrica: MetricaBreakdown): { val: number; fmt: string } {
  switch (metrica) {
    case 'vendas_fyd':
      return { val: p.fytd_vendas, fmt: fmtBRLcompact(p.fytd_vendas) };
    case 'atingimento': {
      const val = p.fytd_atingimento * 100;
      return { val, fmt: `${val.toFixed(0)}%` };
    }
    case 'pipeline':
      return { val: p.pipeline_aberto, fmt: fmtBRLcompact(p.pipeline_aberto) };
    case 'conversao': {
      const val = p.conversao_fytd * 100;
      return { val, fmt: `${val.toFixed(1)}%` };
    }
    case 'cobertura':
      return { val: p.cobertura, fmt: `${p.cobertura}%` };
    case 'ticket':
      return { val: p.ticket_medio_fytd, fmt: fmtBRLcompact(p.ticket_medio_fytd) };
  }
}

function formatarEixo(v: number, metrica: MetricaBreakdown): string {
  if (metrica === 'vendas_fyd' || metrica === 'pipeline' || metrica === 'ticket') return fmtBRLcompact(v);
  if (metrica === 'atingimento' || metrica === 'conversao' || metrica === 'cobertura') return `${v}%`;
  return `${v}`;
}

export function GraficoBreakdown({
  detalhes,
  metrica,
  onMetricaChange,
}: {
  detalhes: PerfDetalheCen[];
  metrica: MetricaBreakdown;
  onMetricaChange: (metrica: MetricaBreakdown) => void;
}) {
  const dados = detalhes
    .map((p) => ({ nome: p.cen.nome, color: p.cen.avatar, ...valorMetrica(p, metrica) }))
    .sort((a, b) => b.val - a.val);

  return (
    <>
      <div className="perf-card-header">
        <h3>Ranking por métrica</h3>
        <select
          className="perf-select"
          value={metrica}
          onChange={(e) => onMetricaChange(e.target.value as MetricaBreakdown)}
        >
          <option value="vendas_fyd">Vendas FYTD (R$)</option>
          <option value="atingimento">% Atingimento FYTD</option>
          <option value="pipeline">Pipeline aberto (R$)</option>
          <option value="conversao">Conversão %</option>
          <option value="cobertura">Cobertura de carteira %</option>
          <option value="ticket">Ticket médio</option>
        </select>
      </div>
      <div className="perf-chart-container">
        <Bar
          data={{
            labels: dados.map((d) => d.nome),
            datasets: [
              {
                data: dados.map((d) => d.val),
                backgroundColor: dados.map((d) => d.color),
                borderRadius: 4,
                borderSkipped: false,
              },
            ],
          }}
          options={{
            responsive: true,
            maintainAspectRatio: false,
            // Sem animação de entrada — mesmo motivo de GraficoTendencia.tsx:
            // estado final idêntico, mas determinístico para o screenshot.
            animation: false,
            indexAxis: 'y',
            plugins: {
              legend: { display: false },
              tooltip: {
                callbacks: {
                  label: (ctx: TooltipItem<'bar'>) => dados[ctx.dataIndex].fmt,
                },
              },
            },
            scales: {
              x: {
                ticks: { callback: (v) => formatarEixo(Number(v), metrica), font: { size: 10 } },
                grid: { color: '#F1F5F9' },
              },
              y: {
                ticks: { font: { size: 11 } },
                grid: { display: false },
              },
            },
          }}
        />
      </div>
    </>
  );
}
