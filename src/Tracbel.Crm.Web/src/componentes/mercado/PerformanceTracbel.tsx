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

import { useState } from 'react';
import { LacunaConhecida } from '../cadastro/SemDado';
import { PainelDeIndicadores, type Indicador } from '../cadastro/Indicadores';
import { reaisCompactos } from '../territorio/escalas';
import { nº } from '../territorio/indicadoresDaAdr';
import type { TotaisDaAdr } from '../territorio/totaisDaAdr';
import { TituloDaSecao } from '../territorio/TituloDaSecao';
import { AbasInternas } from './AbasInternas';

type SubAba = 'captura' | 'vendas' | 'naoCapturado';

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
        subtitulo="Quanto do mercado a Tracbel está levando — e quanto sobra. O que sai de vendas é medido; o resto depende das vendas em unidades."
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
              <LacunaConhecida
                metrica="Captura Tracbel"
                motivo="Vendas da Tracbel em unidades dividido pela demanda anual estimada. Precisa da issue 69, que traz as vendas por município em MÁQUINAS: o faturamento em reais que já existe não serve de numerador para uma demanda medida em máquinas. Não é market share — share exigiria o total vendido por todos os fabricantes, que nenhuma fonte aberta publica."
              />
            ),
          },
          { id: 'vendas', rotulo: 'Vendas', conteudo: <PainelDeIndicadores indicadores={vendas} /> },
          {
            id: 'naoCapturado',
            rotulo: 'Não capturado',
            conteudo: (
              <LacunaConhecida
                metrica="Potencial não capturado"
                motivo="A demanda ajustada menos as vendas, nunca abaixo de zero (issue 162). Depende da mesma issue 69 para sair em unidades e da issue 70 para sair em reais."
              />
            ),
          },
        ]}
      />
    </section>
  );
}
