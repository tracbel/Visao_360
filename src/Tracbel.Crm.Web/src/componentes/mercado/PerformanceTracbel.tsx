/**
 * Performance Tracbel — Vendas | Captura | Não capturado (documento 50, §4.7).
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
 *
 * VENDAS VEM PRIMEIRO, E ABERTA (maquete): é a aba que tem número. Abrir o
 * bloco numa aba de traço seria começar a leitura pelo que falta.
 */

import { ChartNoAxesColumnIncreasing, Tractor, Wrench, type LucideIcon } from 'lucide-react';
import { useState, type ReactNode } from 'react';
import { MetricaAusente } from '../comum/MetricaAusente';
import { ValorAusente } from '../comum/ValorAusente';
import { reaisCompactos } from '../territorio/escalas';
import { nº } from '../territorio/indicadoresDaAdr';
import type { TotaisDaAdr } from '../territorio/totaisDaAdr';
import { TituloDaSecao } from '../territorio/TituloDaSecao';
import { AbasInternas } from './AbasInternas';
import { VariacaoAusente } from './VariacaoAusente';

type SubAba = 'vendas' | 'captura' | 'naoCapturado';

const SEM_TERRITORIO = 'território não carregado neste banco';

/**
 * UM MINI-CARTÃO DE VENDA, no desenho da maquete: ícone à esquerda; o nome EM
 * CIMA, o valor, a variação contra o ano anterior; e, no pé, uma linha de
 * contexto em largura inteira ("175 clientes compraram").
 *
 * É COMPONENTE DAQUI, e não o `MiniIndicador` compartilhado: aquele põe o nome
 * embaixo do número e não tem pé — é o desenho de outros painéis.
 */
function CartaoDeVenda({
  rotulo,
  icone: Icone,
  valor,
  pe,
}: {
  rotulo: string;
  icone: LucideIcon;
  valor: string | null;
  pe: ReactNode;
}) {
  const nome = rotulo.toLowerCase();

  return (
    <div className="mv-venda" data-mini={rotulo}>
      <span className="mv-icone-ladrilho" aria-hidden="true">
        <Icone size={17} strokeWidth={2} />
      </span>
      <div className="mv-venda-corpo">
        <span className="mv-venda-rotulo">{rotulo}</span>
        <strong className="mv-venda-valor">
          {valor === null ? <ValorAusente motivo={SEM_TERRITORIO} oQue={nome} /> : valor}
        </strong>
        <VariacaoAusente deQue={nome} compacta />
      </div>
      <div className="mv-venda-pe">{pe}</div>
    </div>
  );
}

export function PerformanceTracbel({ totais, comTerritorio }: { totais: TotaisDaAdr; comTerritorio: boolean }) {
  const [subAba, setSubAba] = useState<SubAba>('vendas');

  return (
    <section data-bloco="performance-tracbel">
      <TituloDaSecao
        titulo="Performance Tracbel"
        subtitulo="Quanto do mercado a Tracbel está levando."
        metodologia={
          <>
            <p>
              O que sai de Vendas é medido — faturamento líquido pelo endereço principal do cliente. Máquina é o
              faturamento líquido de máquina; pós-venda é peça + serviço, composição provisória.
            </p>
            <p>
              Captura e não capturado dependem das vendas em UNIDADES (issue 69): reais não servem de numerador para
              uma demanda medida em máquinas. Enquanto o denominador for demanda estimada, o número se chama captura,
              e não share.
            </p>
          </>
        }
      />

      <AbasInternas
        rotulo="O que a performance mostra"
        ativa={subAba}
        aoTrocar={setSubAba}
        abas={[
          {
            id: 'vendas',
            rotulo: 'Vendas',
            // TRÊS MINI-CARTÕES COM BORDA (maquete). O que eles diziam por extenso
            // — "faturamento líquido de máquina, pelo endereço do cliente", "peça
            // + serviço — composição provisória" — foi para a dica do título da
            // seção; o pé de cada cartão é a linha curta da maquete.
            conteudo: (
              <div className="mv-vendas">
                <div className="mv-vendas-grade">
                  <CartaoDeVenda
                    rotulo="Vendas no período"
                    icone={ChartNoAxesColumnIncreasing}
                    valor={comTerritorio ? reaisCompactos(totais.vendas) : null}
                    pe={
                      <>
                        <strong>{nº(totais.clientesQueCompraram)}</strong> clientes compraram
                      </>
                    }
                  />
                  <CartaoDeVenda
                    rotulo="Máquina"
                    icone={Tractor}
                    valor={comTerritorio ? reaisCompactos(totais.maquina) : null}
                    // AS MÁQUINAS VENDIDAS NÃO EXISTEM AINDA — o faturamento é em
                    // reais, e a contagem em unidades é a issue 69. O lugar da
                    // maquete fica, com o traço.
                    pe={
                      <>
                        <ValorAusente
                          motivo="As vendas em UNIDADES por município são a issue 69: hoje o faturamento chega em reais, e contar notas não é contar máquinas."
                          oQue="o número de máquinas vendidas"
                        />{' '}
                        máquinas vendidas
                      </>
                    }
                  />
                  <CartaoDeVenda
                    rotulo="Pós-venda"
                    icone={Wrench}
                    valor={comTerritorio ? reaisCompactos(totais.posVenda) : null}
                    pe="Peças e serviços"
                  />
                </div>
              </div>
            ),
          },
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
