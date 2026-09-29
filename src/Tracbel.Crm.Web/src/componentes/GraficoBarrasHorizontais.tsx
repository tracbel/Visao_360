/**
 * Gráfico de barras horizontais — porte do card "Vendas perdidas por motivo"
 * (mountVisao360 → canvas `v360Perdidas`,
 * prototipo/referencia/assets/app.js:8040-8083). Genérico para qualquer
 * ranking de categorias com barra colorida por item.
 */
import {
  Chart as ChartJS,
  BarElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  type ChartOptions,
  type Plugin,
  type TooltipItem,
} from 'chart.js';
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
  /**
   * O NÚMERO ESCRITO NA PONTA DE CADA BARRA (maquete da Visão 360, 29/09/2026) — o mesmo valor do balão, sem precisar
   * passar o ponteiro. Sem isto, o gráfico é o de sempre.
   */
  valoresNaPonta?: boolean;
  /** A espessura da barra, em pixels; sem ela, a do Chart.js. */
  espessura?: number;
  /** O corpo e a cor do nome de cada barra, no eixo da esquerda. */
  tamanhoDoRotulo?: number;
  corDoRotulo?: string;
};

/** Escreve o valor logo depois do fim de cada barra. */
const VALOR_NA_PONTA: Plugin<'bar'> = {
  id: 'valorNaPonta',
  afterDatasetsDraw(chart) {
    const { ctx } = chart;
    const meta = chart.getDatasetMeta(0);
    ctx.save();
    ctx.font = `700 11px ${FONTE_DOS_GRAFICOS}`;
    ctx.fillStyle = '#1F2937';
    ctx.textAlign = 'left';
    ctx.textBaseline = 'middle';
    meta.data.forEach((barra, i) => {
      const valor = chart.data.datasets[0].data[i];
      if (typeof valor !== 'number') return;
      ctx.fillText(valor.toLocaleString('pt-BR'), barra.x + 8, barra.y);
    });
    ctx.restore();
  },
};

export function GraficoBarrasHorizontais({
  itens,
  largura,
  altura,
  valoresNaPonta = false,
  espessura,
  tamanhoDoRotulo = 11,
  corDoRotulo,
}: GraficoBarrasHorizontaisProps) {
  // Redesenha quando a Inter chega: sem isso o rótulo do eixo é medido com a
  // fonte substituta e a primeira letra fica fora da área desenhada.
  const fontesProntas = useFontesProntas();

  // O NOME LONGO NA TELA ESTREITA: o Chart.js dá ao eixo no máximo metade da largura e corta o que passa pela ESQUERDA
  // ("ndição de financiamento"). Com o número na ponta, o nome é cortado no FIM, com reticências; o nome inteiro está no
  // balão da barra.
  const caberNoEixo = (nome: string) => {
    const maximo = Math.max(8, Math.floor((largura / 2 - 14) / (tamanhoDoRotulo * 0.56)));
    return nome.length > maximo ? `${nome.slice(0, maximo - 1).trimEnd()}…` : nome;
  };

  const data = {
    labels: itens.map((i) => i.rotulo),
    datasets: [
      {
        label: 'valor',
        data: itens.map((i) => i.valor),
        backgroundColor: itens.map((i) => i.cor),
        borderRadius: 4,
        borderWidth: 0,
        ...(espessura ? { barThickness: espessura } : {}),
      },
    ],
  };

  const options: ChartOptions<'bar'> = {
    responsive: false,
    indexAxis: 'y',
    // O NÚMERO DA MAIOR BARRA PRECISA DE LUGAR à direita dela.
    ...(valoresNaPonta ? { layout: { padding: { right: 36 } } } : {}),
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
        ticks: {
          font: { size: tamanhoDoRotulo, family: FONTE_DOS_GRAFICOS },
          color: corDoRotulo,
          // TODO NOME APARECE: num gráfico baixo o Chart.js pula um nome sim, outro não, e a barra fica sem dono.
          ...(valoresNaPonta
            ? { autoSkip: false, callback: (_valor: unknown, i: number) => caberNoEixo(itens[i]?.rotulo ?? '') }
            : {}),
        },
      },
    },
  };

  return (
    <Bar
      key={fontesProntas ? 'fontes-prontas' : 'fontes-carregando'}
      data={data}
      options={options}
      plugins={valoresNaPonta ? [VALOR_NA_PONTA] : []}
      width={largura}
      height={altura}
    />
  );
}
