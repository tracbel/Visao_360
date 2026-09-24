/**
 * RECEITA, CUSTO E MARGEM POR CULTURA — o gráfico combinado da maquete de
 * Rentabilidade: barras de receita e custo por hectare, a linha da margem por
 * hectare no mesmo eixo, e a linha da margem em % no eixo da direita, com o
 * rótulo de cada ponto.
 *
 * TAMANHO FIXO, `responsive: false`: a largura vem da `MolduraDeGrafico` e a
 * altura é declarada por quem chama. É o padrão dos gráficos do projeto, e é o
 * que impede o canvas de se medir pelo pai que se mede pelo canvas — a causa
 * clássica da página que cresce sozinha.
 *
 * SEM ANIMAÇÃO: o gráfico redesenha quando a largura muda, e animar cada
 * redesenho faria a captura sair no meio do movimento.
 */

// O REGISTRO VEM ANTES DO PADRÃO — ver o cabeçalho de `registroDoGraficoCombinado`.
import './registroDoGraficoCombinado';
import type { ChartData, ChartOptions, Plugin, TooltipItem } from 'chart.js';
import { Chart } from 'react-chartjs-2';
import { COR_DA_GRADE, FONTE_DOS_GRAFICOS } from '../../padraoDosGraficos';
import { useFontesProntas } from '../../useFontesProntas';
import { CORES_DA_RENTABILIDADE } from './cores';
import { reais, reaisCurtos } from './formatos';

/** O rótulo em caixa sobre cada ponto da margem % — "52%", como na maquete. */
const rotulosDaMargemPercentual: Plugin<'bar' | 'line'> = {
  id: 'rotulosDaMargemPercentual',
  afterDatasetsDraw(chart) {
    const indice = chart.data.datasets.findIndex((d) => d.label === 'Margem %');
    if (indice < 0) return;
    const meta = chart.getDatasetMeta(indice);
    const { ctx, chartArea } = chart;
    ctx.save();
    ctx.font = `600 10px ${FONTE_DOS_GRAFICOS}`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    meta.data.forEach((ponto, i) => {
      const valor = chart.data.datasets[indice].data[i];
      if (typeof valor !== 'number') return;
      const texto = `${Math.round(valor)}%`;
      const largura = ctx.measureText(texto).width + 10;
      const altura = 16;
      const x = ponto.x;
      // Acima do ponto, sem sair da área do gráfico.
      const y = Math.max(chartArea.top + altura / 2, ponto.y - 14);
      ctx.fillStyle = '#F5F3FF';
      ctx.strokeStyle = CORES_DA_RENTABILIDADE.margemPercentual;
      ctx.lineWidth = 1;
      ctx.beginPath();
      ctx.roundRect(x - largura / 2, y - altura / 2, largura, altura, 4);
      ctx.fill();
      ctx.stroke();
      ctx.fillStyle = '#5B21B6';
      ctx.fillText(texto, x, y);
    });
    ctx.restore();
  },
};

export function GraficoReceitaCustoMargem({
  culturas,
  receita,
  custo,
  margem,
  margemPercentual,
  largura,
  altura,
}: {
  culturas: readonly string[];
  receita: readonly (number | null)[];
  custo: readonly (number | null)[];
  margem: readonly (number | null)[];
  /** Em FRAÇÃO (0,52), e o gráfico mostra em % no eixo da direita. */
  margemPercentual: readonly (number | null)[];
  largura: number;
  altura: number;
}) {
  const fontesProntas = useFontesProntas();

  const data: ChartData<'bar' | 'line', (number | null)[], string> = {
    labels: [...culturas],
    datasets: [
      {
        type: 'bar',
        label: 'Receita/ha',
        data: [...receita],
        backgroundColor: CORES_DA_RENTABILIDADE.receita,
        yAxisID: 'y',
        order: 3,
        barPercentage: 0.8,
        categoryPercentage: 0.55,
      },
      {
        type: 'bar',
        label: 'Custo/ha',
        data: [...custo],
        backgroundColor: CORES_DA_RENTABILIDADE.custo,
        yAxisID: 'y',
        order: 3,
        barPercentage: 0.8,
        categoryPercentage: 0.55,
      },
      {
        type: 'line',
        label: 'Margem/ha',
        data: [...margem],
        borderColor: CORES_DA_RENTABILIDADE.margem,
        backgroundColor: CORES_DA_RENTABILIDADE.margem,
        borderWidth: 2,
        pointRadius: 3,
        tension: 0.25,
        yAxisID: 'y',
        order: 1,
      },
      {
        type: 'line',
        label: 'Margem %',
        data: margemPercentual.map((v) => (v === null ? null : v * 100)),
        borderColor: CORES_DA_RENTABILIDADE.margemPercentual,
        backgroundColor: '#FFFFFF',
        pointBorderColor: CORES_DA_RENTABILIDADE.margemPercentual,
        pointBackgroundColor: '#FFFFFF',
        pointBorderWidth: 2,
        borderWidth: 2,
        pointRadius: 4,
        tension: 0.25,
        yAxisID: 'y1',
        order: 0,
      },
    ],
  };

  const options: ChartOptions<'bar' | 'line'> = {
    responsive: false,
    animation: false,
    // A FOLGA DE CIMA É PARA OS RÓTULOS DA MARGEM %, que moram acima do ponto.
    layout: { padding: { top: 18, right: 4 } },
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (item: TooltipItem<'bar' | 'line'>) => {
            const y = item.parsed.y;
            if (y === null || y === undefined) return `${item.dataset.label}: sem dado`;
            return item.dataset.label === 'Margem %'
              ? `${item.dataset.label}: ${Math.round(y)}%`
              : `${item.dataset.label}: ${reais(y)}`;
          },
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
        ticks: { font: { size: 10, family: FONTE_DOS_GRAFICOS }, maxTicksLimit: 5, callback: (v) => reaisCurtos(Number(v)) },
      },
      y1: {
        position: 'right',
        suggestedMin: 0,
        grid: { drawOnChartArea: false },
        ticks: { font: { size: 10, family: FONTE_DOS_GRAFICOS }, maxTicksLimit: 5, callback: (v) => `${Number(v)}%` },
      },
    },
  };

  return (
    <Chart
      key={fontesProntas ? 'fontes-prontas' : 'fontes-carregando'}
      type="bar"
      data={data as ChartData<'bar', (number | null)[], string>}
      options={options as ChartOptions<'bar'>}
      plugins={[rotulosDaMargemPercentual as Plugin<'bar'>]}
      width={largura}
      height={altura}
    />
  );
}
