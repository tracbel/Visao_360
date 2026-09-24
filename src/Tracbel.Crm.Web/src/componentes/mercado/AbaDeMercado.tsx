/**
 * A aba Mercado — os cinco blocos do documento 50, §4.
 *
 * A ORDEM É A DA MAQUETE DE RENTABILIDADE E CRÉDITO (decisão do usuário,
 * 23/09/2026):
 *
 *   1. O mercado da região — os quatro números de decisão, logo abaixo das abas
 *   2. Momento do mercado — em LARGURA INTEIRA
 *   3. A régua do mercado — momento, porte e os cinco números estruturais
 *   4. Visão geográfica — os quatro mapas
 *   5. Potencial estrutural e Performance Tracbel, lado a lado
 *
 * POR QUE O MOMENTO SAIU DA LINHA DE BAIXO. Ele era um terço de uma linha de
 * três colunas, e dentro dele moram Rentabilidade e Crédito — tabela, gráfico e
 * ranking espremidos em ~400px. Era a causa da "rolagem infinita" de Crédito: o
 * painel crescia para baixo o que não tinha de largura. A maquete o põe em
 * largura inteira, logo depois dos quatro números, e é ela que manda.
 *
 * OS QUATRO MAPAS FICAM AQUI, juntos e lado a lado. É a parte da tela que
 * funciona, e é a resposta à pergunta "onde está o mercado" — mandar a diretoria
 * trocar para Território para ver o território seria perder o que ela tem de
 * melhor.
 *
 * A FICHA DO MUNICÍPIO NÃO MORA MAIS AQUI (fidelidade às maquetes). Ela aparecia
 * embaixo dos mapas E ao lado da tabela de Território — duas casas para o mesmo
 * detalhe. Agora clicar num município no mapa escolhe o município (a URL, issue
 * 163) e leva para Território, onde a ficha abre ao lado da linha dele.
 */

import { Sprout } from 'lucide-react';
import { InfoTooltip } from '../InfoTooltip';
import type { ContextoDeAcesso } from '../../dados/api/http';
import type {
  ClassificacaoDeIndicador,
  IndicadoresTerritoriais,
  PotencialDoRecorteNoMapa,
  RegraDePotencialAplicada,
} from '../../tipos/territorio';
import type { MetricaSemDado } from '../../tipos/relacionamento';
import { BlocoCarregando } from '../cadastro/EstadosDeTela';
import type { Indicador } from '../cadastro/Indicadores';
import { MetricasSemDado } from '../cadastro/SemDado';
import { GradeDeMapas } from '../territorio/GradeDeMapas';
import type { LigacaoDoMapa } from '../territorio/mapas/CartaoDeMapa';
import { SecaoDoMercadoDaRegiao } from '../territorio/SecaoDoMercadoDaRegiao';
import { TituloDaSecao } from '../territorio/TituloDaSecao';
import type { TotaisDaAdr } from '../territorio/totaisDaAdr';
import { BlocoDePotencial } from './BlocoDePotencial';
import { FaixaDoMercado } from './FaixaDoMercado';
import { KpisExecutivos } from './KpisExecutivos';
import { BlocoDoMomento } from './BlocoDoMomento';
import { PerformanceTracbel } from './PerformanceTracbel';

