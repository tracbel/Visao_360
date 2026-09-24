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
 * de numerador para uma demanda medida em máquinas. O motivo de cada um faltar
 * NÃO é escrito aqui: é a frase que o servidor devolve nos números de decisão
 * (issue 69, parte A) — a mesma dos cartões do topo da aba.
 *
 * VENDAS VEM PRIMEIRO, E ABERTA (maquete): é a aba que tem número. Abrir o
 * bloco numa aba de traço seria começar a leitura pelo que falta.
 */

import { ChartNoAxesColumnIncreasing, Tractor, Wrench, type LucideIcon } from 'lucide-react';
import { useState, type ReactNode } from 'react';
import type { NumeroDeDecisao, NumerosDeDecisao } from '../../tipos/territorio';
import { MetricaAusente } from '../comum/MetricaAusente';
import { ValorAusente } from '../comum/ValorAusente';
import { reaisCompactos } from '../territorio/escalas';
import { nº, porcento } from '../territorio/indicadoresDaAdr';
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

/**
 * CAPTURA E NÃO CAPTURADO, NA ABA DE CADA UM: o número quando o servidor o
 * manda; senão o traço com a frase DELE — sem uma segunda redação escrita aqui.
 */
function NumeroNaAba({
  metrica,
  numero,
  formatar,
  depois,
}: {
  metrica: string;
  numero: NumeroDeDecisao | null;
  formatar: (v: number) => string;
  /** O que vem depois do número, na frase em que ele aparece. */
  depois: string;
}) {
  if (numero?.valor != null)
    return (
      <p className="mv-potencial-frase">
        <strong>{formatar(numero.valor)}</strong> {depois}
      </p>
    );

  if (numero?.frase) return <MetricaAusente metrica={metrica} motivo={numero.frase} />;

  // A LEITURA AINDA NÃO RESPONDEU: o traço sem dica, porque não se afirma por
  // que um número falta antes de saber se ele falta.
  return (
    <p className="cad-metrica-ausente">
      <span className="cad-metrica-ausente-rotulo">{metrica}</span>
      <span className="cad-ausente">
        <span aria-hidden="true">—</span>
        <span className="cad-so-leitor">sem dado</span>
      </span>
    </p>
  );
}

export function PerformanceTracbel({
  totais,
  comTerritorio,
  numeros,
}: {
  totais: TotaisDaAdr;
  comTerritorio: boolean;
  /** Os números de decisão do recorte, com o motivo de cada ausência — da API (issue 69, parte A). */
  numeros: NumerosDeDecisao | null;
}) {
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
            {/* O PORQUÊ DE CAPTURA E NÃO CAPTURADO FALTAREM não mora aqui: é a
                frase do servidor, na dica de cada um. Aqui fica o que eles são. */}
            <p>
              Captura e não capturado se contam em UNIDADES: reais não servem de numerador para uma demanda medida
              em máquinas. Enquanto o denominador for demanda estimada, o número se chama captura, e não share.
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
              <NumeroNaAba
                metrica="Captura Tracbel"
                numero={numeros?.capturaPercentual ?? null}
                formatar={porcento}
                depois="da demanda anual estimada, em máquinas — captura, e não participação de mercado."
              />
            ),
          },
          {
            id: 'naoCapturado',
            rotulo: 'Não capturado',
            conteudo: (
              <NumeroNaAba
                metrica="Potencial não capturado"
                numero={numeros?.oportunidade ?? null}
                formatar={(v) => nº(Math.round(v))}
                depois="máquinas da demanda ajustada que a Tracbel ainda não vendeu."
              />
            ),
          },
        ]}
      />
    </section>
  );
}
