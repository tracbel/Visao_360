/**
 * A EVOLUÇÃO DO TERMO DE TROCA de uma cultura — as unidades da safra que compram o trator base, mês a mês, e a média da
 * série como referência (issue 70). Só os meses que as duas séries têm; mês sem venda de trator não é ponto.
 *
 * TAMANHO FIXO, `responsive: false`, e SEM ANIMAÇÃO — o padrão dos gráficos do projeto (ver `GraficoReceitaCustoMargem`):
 * a largura vem da `MolduraDeGrafico`, e o canvas não se mede pelo pai que se mede pelo canvas.
 */

// O REGISTRO VEM ANTES DO PADRÃO — ver o cabeçalho de `registroDoGraficoCombinado`.
import './registroDoGraficoCombinado';
import type { ChartData, ChartOptions, TooltipItem } from 'chart.js';
import { Chart } from 'react-chartjs-2';
import { COR_DA_GRADE, FONTE_DOS_GRAFICOS } from '../../padraoDosGraficos';
import { useFontesProntas } from '../../useFontesProntas';
import { mesCurto, numero } from './formatos';
import type { TermoNoMes } from './termoDeTroca';

/** As cores da maquete: a série em verde escuro, a média em verde claro. */
export const CORES_DO_TERMO = { unidades: '#0B5D2A', media: '#8FD19E' } as const;

export function GraficoDoTermoDeTroca({
  termo,
  media,
  unidade,
  largura,
  altura,
}: {
  termo: readonly TermoNoMes[];
  media: number | null;
  /** A unidade comercial da cultura ("saca de 60 kg"), para o balão. */
  unidade: string;
  largura: number;
  altura: number;
}) {
  const fontesProntas = useFontesProntas();

  const data: ChartData<'line', (number | null)[], string> = {
    labels: termo.map((t) => mesCurto(t.mes)),
    datasets: [
      {
        label: 'Unidades por trator',
        data: termo.map((t) => t.unidades),
        borderColor: CORES_DO_TERMO.unidades,
        backgroundColor: CORES_DO_TERMO.unidades,
        borderWidth: 2,
        pointRadius: 3,
        tension: 0.25,
      },
      {
        label: 'Média da série',
        data: termo.map(() => media),
        borderColor: CORES_DO_TERMO.media,
        backgroundColor: CORES_DO_TERMO.media,
        borderWidth: 2,
        borderDash: [6, 4],
        pointRadius: 0,
      },
    ],
  };

  const options: ChartOptions<'line'> = {
    responsive: false,
    animation: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (item: TooltipItem<'line'>) => {
            const y = item.parsed.y;
            if (y === null || y === undefined) return `${item.dataset.label}: sem dado`;
            if (item.datasetIndex === 1) return `Média da série: ${numero(y)} (${unidade})`;
            const ponto = termo[item.dataIndex];
            return `${numero(y)} (${unidade}) — trator a ${numero(ponto.precoDoTrator)} reais, mediana de ${ponto.notasDoTrator} ${ponto.notasDoTrator === 1 ? 'nota' : 'notas'}`;
          },
        },
      },
    },
    scales: {
      x: { grid: { display: false }, ticks: { font: { size: 10, family: FONTE_DOS_GRAFICOS }, maxTicksLimit: 12 } },
      y: {
        beginAtZero: true,
        grid: { color: COR_DA_GRADE },
        ticks: { font: { size: 10, family: FONTE_DOS_GRAFICOS }, maxTicksLimit: 5, callback: (v) => numero(Number(v)) },
      },
    },
  };

  return (
    <Chart
      key={fontesProntas ? 'fontes-prontas' : 'fontes-carregando'}
      type="line"
      data={data}
      options={options}
      width={largura}
      height={altura}
    />
  );
}