export function AbaDeMercado({
  kpisDoMercado,
  carregando,
  ligacao,
  indicadores,
  totais,
  comTerritorio,
  coberturaDaAdr,
  regra,
  recorte,
  semFiltro,
  contexto,
  anoDoCenso,
  anoDoRebanho,
  classificacaoDe,
  metricasSemDado,
  municipioCodigoIbge,
  nomeDoMunicipio,
  produtosDoMunicipio,
  mostrarOsMapas,
}: {
  kpisDoMercado: Indicador[];
  carregando: boolean;
  ligacao: LigacaoDoMapa | null;
  indicadores: IndicadoresTerritoriais | null;
  totais: TotaisDaAdr;
  comTerritorio: boolean;
  coberturaDaAdr: number | null;
  regra: RegraDePotencialAplicada | null;
  recorte: PotencialDoRecorteNoMapa | null;
  semFiltro: boolean;
  contexto: ContextoDeAcesso;
  anoDoCenso: number | null;
  anoDoRebanho: number | null;
  classificacaoDe: (indicador: ClassificacaoDeIndicador['indicador']) => ClassificacaoDeIndicador | null;
  metricasSemDado: MetricaSemDado[] | undefined;
  municipioCodigoIbge: number | null;
  /** O nome do município escolhido, para os painéis dizerem a granularidade da fonte. */
  nomeDoMunicipio: string | null;
  /** Os produtos da PAM do municipio escolhido, por area plantada (issue 168). */
  produtosDoMunicipio: readonly number[];
  /** Se a grade pode ser desenhada — há resposta, há malha e o território está carregado. */
  mostrarOsMapas: boolean;
}) {
  const predominante = indicadores?.momento?.predominante ?? null;

  return (
    <>
      {/* OS QUATRO NÚMEROS DE DECISÃO vêm primeiro (documento 50, §4.1), logo
          abaixo das abas — o título da seção fica só para o leitor de tela. */}
      <SecaoDoMercadoDaRegiao>
        <KpisExecutivos
          momento={indicadores?.momento ?? null}
          demandaEstrutural={recorte?.demandaAnualDeMaquinas ?? null}
          demandaDeSaoPaulo={null}
          carregando={carregando}
          procedenciaDaDemanda={indicadores?.momento?.procedencia ?? null}
        />
      </SecaoDoMercadoDaRegiao>

      {/* O MOMENTO EM LARGURA INTEIRA (maquete de rentabilidade e crédito). Por
          dentro ele não mudou nesta fase: abas, cartões e tabelas são os de
          antes — o redesenho do interior é a fase 3. */}
      <BlocoDoMomento
        municipioSelecionado={municipioCodigoIbge}
        nomeDoMunicipio={nomeDoMunicipio}
        produtosDoMunicipio={produtosDoMunicipio}
        momento={indicadores?.momento ?? null}
      />

      {/* MOMENTO, PORTE E O QUE A REGIÃO TEM, NUMA RÉGUA SÓ (fase T4.8).

          Eram dois blocos brancos empilhados — a faixa de porte e momento e a
          dos cinco indicadores estruturais. São a mesma leitura: como está o
          mercado, e o que existe nele. Juntos numa régua de sete células, com
          filete entre elas, o olho corre de uma ponta à outra. */}
      <FaixaDoMercado indicadores={kpisDoMercado} momento={indicadores?.momento ?? null} />

      <section data-bloco="visao-geografica">
        <TituloDaSecao
          titulo="Visão geográfica"
          subtitulo="Compare os municípios sob quatro perspectivas."
          metodologia={
            <>
              <p>
                Os quatro mapas usam o mesmo enquadramento: o mesmo município fica no mesmo lugar nos quatro, e o
                cursor sobre ele mostra o número dele em todos ao mesmo tempo. Clicar num município escolhe o
                município para a página inteira e abre a ficha dele na aba Território.
              </p>
              {/* AS LIMITAÇÕES DOS DADOS MORAM AQUI (fidelidade às maquetes,
                  23/09/2026). Eram um `<details>` embaixo dos mapas, que a
                  maquete não tem; a lista e o rodapé são os mesmos, inteiros. */}
              <MetricasSemDado metricas={metricasSemDado} titulo="Limitações dos dados" naDica />
            </>
          }
          // A CULTURA PREDOMINANTE VIRA O SELO DA SEÇÃO (protótipo, §.meta-chip).
          //
          // Ela morava na faixa de porte e momento; ao fundir a faixa, o lugar
          // dela deixou de existir. Aqui ela fica melhor: é a resposta a "o que
          // se planta aqui?", que é a pergunta de quem está olhando o mapa.
          //
          // CONTINUA SENDO CONTEXTO, com o critério dito na dica — ela não entra
          // no cálculo do momento, e a dica diz isso com todas as letras.
          acao={
            predominante && (
              <span className="dash-selo-secao" data-contexto="predominante">
                <Sprout size={14} strokeWidth={2} aria-hidden="true" />
                Principal cultura: <strong>{predominante.cultura}</strong> ·{' '}
                {predominante.fatia.toLocaleString('pt-BR', { maximumFractionDigits: 0 })}% da área relevante
                <InfoTooltip
                  rotulo="Qual é o critério da principal cultura"
                  texto={
                    `Critério: ${predominante.criterio}. ` +
                    'Isto é CONTEXTO — responde "o que se planta aqui?" — e não entra no cálculo do momento: o ' +
                    'fator de cada cultura pesa pela demanda que ela representa, não pela área. Trocar qual ' +
                    'cultura tem a maior área muda esta linha e não muda o momento.'
                  }
                />
              </span>
            )
          }
        />
        {carregando && !mostrarOsMapas && <BlocoCarregando oQue="os mapas da ADR" />}
        {mostrarOsMapas && indicadores && ligacao && (
          <GradeDeMapas
            ligacao={ligacao}
            indicadores={indicadores}
            totais={totais}
            coberturaDaAdr={coberturaDaAdr}
            regra={regra}
            anoDoCenso={anoDoCenso}
            anoDoRebanho={anoDoRebanho}
            classificacaoDe={classificacaoDe}
          />
        )}
      </section>

      {/* A LINHA FINAL: POTENCIAL E PERFORMANCE, LADO A LADO.

          Eram três painéis (com o Momento no meio) — a maquete de visão geral
          os mostrava assim. Com o Momento em largura inteira lá em cima (decisão
          de 23/09/2026), ficam os dois que se leem juntos: o que a área comporta
          e quanto a Tracbel leva. A proporção 5 : 4 é a da maquete; abaixo de
          1000px de conteúdo eles empilham. */}
      <div className="dash-linha-final">
        <BlocoDePotencial
          recorte={recorte}
          semFiltro={semFiltro}
          contexto={contexto}
          municipioCodigoIbge={municipioCodigoIbge}
        />

        <PerformanceTracbel totais={totais} comTerritorio={comTerritorio} />
      </div>
    </>
  );
}
