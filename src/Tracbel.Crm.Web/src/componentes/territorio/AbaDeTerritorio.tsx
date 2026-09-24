/**
 * A aba Território — a leitura operacional (documento 50, §5), desenhada como a
 * maquete `territorio-ficha.png` (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * ELA NÃO DUPLICA OS CARTÕES EXECUTIVOS DE MERCADO. Onde um dado já aparece
 * resumido lá, aqui ele aparece na forma de trabalho: a linha, o nome, a data.
 *
 * TRÊS BLOCOS, na ordem da maquete:
 *   1. "A carteira na área de atuação" — o título com o controle de período à
 *      direita e os quatro cartões da carteira (municípios da ADR, cobertura
 *      pela cadência, vendas no período e parque teórico). Eles eram o
 *      `PainelDeIndicadores` do cadastro; agora são o cartão da maquete, e
 *      nenhum número foi ganho nem perdido.
 *   2. A tabela de municípios, paginada e ordenável.
 *   3. A ficha do município escolhido, ao lado da tabela.
 *
 * O QUE AINDA NÃO ESTÁ AQUI: responsáveis, cadência e carteira por município
 * existem hoje **dentro da ficha** do município, e não como painéis próprios.
 * Trazê-los para cá é trabalho da issue 78 e das fases seguintes — inventar
 * painéis vazios agora só encheria a aba.
 */

import type { ClassificacaoDeIndicador, IndicadoresDoMunicipio, IndicadoresForaDoMapa } from '../../tipos/territorio';
import { CartoesDaCarteira } from './carteira/CartoesDaCarteira';
import { ComparacaoDaCarteira } from './carteira/ComparacaoDaCarteira';
import { ProvedorDoPeriodo } from './carteira/periodo';
import { ComoLerEstesNumeros } from './ComoLerEstesNumeros';
import type { CarteiraNaArea } from './kpisDosIndicadores';
import { TabelaDeMunicipios } from './TabelaDeMunicipios';
import { TituloDaSecao } from './TituloDaSecao';
import type { TotaisDaAdr } from './totaisDaAdr';
import type { ReactNode } from 'react';
import '../../estilos/territorio-carteira.css';

export function AbaDeTerritorio({
  kpisDaCarteira,
  carregando,
  indicadores,
  municipios,
  ordenados,
  foraDoMapa,
  totais,
  selecionado,
  aoSelecionar,
  territorioNaoCarregado,
  semFiltro,
  classificacoes,
  ficha,
}: {
  /** Os quatro cartões e a janela de competência que o servidor aplicou. */
  kpisDaCarteira: CarteiraNaArea;
  carregando: boolean;
  /** Nulo enquanto a leitura não respondeu — a tabela não aparece antes disso. */
  indicadores: boolean;
  municipios: IndicadoresDoMunicipio[];
  ordenados: IndicadoresDoMunicipio[];
  foraDoMapa: IndicadoresForaDoMapa[];
  totais: TotaisDaAdr;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
  territorioNaoCarregado: boolean;
  semFiltro: boolean;
  /** Os selos de como ler cada indicador — vão para a dica do título. */
  classificacoes: ClassificacaoDeIndicador[];
  ficha: ReactNode;
}) {
  return (
    // O PERÍODO DESCE POR CONTEXTO até a tabela e a ficha: o "12 meses" da
    // maquete aparece nas três, e a ficha é montada pela casca — ver `periodo.ts`.
    <ProvedorDoPeriodo value={kpisDaCarteira.periodo}>
      <section data-bloco="carteira-na-area" className="terr-cart-secao">
        <TituloDaSecao
          titulo="A carteira na área de atuação"
          // "SUA ÁREA DE ATUAÇÃO", e não "sua região" como a maquete escreve
          // (decisão 2): "região" sozinha é o que a issue 163 tirou da tela.
          subtitulo="Quem está coberto, quanto foi vendido e quanto a área comporta — visão consolidada da sua área de atuação."
          // "COMO INTERPRETAR OS INDICADORES" MORA AQUI TAMBÉM (fidelidade às
          // maquetes, 23/09/2026): era um `<details>` acima das abas, valendo
          // para as duas; a maquete não o tem, e o texto foi para a dica do
          // primeiro título de cada aba.
          metodologia={classificacoes.length > 0 ? <ComoLerEstesNumeros classificacoes={classificacoes} /> : undefined}
          acao={<ComparacaoDaCarteira periodo={kpisDaCarteira.periodo} />}
        />
        <CartoesDaCarteira cartoes={kpisDaCarteira.cartoes} carregando={carregando} />
      </section>

      {/* A FICHA FICA AO LADO DA TABELA, e não abaixo (fase T4.7).

          Abrir um município mandava a ficha para o fim da página: quem clicava
          na linha 40 rolava duzentos pixels para ver o detalhe, e perdia de vista
          a linha de onde tinha vindo. Comparar dois municípios virava um
          vai-e-vem. Lado a lado, a tabela continua na tela enquanto o detalhe é
          lido — que é como se compara.

          SEM MUNICÍPIO ESCOLHIDO A TABELA OCUPA TUDO: uma coluna vazia esperando
          clique seria metade da tela reservada para o que talvez não aconteça. */}
      <div className={ficha ? 'terr-territorio terr-territorio-com-ficha' : 'terr-territorio'}>
        {indicadores && (
          <TabelaDeMunicipios
            municipios={municipios}
            daAdr={ordenados}
            foraDoMapa={foraDoMapa}
            totais={totais}
            selecionado={selecionado}
            aoSelecionar={aoSelecionar}
            territorioNaoCarregado={territorioNaoCarregado}
            semFiltro={semFiltro}
          />
        )}

        {ficha && <div className="terr-territorio-ficha">{ficha}</div>}
      </div>
    </ProvedorDoPeriodo>
  );
}
