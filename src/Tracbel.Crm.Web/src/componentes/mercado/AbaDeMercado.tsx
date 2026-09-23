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
import { PorteEMomento } from './PorteEMomento';
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
  /** A ficha do município escolhido, montada uma vez pela casca. */
  ficha: ReactNode;
}) {
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
      />
      <PorteEMomento momento={indicadores?.momento ?? null} />

      {/* O QUE A REGIÃO TEM continua na tela, agora numa faixa de uma linha
          (T4.6): são o pano de fundo do mercado, e desenhá-los como cartão do
          mesmo tamanho dos quatro de cima fazia nove cartões iguais empilhados —
          uma lista, não uma hierarquia. Nenhum número saiu. */}
      <FaixaDoMercado indicadores={kpisDoMercado} />

      <section data-bloco="visao-geografica">
        <TituloDaSecao
          titulo="Visão geográfica"
          subtitulo="Compare os municípios sob quatro perspectivas."
          metodologia={
            'Os quatro mapas usam o mesmo enquadramento: o mesmo município fica no mesmo lugar nos quatro, e o ' +
            'cursor sobre ele mostra o número dele em todos ao mesmo tempo. Clicar abre a ficha do município e ' +
            'passa a valer para a página inteira.'
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
    </>
  );
}
