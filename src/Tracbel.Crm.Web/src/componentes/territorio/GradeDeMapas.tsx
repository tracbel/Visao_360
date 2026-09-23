/**
 * OS QUATRO MAPAS LADO A LADO, NO MESMO QUADRO (documento 50, §4.4).
 *
 * O enquadramento é a ADR inteira e é o mesmo nos quatro, então o mesmo município
 * está no mesmo lugar em todos — e o cursor sobre ele mostra o detalhe dele nos
 * quatro ao mesmo tempo. Os quatro cartões são filhos DIRETOS desta grade: é o
 * que o CSS transforma em 2×2, e o que a issue 76 manda preservar.
 */

import type {
  ClassificacaoDeIndicador,
  IndicadoresTerritoriais,
  RegraDePotencialAplicada,
} from '../../tipos/territorio';
import { MapaDaEstrutura } from './mapas/MapaDaEstrutura';
import { MapaDeCobertura } from './mapas/MapaDeCobertura';
import { MapaDeVendas } from './mapas/MapaDeVendas';
import { MapaDoPotencial } from './mapas/MapaDoPotencial';
import type { LigacaoDoMapa } from './mapas/CartaoDeMapa';
import type { TotaisDaAdr } from './totaisDaAdr';

export function GradeDeMapas({
  ligacao,
  indicadores,
  totais,
  coberturaDaAdr,
  regra,
  anoDoCenso,
  anoDoRebanho,
  classificacaoDe,
}: {
  ligacao: LigacaoDoMapa;
  indicadores: IndicadoresTerritoriais;
  totais: TotaisDaAdr;
  coberturaDaAdr: number | null;
  regra: RegraDePotencialAplicada | null;
  anoDoCenso: number | null;
  anoDoRebanho: number | null;
  classificacaoDe: (indicador: ClassificacaoDeIndicador['indicador']) => ClassificacaoDeIndicador | null;
}) {
  return (
    <div className="terr-grade-mapas" data-bloco="mapas">
      <MapaDeCobertura
        ligacao={ligacao}
        totais={totais}
        coberturaDaAdr={coberturaDaAdr}
        classificacao={classificacaoDe('coberturaDeVisita')}
      />
      <MapaDeVendas
        ligacao={ligacao}
        totais={totais}
        competenciaInicial={indicadores.competenciaInicial}
        competenciaFinal={indicadores.competenciaFinal}
        classificacao={classificacaoDe('vendas')}
      />
      <MapaDoPotencial
        ligacao={ligacao}
        totais={totais}
        regra={regra}
        enderecos={indicadores.enderecos}
        enderecosComArea={indicadores.enderecosComArea}
        classificacao={classificacaoDe('potencial')}
      />
      <MapaDaEstrutura ligacao={ligacao} totais={totais} anoDoCenso={anoDoCenso} anoDoRebanho={anoDoRebanho} />
    </div>
  );
}
