/**
 * Barras horizontais empilhadas — uma barra por item, dividida em faixas.
 *
 * POR QUE ELE EXISTE: as telas de cobertura mostravam a distribuição por faixa
 * de tempo sem contato em tabela, uma coluna por faixa. A tabela responde
 * "quanto", mas exige que quem lê compare números de linhas distantes para
 * responder "quem está pior" — que é a única pergunta que a gerência faz
 * nessa tela. Empilhado, a resposta é a cor: a barra mais vermelha é a pior,
 * sem conta nenhuma.
 *
 * As faixas são passadas de fora, e não fixadas aqui, porque as mesmas barras
 * servem carteira, filial e CEN — o que muda é só o rótulo do eixo.
 *
 * Segue `GraficoBarrasHorizontais` no que importa para bater com o protótipo:
 * `responsive: false` com largura e altura em pixels (a `MolduraDeGrafico` é
 * quem mede o espaço), e a fonte do Chart.js fixada explicitamente.
 */
import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  LinearScale,
  Tooltip,
  type ChartOptions,
  type TooltipItem,
} from 'chart.js';
import { Bar } from 'react-chartjs-2';
import { COR_DA_GRADE, FONTE_DOS_GRAFICOS } from './padraoDosGraficos';
import { useFontesProntas } from './useFontesProntas';

ChartJS.register(BarElement, CategoryScale, LinearScale, Tooltip);


/** Uma faixa da pilha: a mesma cor e o mesmo nome em todas as barras. */
export type FaixaEmpilhada = {
  nome: string;
  cor: string;
  /** O valor desta faixa em cada item, na ordem de `itens`. */
  valores: number[];
};

export type GraficoBarrasEmpilhadasProps = {
  /** O rótulo do eixo, uma entrada por barra. */
  itens: string[];
  faixas: FaixaEmpilhada[];
  largura: number;
  altura: number;
  /**
   * Quando `true`, cada barra vai até 100% e as faixas viram fração dela.
   * É o modo de comparar carteiras de tamanhos muito diferentes — 4.634
   * clientes contra 79 — sem que a menor vire um traço.
   */
  proporcional?: boolean;
};

export function GraficoBarrasEmpilhadas({
  itens,
  faixas,
  largura,
  altura,
  proporcional = false,
}: GraficoBarrasEmpilhadasProps) {
  const fontesProntas = useFontesProntas();

  const totais = itens.map((_, i) => faixas.reduce((s, f) => s + (f.valores[i] ?? 0), 0));

  const data = {
    labels: itens,
    datasets: faixas.map((faixa) => ({
      label: faixa.nome,
      data: itens.map((_, i) => {
        const valor = faixa.valores[i] ?? 0;
        if (!proporcional) return valor;
        return totais[i] > 0 ? (valor / totais[i]) * 100 : 0;
      }),
      backgroundColor: faixa.cor,
      borderWidth: 0,
      borderRadius: 2,
    })),
  };

  const options: ChartOptions<'bar'> = {
    responsive: false,
    indexAxis: 'y',
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          // O tooltip mostra sempre a contagem, mesmo no modo proporcional: a
          // barra responde "quanto do total", e o número responde "quantos".
          label: (item: TooltipItem<'bar'>) => {
            const faixa = faixas[item.datasetIndex];
            const valor = faixa?.valores[item.dataIndex] ?? 0;
            const total = totais[item.dataIndex] ?? 0;
            const parte = total > 0 ? Math.round((valor / total) * 100) : 0;
            return `${faixa?.nome}: ${valor.toLocaleString('pt-BR')} (${parte}%)`;
          },
        },
      },
    },
    scales: {
      x: {
        stacked: true,
        beginAtZero: true,
        max: proporcional ? 100 : undefined,
        grid: { color: COR_DA_GRADE },
        ticks: {
          font: { size: 10, family: FONTE_DOS_GRAFICOS },
          callback: (valor) => (proporcional ? `${valor}%` : Number(valor).toLocaleString('pt-BR')),
        },
      },
      y: {
        stacked: true,
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
