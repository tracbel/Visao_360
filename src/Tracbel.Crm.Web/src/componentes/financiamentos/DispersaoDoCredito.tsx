/**
 * A DISPERSÃO VALOR × LINHAS POR MUNICÍPIO (issue 261) — cada município do recorte é uma bolha: linhas do SICOR no eixo
 * horizontal, valor financiado no vertical, o tamanho pelo valor e a cor pela faixa do índice.
 *
 * O QUE ELA MOSTRA QUE A TABELA NÃO MOSTRA: quem está acima da diagonal financia operações maiores (poucas linhas, muito
 * valor); quem está abaixo tem crédito pulverizado. A cor diz se o município está esquentando ou esfriando.
 */

// O REGISTRO VEM ANTES DO PADRÃO DOS GRÁFICOS: ver o comentário de registroDaDispersao.ts.
import './registroDaDispersao';
import type { ChartOptions, TooltipItem } from 'chart.js';
import { Bubble } from 'react-chartjs-2';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { COR_DA_GRADE, FONTE_DOS_GRAFICOS } from '../padraoDosGraficos';
import { reaisCurtos } from '../mercado/momento/formatos';
import { GraficoSemSerie, Legenda } from '../mercado/momento/pecas';
import { useFontesProntas } from '../useFontesProntas';
import type { CreditoDoMunicipio } from '../../tipos/mercado';
import { COR_DA_FAIXA, COR_SEM_FAIXA, n, NOME_DA_FAIXA, situacaoDe } from './financiamentos';

export function DispersaoDoCredito({ municipios }: { municipios: readonly CreditoDoMunicipio[] }) {
  const fontesProntas = useFontesProntas();
  const comCredito = municipios.filter((m) => m.janelas.linhas > 0);
  const maior = Math.max(...comCredito.map((m) => m.janelas.valor), 1);

  if (comCredito.length === 0)
    return (
      <GraficoSemSerie
        altura={300}
        frase="Nenhum município com crédito no período"
        motivo="O recorte não teve linha do SICOR de máquina no período escolhido."
        oQue="a dispersão do crédito"
      />
    );

  const data = {
    datasets: [
      {
        label: 'Municípios',
        data: comCredito.map((m) => ({ x: m.janelas.linhas, y: m.janelas.valor, r: 4 + 14 * Math.sqrt(m.janelas.valor / maior) })),
        backgroundColor: comCredito.map((m) => `${m.indice?.faixa ? COR_DA_FAIXA[m.indice.faixa] : COR_SEM_FAIXA}B3`),
        borderColor: '#FFFFFF',
        borderWidth: 0.5,
      },
    ],
  };

  const fonte = { size: 10, family: FONTE_DOS_GRAFICOS };
  const options: ChartOptions<'bubble'> = {
    responsive: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (item: TooltipItem<'bubble'>) => {
            const m = comCredito[item.dataIndex];
            return [
              m.nome,
              `${n(m.janelas.linhas)} linhas · ${reaisCurtos(m.janelas.valor)}`,
              `Situação: ${situacaoDe(m.indice, m.janelas)}`,
            ];
          },
        },
      },
    },
    scales: {
      x: {
        beginAtZero: true,
        title: { display: true, text: 'linhas do SICOR', font: fonte },
        grid: { color: COR_DA_GRADE },
        ticks: { font: fonte, precision: 0 },
      },
      y: {
        beginAtZero: true,
        title: { display: true, text: 'valor financiado', font: fonte },
        grid: { color: COR_DA_GRADE },
        ticks: { font: fonte, callback: (v) => reaisCurtos(Number(v)), maxTicksLimit: 5 },
      },
    },
  };

  return (
    <div className="fin-dispersao" data-bloco="dispersao">
      <MolduraDeGrafico altura={300}>
        {(l, a) => (
          <Bubble key={fontesProntas ? 'fontes-prontas' : 'fontes-carregando'} data={data} options={options} width={l} height={a} />
        )}
      </MolduraDeGrafico>
      <Legenda
        itens={[
          ...Object.entries(NOME_DA_FAIXA).map(([faixa, nome]) => ({ nome, cor: COR_DA_FAIXA[faixa as keyof typeof COR_DA_FAIXA] })),
          { nome: 'Sem base', cor: COR_SEM_FAIXA },
        ]}
      />
    </div>
  );
}
