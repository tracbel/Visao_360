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

import type { ReactNode } from 'react';
import type { ContextoDeAcesso } from '../../dados/api/http';
import type {
  ClassificacaoDeIndicador,
  IndicadoresTerritoriais,
  PotencialDoRecorteNoMapa,
  RegraDePotencialAplicada,
} from '../../tipos/territorio';
import type { MetricaSemDado } from '../../tipos/relacionamento';
import { BlocoCarregando } from '../cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../cadastro/Indicadores';
import { MetricasSemDado } from '../cadastro/SemDado';
import { GradeDeMapas } from '../territorio/GradeDeMapas';
import type { LigacaoDoMapa } from '../territorio/mapas/CartaoDeMapa';
import { SecaoDoMercadoDaRegiao } from '../territorio/SecaoDoMercadoDaRegiao';
import { TituloDaSecao } from '../territorio/TituloDaSecao';
import type { TotaisDaAdr } from '../territorio/totaisDaAdr';
import { BlocoDePotencial } from './BlocoDePotencial';
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
  mostrarOsMapas,
  ficha,
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
  /** Se a grade pode ser desenhada — há resposta, há malha e o território está carregado. */
  mostrarOsMapas: boolean;
  /** A ficha do município escolhido, montada uma vez pela casca. */
  ficha: ReactNode;
}) {
  return (
    <>
      <SecaoDoMercadoDaRegiao />
      <PainelDeIndicadores indicadores={kpisDoMercado} carregando={carregando} />

      <section data-bloco="visao-geografica">
        <TituloDaSecao
          titulo="Visão geográfica"
          subtitulo="Os quatro mapas no mesmo enquadramento: o mesmo município está no mesmo lugar nos quatro, e o cursor sobre ele mostra o número dele em todos. Clique para abrir a ficha."
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
        <MetricasSemDado metricas={metricasSemDado} titulo="O que estes mapas não dizem" />
      </section>

      <BlocoDePotencial
        recorte={recorte}
        semFiltro={semFiltro}
        contexto={contexto}
        municipioCodigoIbge={municipioCodigoIbge}
      />

      <BlocoDoMomento />

      <PerformanceTracbel totais={totais} comTerritorio={comTerritorio} />
    </>
  );
}
