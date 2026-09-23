/**
 * Performance Tracbel — Captura | Vendas | Não capturado (documento 50, §4.7).
 *
 * CAPTURA, E NÃO MARKET SHARE. Enquanto o denominador for a demanda ESTIMADA
 * pelo motor, o número se chama captura (issue 162). Share exigiria o total de
 * máquinas vendidas no município por todos os fabricantes, e nenhuma fonte
 * aberta dá isso — usar a palavra errada faria a diretoria ler uma estimativa
 * como um fato de mercado.
 *
 * SÓ VENDAS TEM DADO HOJE. O faturamento existe e é real; captura e não
 * capturado dependem das vendas em UNIDADES (issue 69), porque reais não servem
 * de numerador para uma demanda medida em máquinas.
 */

import { ShoppingCart, Tractor, Wrench } from 'lucide-react';
import { useState } from 'react';
import { MetricaAusente } from '../comum/MetricaAusente';
import { GradeDeMiniIndicadores, MiniIndicador } from '../dashboard/Dashboard';
import type { Indicador } from '../cadastro/Indicadores';
import { reaisCompactos } from '../territorio/escalas';
import { nº } from '../territorio/indicadoresDaAdr';
import type { TotaisDaAdr } from '../territorio/totaisDaAdr';
import { TituloDaSecao } from '../territorio/TituloDaSecao';
import { AbasInternas } from './AbasInternas';

type SubAba = 'captura' | 'vendas' | 'naoCapturado';

/**
 * O ícone de cada número, POR RÓTULO e não por posição — mesma regra da régua do
 * mercado: amarrar o ícone ao índice faria o pós-venda virar trator no dia em
 * que alguém reordenasse a lista.
 */
const ICONES_DA_PERFORMANCE: Record<string, typeof Tractor> = {
  'Vendas no período': ShoppingCart,
  Máquina: Tractor,
  'Pós-venda': Wrench,
};

export function PerformanceTracbel({ totais, comTerritorio }: { totais: TotaisDaAdr; comTerritorio: boolean }) {
  const [subAba, setSubAba] = useState<SubAba>('vendas');

  const vendas: Indicador[] = [
    {
      rotulo: 'Vendas no período',
      valor: comTerritorio ? reaisCompactos(totais.vendas) : null,
      deOnde: `${nº(totais.clientesQueCompraram)} clientes compraram`,
      semDado: 'território não carregado neste banco',
    },
    {
      rotulo: 'Máquina',
      valor: comTerritorio ? reaisCompactos(totais.maquina) : null,
      deOnde: 'faturamento líquido de máquina, pelo endereço do cliente',
      semDado: 'território não carregado neste banco',
    },
    {
      rotulo: 'Pós-venda',
      valor: comTerritorio ? reaisCompactos(totais.posVenda) : null,
      deOnde: 'peça + serviço — composição provisória',
      semDado: 'território não carregado neste banco',
    },
  ];

  return (
    <section data-bloco="performance-tracbel">
      <TituloDaSecao
        titulo="Performance Tracbel"
        subtitulo="Quanto do mercado a Tracbel está levando."
        metodologia={
          'O que sai de Vendas é medido — faturamento líquido pelo endereço principal do cliente. Captura e não ' +
          'capturado dependem das vendas em UNIDADES (issue 69): reais não servem de numerador para uma demanda ' +
          'medida em máquinas. Enquanto o denominador for demanda estimada, o número se chama captura, e não share.'
        }
      />

      <AbasInternas
        rotulo="O que a performance mostra"
        ativa={subAba}
        aoTrocar={setSubAba}
        abas={[
          {
            id: 'captura',
            rotulo: 'Captura',
            conteudo: (
              <MetricaAusente metrica="Captura Tracbel"
                motivo="Vendas da Tracbel em unidades dividido pela demanda anual estimada. Precisa da issue 69, que traz as vendas por município em MÁQUINAS: o faturamento em reais que já existe não serve de numerador para uma demanda medida em máquinas. Não é market share — share exigiria o total vendido por todos os fabricantes, que nenhuma fonte aberta publica."
              />
            ),
          },
          {
            id: 'vendas',
            rotulo: 'Vendas',
            // A MESMA GRADE DOS NÚMEROS DO TOPO (fase T4.6): três cartões com a
            // mesma altura, o mesmo padding e o valor no mesmo eixo. Eles ficavam
            // espalhados com larguras diferentes, e três cartões de venda não
            // podem parecer coisas de naturezas diferentes.
            //
            // TRÊS COLUNAS PARA TRÊS NÚMEROS (fase T4.7). A grade de quatro
            // deixava um quarto vazio guardando lugar para Captura e Não
            // capturado — e na captura de 1440 isso lê como cartão que não
            // carregou, não como espaço reservado. Quando a issue 69 trouxer os
            // dois que faltam, o atributo sai e a grade volta a quatro.
            // MINI-CARTÕES COM SELO DE ÍCONE (fase T4.9 — maquete). Eles eram os
            // mesmos `CartaoDeIndicador` dos quatro números de decisão do topo,
            // e dentro de uma coluna de um terço isso dava três cartões da
            // altura de um KPI executivo para um detalhe de painel. Na maquete
            // são cartõezinhos de duas linhas com o ícone à esquerda — que é o
            // nível de leitura certo para eles.
            conteudo: (
              <GradeDeMiniIndicadores data-colunas="3">
                {vendas.map((v) => (
                  <MiniIndicador
                    key={v.rotulo}
                    rotulo={v.rotulo}
                    icone={ICONES_DA_PERFORMANCE[v.rotulo]}
                    tom="demanda"
                    valor={v.valor}
                    detalhe={v.deOnde}
                    motivoSemDado={v.semDado}
                  />
                ))}
              </GradeDeMiniIndicadores>
            ),
          },
          {
            id: 'naoCapturado',
            rotulo: 'Não capturado',
            conteudo: (
              <MetricaAusente metrica="Potencial não capturado"
                motivo="A demanda ajustada menos as vendas, nunca abaixo de zero (issue 162). Depende da mesma issue 69 para sair em unidades e da issue 70 para sair em reais."
              />
            ),
          },
        ]}
      />
    </section>
  );
}
