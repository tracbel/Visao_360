/**
 * A casca comum dos quatro cartões de mapa (issue 170, parte A).
 *
 * OS QUATRO TÊM O MESMO ESQUELETO — título, subtítulo, resumo, alternador, mapa,
 * linha do cursor, legenda e aviso —, e antes ele estava escrito quatro vezes.
 * Aqui ele está uma vez só, e cada mapa traz o que é dele. O desenho não muda:
 * as classes e a ordem dos elementos são as mesmas de antes.
 *
 * O ENQUADRAMENTO É COMPARTILHADO: os quatro recebem a mesma `ligacao`, e é por
 * isso que o mesmo município fica no mesmo lugar nos quatro e o cursor sobre ele
 * acende os quatro ao mesmo tempo.
 */

import type { ComponentProps, ReactNode } from 'react';
import type { IndicadoresDoMunicipio } from '../../../tipos/territorio';
import {
  LegendaDoMapa,
  MapaDeMunicipios,
  type EstadoNoMapa,
  type PoligonoProjetado,
} from '../MapaDeMunicipios';
import type { Enquadramento } from '../projecao';

/** O que os quatro mapas compartilham: o desenho, o recorte e o cursor. */
export type LigacaoDoMapa = {
  enquadramento: Enquadramento;
  poligonos: PoligonoProjetado[];
  /** Os municípios da ADR, que ganham o contorno verde. */
  adr: ReadonlySet<number>;
  porCodigo: ReadonlyMap<number, IndicadoresDoMunicipio>;
  nomeDoPoligono: ReadonlyMap<number, string>;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
  emFoco: number | null;
  aoPassar: (codigo: number | null) => void;
};

export function CartaoDeMapa({
  mapa,
  id,
  titulo,
  subtitulo,
  resumo,
  antesDoAlternador,
  alternador,
  tituloDoMapa,
  estadoDe,
  faixas,
  unidade,
  aviso,
  classeDoAviso = 'terr-aviso',
  ligacao,
}: {
  /** O nome do mapa para o teste de estrutura; nenhuma regra de CSS o usa. */
  mapa: string;
  /** Separa os padrões de hachura dos SVGs. */
  id: string;
  titulo: ReactNode;
  subtitulo: ReactNode;
  resumo: ReactNode;
  /** O que entra entre o resumo e o alternador — só o mapa de potencial usa. */
  antesDoAlternador?: ReactNode;
  alternador: ReactNode;
  tituloDoMapa: string;
  estadoDe: (codigo: number) => EstadoNoMapa;
  faixas: ComponentProps<typeof LegendaDoMapa>['faixas'];
  unidade: string;
  aviso: ReactNode;
  classeDoAviso?: string;
  ligacao: LigacaoDoMapa;
}) {
  return (
    <div className="card cad-cartao terr-mapa" data-mapa={mapa}>
      <div className="card-title">{titulo}</div>
      <div className="card-subtitle">{subtitulo}</div>
      <p className="terr-mapa-resumo">{resumo}</p>
      {antesDoAlternador}
      {alternador}
      <MapaDeMunicipios
        id={id}
        titulo={tituloDoMapa}
        enquadramento={ligacao.enquadramento}
        poligonos={ligacao.poligonos}
        estadoDe={estadoDe}
        adr={ligacao.adr}
        selecionado={ligacao.selecionado}
        aoSelecionar={ligacao.aoSelecionar}
        emFoco={ligacao.emFoco}
        aoPassar={ligacao.aoPassar}
      />
      <p className="terr-mapa-foco" aria-live="polite">{textoDoFoco(ligacao, estadoDe)}</p>
      <LegendaDoMapa faixas={faixas} unidade={unidade} />
      <p className={classeDoAviso}>{aviso}</p>
    </div>
  );
}

/** O detalhe do município sob o cursor, na medida de cada mapa. */
function textoDoFoco(ligacao: LigacaoDoMapa, estadoDe: (codigo: number) => EstadoNoMapa): string {
  if (ligacao.emFoco === null)
    return 'Passe o cursor sobre um município para ver o número dele nos três mapas; clique para abrir a ficha.';
  return `${ligacao.nomeDoPoligono.get(ligacao.emFoco) ?? ligacao.emFoco} — ${estadoDe(ligacao.emFoco).detalhe}`;
}
