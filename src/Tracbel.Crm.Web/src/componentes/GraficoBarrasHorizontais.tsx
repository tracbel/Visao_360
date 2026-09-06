/**
 * Gráfico de barras horizontais — porte do card "Vendas perdidas por motivo"
 * (mountVisao360 → canvas `v360Perdidas`,
 * prototipo/referencia/assets/app.js:8040-8083). Genérico para qualquer
 * ranking de categorias com barra colorida por item.
 */
import { Chart as ChartJS, BarElement, CategoryScale, LinearScale, Tooltip, type ChartOptions, type TooltipItem } from 'chart.js';
import { Bar } from 'react-chartjs-2';
import { COR_DA_GRADE, FONTE_DOS_GRAFICOS } from './padraoDosGraficos';
import { useFontesProntas } from './useFontesProntas';

ChartJS.register(BarElement, CategoryScale, LinearScale, Tooltip);

export type BarraHorizontalItem = {
  rotulo: string;
  valor: number;
  cor: string;
  /** Linhas extras do tooltip (ex.: `['87 negócios', 'Valor: R$ 42.8M']`). */
  tooltipLinhas?: string[];
};

/**
 * O protótipo original nunca altera a fonte padrão do Chart.js — os rótulos
 * saem na pilha padrão da biblioteca, que é mais estreita que a Inter. Como
 * outros componentes nossos mexem nesse padrão global, este fixa a fonte
 * explicitamente para não depender de quem carregou primeiro.
 */

export type GraficoBarrasHorizontaisProps = {
  itens: BarraHorizontalItem[];
  largura: number;
  altura: number;
};

export function GraficoBarrasHorizontais({ itens, largura, altura }: GraficoBarrasHorizontaisProps) {
  // Redesenha quando a Inter chega: sem isso o rótulo do eixo é medido com a
  // fonte substituta e a primeira letra fica fora da área desenhada.
  const fontesProntas = useFontesProntas();

  const data = {
    labels: itens.map((i) => i.rotulo),
    datasets: [
      {
        label: 'valor',
        data: itens.map((i) => i.valor),
        backgroundColor: itens.map((i) => i.cor),
        borderRadius: 4,
        borderWidth: 0,
      },
    ],
  };

  const options: ChartOptions<'bar'> = {
    responsive: false,
    indexAxis: 'y',
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (item: TooltipItem<'bar'>) => itens[item.dataIndex]?.tooltipLinhas ?? `${item.parsed.x}`,
        },
      },
    },
    scales: {
      x: {
        beginAtZero: true,
        grid: { color: COR_DA_GRADE },
        ticks: { font: { size: 10, family: FONTE_DOS_GRAFICOS } },
      },
      y: {
        grid: { display: false },
        ticks: { font: { size: 11, family: FONTE_DOS_GRAFICOS } },
      },
    },
  };

  return (
    <Bar
      key={fontesProntas ? 'fontes-prontas' : 'fontes-carregando'}
      data={data}
      options={options}
      width={largura}
      height={altura}
    />
  );
}
