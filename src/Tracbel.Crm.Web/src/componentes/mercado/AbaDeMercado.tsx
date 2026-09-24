/**
 * A aba Mercado — os cinco blocos do documento 50, §4.
 *
 *   1. O mercado da região        4. Momento do mercado
 *   2. Visão geográfica           5. Performance Tracbel
 *   3. Potencial estrutural
 *
 * OS QUATRO MAPAS FICAM AQUI, juntos e lado a lado. É a parte da tela que
 * funciona, e é a resposta à pergunta "onde está o mercado" — mandar a diretoria
 * trocar para Território para ver o território seria perder o que ela tem de
 * melhor. Eles mudaram de posição na página; por dentro, nada.
 */

import { Sprout } from 'lucide-react';
import type { ReactNode } from 'react';
import { InfoTooltip } from '../InfoTooltip';
import type { ContextoDeAcesso } from '../../dados/api/http';
import type {
  ClassificacaoDeIndicador,
  IndicadoresTerritoriais,
  NumerosDeDecisao,
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
  ficha,
  numerosDeDecisao,
}: {
  /** Os quatro números do topo, com o motivo de cada ausência — da API (issue 69, parte A). */
  numerosDeDecisao: NumerosDeDecisao | null;
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
  /** A ficha do município escolhido, montada uma vez pela casca. */
  ficha: ReactNode;
}) {
  const predominante = indicadores?.momento?.predominante ?? null;

  return (
    <>
      <SecaoDoMercadoDaRegiao />

      {/* OS QUATRO NÚMEROS DE DECISÃO vêm primeiro (documento 50, §4.1), e o
          porte e o momento logo abaixo — dois números, nunca um. */}
      <KpisExecutivos
        momento={indicadores?.momento ?? null}
        demandaEstrutural={recorte?.demandaAnualDeMaquinas ?? null}
        demandaDeSaoPaulo={null}
        carregando={carregando}
        procedenciaDaDemanda={indicadores?.momento?.procedencia ?? null}
        numeros={numerosDeDecisao}
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
            'Os quatro mapas usam o mesmo enquadramento: o mesmo município fica no mesmo lugar nos quatro, e o ' +
            'cursor sobre ele mostra o número dele em todos ao mesmo tempo. Clicar abre a ficha do município e ' +
            'passa a valer para a página inteira.'
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
        {ficha}
        {/* AUDITORIA É NÍVEL 4 (issue 33): fica na tela, inteira, mas recolhida —
            ela não pode competir com mercado, potencial e mapas. */}
        <MetricasSemDado metricas={metricasSemDado} titulo="Limitações dos dados" compacto />
      </section>

      {/* OS TRÊS PAINÉIS DO RODAPÉ FICAM LADO A LADO (fase T4.7).

          Empilhados, cada um esticado de ponta a ponta, eles faziam mil e
          duzentos pixels de rolagem para responder três perguntas que se olham
          juntas: o que a área comporta, como o mercado está agora, e quanto a
          Tracbel leva. Lado a lado, a leitura é uma só — e é a composição que
          justifica a largura do container, em vez de deixá-la virar corredor.

          Abaixo de 1400px eles voltam a empilhar: três colunas de 400px com
          tabela e gráfico dentro não são três painéis, são três becos. */}
      {/* OS TRÊS PAINÉIS DO RODAPÉ, LADO A LADO — como na imagem base.

          O HTML do protótipo põe dois e um (Potencial e Momento juntos,
          Performance em largura inteira), mas a MAQUETE mostra os três numa
          linha, e é ela que manda: eles respondem três perguntas que se olham
          juntas — o que a área comporta, como o mercado está agora, e quanto a
          Tracbel leva.

          Abaixo de 1400px eles empilham: três colunas de 400px com tabela e
          gráfico dentro não são três painéis, são três becos. */}
      <div className="dash-tres-colunas">
        <BlocoDePotencial
          recorte={recorte}
          semFiltro={semFiltro}
          contexto={contexto}
          municipioCodigoIbge={municipioCodigoIbge}
        />

        <BlocoDoMomento
          municipioSelecionado={municipioCodigoIbge}
          nomeDoMunicipio={nomeDoMunicipio}
          produtosDoMunicipio={produtosDoMunicipio}
          momento={indicadores?.momento ?? null}
        />

        <PerformanceTracbel totais={totais} comTerritorio={comTerritorio} />
      </div>
    </>
  );
}
